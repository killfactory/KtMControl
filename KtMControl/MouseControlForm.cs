using System.Runtime.InteropServices;

namespace KtMControl;

public partial class MouseControlForm : Form
{
    private readonly Button moveMouseButton = new();

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
        base.OnFormClosed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam == HotkeyId)
        {
            MoveMouseToFirstScreen50_50();
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
        MoveMouseToFirstScreen50_50();
    }

    private static void MoveMouseToFirstScreen50_50()
    {
        var firstScreen = Screen.AllScreens[0];
        Cursor.Position = new Point(firstScreen.Bounds.Left + 50, firstScreen.Bounds.Top + 50);
    }
}