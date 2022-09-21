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
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class ucOptions : UserControl
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

        private static ucOptions _instance;
        public static ucOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucOptions();
                return _instance;
            }
        }

        private ucOptions()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            lblSeparator1.BackColor = Color.FromArgb(64, 128, 128, 128);
            lblSeparator2.BackColor = Color.FromArgb(64, 128, 128, 128);
            lblSeparator3.BackColor = Color.FromArgb(64, 128, 128, 128);

            //load options
            string serverRegion = Preferences.Instance.Options.ServerRegion;

            if(serverRegion == "NA")
                optServerNA.Checked = true;
            else if(serverRegion == "JP")
                optServerJP.Checked = true;
            else
                optServerEU.Checked = true;

            PopulateLobbyComboBox();

            //chkLegacy.Checked = Preferences.Instance.Options.ShowLegacyTag;
        }

        private void ChangeServerOptions(string serverRegion)
        {
            Preferences.Instance.Options.ServerRegion = serverRegion;
            Preferences.Instance.SaveConfigFile();
        }

        private void optServerNA_CheckedChanged(object sender, System.EventArgs e) => ChangeServerOptions("NA");

        private void optServerJP_CheckedChanged(object sender, System.EventArgs e) => ChangeServerOptions("JP");

        private void optServerEU_CheckedChanged(object sender, System.EventArgs e) => ChangeServerOptions("EU");

        private void PopulateLobbyComboBox()
        {
            var lobbyOptions = Enum.GetValues(typeof(LobbyOptions));
            int selectedIndex = 0;
            
            foreach(var option in lobbyOptions)
            {
                string name = option.GetType().GetMember(option.ToString()).First().GetCustomAttribute<DisplayAttribute>().Name;
                int value = (int)Enum.Parse(typeof(LobbyOptions), option.ToString());  
                
                if((byte)value == Preferences.Instance.Options.LobbyOption)
                    selectedIndex = Array.IndexOf(lobbyOptions, option);

                cmbLobbyOptions.Items.Add(new ComboboxItem(name, value));
            }

            cmbLobbyOptions.SelectedIndex = selectedIndex;
        }

        private void cmbLobbyOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            Preferences.Instance.Options.LobbyOption = (byte)((ComboboxItem)cmbLobbyOptions.SelectedItem).Value;
            Preferences.Instance.SaveConfigFile();
        }
    }

    /// <summary>
    /// From: https://stackoverflow.com/questions/3063320/combobox-adding-text-and-value-to-an-item-no-binding-source
    /// </summary>
    public class ComboboxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }

        public ComboboxItem(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
