using System.Runtime.InteropServices;

namespace KtMControl;

public partial class MouseControlForm : Form
{
    private readonly GuidanceOverlayForm guidanceOverlayForm = new();
    private readonly System.Windows.Forms.Timer ctrlStateTimer = new();
    private readonly NotifyIcon trayIcon = new();
    private readonly ContextMenuStrip trayMenu = new();
    private readonly ToolStripMenuItem exitMenuItem = new("Exit");

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
    private const int HotkeyIdNumpadMultiply = 110;
    private const int HotkeyIdNumpadDivide = 111;

    private const uint VkNumpad1 = 0x61;
    private const uint VkNumpad2 = 0x62;
    private const uint VkNumpad3 = 0x63;
    private const uint VkNumpad4 = 0x64;
    private const uint VkNumpad5 = 0x65;
    private const uint VkNumpad6 = 0x66;
    private const uint VkNumpad7 = 0x67;
    private const uint VkNumpad8 = 0x68;
    private const uint VkNumpad9 = 0x69;
    private const uint VkMultiply = 0x6A;
    private const uint VkDivide = 0x6F;

    private const ushort VkLControl = 0xA2;
    private const ushort VkRControl = 0xA3;
    private const int VkControl = 0x11;

    private const uint InputMouse = 0;
    private const uint InputKeyboard = 1;
    private const uint KeyeventfKeyup = 0x0002;
    private const uint MouseeventfLeftdown = 0x0002;
    private const uint MouseeventfLeftup = 0x0004;
    private const uint MouseeventfRightdown = 0x0008;
    private const uint MouseeventfRightup = 0x0010;

    private Rectangle activeArea = Rectangle.Empty;
    private Screen? activeScreen;
    private bool hideOnFirstShown = true;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public MouseControlForm()
    {
        InitializeComponent();
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        InitializeCtrlStateTimer();
        InitializeTrayIcon();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (hideOnFirstShown)
        {
            hideOnFirstShown = false;
            Hide();
        }
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
        RegisterNumpadHotkey(HotkeyIdNumpadMultiply, VkMultiply);
        RegisterNumpadHotkey(HotkeyIdNumpadDivide, VkDivide);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        ctrlStateTimer.Stop();

        UnregisterHotKey(Handle, HotkeyIdNumpad1);
        UnregisterHotKey(Handle, HotkeyIdNumpad2);
        UnregisterHotKey(Handle, HotkeyIdNumpad3);
        UnregisterHotKey(Handle, HotkeyIdNumpad4);
        UnregisterHotKey(Handle, HotkeyIdNumpad5);
        UnregisterHotKey(Handle, HotkeyIdNumpad6);
        UnregisterHotKey(Handle, HotkeyIdNumpad7);
        UnregisterHotKey(Handle, HotkeyIdNumpad8);
        UnregisterHotKey(Handle, HotkeyIdNumpad9);
        UnregisterHotKey(Handle, HotkeyIdNumpadMultiply);
        UnregisterHotKey(Handle, HotkeyIdNumpadDivide);

        trayIcon.Visible = false;
        exitMenuItem.Click -= ExitMenuItem_Click;
        trayIcon.Dispose();
        trayMenu.Dispose();
        ctrlStateTimer.Dispose();
        guidanceOverlayForm.Dispose();

        base.OnFormClosed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey)
        {
            var hotkeyId = (int)m.WParam;

            if (hotkeyId == HotkeyIdNumpadDivide)
            {
                PerformLeftClick();
                return;
            }

            if (hotkeyId == HotkeyIdNumpadMultiply)
            {
                PerformRightClick();
                return;
            }

            var keypadNumber = HotkeyIdToKeypadNumber(hotkeyId);
            if (keypadNumber is >= 1 and <= 9)
            {
                HandleNavigation(keypadNumber.Value);
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void InitializeCtrlStateTimer()
    {
        ctrlStateTimer.Interval = 25;
        ctrlStateTimer.Tick += CtrlStateTimer_Tick;
    }

    private void InitializeTrayIcon()
    {
        exitMenuItem.Click += ExitMenuItem_Click;
        trayMenu.Items.Add(exitMenuItem);

        trayIcon.Text = "KtMControl";
        trayIcon.Icon = Icon ?? SystemIcons.Application;
        trayIcon.ContextMenuStrip = trayMenu;
        trayIcon.Visible = true;
    }

    private void CtrlStateTimer_Tick(object? sender, EventArgs e)
    {
        if (!IsCtrlCurrentlyDown())
        {
            ResetImmediatelyToInitialState();
        }
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void RegisterNumpadHotkey(int hotkeyId, uint virtualKey)
    {
        var registered = RegisterHotKey(Handle, hotkeyId, ModControl, virtualKey);
        if (!registered)
        {
            var hotkeyName = hotkeyId switch
            {
                HotkeyIdNumpadMultiply => "Ctrl + NumPad *",
                HotkeyIdNumpadDivide => "Ctrl + NumPad /",
                _ => $"Ctrl + NumPad{hotkeyId - 100}"
            };

            MessageBox.Show(
                $"Could not register {hotkeyName}. It may already be in use.",
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

        guidanceOverlayForm.ShowGuidance(activeScreen, activeArea);
        StartCtrlTracking();
    }

    private void PerformLeftClick()
    {
        SendMouseClick(MouseeventfLeftdown, MouseeventfLeftup);
        ResetImmediatelyToInitialState();
    }

    private void PerformRightClick()
    {
        SendMouseClick(MouseeventfRightdown, MouseeventfRightup);
        ResetImmediatelyToInitialState();
    }

    private static void SendMouseClick(uint mouseDownFlag, uint mouseUpFlag)
    {
        var leftCtrlWasDown = IsVirtualKeyDown(VkLControl);
        var rightCtrlWasDown = IsVirtualKeyDown(VkRControl);

        var inputs = new List<INPUT>();

        if (leftCtrlWasDown)
        {
            inputs.Add(CreateKeyboardInput(VkLControl, KeyeventfKeyup));
        }

        if (rightCtrlWasDown)
        {
            inputs.Add(CreateKeyboardInput(VkRControl, KeyeventfKeyup));
        }

        inputs.Add(CreateMouseInput(mouseDownFlag));
        inputs.Add(CreateMouseInput(mouseUpFlag));

        if (leftCtrlWasDown)
        {
            inputs.Add(CreateKeyboardInput(VkLControl, 0));
        }

        if (rightCtrlWasDown)
        {
            inputs.Add(CreateKeyboardInput(VkRControl, 0));
        }

        var sent = SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());
        if (sent != inputs.Count)
        {
            throw new InvalidOperationException("Failed to send the simulated mouse click.");
        }
    }

    private static bool IsVirtualKeyDown(int virtualKey)
    {
        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    private static INPUT CreateKeyboardInput(ushort virtualKey, uint flags)
    {
        return new INPUT
        {
            type = InputKeyboard,
            Anonymous = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = nuint.Zero
                }
            }
        };
    }

    private static INPUT CreateMouseInput(uint flags)
    {
        return new INPUT
        {
            type = InputMouse,
            Anonymous = new INPUTUNION
            {
                mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = nuint.Zero
                }
            }
        };
    }

    private void StartCtrlTracking()
    {
        if (!ctrlStateTimer.Enabled)
        {
            ctrlStateTimer.Start();
        }
    }

    private void StopCtrlTracking()
    {
        if (ctrlStateTimer.Enabled)
        {
            ctrlStateTimer.Stop();
        }
    }

    private void ResetImmediatelyToInitialState()
    {
        StopCtrlTracking();
        guidanceOverlayForm.HideGuidance();
        ResetToInitialState();
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

    private static bool IsCtrlCurrentlyDown()
    {
        return IsVirtualKeyDown(VkControl);
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

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION Anonymous;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }
}