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

using System.Drawing;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class ucLog : UserControl
    {
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x2000000;
                return cp;
            }
        }

        private static ucLog _instance;
        public static ucLog Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucLog();
                return _instance;
            }
        }

        private ucLog()
        {
            InitializeComponent();            
            CheckForIllegalCrossThreadCalls = false;    
            BackColor = Color.FromArgb(128, 0, 0, 0); //semi-transparent bg    
        }

        public void WriteLogMessage(string message)
        {
            if(!string.IsNullOrEmpty(message))
                LbxServerLog.Items.Insert(0, message);
        }
    }
}
