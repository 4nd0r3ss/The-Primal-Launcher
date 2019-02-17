using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class ucLogWindow : UserControl
    {
        private Log _log { get; set; } = Log.Instance;
        private static ucLogWindow _instance;
        public static ucLogWindow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucLogWindow();
                return _instance;
            }
        }


        private ucLogWindow()
        {
            InitializeComponent();

            //dirty way to get rid of cross-thread exception
            CheckForIllegalCrossThreadCalls = false;
        }

        public void WriteLogMessage(string message)
        {
            LbxOutput.Items.Insert(0, message);
        }

        private void Lbxoutput_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            e.Graphics.DrawString(
                LbxOutput.Items[e.Index].ToString(),
                new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular),
                new SolidBrush(_log.GetMessageColor(LbxOutput.Items[e.Index].ToString())), e.Bounds);
        }
    }
}
