using GhPython.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GhPython.Component
{
  public class PythonComponentAttributes : GH_ComponentAttributes
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
        attachedComp.CheckIfSetupActionsAreNecessary();

        if (m_form == null || m_form.IsDisposed)
          m_form = new PythonScriptForm(attachedComp);

        if (!m_form.Visible)
        {
          m_form.Show(Grasshopper.Instances.DocumentEditor);
          attachedComp.OnDisplayExpired(true);
        }
        else
        {
          m_form.Focus();
        }
      }
    }

    public bool TryGetEditor(out Form editor)
    {
      if (m_form == null || m_form.IsDisposed)
      {
        editor = null;
        return false;
      }

      editor = m_form;
      return true;
    }

    internal void DisableLinkedEditor(bool close)
    {
      if (close && m_form != null && !m_form.IsDisposed)
        m_form.Disable();

      m_form = null;
    }

    public bool TrySetLinkedEditorHelpText(string text)
    {
      if (m_form != null && !m_form.IsDisposed)
      {
        m_form.HelpText(text);
        return true;
      }
      return false;
    }

    protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
    {
      base.Render(canvas, graphics, channel);

      if (m_form == null || m_form.IsDisposed) return;

      if (channel == GH_CanvasChannel.Overlay &&
          (canvas.DrawingMode == GH_CanvasMode.Export ||
          canvas.DrawingMode == GH_CanvasMode.Control)
        )
      {
        Rectangle targetRectangle;
        if (canvas.DrawingMode == GH_CanvasMode.Export)
        {
          System.Windows.Forms.Control editorControl = m_form.m_texteditor;
          if (editorControl == null) return;
          targetRectangle = editorControl.ClientRectangle;
          targetRectangle = m_form.RectangleToScreen(targetRectangle);
        }
        else
        {
          targetRectangle = m_form.DesktopBounds;
        }

        RectangleF windowOnCanvas;
        {
          targetRectangle.Inflate(-1, -1);

          var desktopForm = canvas.RectangleToClient(targetRectangle);
          windowOnCanvas = canvas.Viewport.UnprojectRectangle(desktopForm);
        }

        var transparent = Color.Transparent;

        var desk_tl = new PointF(windowOnCanvas.Left, windowOnCanvas.Top);
        var desk_tr = new PointF(windowOnCanvas.Right, windowOnCanvas.Top);
        var desk_bl = new PointF(windowOnCanvas.Left, windowOnCanvas.Bottom);
        var desk_br = new PointF(windowOnCanvas.Right, windowOnCanvas.Bottom);

        var comp_tl = new PointF(Bounds.Left, Bounds.Top);
        var comp_tr = new PointF(Bounds.Right, Bounds.Top);
        var comp_bl = new PointF(Bounds.Left, Bounds.Bottom);
        var comp_br = new PointF(Bounds.Right, Bounds.Bottom);

        if (Bounds.Top < windowOnCanvas.Top)
          BoxSide(graphics, Color.FromArgb(155,255,255,255), transparent, desk_tl, desk_tr, comp_tl, comp_tr);
        if (Bounds.Right > windowOnCanvas.Right)
          BoxSide(graphics, Color.FromArgb(155, 240, 240, 240), transparent, desk_tr, desk_br, comp_tr, comp_br);
        if (Bounds.Bottom > windowOnCanvas.Bottom)
          BoxSide(graphics, Color.FromArgb(155, 120, 120, 120), transparent, desk_bl, desk_br, comp_bl, comp_br);
        if (Bounds.Left < windowOnCanvas.Left)
          BoxSide(graphics, Color.FromArgb(155, 240, 240, 240), transparent, desk_tl, desk_bl, comp_tl, comp_bl);

        BoxEdge(graphics, Color.Black, Color.Transparent, 1, desk_tl, comp_tl, AnchorStyles.Top | AnchorStyles.Left);
        BoxEdge(graphics, Color.Black, Color.Transparent, 1, desk_tr, comp_tr, AnchorStyles.Top | AnchorStyles.Right);
        BoxEdge(graphics, Color.Black, Color.Transparent, 1, desk_br, comp_br, AnchorStyles.Bottom | AnchorStyles.Right);
        BoxEdge(graphics, Color.Black, Color.Transparent, 1, desk_bl, comp_bl, AnchorStyles.Bottom | AnchorStyles.Left);

        if (canvas.DrawingMode == GH_CanvasMode.Export)
        {
          System.Windows.Forms.Control editorControl = m_form.m_texteditor;
          if (editorControl == null) return;

          using (var bitmap = new Bitmap(editorControl.Width, editorControl.Height))
          {
            editorControl.DrawToBitmap(bitmap, editorControl.Bounds);

            var ot = graphics.Transform;
            var loc = canvas.Viewport.ProjectPoint(windowOnCanvas.Location);

            graphics.ResetTransform();

            graphics.DrawImage(bitmap,
              (int)loc.X, (int)loc.Y,
              editorControl.Width, editorControl.Height);

            graphics.Transform = ot;
          }
        }
      }
    }

    private static void BoxSide(Graphics graphics, Color from, Color to,
      PointF A, PointF B, PointF C, PointF D)
    {
      using (var gradientA = new LinearGradientBrush(
          new PointF(A.X != B.X ? 0 : (A.X + B.X) * 0.5f, A.X == B.X ? 0 : (A.Y + B.Y) * 0.5f),
          new PointF(A.X != B.X ? 0 : (C.X + D.X) * 0.5f, A.X == B.X ? 0 : (C.Y + D.Y) * 0.5f),
          from, to))
      using (var path = new GraphicsPath())
      {
        gradientA.WrapMode = WrapMode.TileFlipXY;
        path.AddLine(A, B);
        path.AddLine(B, D);
        path.AddLine(D, C);
        path.CloseFigure();
        
        graphics.FillPath(gradientA, path);
      }
    }

    private static void BoxEdge(Graphics graphics, Color from, Color to,
      float size, PointF A, PointF B, AnchorStyles side)
    {
      using (var gradientA = new LinearGradientBrush(A, B, from, to))
      using (var penA = new Pen(gradientA, size))
      {
        bool visible = IsVisibleExtrusionEdge(B, A, side);
        if (!visible)
        {
          penA.DashStyle = DashStyle.Dash;
          penA.DashPattern = new float[] { 5, 5 };
          penA.DashCap = DashCap.Triangle;
        }
        graphics.DrawLine(penA, A, B);
      }
    }

    private static bool IsVisibleExtrusionEdge(PointF back, PointF front, AnchorStyles sides)
    {
      bool toReturn = false;

      if ((sides & AnchorStyles.Top) == AnchorStyles.Top)
        toReturn |= back.Y < front.Y;
      else if ((sides & AnchorStyles.Bottom) == AnchorStyles.Bottom)
        toReturn |= back.Y > front.Y;

      if ((sides & AnchorStyles.Right) == AnchorStyles.Right)
        toReturn |= back.X > front.X;
      else if ((sides & AnchorStyles.Left) == AnchorStyles.Left)
        toReturn |= back.X < front.X;

      return toReturn;
    }
  }
}