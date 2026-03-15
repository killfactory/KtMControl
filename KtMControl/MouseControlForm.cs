using System.Runtime.InteropServices;

namespace KtMControl;

public partial class MouseControlForm : Form
{
    private readonly Button moveMouseButton = new();
    private readonly GuidanceOverlayForm guidanceOverlayForm = new();

    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;

    private const int HotkeyIdNumpad1 = 101;
    private const int HotkeyIdNumpad2 = 102;
    private const int HotkeyIdNumpad3 = 103;
    private const int HotkeyIdNumpad4 = 104;
    private const int HotkeyIdNumpad5 = 105;
    private const int HotkeyIdNumpad6 = 106;
    private const int HotkeyIdNumpad7 = 107;
    private const int HotkeyIdNumpad8 = 108;
    private const int HotkeyIdNumpad9 = 109;

    private const uint VkNumpad1 = 0x61;
    private const uint VkNumpad2 = 0x62;
    private const uint VkNumpad3 = 0x63;
    private const uint VkNumpad4 = 0x64;
    private const uint VkNumpad5 = 0x65;
    private const uint VkNumpad6 = 0x66;
    private const uint VkNumpad7 = 0x67;
    private const uint VkNumpad8 = 0x68;
    private const uint VkNumpad9 = 0x69;

    private Rectangle activeArea = Rectangle.Empty;
    private Screen? activeScreen;

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

        RegisterNumpadHotkey(HotkeyIdNumpad1, VkNumpad1);
        RegisterNumpadHotkey(HotkeyIdNumpad2, VkNumpad2);
        RegisterNumpadHotkey(HotkeyIdNumpad3, VkNumpad3);
        RegisterNumpadHotkey(HotkeyIdNumpad4, VkNumpad4);
        RegisterNumpadHotkey(HotkeyIdNumpad5, VkNumpad5);
        RegisterNumpadHotkey(HotkeyIdNumpad6, VkNumpad6);
        RegisterNumpadHotkey(HotkeyIdNumpad7, VkNumpad7);
        RegisterNumpadHotkey(HotkeyIdNumpad8, VkNumpad8);
        RegisterNumpadHotkey(HotkeyIdNumpad9, VkNumpad9);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        UnregisterHotKey(Handle, HotkeyIdNumpad1);
        UnregisterHotKey(Handle, HotkeyIdNumpad2);
        UnregisterHotKey(Handle, HotkeyIdNumpad3);
        UnregisterHotKey(Handle, HotkeyIdNumpad4);
        UnregisterHotKey(Handle, HotkeyIdNumpad5);
        UnregisterHotKey(Handle, HotkeyIdNumpad6);
        UnregisterHotKey(Handle, HotkeyIdNumpad7);
        UnregisterHotKey(Handle, HotkeyIdNumpad8);
        UnregisterHotKey(Handle, HotkeyIdNumpad9);

        guidanceOverlayForm.Dispose();
        base.OnFormClosed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey)
        {
            var keypadNumber = HotkeyIdToKeypadNumber((int)m.WParam);
            if (keypadNumber is >= 1 and <= 9)
            {
                HandleNavigation(keypadNumber.Value);
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void InitializeMoveMouseButton()
    {
        moveMouseButton.Text = "Start drill-down (NumPad 5)";
        moveMouseButton.AutoSize = true;
        moveMouseButton.Location = new Point(20, 20);
        moveMouseButton.Click += MoveMouseButton_Click;

        Controls.Add(moveMouseButton);
    }

    private void MoveMouseButton_Click(object? sender, EventArgs e)
    {
        HandleNavigation(5);
    }

    private void RegisterNumpadHotkey(int hotkeyId, uint virtualKey)
    {
        var registered = RegisterHotKey(Handle, hotkeyId, ModControl, virtualKey);
        if (!registered)
        {
            MessageBox.Show(
                $"Could not register Ctrl + NumPad{hotkeyId - 100}. It may already be in use.",
                "Hotkey Registration Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void HandleNavigation(int keypadNumber)
    {
        EnsureActiveArea();

        var nextArea = GetSubRectangle(activeArea, keypadNumber);
        var centerPoint = GetRectangleCenter(nextArea);

        Cursor.Position = centerPoint;

        activeArea = nextArea;
        activeScreen = Screen.FromPoint(centerPoint);

        guidanceOverlayForm.ShowGuidance(activeScreen, activeArea, ResetToInitialState);
    }

    private void EnsureActiveArea()
    {
        if (guidanceOverlayForm.IsGuidanceVisible && activeScreen is not null && activeArea != Rectangle.Empty)
        {
            return;
        }

        ResetToInitialState();
    }

    private void ResetToInitialState()
    {
        var mousePosition = Cursor.Position;
        activeScreen = Screen.FromPoint(mousePosition);
        activeArea = activeScreen.Bounds;
    }

    private static Point GetRectangleCenter(Rectangle rectangle)
    {
        return new Point(
            rectangle.Left + rectangle.Width / 2,
            rectangle.Top + rectangle.Height / 2);
    }

    private static int? HotkeyIdToKeypadNumber(int hotkeyId)
    {
        return hotkeyId switch
        {
            HotkeyIdNumpad1 => 1,
            HotkeyIdNumpad2 => 2,
            HotkeyIdNumpad3 => 3,
            HotkeyIdNumpad4 => 4,
            HotkeyIdNumpad5 => 5,
            HotkeyIdNumpad6 => 6,
            HotkeyIdNumpad7 => 7,
            HotkeyIdNumpad8 => 8,
            HotkeyIdNumpad9 => 9,
            _ => null
        };
    }

    private static Rectangle GetSubRectangle(Rectangle area, int keypadNumber)
    {
        var left = area.Left;
        var top = area.Top;
        var width = area.Width;
        var height = area.Height;

        var x1 = left + width / 3;
        var x2 = left + (2 * width) / 3;
        var y1 = top + height / 3;
        var y2 = top + (2 * height) / 3;

        return keypadNumber switch
        {
            7 => Rectangle.FromLTRB(left, top, x1, y1),
            8 => Rectangle.FromLTRB(x1, top, x2, y1),
            9 => Rectangle.FromLTRB(x2, top, left + width, y1),
            4 => Rectangle.FromLTRB(left, y1, x1, y2),
            5 => Rectangle.FromLTRB(x1, y1, x2, y2),
            6 => Rectangle.FromLTRB(x2, y1, left + width, y2),
            1 => Rectangle.FromLTRB(left, y2, x1, top + height),
            2 => Rectangle.FromLTRB(x1, y2, x2, top + height),
            3 => Rectangle.FromLTRB(x2, y2, left + width, top + height),
            _ => throw new ArgumentOutOfRangeException(nameof(keypadNumber))
        };
    }
}

internal sealed class GuidanceOverlayForm : Form
{
    private readonly System.Windows.Forms.Timer hideTimer = new();
    private Rectangle activeArea = Rectangle.Empty;
    private Action? onGuidanceHidden;

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

        hideTimer.Interval = 1000;
        hideTimer.Tick += HideTimer_Tick;
    }

    public void ShowGuidance(Screen screen, Rectangle area, Action onHidden)
    {
        Bounds = screen.Bounds;
        activeArea = area;
        onGuidanceHidden = onHidden;

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

    private void HideTimer_Tick(object? sender, EventArgs e)
    {
        hideTimer.Stop();
        activeArea = Rectangle.Empty;
        Hide();
        onGuidanceHidden?.Invoke();
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