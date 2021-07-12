using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class ucAbout : UserControl
    {
        private static ucAbout _instance;
        public static ucAbout Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucAbout();
                return _instance;
            }
        }
        private ucAbout()
        {
            InitializeComponent();
        }
    }
}
