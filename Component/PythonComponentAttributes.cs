using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using GhPython.Forms;

namespace GhPython.Component
{
    class PythonComponentAttributes : GH_ComponentAttributes
    {
        public PythonComponentAttributes(SafeComponent r) : base(r)
        {
            _attachedComp = r;
        }

        PythonScriptForm m_form;

        SafeComponent _attachedComp;

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            _attachedComp.CheckAndSetupActions();

            if (m_form == null || m_form.IsDisposed)
            {
                m_form = new PythonScriptForm();
                m_form.LinkedComponent = (PythonComponent)this.Owner;
                m_form.TopLevel = true;
            }

            if (!m_form.Visible)
                m_form.Show(Rhino.RhinoApp.MainWindow());

            m_form.TopMost = true;

            return base.RespondToMouseDoubleClick(sender, e);
        }

        public void ClearLinkedForm()
        {
            m_form = null;
        }

        public void CloseLinkedForm()
        {
            if (m_form != null && !m_form.IsDisposed)
            {
                m_form.TopMost = false;
                m_form.CloseWithoutUserCancel();
                ClearLinkedForm();
            }
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
