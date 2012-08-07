using GhPython.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using System;

namespace GhPython.Component
{
  internal class PythonComponentAttributes : GH_ComponentAttributes
  {
    private PythonScriptForm _form;
    private readonly SafeComponent _safeComponent;

    public PythonComponentAttributes(SafeComponent safeComponent)
      : base(safeComponent)
    {
      if (safeComponent == null)
        throw new ArgumentNullException("safeComponent");
      _safeComponent = safeComponent;
    }

    public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
      OpenEditor();

      return base.RespondToMouseDoubleClick(sender, e);
    }

    public void OpenEditor()
    {
      if (!_safeComponent.Locked)
      {
        var attachedComp = this.Owner as ScriptingAncestorComponent;
        if (attachedComp != null)
        {
          attachedComp.CheckAndSetupActions();

          if (_form == null || _form.IsDisposed)
          {
            _form = new PythonScriptForm(attachedComp);
          }
          if (!_form.Visible)
            _form.Show(Grasshopper.Instances.DocumentEditor);
        }
      }
    }

    public void DisableLinkedForm(bool close)
    {
      if (close && _form != null && !_form.IsDisposed)
        _form.Disable();

      _form = null;
    }

    public bool TrySetLinkedFormHelpText(string text)
    {
      if (_form != null && !_form.IsDisposed)
      {
        _form.HelpText(text);
        return true;
      }
      return false;
    }
  }
}