using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ExternalUsage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project.View.Client;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace XNA_Winforms_Wrapper
{
    public class ControlCache
    {
        public Texture2D tex;
        public Control C;
    }

    public class XNA_WF_Wrapper
    {
        /// <summary>
        /// call Init() instead 
        /// </summary>
        private static SpriteBatch SB;
        private static Game g;

        //form stuff
        private Form FormInit;
        private Texture2D FormBack;

        /// <summary>
        /// used to see if keys are being held down
        /// </summary>
        private MouseClass MC = new MouseClass();

        /// <summary>
        /// holds the last control drawn and its texture to save fps
        /// </summary>
        private static Dictionary<String, ControlCache> ControlTextures =
            new Dictionary<String, ControlCache>();

        /// <summary>
        /// IMPORTANT: call this first
        /// </summary>
        /// <param name="gg"></param>
        public static void StaticInit(Game gg)
        {
            g = gg;
            XNA.Init(g);

            SB = new SpriteBatch(g.GraphicsDevice);
        }

        /// <summary>
        /// IMPORTANT: call this from your form
        /// </summary>
        /// <param name="baseform"></param>
        public void Init(Form baseform)
        {
            FormInit = baseform;
            FormInit.Width = FormInit.Height = -1;
        }

        private int HandleListboxClick(ListBox lb, Rectangle clickloc)
        {
            for (var a = 0; a < lb.Items.Count; a++)
            {
                var a1 = lb.GetItemRectangle(a);
                var r = new Rectangle(lb.Location.X + a1.X, lb.Location.Y + a1.Y, a1.Width, a1.Height);
                if (r.Intersects(clickloc))
                {
                    lb.SelectedIndex = a;
                    return a;
                }
            }
            lb.SelectedIndex = -1;
            return -1;
        }

        private void HandleCheckboxClick(CheckBox cb)
        {
            cb.Checked = !cb.Checked;
        }

        //double s = Shared.mapRange(map.gravityKickIn + .1f, shipModel.maxSpeed, 0f, 1000f, distance);
        private static double mapRange(double value, double rawValueStart, double rawValueEnd, double mapRangeStart,
                                      double mapRangeEnd)
        {
            var dif = rawValueEnd - rawValueStart;
            if (dif == 0)
                dif = 1;

            var p = (value - rawValueStart) / (dif);
            if (p > 1)
                p = 1;
            if (p < 0)
                p = 0;

            var o = p * (mapRangeEnd - mapRangeStart);
            o += mapRangeStart;

            return o;
        }

        private void HandleTrackBarClick(TrackBar tb, Rectangle clickloc)
        {
            //off set start and end to get the true max and mix
            var startx = GetBasicRect(tb).X+10;
            var endx = startx + tb.Width-20;

            var val = (int)mapRange(clickloc.X, startx, endx, tb.Minimum, tb.Maximum);

            if (val < tb.Minimum)
                val = tb.Minimum;

            else if (val > tb.Maximum)
                val = tb.Maximum;

            tb.Value = val;
        }

        private void HandleRadioButtonClick(RadioButton rb, Rectangle clickloc)
        {
            //get all radio buttons for parent, uncheck depending on current checked
            foreach (var c in rb.Parent.Controls)
            {
                if (c is RadioButton)
                    ((RadioButton)c).Checked = (c == rb);
            }
        }

        /// <summary>
        /// IMPORTANT: insert into your mouse update function
        /// </summary>
        /// <param name="gt"></param>
        /// <param name="ms"></param>
        /// <returns></returns>
        public Control MouseUpdate(GameTime gt)
        {
            MC.UpdateButtons(Mouse.GetState(), gt);

            var ms = Mouse.GetState();
            var currposrect = new Rectangle(ms.X, ms.Y, 1, 1);
            var currposrect2 = new System.Drawing.Rectangle(ms.X, ms.Y, 1, 1);

            if (ms.LeftButton != ButtonState.Pressed)
            {
                MC.SwitchStates();
                return null;
            }

            Control ret = null;
            foreach (var c in GetControls(FormInit))
            {
                if (c == null || c.Enabled == false)
                    continue;

                var l = GetBasicRect(c);

                if (l.Intersects(currposrect))
                {
                    //press, not hold
                    if (MC.ButtonsDown.ContainsKey(MouseClass.mouseButtons.left))
                    {
                        //set selected index for listbox
                        if (c is ListBox)
                        {
                            HandleListboxClick(c as ListBox, currposrect);
                        }
                        else if (c is ListView)
                        {
                            HandleListViewClick(c as ListView, currposrect2);
                        }
                        else if (c is RadioButton)
                        {
                            HandleRadioButtonClick(c as RadioButton, currposrect);
                        }
                        else if (c is CheckBox)
                        {
                            HandleCheckboxClick(c as CheckBox);
                        }
                    }

                    if (c is TrackBar)
                    {
                        HandleTrackBarClick(c as TrackBar, currposrect);
                    }

                    ret = c;
                    break;
                }
            }

            MC.SwitchStates();
            return ret;
        }

        private void HandleListViewClick(ListView lv, System.Drawing.Rectangle r)
        {
            var lvir = GetBasicRect(lv);
            var found = false;

            foreach (ListViewItem lvi in lv.Items)
            {
                var rr = new System.Drawing.Rectangle(lvir.X + lvi.Bounds.X, lvir.Y + lvi.Bounds.Y,
                                                      lvir.Width + lvi.Bounds.Width, lvir.Height + lvi.Bounds.Height);
                if (rr.IntersectsWith(r))
                {
                    found = true;
                    lvi.Selected = true;
                }
            }

            if (found == false)
            {
                foreach (ListViewItem lvi in lv.Items)
                    lvi.Selected = false;
            }
        }

        private IEnumerable<Control> GetControls(Control basecontrol)
        {
            var ret = new List<Control>();

            foreach (var c in basecontrol.Controls)
            {
                var cc = c as Control;
                foreach (var ccc in cc.Controls)
                {
                    ret.AddRange(GetControls(ccc as Control));
                }
                if ((cc is Form) == false)
                    ret.Add(cc);
            }

            if ((basecontrol is Form) == false)
                ret.Add(basecontrol);

            return ret;
        }

        /// <summary>
        /// From a windows colour, get the matching XNA colour
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color GetXNAColor(System.Drawing.Color c)
        {
            return new Color(c.R, c.G, c.B);
        }

        private Rectangle GetBasicRect(Control c)
        {
            var size = c.Size;
            var r = new Rectangle(0, 0, size.Width, size.Height);
            //add parents locations as well
            var x = c;
            while (x != null)
            {
                r.X += x.Location.X;
                r.Y += x.Location.Y;

                x = x.Parent;
                if (x is Form || x is TableLayoutPanel)
                    break;
            }

            return r;
        }

        private void DrawFormBackground(bool transparentBackground = false)
        {
            if (transparentBackground == false)
                SB.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            else
                SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            var rr = GetBasicRect(FormInit);

            if (FormInit.BackgroundImage == null)
            {
                var bc = GetXNAColor(FormInit.BackColor);
                XNA.DrawCamRectangle(SB, bc);
            }
            else
            {
                if (FormBack == null)
                {
                    FormBack = BitmapToTexture2D(FormInit.BackgroundImage as Bitmap);
                }

                SB.Draw(FormBack, new Vector2(rr.X, rr.Y), Color.White);
            }

            SB.End();
        }

        /// <summary>
        /// IMPORTANT:will return true if your window has been resized. if this happens, call your forms Controls.Clear(), InitializeComponent() functions
        /// </summary>
        /// <returns></returns>
        public bool Resized()
        {
            var r = (FormInit.Width != g.GraphicsDevice.Viewport.Width ||
                      FormInit.Height != g.GraphicsDevice.Viewport.Height);
            return r;
        }

        /// <summary>
        /// IMPORTANT: insert this function into your draw method
        /// </summary>
        /// <param name="transparentBackground"></param>
        public void Draw(bool transparentBackground = false)
        {
            if (Resized())
            {
                FormInit.Width = g.GraphicsDevice.Viewport.Width;
                FormInit.Height = g.GraphicsDevice.Viewport.Height;
                ControlTextures.Clear();
            }

            DrawFormBackground(transparentBackground);

            foreach (var cc in FormInit.Controls)
            {
                if (cc is Control)
                {
                    var c = cc as Control;
                    DrawControlTree(c);
                }
            }
        }

        private void DrawControlTree(Control c)
        {
            DrawControl(c);

            foreach (var cc in c.Controls)
            {
                DrawControlTree(cc as Control);
            }
        }

        private string GetControlName(Control c)
        {
            var p = c.Parent;
            var ret = c.Name;

            while (p != null)
            {
                ret = p.Name + "/" + ret;
                p = p.Parent;
            }
            return ret;
        }

        private Texture2D GetTextureFromCache(Control c)
        {
            var cname = GetControlName(c);
            //remove if dirty
            if (ControlTextures.ContainsKey(cname) && ControlDirty(c))
                ControlTextures.Remove(cname);

            //add if it doesnt exist
            if (ControlTextures.ContainsKey(cname) == false)
            {
                //control cant be drawn if it doesnt have width or height
                if (c.Width <= 0 || c.Height <= 0)
                    return null;

                var b = new Bitmap(c.Width, c.Height);
                c.DrawToBitmap(b, new System.Drawing.Rectangle(0, 0, c.Width, c.Height));
                var btex = BitmapToTexture2D(b);

                var newc = (Control)CloneObject(c);

                var CC = new ControlCache() { C = newc, tex = btex };

                ControlTextures.Add(cname, CC);
            }

            //use tex from dic
            var tex = ControlTextures[cname].tex;
            return tex;
        }

        /// <summary>
        /// override drawing a textbox since for some reason it is always black
        /// </summary>
        /// <param name="c"></param>
        private void DrawTextbox(TextBox c)
        {
            //draw outside
            var r = GetBasicRect(c);
            var bc = GetXNAColor(c.BackColor);
            var fc = GetXNAColor(c.ForeColor);

            XNA.DrawRectangle(SB, r, bc);

            //draw text
            var dets = GetDrawStringDetails(c);
            SB.End();

            //draw text in blend
            SB.Begin();
            var text = c.Text;
            SB.DrawString(dets.Item1, text, dets.Item3, fc, 0, Vector2.Zero, dets.Item2, SpriteEffects.None, 0);
        }

        private Tuple<SpriteFont, float, Vector2> GetDrawStringDetails(Control c)
        {
            var font = XNA.GetFont(c.Font.Name);

            var fontsize = GetFontScale(c);

            //get height of string
            var itemsize = font.MeasureString("test") * fontsize;

            //var width = itemsize.X;
            var height = itemsize.Y;

            var r = GetBasicRect(c);
            float startx = r.X + 2;
            float starty = r.Y;
            /*
            //if the text is smaller vertically than the textbox, try to pad either side with space
            if (height<r.Y)
            {
                var dif2 = (r.Y - height)/2f;
                starty += dif2;
            }
                //otherwise just draw in the middle and hope for the best 
            else
            {
                starty +=(height/2f);
            }
            */
            var startpos = new Vector2(startx, starty);

            return new Tuple<SpriteFont, float, Vector2>(font, fontsize, startpos);
        }

        private static float GetFontScale(Control c)
        {
            return c.Font.Size / 100f;
        }

        /// <summary>
        /// can use this to get the caret position in a textbox from the cursor position
        /// </summary>
        /// <param name="clickpos"></param>
        /// <param name="tb"></param>
        /// <returns></returns>
        public int GetClickLocation(Vector2 clickpos, TextBox tb)
        {
            var dets = GetDrawStringDetails(tb);

            //get the total size of the string
            var itemsize = dets.Item1.MeasureString("A") * dets.Item2;

            //go through till the cursor matches

            var currx = dets.Item3.X;
            var width = itemsize.X;
            var pos = 0;

            while (currx < clickpos.X)
            {
                currx += width;
                pos++;
            }

            if (pos >= tb.Text.Length)
                pos = tb.Text.Length;
            return pos;
        }

        private void DrawControl(Control c, bool transparent = false)
        {
            if (transparent == false)
                SB.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            else
                SB.Begin();

            //have to override textbox
            if (c is TextBox)
            {
                DrawTextbox(c as TextBox);
            }
            else
            {
                var tex = GetTextureFromCache(c);
                var rr = GetBasicRect(c);

                if (tex == null)
                    SB.Draw(XNA.PixelTexture, new Vector2(rr.X, rr.Y), Color.White);
                else
                    SB.Draw(tex, new Vector2(rr.X, rr.Y), Color.White);
            }

            SB.End();
        }

        /// <summary>
        /// main method for determining which controls to redraw.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool ControlDirty(Control c)
        {
            var cname = GetControlName(c);

            //if there isnt an old control, its dirty and we need to create
            if (ControlTextures.ContainsKey(cname) == false)
            {
                ControlTextures.Add(cname, new ControlCache() { C = c });
                return true;
            }

            var controlcache = ControlTextures[cname];
            var oldc = controlcache.C;

            var t1 = c.GetType();
            var t2 = oldc.GetType();

            if (t1 != t2)
                return true;

            var basicdirty = (c.Text != oldc.Text ||
                               c.Enabled != oldc.Enabled ||
                               c.BackColor != oldc.BackColor ||
                               c.ForeColor != oldc.ForeColor);

            if (basicdirty)
                return true;

            var dirty = false;

            if (c is ListBox)
            {
                var lv = c as ListBox;
                var lvOld = oldc as ListBox;

                dirty = (lv.SelectedItem != lvOld.SelectedItem ||
                        lv.Items.Count != lvOld.Items.Count);
            }
            else if (c is TableLayoutPanel)
            {
                var pb = c as TableLayoutPanel;
                var pbold = oldc as TableLayoutPanel;

                dirty = (pb.RowCount != pbold.RowCount ||
                            pb.ColumnCount != pbold.ColumnCount
                              );
            }

            else if (c is ProgressBar)
            {
                var pb = c as ProgressBar;
                var pbold = oldc as ProgressBar;

                dirty = (pb.Value != pbold.Value ||
                              pb.Maximum != pbold.Maximum ||
                              pb.Minimum != pbold.Minimum);
            }

            else if (c is CheckBox)
            {
                var cb = c as CheckBox;
                var cbold = oldc as CheckBox;

                dirty = (cb.Checked != cbold.Checked);
            }

            else if (c is TrackBar)
            {
                var tb = c as TrackBar;
                var tbold = oldc as TrackBar;

                dirty = (tb.Value != tbold.Value ||
                    tb.Minimum != tbold.Minimum ||
                    tb.Maximum != tbold.Maximum);
            }

            else if (c is RadioButton)
            {
                var rb = c as RadioButton;
                var rbold = oldc as RadioButton;

                dirty = (rb.Checked != rbold.Checked);
            }

            else if (c is TextBox)
            {
                var tb = c as TextBox;
                var tbold = oldc as TextBox;

                dirty = (tb.Text.Equals(tbold.Text) == false);
            }
            else if (c is Panel || c.GetType().BaseType == typeof(Panel))
            {
                foreach (var cc in c.Controls)
                {
                    dirty = dirty | ControlDirty(cc as Control);
                }
                if (dirty)
                    ControlTextures.Clear();
            }
            else if (c is ListView)
            {
                var lv = c as ListView;
                var lvold = oldc as ListView;

                dirty = (lv.Items.Count != lvold.Items.Count ||
                         lv.SelectedItems.Count != lvold.SelectedItems.Count);
            }

            if (dirty)
                return true;

            return false;
        }

        private static Control CloneObject(Control o)
        {
            var t = o.GetType();
            var properties = t.GetProperties();

            var p = (Control)t.InvokeMember("", BindingFlags.CreateInstance, null, o, null);

            if (o is ComboBox)
            {
                foreach (String s in ((ComboBox)o).Items)
                {
                    ((ComboBox)p).Items.Add(s);
                }
            }

            else if (o is ListBox)
            {
                foreach (String s in ((ListBox)o).Items)
                {
                    ((ListBox)p).Items.Add(s);
                }
            }
            else if (o is ListView)
            {
                var ol = ((ListView)o);
                var op = ((ListView)p);

                foreach (ListViewItem lvi in ol.Items)
                {
                    var lvinew = (ListViewItem)lvi.Clone();
                    op.Items.Add(lvinew);
                    lvinew.Selected = lvi.Selected;
                }
            }

            for (var a = 0; a < o.Controls.Count; a++)
            {
                var c = o.Controls[a];
                p.Controls.Add(CloneObject(c));
            }

            //if we copy the parent property, it will add the newly cloned object to those lists which we dont want,
            foreach (var pi in properties)
            {
                //dont write parent property
                if (pi.Name.Equals("Parent"))
                    continue;

                try
                {
                    if (pi.CanWrite)
                    {
                        pi.SetValue(p, pi.GetValue(o, null), null);
                    }
                }
                catch (Exception)
                {

                }
            }

            return p;
        }

        private Texture2D BitmapToTexture2D(Bitmap bmp)
        {
            var pixels = new Color[bmp.Width * bmp.Height];
            for (var y = 0; y < bmp.Height; y++)
            {
                for (var x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    pixels[(y * bmp.Width) + x] = new Color(c.R, c.G, c.B, c.A);
                }
            }

            var myTex = new Texture2D(
              g.GraphicsDevice,
              bmp.Width,
              bmp.Height, false,
              SurfaceFormat.Color);

            myTex.SetData<Color>(pixels);
            return myTex;
        }
    }
}
