using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using GhPython.Forms;

namespace GhPython.Component
{
    class PythonComponentAttributes : GH_ComponentAttributes
    {
        PythonScriptForm _form;

        public PythonComponentAttributes(SafeComponent r)
        : base(r)
        {
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            var attachedComp = this.Owner as PythonComponent;
            if (attachedComp != null)
            {
                attachedComp.CheckAndSetupActions();

                if (_form == null || _form.IsDisposed)
                {
                    _form = new PythonScriptForm(attachedComp);
                }
                if (!_form.Visible)
                    _form.Show(Grasshopper.GH_InstanceServer.DocumentEditor);
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }

        public void ClearLinkedForm(bool close)
        {
            if (close && _form != null && !_form.IsDisposed)
                _form.CloseWithoutUserCancel();

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
