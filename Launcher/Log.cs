using System.Collections.Generic;
using System.Drawing;

namespace Launcher
{
    public sealed class Log
    {
        #region Message labels
        private string CLI { get; } = "[client] ";
        private string MESSAGE { get; } = "[message]";
        private string ERROR { get; } = "[error]  ";
        private string WARNING { get; } = "[warning]";
        private string SUCCESS { get; } = "[success]";
        private string UPDATE { get; } = "[update]";
        private string HTTP { get; } = "[http]";
        private string SEPARATOR { get; } = "========================================================================================";
        #endregion

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
                        _instance = new Log();                    

                    return _instance;
                }
                 
            }
        }    
        
        private Log() { }
             
        private void Enqueue(string type, string msg) => MessageQueue.Enqueue(new KeyValuePair<string, string>(type, msg));

        #region Message types
        public void Message(string msg) => Enqueue(MESSAGE, msg);

        public void Success(string msg) => Enqueue(SUCCESS, msg);

        public void Warning(string msg) => Enqueue(WARNING, msg);

        public void Error(string msg) => Enqueue(ERROR, msg);

        public void Blank() => Enqueue("","");

        public void Separator() => Enqueue("", SEPARATOR);
        #endregion

        public string GetLogMessage()
        {            
            KeyValuePair<string, string> msg = MessageQueue.Dequeue();  
            return msg.Key + " " + msg.Value;
        }

        public bool HasLogMessages() => (MessageQueue.Count != 0);
        
        public Color GetMessageColor(string str)
        {
            Color c = Color.Black; //default color

            if (str.IndexOf(ERROR) >= 0)
                c = Color.Red;
            else if (str.IndexOf(WARNING) >= 0)
                c = Color.DarkOrange;
            else if (str.IndexOf(SUCCESS) >= 0)
                c = Color.DarkGreen;

            return c;
        }
    }
}
