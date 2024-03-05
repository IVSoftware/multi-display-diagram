using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace multi_display_diagram
{
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
            var scaleX = tableLayoutPanel.Width / (double)workspace.Width;
            var scaleY = tableLayoutPanel.Height / (double)workspace.Height;
            var scale = Math.Min(scaleX, scaleY); 
            workspace.Size = new Size((int)(workspace.Width * scale), (int)(workspace.Height * scale));

            var workspacePanel = new Panel
            {
                Size = workspace.Size,
                BackColor = Color.White,
                Anchor = AnchorStyles.None,
            };

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
                    Text = $"Display {screens.ToList().IndexOf(screen) + 1}\n({screen.DeviceName})",
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
            var buttonArray = workspacePanel
                .Controls.OfType<Button>()
                .ToArray();
            var bottomArray =
                buttonArray
                .OrderBy(_ => _.Top).ThenBy(_ => _.Left) 
                .ToArray();
            for (int i = 1; i < bottomArray.Length; i++)
            {
                var gapY = bottomArray[i].Top - bottomArray[i - 1].Bottom;
                if (gapY > 0)
                {
                    for (int j = i; j < bottomArray.Length; j++)
                    {
                        bottomArray[j].Top -= gapY;
                    }
                }
            }

            var leftArray =
                buttonArray
                .OrderBy(_ => _.Left).ThenBy(_ => _.Top)
                .ToArray();
            for (int i = 1; i < leftArray.Length; i++)
            {
                var gapX = leftArray[i].Left - leftArray[i - 1].Right;
                if (gapX > 0)
                {
                    for (int j = i; j < leftArray.Length; j++)
                    {
                        leftArray[j].Left -= gapX; 
                    }
                }
            }
            var widthB4 = workspacePanel.Width;
            var heightB4 = workspacePanel.Height;
            // Trim
            maxRight = buttonArray.Max(btn => btn.Right);
            maxBottom = buttonArray.Max(btn => btn.Bottom);
            workspacePanel.Size = new Size(maxRight, maxBottom);
        }
    }
}
