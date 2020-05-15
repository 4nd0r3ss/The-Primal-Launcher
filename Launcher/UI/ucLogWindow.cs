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


            //Task.Run(() => {
            //    while (true)
            //    {
            //        lblEorzeaTime.Text = Clock.Instance.Time;
            //    }
            //});
            
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

        public void PrintPlayerPosition(Position position, byte[] unknown)
        {
            lblPCPositionZone.Text = position.ZoneId.ToString();
            lblPCPositionX.Text = position.X.ToString();
            lblPCPositionY.Text = position.Y.ToString();
            lblPCPositionZ.Text = position.Z.ToString();
            lblPCPositionR.Text = position.R.ToString();
                
                
                
                
                //"x: " + position.X + ", y: " + position.Y + ", z: " + position.Z + ", r: " + position.R + ", region id: " + position.ZoneId + ", unknown: " +
                //unknown[0].ToString("X2") + " " + unknown[1].ToString("X2") + " " + unknown[2].ToString("X2") + " " +
                //unknown[3].ToString("X2") + " " + unknown[4].ToString("X2") + " " + unknown[5].ToString("X2");

        }
    }
}
