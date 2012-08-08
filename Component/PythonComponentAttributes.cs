using GhPython.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

namespace GhPython.Component
{
  class PythonComponentAttributes : GH_ComponentAttributes
  {
    private PythonScriptForm m_form;

    public PythonComponentAttributes(SafeComponent safeComponent)
      : base(safeComponent)
    {
    }

    public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
      OpenEditor();
      return base.RespondToMouseDoubleClick(sender, e);
    }

    public void OpenEditor()
    {
      var attachedComp = this.Owner as ScriptingAncestorComponent;
      if (attachedComp != null && !attachedComp.Locked)
      {
        attachedComp.CheckAndSetupActions();

        if (m_form == null || m_form.IsDisposed)
          m_form = new PythonScriptForm(attachedComp);

        if (!m_form.Visible)
          m_form.Show(Grasshopper.Instances.DocumentEditor);
      }
    }

    public void DisableLinkedForm(bool close)
    {
      if (close && m_form != null && !m_form.IsDisposed)
        m_form.Disable();

      m_form = null;
    }

    public bool TrySetLinkedFormHelpText(string text)
    {
      if (m_form != null && !m_form.IsDisposed)
      {
        m_form.HelpText(text);
        return true;
      }
      return false;
    }
  }
}