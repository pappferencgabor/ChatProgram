using ChatProgram.Models;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ChatProgram
{
    public partial class Chat : Window
    {
        private ClientWebSocket _clientWebSocket;
        public readonly HttpClient _httpClient = new();
        private static System.Timers.Timer aTimer;

        public User usr;
        public Chat(User user)
        {
            InitializeComponent();

            usr = user;
            cbUsers.Items.Add("Mindenki");
            cbUsers.SelectedIndex = 0;

            ConnectToWebSocket();

            getConnectedUsers(null, null);

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(5000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += getConnectedUsers;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async Task ConnectToWebSocket()
        {
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(new Uri($"ws://127.0.0.1:7777/chat?username={usr.Name}"), CancellationToken.None);

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
            var sentTo = cbUsers.Text == "" ? "Mindenki" : cbUsers.Text;

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
            }            
        }

        private async void getConnectedUsers(object source, ElapsedEventArgs e)
        {
            string url = "http://127.0.0.1:7777/api/users";
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ConnectedUsers jsonData = JsonSerializer.Deserialize<ConnectedUsers>(jsonResponse);

                // A UI frissítését a Dispatcher segítségével a fő szálra kell irányítani
                Dispatcher.Invoke(() =>
                {
                    clearCbCustom();
                    //cbUsers.Items.Clear(); // Töröljük a ComboBox elemeit
                    //cbUsers.Items.Add("Mindenki"); // Alapértelmezett elem hozzáadása

                    foreach (var user in jsonData.users)
                    {
                        if (user != usr.Name)
                        {
                            cbUsers.Items.Add(user); // Új felhasználó hozzáadása
                        }
                    }
                });
            }
        }

        private void clearCbCustom()
        {
            // Először összegyűjtjük az eltávolítandó elemeket
            var itemsToRemove = new List<object>();

            foreach (var user in cbUsers.Items)
            {
                if (user.ToString() != "Mindenki")
                {
                    itemsToRemove.Add(user);
                }
            }

            // Most eltávolítjuk őket a gyűjteményből
            foreach (var user in itemsToRemove)
            {
                cbUsers.Items.Remove(user);
            }

            cbUsers.SelectedIndex = 0;
        }
    }
}
