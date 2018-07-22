using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FChart.Chart.Blocks
{
    /// <summary>
    /// 判断块
    /// </summary>
    public class FCCABlock : FCBlock
    {
        public FCCABlock(ChartDrawer chartDrawer) : base(chartDrawer)
        {
            BlockType = FCBlockType.BlockCase;
            borderPen = new Pen(Color.FromArgb(notEnteredAlpha, 255, 128, 0), 2);
            fillBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            redrawPadding = new Padding(6, 2, 10, 2);
        }

        [Description("块边框颜色")]
        [Category("外观")]
        public Color BorderColor
        {
            get
            {
                return borderPen.Color;
            }
            set
            {
                borderPen.Color = value;
            }
        }
        [Description("块背景颜色")]
        [Category("外观")]
        public Color BackColor
        {
            get
            {
                return fillBrush.Color;
            }
            set
            {
                fillBrush.Color = value;
            }
        }

        private Padding redrawPadding;
        private Point[] points = new Point[4];
        private Point[] realpoints = new Point[4];
        private Pen borderPen { get; set; }
        private SolidBrush fillBrush { get; set; }

        protected override void OnDrawShape(Graphics g, Point moveOffest)
        {
            for (int i = 0; i < 4; i++)
            {
                realpoints[i].X = points[i].X - moveOffest.X;
                realpoints[i].Y = points[i].Y - moveOffest.Y;
            }

            g.FillPolygon(fillBrush, realpoints);
            g.DrawPolygon(borderPen, realpoints);
        }
        protected override void OnLocationChanged()
        {
            base.OnLocationChanged();
            points[0] = new Point(Location.X, Location.Y + Size.Height / 2);
            points[1] = new Point(Location.X + Size.Width / 2, Location.Y);
            points[2] = new Point(Location.X + Size.Width, Location.Y + Size.Height / 2);
            points[3] = new Point(Location.X + Size.Width / 2, Location.Y + Size.Height);
        }
        protected override void OnMouseEnter()
        {
            base.OnMouseEnter();
            BorderColor = Color.FromArgb(255, BorderColor);
        }
        protected override void OnMouseLeave()
        {
            base.OnMouseLeave();
            BorderColor = Color.FromArgb(notEnteredAlpha, BorderColor);
        }

        protected override string GetBlockTypeString()
        {
            return "判断块";
        }
        public override Padding GetRedrawPadding()
        {
            return redrawPadding;
        }
    }
}
