using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChart.Chart
{
    /// <summary>
    /// 流程线
    /// </summary>
    public class FCLine
    {
        public FCLine()
        {
            Pen = new Pen(Color.DodgerBlue);
            Vertexes = new List<Point>();
        }

        /// <summary>
        /// 开始引脚
        /// </summary>
        public FCPin StartPin { get; set;}
        /// <summary>
        /// 结束引脚
        /// </summary>
        public FCPin EndPin { get; set; }

        /// <summary>
        /// 流程线顶点
        /// </summary>
        public List<Point> Vertexes { get; private set; }
        public Pen Pen { get; private set; }
        public Color Color
        {
            get
            {
                return Pen.Color;
            }
            set
            {
                Pen.Color = value;
            }
        }

        private Point _RealLocation = new Point();
        private Point LocationToRaelPos(Point pos, Point moveOffest)
        {
            _RealLocation.X = pos.X - moveOffest.X;
            _RealLocation.Y = pos.Y - moveOffest.Y;
            return _RealLocation;
        }

        public virtual void OnDraw(Graphics g, Point moveOffest, Rectangle refeshRc)
        {
            if (StartPin != null && EndPin != null)
                if (Vertexes.Count == 0)
                    g.DrawLine(Pen, StartPin.LocationToRaelPos(moveOffest), EndPin.LocationToRaelPos(moveOffest));
                else
                {
                    Point currentDrawPt1 = StartPin.Location;
                    Point currentDrawPt2 = Point.Empty;
                    Rectangle currentDrawRc = Rectangle.Empty;
                    for (int i = 0; i < Vertexes.Count; i++)
                    {
                        currentDrawPt2 = Vertexes[i];
                        currentDrawRc = FCHilper.GetLineRect(currentDrawPt1, currentDrawPt2);
                        if (currentDrawRc.IntersectsWith(refeshRc))
                            g.DrawLine(Pen, LocationToRaelPos(currentDrawPt1, moveOffest), LocationToRaelPos(currentDrawPt2, moveOffest));
                        currentDrawPt1 = Vertexes[i];
                    }
                    currentDrawPt2 = EndPin.Location;
                    currentDrawRc = FCHilper.GetLineRect(currentDrawPt1, currentDrawPt2);
                    if (currentDrawRc.IntersectsWith(refeshRc))
                        g.DrawLine(Pen, LocationToRaelPos(currentDrawPt1, moveOffest), LocationToRaelPos(currentDrawPt2, moveOffest));
                }
        }
    }
}
