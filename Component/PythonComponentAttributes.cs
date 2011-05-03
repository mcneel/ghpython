using GhPython.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

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
            OpenEditor();
            return base.RespondToMouseDoubleClick(sender, e);
        }

        public void OpenEditor()
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
