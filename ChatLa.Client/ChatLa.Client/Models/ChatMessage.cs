using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLa.Client.Models
{
    public class ChatMessage
    {
        public string ChatName { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }
        public Sender Sender { get; set; }
        public ChatMessage()
        {
            ChatName = "";
            Message = "";
            Sender = Sender.Partner;
            SendTime = DateTime.Now;
        }
    }
}
