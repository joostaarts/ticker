using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ticker
{
    class TickerWebSocketConnection
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly string url;
        private ClientWebSocket socket;
        private readonly System.Threading.ManualResetEvent stopped = new System.Threading.ManualResetEvent(true);

        public event EventHandler Disconnected;
        public event EventHandler<TickerUpdateEventArgs> TickerUpdate;

        public TickerWebSocketConnection(string url)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.url = url;
        }

        public async Task Open()
        {
            socket?.Dispose();
            socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(url), System.Threading.CancellationToken.None);

            var socketThread = new System.Threading.Thread(() =>
            {
                try
                {
                    socketLoop();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }

            });
            socketThread.Start();
        }

        private async void socketLoop()
        {
            var segment = new ArraySegment<byte>(new byte[2048]);
            var ms = new System.IO.MemoryStream();

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                stopped.Reset();

                WebSocketReceiveResult result;

                try
                {
                    do
                    {
                        result = await socket.ReceiveAsync(segment, cancellationTokenSource.Token);
                        ms.Write(segment.Array, segment.Offset, result.Count);
                    } while (!result.EndOfMessage);
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    break;
                }
                catch 
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    socket.Dispose();
                    return;
                }


                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                ms.Position = 0;

                using (var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true))
                {
                    var payload = await reader.ReadToEndAsync();
                    var symbol = JsonSerializer.Deserialize<DTO.Symbol>(payload);
                    TickerUpdate?.Invoke(this, new TickerUpdateEventArgs(symbol));
                }

                ms.Position = 0;
                ms.SetLength(0);
            }

            socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, null, System.Threading.CancellationToken.None);

            stopped.Set();
        }

        internal void Close()
        {
            cancellationTokenSource.Cancel();
            stopped.WaitOne();
        }
    }
}
