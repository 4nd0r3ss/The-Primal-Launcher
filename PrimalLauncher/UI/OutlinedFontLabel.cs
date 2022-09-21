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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PrimalLauncher
{
    /// <summary>
    /// From: https://stackoverflow.com/questions/19842722/setting-a-font-with-outline-color-in-c-sharp
    /// All credit goes to original author.
    /// </summary>
    [ToolboxBitmap(typeof(Label)), DesignerCategory("")]
    public class OutlinedFontLabel : Label
    {
        public Color OutlineForeColor { get; set; }
        public float OutlineWidth { get; set; }

        public OutlinedFontLabel()
        {
            OutlineForeColor = Color.Black;
            OutlineWidth = 2;
        }               

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            using (GraphicsPath gp = new GraphicsPath())
            using (Pen outline = new Pen(OutlineForeColor, OutlineWidth)
            { LineJoin = LineJoin.Round })
            using (StringFormat sf = new StringFormat())
            using (Brush foreBrush = new SolidBrush(ForeColor))
            {
                gp.AddString(Text, Font.FontFamily, (int)Font.Style,
                    Font.Size, ClientRectangle, sf);
                e.Graphics.ScaleTransform(1.3f, 1.35f);
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawPath(outline, gp);
                e.Graphics.FillPath(foreBrush, gp);
            }
        }
    }
}
