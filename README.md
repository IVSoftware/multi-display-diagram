## Multi-Display diagram

This could be simplified to not use P/Invoke perhaps. One solution you could experiment with is to create a rectangle representing the max bounds of the multi-screen working area, and scale it to fit within a cell of a `TableLayoutPanel`. By setting it to `Anchor.None` it will be automatically centered in the single cell of the TLP at which point buttons representing the individual screens can be added.

[![screen selector splash][1]][1]

When one of the buttons is clicked, copy the location and size of the display that has been clicked to the Main Form.

___
```csharp

public partial class DisplaySelectorForm : Form
{
    public DisplaySelectorForm() => InitializeComponent();

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var screens = Screen.AllScreens;

        var minLeft = screens.Min(screen => screen.WorkingArea.Left);
        var minTop = screens.Min(screen => screen.WorkingArea.Top);
        var maxRight = screens.Max(screen => screen.WorkingArea.Right);
        var maxBottom = screens.Max(screen => screen.WorkingArea.Bottom);
        var workspace = new Rectangle
        {
            Width = maxRight - minLeft,
            Height = maxBottom - minTop,
        };
        var workspacePanel =new Panel
        {
            Size = workspace.Size,
            Anchor = AnchorStyles.None,
            BackColor = Color.White,
            Dock = DockStyle.None,
        };

        var scaleX = tableLayoutPanel.Width / (double)workspacePanel.Width;
        var scaleY = tableLayoutPanel.Height / (double)workspacePanel.Height;
        var scale = Math.Min(scaleX, scaleY); 

        workspacePanel.Size = new Size((int)(workspace.Width * scale), (int)(workspace.Height * scale));

        tableLayoutPanel.Controls.Add(workspacePanel, 0, 0);

        foreach (var screen in screens)
        {
            var screenLeft = screen.WorkingArea.Left - minLeft;
            var screenTop = screen.WorkingArea.Top - minTop;
            var screenScaledLeft = (int)(screenLeft * scale);
            var screenScaledTop = (int)(screenTop * scale);
            var screenScaledWidth = (int)(screen.WorkingArea.Width * scale);
            var screenScaledHeight = (int)(screen.WorkingArea.Height * scale);
            var screenButton = new Button
            {
                Text = screen.DeviceName,
                Size = new Size(screenScaledWidth, screenScaledHeight),
                Location = new Point(screenScaledLeft, screenScaledTop),
                BackColor = screen.Primary ? Color.CornflowerBlue : Color.LightGray,
                Tag = screen,
            };
            screenButton.Click += (sender, e) =>
            {
                if(
                    sender is Control control
                    &&
                    control.Tag is Screen selectedScreen
                    &&
                    Owner is MainForm mainForm)
                {
                    mainForm.Location = selectedScreen.WorkingArea.Location;
                    mainForm.Size = selectedScreen.WorkingArea.Size;
                    DialogResult = DialogResult.OK;
                }
            };
            workspacePanel.Controls.Add(screenButton);
        }
    }
}
```
___

The main form code uses a technique to show the `DisplaySelectorForm` as a splash screen as described in this [answer](https://stackoverflow.com/a/75534137/5438626).

```csharp
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        _ = Handle;
        BeginInvoke(new Action(() => execDisplaySelectionFlow()));
    }
    protected override void SetVisibleCore(bool value) =>
        base.SetVisibleCore(value && _initialized);

    private void execDisplaySelectionFlow()
    {
        using (var selector = new DisplaySelectorForm())
        {
            selector.ShowDialog(this);
        }
        _initialized = true;
        WindowState = FormWindowState.Maximized;
        Show();
    }
    bool _initialized = false;
}
```



  [1]: https://i.stack.imgur.com/3zoN4.png