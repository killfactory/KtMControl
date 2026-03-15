namespace KtMControl;

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
    }
}