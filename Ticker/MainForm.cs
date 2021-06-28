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
        private bool closing = false;

        public delegate void TickerUpdate(DTO.Symbol symbol);

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
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

            ConnectWebsocket();
        }

        private void WebSocket_Disconnected(object sender, EventArgs e)
        {
            ConnectWebsocket();
        }

        private async void ConnectWebsocket()
        {
            var connected = false;

            while (!connected & !closing)
            {
                try
                {
                    await webSocket.Open();
                    connected = true;
                }
                catch (Exception)
                {
                }
            }
        }


        private void WebSocket_TickerUpdate(object sender, TickerUpdateEventArgs e)
        {
            Invoke(new TickerUpdate(HandleUpdate), e.Symbol);
        }

        private void HandleUpdate(Symbol dto)
        {
            var value = dto.Value.ClosingValue.TrimEnd('0').PadRight(7, '0');
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
            closing = true;
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

        private void SettingsStripMenuItem1_Click(object sender, EventArgs e)
        {
            var val = Microsoft.VisualBasic.Interaction.InputBox("Enter alarm threshold.");

            if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold))
            {
                alarm.Threshold = threshold;
            }
            else
            {
                MessageBox.Show($"'{val}' is not a valid float");
            }
        }
    }
}
