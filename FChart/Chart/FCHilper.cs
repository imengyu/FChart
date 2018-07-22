using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FChart.Chart
{
    public class FCHilper
    {
        private static Rectangle _LineRect = new Rectangle();

        public static Rectangle GetLineRect(Point p1, Point p2)
        {
            int x, y, w, h;
            if (p1.X > p2.X)
            {
                x = p2.X;
                w = p1.X - p2.X;
            }
            else
            {
                x = p1.X;
                w = p2.X - p1.X;
            }
            if (p1.Y > p2.Y)
            {
                y = p2.Y;
                h = p1.Y - p2.Y;
            }
            else
            {
                y = p1.Y;
                h = p2.Y - p1.Y;
            }
            _LineRect.X = x; _LineRect.Y = y;
            _LineRect.Width = w; _LineRect.Height = h;
            return _LineRect;
        }
        public static XmlNode FindChildNode(XmlNode parent, string name)
        {

            return null;
        }
    }
}
