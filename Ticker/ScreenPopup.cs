using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ticker
{
    public partial class ScreenPopup : Form
    {
        private Point MouseDownLocation;

        public ScreenPopup()
        {
            InitializeComponent();
            BringToFront();
            TopMost = true;
            ShowInTaskbar = false;
        }

        public void SetLabelText(string text)
        {
            label1.Text = text;            
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                MouseDownLocation = e.Location;
            }
        }

        private void MyControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Left = e.X + this.Left - MouseDownLocation.X;
                this.Top = e.Y + this.Top - MouseDownLocation.Y;
            }
        }

        private void ScreenPopup_Load(object sender, EventArgs e)
        {
            this.MouseMove += MyControl_MouseMove;
            label1.MouseMove += MyControl_MouseMove;            
            this.label1.MouseDown += MouseDownHandler;

            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
        }
    }
}
