using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GhPython.Component;
using GhPython.Properties;
using Grasshopper.GUI.HTML;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GhPython.Forms
{
  public partial class PythonScriptForm : Form
  {
    internal readonly Control m_texteditor;
    bool _showClosePrompt = true;
    string m_previous_script = null;

    /// <summary>
    /// The linked component. This field might be null.
    /// </summary>
    ScriptingAncestorComponent _component;

    // keep default constructor around to not "confuse" Visual Studio's designer
    public PythonScriptForm()
      : this(null)
    {
    }
    public PythonScriptForm(ScriptingAncestorComponent linkedComponent)
    {
      InitializeComponent();

      _component = linkedComponent;

      if (_component != null)
      {
        m_texteditor = _component.CreateEditorControl(OnPythonHelp);
        this.splitContainer.Panel1.Controls.Add(m_texteditor);
        m_texteditor.Dock = DockStyle.Fill;

        m_texteditor.Text = _component.Code;
        m_previous_script = m_texteditor.Text;
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
        KeyDown += ScriptForm_KeyDown;
        HelpRequested += rhinoscriptsyntaxHelp;
        Move += PythonScriptForm_MoveResize;
        Resize += PythonScriptForm_MoveResize;
        Grasshopper.Instances.DocumentEditor.Move += PythonScriptForm_MoveResize;

        if (_component.DefaultEditorLocation != null && IsOnScreen(_component.DefaultEditorLocation.Value))
          Location = _component.DefaultEditorLocation.Value;

        if (_component.DefaultEditorSize != Size.Empty)
          Size = _component.DefaultEditorSize;
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
      if (m_texteditor == null)
        return;
      // 22 April 2011 - S. Baer
      // I realize this is very "hacky", but it will keep
      // things working until I move this logic into the control itself
      var mi = m_texteditor.GetType().GetMethod("ProcessKeyDown");
      if (mi != null)
        mi.Invoke(m_texteditor, new object[] { e });

      // 30 May 2012 - G. Piacentino
      // This is here for the same reason as the above: no win message pump.
      if (_handlers == null)
      {
        _handlers = new Dictionary<Keys, EventHandler> {
                  { Keys.Control | Keys.E, exportAs_Click },
                  { Keys.Control | Keys.I, importFrom_Click },
                  { Keys.F5, applyButton_Click },
                  { Keys.Control | Keys.F5, okButton_Click },
                };
      }

      if (_handlers.ContainsKey(e.KeyData))
        _handlers[e.KeyData](sender, e);
    }

    void PythonScriptForm_MoveResize(object sender, EventArgs e)
    {
      if (_component == null) return;

      _component.OnDisplayExpired(true);
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
      catch (Exception ex)
      {
        LastHandleException(ex);
      }
    }

    private void SetDefinitionValue(bool close, bool expire)
    {
      if (_component != null)
      {
        var codeInput = _component.CodeInputParam;

        if (codeInput != null)
        {
          if (_component.IsCodeInputLinked())
          {
            const string msg = "There is dynamic inherited input that overrides this components behaviour.\nPlease unlink the first input to see the result.";
            if (MessageBox.Show(msg, "Rhino.Python", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
              return;
          }

          GH_Document ghd = _component.OnPingDocument();
          if (ghd != null)
            ghd.UndoServer.PushUndoRecord("Python code changed",
                new Grasshopper.Kernel.Undo.Actions.GH_GenericObjectAction(codeInput));

          codeInput.PersistentData.Clear();
          string newCode = m_texteditor.Text;

          if (!string.IsNullOrEmpty(newCode))
          {
            codeInput.SetPersistentData(new GH_String(newCode));
            m_previous_script = newCode;
          }
        }
        else
        {
          GH_Document ghd = _component.OnPingDocument();
          if (ghd != null)
            ghd.UndoServer.PushUndoRecord("Python code changed",
                new Grasshopper.Kernel.Undo.Actions.GH_GenericObjectAction(_component));

          _component.Code = m_texteditor.Text;
          m_previous_script = m_texteditor.Text;
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
        var textHasChanged = m_previous_script != m_texteditor.Text;

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
          var attributes = _component.Attributes;

          if (attributes != null)
          {
            attributes.DisableLinkedEditor(false);
          }

          //we need to remove origin hinting
          _component.OnDisplayExpired(true); 

          //store last location
          _component.DefaultEditorLocation = Location;
          _component.DefaultEditorSize = (WindowState == FormWindowState.Normal) ? Size : RestoreBounds.Size;

          _component = null;
        }

        Grasshopper.Instances.DocumentEditor.Move -= PythonScriptForm_MoveResize;
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
        Rhino.Runtime.HostUtils.ExceptionReport(ex);

        // for now let's keep also the previous route open...
        MessageBox.Show("An error occurred in the Python script window.\nPlease send a screenshot of this to giulio@mcneel.com.\nThanks.\n\n" + ex,
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
            m_texteditor.Text = text;
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
            File.WriteAllText(fd.FileName, m_texteditor.Text, Encoding.UTF8);
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
          const HelpNavigator mode = HelpNavigator.KeywordIndex;
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
        var t = m_texteditor as dynamic;
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
        if (!string.IsNullOrWhiteSpace(m_texteditor.Text))
        {
          var result = MessageBox.Show("Open the sample will remove all changes.",
              "Sample opening", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

          if (result != System.Windows.Forms.DialogResult.OK)
            return;
        }

        m_texteditor.Text = sample;
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
        helpForm.Show(Grasshopper.Instances.DocumentEditor);
      }
      catch (Exception ex)
      {
        LastHandleException(ex);
      }
    }
  }
}