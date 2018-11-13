using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public sealed class Log
    {
        public string CLI { get; } = "[client] ";
        public string MSG { get; } = "[message]";
        public string ERR { get; } = "[error]  ";
        public string WNG { get; } = "[warning]";
        public string OK { get; } = "[success]";
        public string SEPARATOR { get; } = "========================================================================================";

        private static Log _instance = null;
        private static readonly object _padlock = new object();
        private Queue<KeyValuePair<string, string>> MessageQueue { get; set; } = new Queue<KeyValuePair<string, string>>();

        public static Log Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new Log();
                    }

                    return _instance;
                }
                 
            }
        }

        private Log(){ }
             
        public void LogMessage(string type, string msg)
        {
            MessageQueue.Enqueue(new KeyValuePair<string, string>(type, msg));
        }

        public void LogMessage()
        {
            //Draw a blank line in the output.
            MessageQueue.Enqueue(new KeyValuePair<string, string>("", ""));
        }

        public void LogMessage(string msg)
        {
            //Draw a message with no header.
            MessageQueue.Enqueue(new KeyValuePair<string, string>("", msg));
        }

        public string GetLogMessage()
        {
            string result = "";
            KeyValuePair<string, string> msg = MessageQueue.Dequeue();

            if (!msg.Key.Equals(""))
                result = msg.Key + " ";

            result = result + msg.Value;

            return result;
        }

        public bool HasLogMessages()
        {
            if (MessageQueue.Count != 0)
                return true;
            else
                return false;
        }
    }
}
