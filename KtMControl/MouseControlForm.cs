using System.Runtime.InteropServices;

namespace KtMControl;

public partial class MouseControlForm : Form
{
    private readonly Button moveMouseButton = new();
    private readonly ScreenOverlayForm screenOverlayForm = new();

    private const int HotkeyId = 1;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModAlt = 0x0001;
    private const uint VkNumpad5 = 0x65;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    public MouseControlForm()
    {
        InitializeComponent();
        InitializeMoveMouseButton();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        var registered = RegisterHotKey(Handle, HotkeyId, ModControl | ModAlt, VkNumpad5);
        if (!registered)
        {
            MessageBox.Show(
                "Could not register Ctrl + Alt + NumPad5. It may already be in use.",
                "Hotkey Registration Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        UnregisterHotKey(Handle, HotkeyId);
        screenOverlayForm.Dispose();
        base.OnFormClosed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam == HotkeyId)
        {
            MoveMouseAndShowRectangle();
            return;
        }

        base.WndProc(ref m);
    }

    private void InitializeMoveMouseButton()
    {
        moveMouseButton.Text = "Move mouse to (50, 50)";
        moveMouseButton.AutoSize = true;
        moveMouseButton.Location = new Point(20, 20);
        moveMouseButton.Click += MoveMouseButton_Click;

        Controls.Add(moveMouseButton);
    }

    private void MoveMouseButton_Click(object? sender, EventArgs e)
    {
        MoveMouseAndShowRectangle();
    }

    private void MoveMouseAndShowRectangle()
    {
        var firstScreen = Screen.AllScreens[0];
        Cursor.Position = new Point(firstScreen.Bounds.Left + 50, firstScreen.Bounds.Top + 50);
        screenOverlayForm.ShowRectangleOnScreen(firstScreen);
    }
}

internal sealed class ScreenOverlayForm : Form
{
    private readonly System.Windows.Forms.Timer hideTimer = new();
    private Rectangle rectangleToDraw = Rectangle.Empty;

    public ScreenOverlayForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;

        hideTimer.Interval = 1000;
        hideTimer.Tick += HideTimer_Tick;
    }

    public void ShowRectangleOnScreen(Screen screen)
    {
        Bounds = screen.Bounds;
        rectangleToDraw = new Rectangle(2, 2, 96, 96);

        if (!Visible)
        {
            Show();
        }

        BringToFront();
        Invalidate();

        hideTimer.Stop();
        hideTimer.Start();
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

        if (rectangleToDraw == Rectangle.Empty)
        {
            return;
        }

        using var pen = new Pen(Color.Red, 3);
        e.Graphics.DrawRectangle(pen, rectangleToDraw);
    }

    private void HideTimer_Tick(object? sender, EventArgs e)
    {
        hideTimer.Stop();
        rectangleToDraw = Rectangle.Empty;
        Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            hideTimer.Tick -= HideTimer_Tick;
            hideTimer.Dispose();
        }

        base.Dispose(disposing);
    }
}