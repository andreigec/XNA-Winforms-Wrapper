using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ExternalUsage
{
    public static class XNA
    {
        /// <summary>
        /// can access this texture in your game 
        /// </summary>
        public static Texture2D PixelTexture;
        private static Game parent;

        /// <summary>
        /// sprite name to sprite font
        /// </summary>
        private static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

        /// <summary>
        /// By default, your XNA content project should have a "Fonts" folder
        /// </summary>
        public static string fontDirectory="Fonts";
        
        /// <summary>
        /// if set to true, will output errors to the console
        /// </summary>
        public static bool Debugging = false;

        private static char[] hexDigits = {
		                                  	'0', '1', '2', '3', '4', '5', '6', '7',
		                                  	'8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
		                                  };

        private static void setPrimitiveColour(Color c)
        {
            var hex = ColorToHexString(c);
            var pixel = new Int32[1];
            pixel[0] = Int32.Parse(hex, NumberStyles.HexNumber);
            PixelTexture.SetData<Int32>(pixel, 0, PixelTexture.Width * PixelTexture.Height);
        }

        private static string ColorToHexString(Color color)
        {
            var bytes = new byte[3];
            bytes[0] = color.R;
            bytes[1] = color.G;
            bytes[2] = color.B;
            var chars = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        /// <summary>
        /// Get an XNA usable font from a name. will default to the first font if not found in the folder
        /// </summary>
        /// <param name="fontname"></param>
        /// <returns></returns>
        public static SpriteFont GetFont(String fontname)
        {
            var catchf = false;
            if (fonts.ContainsKey(fontname) == false)
            {
                String fontpath;
                if (String.IsNullOrEmpty(fontDirectory))
                    fontpath = fontname;
                else
                    fontpath = fontDirectory + "/" + fontname;
                try
                {
                    var sm = parent.Content.Load<SpriteFont>(fontpath);
                    fonts.Add(fontname, sm);
                }
                catch (Exception)
                {
                    catchf = true;
                }
            }

            if (catchf || fonts.ContainsKey(fontname) == false)
            {
                if (Debugging)
                    Console.WriteLine("XNA_WF_WRAPPER ERROR:.spritefont not found for:" + fontname);

                if (fonts.Count >= 1)
                {
                    var sm = fonts.First().Value;

                    if (Debugging)
                        Console.WriteLine("Returning first font in loaded fonts by default" +
                            "Will default to:'" + sm + "' in the future when:'" + fontname + "' is referenced");

                    //add to fonts array
                    fonts.Add(fontname, sm);
                    return sm;
                }

                var cd = parent.Content.RootDirectory + "\\";
                var dir = cd + fontDirectory;
                if (Directory.Exists(dir))
                {
                    var ff = Directory.GetFiles(dir);
                    if (ff.Count() >= 1)
                    {
                        var f = ff[0];
                        //remove ext
                        f = f.Substring(0, f.IndexOf('.'));
                        //just get file name
                        //f = f.Substring(f.LastIndexOf('\\')+1);
                        //remove content dir
                        f = f.Substring(cd.Length);

                        try
                        {
                            var sm = parent.Content.Load<SpriteFont>(f);
                            //add to fonts array
                            fonts.Add(fontname, sm);

                            if (Debugging)
                                Console.WriteLine("Returning first font loaded manually from contents folder.\n" +
                                                  "Will default to:'" + f + "' in the future when:'" + fontname + "' is referenced");
                            return sm;
                        }
                        catch (Exception)
                        {

                        }
                    }
                }

                if (Debugging)
                    Console.WriteLine("Error, no replacement font could be found!");
                return null;
            }

            return fonts[fontname];
        }


        /// <summary>
        /// call this to use the pixel texture or any draw items
        /// </summary>
        /// <param name="parentIN"></param>
        public static void Init(Game parentIN)
        {
            parent = parentIN;
            //TEXTURE
            if (PixelTexture==null)
            {
                PixelTexture = new Texture2D(parent.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);

                Int32[] pixel = { 0xFFFFFF }; // White. 0xFF is Red, 0xFF0000 is Blue
                PixelTexture.SetData<Int32>(pixel, 0, PixelTexture.Width * PixelTexture.Height);
                setPrimitiveColour(Color.White);    
            }
        }

        public static void DrawLineRectangle(SpriteBatch SB, float width, Color color, Rectangle OutlineRectangle)
        {
            //we need to use the original control size as an outline, and draw the actual button inside of that
            var topleft = new Vector2(OutlineRectangle.X, OutlineRectangle.Y);
            var topright = new Vector2(OutlineRectangle.X + OutlineRectangle.Width, OutlineRectangle.Y);
            var bottomright = new Vector2(OutlineRectangle.X + OutlineRectangle.Width, OutlineRectangle.Y + OutlineRectangle.Height);
            var bottomleft = new Vector2(OutlineRectangle.X, OutlineRectangle.Y + OutlineRectangle.Height);

            DrawLine(SB,width,color,topleft,topright);
            DrawLine(SB, width, color, topright, bottomright);
            DrawLine(SB, width, color, bottomleft, bottomright);
            DrawLine(SB, width, color, topleft, bottomleft);
        }

        public static void DrawLine(SpriteBatch batch, float width, Color color, Vector2 point1, Vector2 point2)
        {
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            var length = Vector2.Distance(point1, point2);

            batch.Draw(PixelTexture, point1, null, color, angle, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0);
        }

        /// <summary>
        /// draw a rectangle as big as the camera - to clear
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="c"></param>
        public static void DrawCamRectangle(SpriteBatch batch, Color c)
        {
            var r = new Rectangle(0, 0, batch.GraphicsDevice.Viewport.Width, batch.GraphicsDevice.Viewport.Height);
            batch.Draw(PixelTexture, r, c);
        }

        public static void DrawRectangle(SpriteBatch batch, Rectangle r, Color c)
        {
            batch.Draw(PixelTexture, r, c);
        }

        /// <summary>
        /// Creates a circle starting from 0, 0.
        /// </summary>
        /// <param name="radius">The radius (half the width) of the circle.</param>
        /// <param name="sides">The number of sides on the circle (the more the detailed).</param>
        public static void DrawCircle(SpriteBatch batch, float radius, int sides, Color c, int width)
        {
            var vectors = new ArrayList();

            var max = 2 * (float)Math.PI;
            var step = max / (float)sides;

            for (float theta = 0; theta < max; theta += step)
            {
                vectors.Add(new Vector2(radius * (float)Math.Cos((double)theta),
                    radius * (float)Math.Sin((double)theta)));
            }

            // then add the first vector again so it's a complete loop
            vectors.Add(new Vector2(radius * (float)Math.Cos(0),
                    radius * (float)Math.Sin(0)));

            DrawVectors(batch, vectors, width, c);
        }

        /// <summary>
        /// Creates an ellipse starting from 0, 0 with the given width and height.
        /// Vectors are generated using the parametric equation of an ellipse.
        /// </summary>
        /// <param name="semimajor_axis">The width of the ellipse at its center.</param>
        /// <param name="semiminor_axis">The height of the ellipse at its center.</param>
        /// <param name="angle_offset">The counterlockwise rotation in radians.</param>
        /// <param name="sides">The number of sides on the ellipse (a higher value yields more resolution).</param>
        public static void DrawEllipse(SpriteBatch batch, float semimajor_axis, float semiminor_axis, float angle_offset, Color c, int width, Vector2 middle,int sides=11)
        {
            var vectors = new ArrayList();
            vectors.Clear();
            var max = 2.0f * (float)Math.PI;
            var step = max / (float)sides;
            var h = middle.X;
            var k = middle.Y;

            for (var t = 0.0f; t < max; t += step)
            {
                // center point: (h,k); add as argument if you want (to circumvent modifying this.Position)
                // x = h + a*cos(t)  -- a is semimajor axis, b is semiminor axis
                // y = k + b*sin(t)
                vectors.Add(new Vector2((float)(h + semimajor_axis * Math.Cos(t)),
                                        (float)(k + semiminor_axis * Math.Sin(t))));
            }

            // then add the first vector again so it's a complete loop
            vectors.Add(new Vector2((float)(h + semimajor_axis * Math.Cos(step)),
                                    (float)(k + semiminor_axis * Math.Sin(step))));

            // now rotate it as necessary
            var m = Matrix.CreateRotationZ(angle_offset);
            for (var i = 0; i < vectors.Count; i++)
            {
                vectors[i] = Vector2.Transform((Vector2)vectors[i], m);
            }
            DrawVectors(batch, vectors, width, c);
        }

        public static void DrawVectors(SpriteBatch batch, ArrayList vectors, int width, Color c)
        {
            for (var i = 1; i < vectors.Count; i++)
            {
                var vector1 = (Vector2)vectors[i - 1];
                var vector2 = (Vector2)vectors[i];
                // stretch the pixel between the two vectors
                DrawLine(batch, width, c, vector1, vector2);
            }
        }
    }
}
