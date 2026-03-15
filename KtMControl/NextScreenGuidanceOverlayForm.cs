namespace KtMControl;

using System.Drawing.Drawing2D;

internal sealed class NextScreenGuidanceOverlayForm : Form
{

    public NextScreenGuidanceOverlayForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        DoubleBuffered = true;
    }

    public void ShowGuidance(Screen nextScreen)
    {
        Bounds = nextScreen.Bounds;
        
        if (!Visible)
        {
            Show();
        }

        BringToFront();
        Invalidate();
    }

    public void HideGuidance()
    {
        Hide();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int WsExToolWindow = 0x00000080;
            const int WsExNoActivate = 0x08000000;

            var cp = base.CreateParams;
            cp.ExStyle |= WsExToolWindow | WsExNoActivate;
            return cp;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        var zeroFontSize = Bounds.Height * 0.5f / 3;
        DrawOutlinedNumber(
            e.Graphics,
            "0",
            new RectangleF(0, 0, Bounds.Width, Bounds.Height),
            zeroFontSize);
    }

    private static void DrawOutlinedNumber(Graphics graphics, string text, RectangleF bounds, float fontSize)
    {
        using var path = new GraphicsPath();
        using var format = new StringFormat();
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        using var outlinePen = new Pen(Color.Red, GuidanceOverlayForm.GetLineWidth(fontSize));
        outlinePen.LineJoin = LineJoin.Round;

        path.AddString(
            text,
            FontFamily.GenericSansSerif,
            (int)FontStyle.Bold,
            fontSize,
            bounds,
            format);

        graphics.DrawPath(outlinePen, path);
    }
}