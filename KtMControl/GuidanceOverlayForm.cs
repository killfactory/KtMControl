namespace KtMControl;

using System.Drawing.Drawing2D;
using System.Drawing.Text;

internal sealed class GuidanceOverlayForm : Form
{
    private Rectangle activeArea = Rectangle.Empty;

    public bool IsGuidanceVisible => Visible;

    public GuidanceOverlayForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        DoubleBuffered = true;
    }

    public void ShowGuidance(Screen screen, Rectangle area)
    {
        Bounds = screen.Bounds;
        activeArea = area;

        if (!Visible)
        {
            Show();
        }

        BringToFront();
        Invalidate();
    }

    public void HideGuidance()
    {
        activeArea = Rectangle.Empty;
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

        if (activeArea == Rectangle.Empty)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var localArea = new Rectangle(
            activeArea.Left - Bounds.Left,
            activeArea.Top - Bounds.Top,
            activeArea.Width,
            activeArea.Height);

        var x1 = localArea.Left + localArea.Width / 3;
        var x2 = localArea.Left + (2 * localArea.Width) / 3;
        var y1 = localArea.Top + localArea.Height / 3;
        var y2 = localArea.Top + (2 * localArea.Height) / 3;

        using var pen = new Pen(Color.Red, 2);

        e.Graphics.DrawLine(pen, x1, localArea.Top, x1, localArea.Bottom);
        e.Graphics.DrawLine(pen, x2, localArea.Top, x2, localArea.Bottom);
        e.Graphics.DrawLine(pen, localArea.Left, y1, localArea.Right, y1);
        e.Graphics.DrawLine(pen, localArea.Left, y2, localArea.Right, y2);

        var cellWidth = localArea.Width / 3f;
        var cellHeight = localArea.Height / 3f;
        var guideFontSize = cellHeight * 0.5f;
        if (guideFontSize > 8) {
            DrawOutlinedNumber(e.Graphics, "7", new RectangleF(localArea.Left + (0 * cellWidth), localArea.Top + (0 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "8", new RectangleF(localArea.Left + (1 * cellWidth), localArea.Top + (0 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "9", new RectangleF(localArea.Left + (2 * cellWidth), localArea.Top + (0 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "4", new RectangleF(localArea.Left + (0 * cellWidth), localArea.Top + (1 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "5", new RectangleF(localArea.Left + (1 * cellWidth), localArea.Top + (1 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "6", new RectangleF(localArea.Left + (2 * cellWidth), localArea.Top + (1 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "1", new RectangleF(localArea.Left + (0 * cellWidth), localArea.Top + (2 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "2", new RectangleF(localArea.Left + (1 * cellWidth), localArea.Top + (2 * cellHeight), cellWidth, cellHeight), guideFontSize);
            DrawOutlinedNumber(e.Graphics, "3", new RectangleF(localArea.Left + (2 * cellWidth), localArea.Top + (2 * cellHeight), cellWidth, cellHeight), guideFontSize);
        }
        
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
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        using var outlinePen = new Pen(Color.Red, 1)
        {
            LineJoin = LineJoin.Round
        };

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