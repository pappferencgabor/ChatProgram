using ChatProgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatProgram
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public User usr;
        public Chat(User user)
        {
            InitializeComponent();
            usr = user;
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            ApiRequestBody body = new ApiRequestBody();
            body.Username = usr.Name;
            body.Message = txtMessage.Text;

            string url = "http://127.0.0.1:7777/api/chat/v1/send";

            using (HttpClient client = new HttpClient())
            {
                string json = JsonSerializer.Serialize(body);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    ApiResponse messageResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse);
                    lbMessages.Items.Add($"[{string.Format("{0:HH:mm}", DateTime.Now)}] {messageResponse.response.Username}: {messageResponse.response.Message}");
                }
                else
                {
                    MessageBox.Show($"Error: {response.StatusCode}");
                }
            }
        }
    }
}
