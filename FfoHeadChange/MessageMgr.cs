using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FfoHeadChange
{
    public class StringEventArgs: EventArgs
    {
        public StringEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
    public class MessageMgr
    {
        public static event EventHandler<StringEventArgs> OnMessageSent;

        public static void SendMessage(object sender, string message)
        {
            OnMessageSent?.Invoke(sender, new StringEventArgs(message));
        }
    }
}
