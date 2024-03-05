using System.Diagnostics;

namespace multi_display_diagram
{
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
}
