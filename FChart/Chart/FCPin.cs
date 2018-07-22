using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChart.Chart
{
    /// <summary>
    /// 元件块引脚
    /// </summary>
    public class FCPin : FCObject
    {
        public FCPin(FCBlock parent)
        {
            DrawLocation = Point.Empty;
            Parent = parent;
        }

        public string Text { get; set; }
        public Point DrawLocation { get; set; }
        public FCBlock Parent { get; private set; }

        public virtual void OnDraw(Graphics g, Point pos, Point moveOffest)
        {
            _Location.X = Parent.Location.X + pos.X - moveOffest.X;
            _Location.Y = Parent.Location.Y + pos.Y - moveOffest.Y;
            g.DrawEllipse(Pens.OrangeRed, new Rectangle(LocationToRaelPos(moveOffest), Size));
        }
    }
}
