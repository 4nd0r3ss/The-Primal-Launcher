/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.IO;

namespace PrimalLauncher
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
           ucLog.Instance.WriteLogMessage(type + " " + msg);
        }

        #region Message types
        public void Info(string msg) => SendMessage(_info, msg);
        public void Success(string msg) => SendMessage(_success, msg);
        public void Warning(string msg) => SendMessage(_warning, msg);
        public void Error(string msg)
        {
            SendMessage(_error, msg);
            File.AppendAllText(Preferences.Instance.AppUserFolder + @"error_log.txt", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + " " + msg + "\n");
        }
        public void Chat(string msg) => SendMessage(_chat, msg);

        public void Blank() => SendMessage("","");

        public void Separator() => SendMessage("", _separator);
        #endregion       
        
        public Color GetMessageColor(string str)
        {
            Color c = Color.LightGray; //default color

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
