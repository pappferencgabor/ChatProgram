using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatProgram.Models
{
    // Message Handler Response
    internal class ApiResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public ApiRequestBody response { get; set; }
    }

    internal class MessageData
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public string SentTo { get; set; }
        public string Date { get; set; }

        public MessageData(string username, string message, string sentTo, string date)
        {
            Username = username;
            Message = message;
            SentTo = sentTo;
            Date = date;
        }
    }



    // Users Handler
    internal class ConnectedUsers
    {
        public List<string> users { get; set; }
    }
}
