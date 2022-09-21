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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrimalLauncher
{
    /// <summary>
    /// Trasparent ListBox implementation from:
    /// https://stackoverflow.com/questions/58997329/how-to-make-transparent-background-to-listbox
    /// All credit goes to the author.
    /// </summary>
    [ToolboxBitmap(typeof(ListBox)), DesignerCategory("")]
    class TransparentListBox : ListBox
    {
        private const int WM_KILLFOCUS = 0x8;
        private const int WM_VSCROLL = 0x115;
        private const int WM_HSCROLL = 0x114;

        public TransparentListBox() : base()
        {
            SetStyle(
                ControlStyles.Opaque |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        public Color SelectionBackColor { get; set; } = Color.DarkOrange;

        [DllImport("uxtheme", ExactSpelling = true)]
        private extern static int DrawThemeParentBackground(
            IntPtr hWnd,
            IntPtr hdc,
            ref Rectangle pRect
            );

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rec = ClientRectangle;

            IntPtr hdc = g.GetHdc();
            DrawThemeParentBackground(this.Handle, hdc, ref rec);
            g.ReleaseHdc(hdc);

            using (Region reg = new Region(e.ClipRectangle))
            {
                if (Items.Count > 0)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        rec = GetItemRectangle(i);

                        if (e.ClipRectangle.IntersectsWith(rec))
                        {
                            if ((SelectionMode == SelectionMode.One && SelectedIndex == i) ||
                                (SelectionMode == SelectionMode.MultiSimple && SelectedIndices.Contains(i)) ||
                                (SelectionMode == SelectionMode.MultiExtended && SelectedIndices.Contains(i)))
                                OnDrawItem(new DrawItemEventArgs(g, Font, rec, i, DrawItemState.Selected, ForeColor, BackColor));
                            else
                                OnDrawItem(new DrawItemEventArgs(g, Font, rec, i, DrawItemState.Default, ForeColor, BackColor));

                            reg.Complement(rec);
                        }
                    }
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var rec = e.Bounds;
            var g = e.Graphics;

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                using (SolidBrush sb = new SolidBrush(SelectionBackColor))
                    g.FillRectangle(sb, rec);


            //foreach(var item in Items)
            //{
            //    if (item == null) Items.Remove(item);
            //}

            using (Font font = new Font(FontFamily.GenericMonospace, 9, FontStyle.Bold))
            {
                //if (e.Index <= Items.Count)
                //{
                var listBoxItem = Items[e.Index];
                Color color = Log.Instance.GetMessageColor(listBoxItem.ToString());

                using (SolidBrush sb = new SolidBrush(color))
                    g.DrawString(
                        GetItemText(Items[e.Index]),
                        font  /*Font*/,
                        sb,
                        e.Bounds
                    );
                //}
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg != WM_KILLFOCUS &&
                (m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL))
                Invalidate();
            base.WndProc(ref m);
        }
    }
}
