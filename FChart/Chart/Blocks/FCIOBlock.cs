using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FChart.Chart.Blocks
{
    /// <summary>
    /// 输入输出块
    /// </summary>
    public class FCIOBlock : FCBlock
    {
        public FCIOBlock(ChartDrawer chartDrawer) : base(chartDrawer)
        {
            borderPen = new Pen(Color.FromArgb(notEnteredAlpha, 30, 160, 125), 2);
            fillBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            BlockType = FCBlockType.BlockIO;
            redrawPadding = new Padding(16, 2, 16, 2);
            OnLocationChanged();
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
        [Browsable(false)]
        public bool IsIn { get; set; }

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
            int off = Size.Height / 10;
            redrawPadding.Left = off + 2;
            redrawPadding.Right = off + 2;
            points[0] = new Point(Location.X + off, Location.Y);
            points[1] = new Point(Location.X + off + Size.Width, Location.Y);
            points[3] = new Point(Location.X - off, Location.Y + Size.Height);
            points[2] = new Point(Location.X + Size.Width - off, Location.Y + Size.Height);
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
            return IsIn ? "输入块" : "输出块";
        }
        public override Padding GetRedrawPadding()
        {
            return redrawPadding;
        }
    }
}
