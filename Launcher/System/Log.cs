using MaterialSkin.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace Launcher
{
    public sealed class Log
    {
        #region Message labels
        private string _cli { get; } =       "[client] ";
        private string _info { get; } =   "[info]";
        private string _error { get; } =     "[error]";
        private string _warning { get; } =   "[warn]";
        private string _success { get; } =   "[done]";
        private string _update { get; } =    "[update]";
        private string _http { get; } =      "[http]";
        private string _chat { get; } =      "[chat]";
        private string _separator { get; } = "========================================================================================";
        #endregion

        private static Log _instance = null;
        private static readonly object _padlock = new object();

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

        private void SendMessage(string type, string msg)
        {
            //Change Form1 to whatever your form is called
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(MainWindow))
                {
                    MainWindow frmTemp = (MainWindow)frm;
                    frmTemp.WriteLogMessage(type + " " + msg);

                }
            }
        }
        #region Message types
        public void Info(string msg) => SendMessage(_info, msg);
        public void Success(string msg) => SendMessage(_success, msg);
        public void Warning(string msg) => SendMessage(_warning, msg);
        public void Error(string msg) => SendMessage(_error, msg);
        public void Chat(string msg) => SendMessage(_chat, msg);

        public void Blank() => SendMessage("","");

        public void Separator() => SendMessage("", _separator);
        #endregion       
        
        public Color GetMessageColor(string str)
        {
            Color c = Color.Black; //default color

            if (str.IndexOf(_error) >= 0)
                c = Color.Red;
            else if (str.IndexOf(_warning) >= 0)
                c = Color.DarkOrange;
            else if (str.IndexOf(_success) >= 0)
                c = Color.DarkGreen;
            else if (str.IndexOf(_chat) >= 0)
                c = Color.DarkOrchid;

            return c;
        }
    }
}
