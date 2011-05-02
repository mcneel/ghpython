using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GhPython.Component;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Grasshopper.Kernel.Undo;

namespace GhPython.Forms
{
    public partial class PythonScriptForm : Form
    {
        Control _texteditor;
        bool _showClosePrompt = true;

        /// <summary>
        /// The linked component. This field might be null.
        /// </summary>
        PythonComponent _component;
        int _targetVariableMenuIndex = -1;

        // keep default constructor around to not "confuse" Visual Studio's designer
        public PythonScriptForm() : this(null)
        {
        }
        public PythonScriptForm(PythonComponent linkedComponent)
        {
            InitializeComponent();
            this.KeyDown += ScriptForm_KeyDown;

            _component = linkedComponent;

            if (_component != null)
            {
                _texteditor = _component.CreateEditorControl(OnPythonHelp);
                this.splitContainer.Panel1.Controls.Add(_texteditor);
                _texteditor.Dock = DockStyle.Fill;

                var inputCode = _component.Params.Input[0];
                if (inputCode.VolatileDataCount > 0 && inputCode.VolatileData.PathCount > 0)
                {
                    var goo = inputCode.VolatileData.get_Branch(0)[0] as IGH_Goo;
                    if (goo != null)
                    {
                        string code;
                        if (goo.CastTo(out code) && !string.IsNullOrEmpty(code))
                            _texteditor.Text = code;
                    }
                }


                _targetVariableMenuIndex = 
                    fileToolStripMenuItem.DropDownItems.Add(_component.GetTargetVariableMenuItem());
            }

            versionLabel.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        void OnPythonHelp(string str)
        {
          richTextBox1.Text = str;
        }

        void ScriptForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_texteditor == null)
                return;
            // 22 April 2011 - S. Baer
            // I realize this is very "hacky", but it will keep
            // things working until I move this logic into the control itself
            var mi = _texteditor.GetType().GetMethod("ProcessKeyDown");
            if (mi != null)
                mi.Invoke(_texteditor, new object[] { e });
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetDefinitionValue(true, true);
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetDefinitionValue(false, true);
            }
            catch(Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void SetDefinitionValue(bool close, bool expire)
        {
            if (_component != null)
            {
                IList<IGH_Param> outs = _component.Params.Input;

                if (outs.Count > 0)
                {
                    var codeInput = outs[0] as GH_PersistentParam<GH_String>;

                    if (codeInput != null)
                    {
                        if (codeInput.SourceCount != 0)
                        {
                            if (MessageBox.Show("There is dynamic inherited input that overrides this components behaviour.\nPlease unlink the first input to see the result.",
                                "Rhino.Python", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                            {
                                return;
                            }
                        }

                        GH_Document ghd = _component.OnPingDocument();
                        if(ghd != null)
                        ghd.UndoServer.PushUndoRecord("Python code changed",
                            new Grasshopper.Kernel.Undo.Actions.GH_GenericObjectAction(codeInput));

                        codeInput.ClearPersistentData();
                        string newCode = _texteditor.Text;

                        if (!string.IsNullOrEmpty(newCode))
                        {
                            codeInput.AddPersistentData(new GH_String(newCode));
                        }

                        if (expire)
                            codeInput.ExpireSolution(true);

                        if (close)
                        {
                            _showClosePrompt = false;
                            Close();
                        }
                    }
                }
            }
        }

        public void HelpText(string input)
        {
            richTextBox1.Text = input;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (_showClosePrompt)
                {
                    var result = MessageBox.Show("Do you want to apply before closing?",
                        "Rhino.Python closing", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SetDefinitionValue(false, true);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }

            if (!e.Cancel)
            {
                if (_component != null)
                {
                    var attributes = _component.Attributes as PythonComponentAttributes;

                    if (attributes != null)
                    {
                        attributes.DisableLinkedForm(false);
                    }
                }
            }
            
            base.OnClosing(e);
        }

        public void Disable()
        {
            _showClosePrompt = false;

            SuspendLayout();
            okButton.Enabled = false;
            testButton.Enabled = false;

            this.Text += " (disabled)";
            mainStatusText.Text = "Window is disabled because linked component was cancelled.";

            if(_targetVariableMenuIndex >= 0 && _targetVariableMenuIndex < fileToolStripMenuItem.DropDownItems.Count)
                fileToolStripMenuItem.DropDownItems.RemoveAt(_targetVariableMenuIndex);

            _component = null;
            ResumeLayout(true);
        }

        private void LastHandleException(Exception ex)
        {
            if (ex != null)
            {
                MessageBox.Show("An error occurred in the Python script window.\nIt would be great if you could take a screenshot of this and send it to giulio@mcneel.com.\nThanks.\n\n" + ex.Message,
                    "Error in Python script window (" + ex.GetType().Name + ")", MessageBoxButtons.OK);
            }
        }

        private void importFrom_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fd = new OpenFileDialog())
                {
                    fd.Filter = "Python files (*.py)|*.py|All files (*.*)|*.*";
                    fd.FilterIndex = 0;

                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        string text = File.ReadAllText(fd.FileName);
                        _texteditor.Text = text;
                    }
                }
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void exportAs_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fd = new SaveFileDialog())
                {
                    fd.Filter = "Python files (*.py)|*.py|All files (*.*)|*.*";
                    fd.FilterIndex = 0;

                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(fd.FileName, _texteditor.Text, Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void rhinoPythonWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://python.rhino3d.com/");
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void grasshopperForumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://www.grasshopper3d.com/forum/");
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void pythonDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://docs.python.org/");
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }
    }
}
