using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Ticker.DTO;

namespace Ticker
{
    public partial class MainForm : Form
    {
        private Alarm alarm;
        private TickerWebSocketConnection webSocket;
        private ScreenPopup popup;
        private IConfigurationRoot configuration;

        public delegate void TickerUpdate(DTO.Symbol symbol);

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var builder = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json",
                  optional: true,
                  reloadOnChange: true);

            configuration = builder.Build();

            Visible = false;
            ShowInTaskbar = false;

            popup = new ScreenPopup();
            popup.Show();

            alarm = new Alarm(double.Parse(configuration["AlertThreshold"], CultureInfo.InvariantCulture));

            webSocket = new TickerWebSocketConnection(configuration["TickerUrl"]);
            webSocket.TickerUpdate += WebSocket_TickerUpdate;
            webSocket.Disconnected += WebSocket_Disconnected;

            try
            {
                webSocket.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void WebSocket_Disconnected(object sender, EventArgs e)
        {
            try
            {
                webSocket.Open();
            }
            catch (Exception)
            {
                popup.SetLabelText("Disconnected");
            }
        }

        private void WebSocket_TickerUpdate(object sender, TickerUpdateEventArgs e)
        {
            Invoke(new TickerUpdate(HandleUpdate), e.Symbol);
        }

        private void HandleUpdate(Symbol dto)
        {
            var value = dto.Value.ClosingValue.TrimEnd('0');
            Text = value;
            popup.SetLabelText(value);

            if (!contextMenuStrip.Visible)
            {
                popup.BringToFront();
            }

            notifyIcon.BalloonTipText = value;
            notifyIcon.Text = value;

            alarm.TriggerAlarm(double.Parse(value, CultureInfo.InvariantCulture));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            webSocket.Disconnected -= WebSocket_Disconnected;
            webSocket.TickerUpdate -= WebSocket_TickerUpdate;

            base.OnClosing(e);
            webSocket.Close();
        }

        private void disableAlarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            alarm.Enabled = !alarm.Enabled;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showTickerValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleVisibility();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ToggleVisibility();
        }

        private void ToggleVisibility()
        {
            popup.Visible = !popup.Visible;
            showTickerValueToolStripMenuItem.Checked = popup.Visible;
        }
    }
}
