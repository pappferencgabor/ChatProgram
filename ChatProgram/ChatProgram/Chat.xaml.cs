using ChatProgram.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

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
                var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var messageData = JsonSerializer.Deserialize<MessageData>(messageJson);

                // Üzenetek megjelenítése a ListBox-ban
                Dispatcher.Invoke(() =>
                {
                    ListBoxItem item = new ListBoxItem();

                    if (messageData.SentTo == usr.Name || messageData.SentTo == "Mindenki" || messageData.Username == usr.Name)
                    {
                        //MessageBox.Show(messageData.Username);
                        //MessageBox.Show(usr.Name);
                        //MessageBox.Show((messageData.Username == usr.Name).ToString());
                        item.Content = $"[{messageData.Date}] {messageData.Username} -> {messageData.SentTo}: {messageData.Message}";

                        if (messageData.Username == usr.Name)
                        {
                            item.Background = new SolidColorBrush(Colors.LightGreen);
                            item.HorizontalContentAlignment = HorizontalAlignment.Right;
                        }
                        else if (messageData.SentTo == usr.Name)
                        {
                            item.Background = new SolidColorBrush(Colors.LightSalmon);
                            item.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }
                        else if (messageData.SentTo == "Mindenki")
                        {
                            item.Background = new SolidColorBrush(Colors.LightGray);
                            item.HorizontalContentAlignment = HorizontalAlignment.Left;
                        }

                        lbMessages.Items.Add(item);
                    }
                });
            }
        }

        private async void SendMessage(MessageData message)
        {
            var messageJson = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(messageJson);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            var message = txtMessage.Text.Trim();
            var sentTo = txtSentTo.Text.Trim() == "" ? "Mindenki" : txtSentTo.Text.Trim();

            if (message == "")
            {
                return;
            }
            else
            {
                MessageData messageData = new MessageData(
                    usr.Name,
                    message,
                    sentTo,
                    string.Format("{0:HH:mm}", DateTime.Now)
                );

                SendMessage(messageData);

                txtMessage.Clear();
                txtSentTo.Clear();
            }            
        }
    }
}
