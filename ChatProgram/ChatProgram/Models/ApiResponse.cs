using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatProgram.Models
{
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
    }
}
