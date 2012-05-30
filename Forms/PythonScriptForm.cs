using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GhPython.Component;
using GhPython.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using Grasshopper.GUI.HTML;
using Grasshopper;

namespace GhPython.Forms
{
    public partial class PythonScriptForm : Form
    {
        Control _texteditor;
        bool _showClosePrompt = true;
        TextHashMaintainer _hash = new TextHashMaintainer();

        /// <summary>
        /// The linked component. This field might be null.
        /// </summary>
        ScriptingAncestorComponent _component;

        // keep default constructor around to not "confuse" Visual Studio's designer
        public PythonScriptForm() : this(null)
        {
        }
        public PythonScriptForm(ScriptingAncestorComponent linkedComponent)
        {
            InitializeComponent();
            this.KeyDown += ScriptForm_KeyDown;
            this.HelpRequested += rhinoscriptsyntaxHelp;

            _component = linkedComponent;

            if (_component != null)
            {
                _texteditor = _component.CreateEditorControl(OnPythonHelp);
                this.splitContainer.Panel1.Controls.Add(_texteditor);
                _texteditor.Dock = DockStyle.Fill;

                _texteditor.Text = _component.CodeInput;

                _hash.HashText(_texteditor.Text);
            }

            versionLabel.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static bool IsOnScreen(Point pt)
        {
            foreach (var screen in Screen.AllScreens)
                if (screen.WorkingArea.Contains(pt))
                    return true;

            return false;
        }

        private void PythonScriptForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (IsOnScreen(Settings.Default.EditorLocation))
                    Location = Settings.Default.EditorLocation;

                if (Settings.Default.EditorSize != Size.Empty)
                    Size = Settings.Default.EditorSize;
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void PythonScriptForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Settings.Default.EditorLocation = Location;
                Settings.Default.EditorSize = (WindowState == FormWindowState.Normal) ? Size : RestoreBounds.Size;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        void OnPythonHelp(string str)
        {
            richTextBox1.Text = str;
        }

        Dictionary<Keys, EventHandler> _handlers;

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
            
            // 30 May 2012 - G. Piacentino
            // This is here for the same reason as the above: no win message pump.
            if (_handlers == null)
            {
                 _handlers = new Dictionary<Keys, EventHandler>() {
                  { Keys.Control | Keys.E, exportAs_Click },
                  { Keys.Control | Keys.I, importFrom_Click },
                  { Keys.F5, applyButton_Click },
                  { Keys.Control | Keys.F5, okButton_Click },
                };
            }

            if (_handlers.ContainsKey(e.KeyData))
              _handlers[e.KeyData](sender, e);
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
                var codeInput = _component.CodeInputParam;

                string newCode;
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
                    if (ghd != null)
                        ghd.UndoServer.PushUndoRecord("Python code changed",
                            new Grasshopper.Kernel.Undo.Actions.GH_GenericObjectAction(codeInput));

                    codeInput.ClearPersistentData();
                    newCode = _texteditor.Text;

                    if (!string.IsNullOrEmpty(newCode))
                    {
                        codeInput.AddPersistentData(new GH_String(newCode));
                        _hash.HashText(newCode);
                    }
                }
                else
                {
                    GH_Document ghd = _component.OnPingDocument();
                    if (ghd != null)
                        ghd.UndoServer.PushUndoRecord("Python code changed",
                            new Grasshopper.Kernel.Undo.Actions.GH_GenericObjectAction(_component));

                    _component.CodeInput = _texteditor.Text;
                    _hash.HashText(_texteditor.Text);
                }

                if (expire)
                    _component.ExpireSolution(true);

                if (close)
                {
                    _showClosePrompt = false;
                    Close();
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
                var textHasChanged = !_hash.IsSameHashAsBefore(_texteditor.Text);
                if (_showClosePrompt && textHasChanged)
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
            mainStatusText.Text = "Window is disabled because linked component was deleted.";

            _component = null;
            ResumeLayout(true);
        }

        public static void LastHandleException(Exception ex)
        {
            if (ex != null)
            {
                MessageBox.Show("An error occurred in the Python script window.\nPlease send a screenshot of this to giulio@mcneel.com.\nThanks.\n\n" + ex.ToString(),
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

        private void rhinoscriptsyntaxHelp(object sender, EventArgs e)
        {
            try
            {
                var py = Rhino.Runtime.PythonScript.Create();
                var a = py.GetType().Assembly;
                string dir = Path.GetDirectoryName(a.Location);
                string filename = Path.Combine(dir, "RhinoIronPython.chm");

                if (System.IO.File.Exists(filename))
                {
                    var topic = GetCurrentWord();
                    var mode = HelpNavigator.KeywordIndex;
                    System.Windows.Forms.Help.ShowHelp(this, filename);

                    // 2011 Aug 22 Giulio Piacentino
                    // A second call is necessary as the first one opens with correct focus,
                    // but passing arguments opens a window that disappears when it is not focused
                    System.Windows.Forms.Help.ShowHelp(this, filename, mode, topic);
                }
                else
                    throw new FileNotFoundException(string.Format("The Python help file does not exist in {0}", filename));
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private string GetCurrentWord()
        {
            try
            {
                var t = _texteditor as dynamic;
                var actTxtCtrl = t.ActiveTextAreaControl;
                var caret = actTxtCtrl.Caret;
                var pos = caret.Position;
                object doc = actTxtCtrl.Document;
                int line = pos.Line;
                var mi = doc.GetType().GetMethod("GetLineSegment", BindingFlags.Public | BindingFlags.Instance);
                var seg = mi.Invoke(doc, new object[] { line }) as dynamic;
                var word = seg.GetWord(pos.Column);

                if (word != null)
                {
                    string text = word.Word;
                    return text;
                }
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
            return string.Empty;
        }

        private void rhinoscriptsyntaxBasicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSample(Resources.sampleScript);
        }

        private void rhinoCommonBasicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSample(Resources.sampleCommon);
        }

        private void SetSample(string sample)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_texteditor.Text))
                {
                    var result = MessageBox.Show("Open the sample will remove all changes.",
                        "Sample opening", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                    if (result != System.Windows.Forms.DialogResult.OK)
                        return;
                }

                _texteditor.Text = sample;
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }

        private void ghPythonGrasshopperHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var helpForm = new GH_HtmlHelpPopup();
                if (!helpForm.LoadObject(_component)) return;
                helpForm.SetLocation(Cursor.Position);
                helpForm.Show(GH_InstanceServer.DocumentEditor);
            }
            catch (Exception ex)
            {
                LastHandleException(ex);
            }
        }
    }
}