using ChatProgram.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChatProgram
{
    public partial class Chat : Window
    {
        private ClientWebSocket _clientWebSocket;

        public User usr;
        public Chat(User user)
        {
            InitializeComponent();
            usr = user;
            ConnectToWebSocket();
        }

        private async Task ConnectToWebSocket()
        {
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(new Uri("ws://127.0.0.1:7777/chat"), CancellationToken.None);

            // Kezdjük el az üzenetek fogadását
            ReceiveMessages();
        }

        private async void ReceiveMessages()
        {
            var buffer = new byte[1024 * 4];
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Üzenetek megjelenítése a ListBox-ban
                Dispatcher.Invoke(() =>
                {
                    lbMessages.Items.Add(message);
                });
            }
        }

        private async void SendMessage(string message)
        {
            var completeMessage = $"[{string.Format("{0:HH:mm}", DateTime.Now)}] {usr.Name}: {message}";
            var bytes = Encoding.UTF8.GetBytes(completeMessage);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            var message = txtMessage.Text;
            SendMessage(message);
            txtMessage.Clear();
        }
    }
}
