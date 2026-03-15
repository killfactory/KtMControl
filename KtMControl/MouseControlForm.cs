namespace KtMControl;

public partial class MouseControlForm : Form
{
    private readonly Button moveMouseButton = new();

    public MouseControlForm()
    {
        InitializeComponent();
        InitializeMoveMouseButton();
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
        var firstScreen = Screen.AllScreens[0];
        Cursor.Position = new Point(firstScreen.Bounds.Left + 50, firstScreen.Bounds.Top + 50);
    }
}