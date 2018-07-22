using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FChart.Chart.Blocks
{
    /// <summary>
    /// 判断块
    /// </summary>
    public class FCSEBlock : FCBlock
    {
        public FCSEBlock(ChartDrawer chartDrawer) : base(chartDrawer)
        {
            borderPen = new Pen(Color.FromArgb(notEnteredAlpha, 0, 160, 233), 2);
            fillBrush = new SolidBrush(Color.FromArgb(200, 255, 255));
            BlockType = FCBlockType.BlockStartOrEnd;
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

        [Description("是开始还是结束")]
        [Category("功能")]
        public FCEndOrStartType Type { get; set; }

        private Pen borderPen { get; set; }
        private SolidBrush fillBrush { get; set; }

        public override Padding GetRedrawPadding()
        {
            return new Padding(6, 6, 10, 10);
        }

        protected override void OnDrawShape(Graphics g, Point moveOffest)
        {
            Rectangle rectangle = Rectangle;
            rectangle.X -= moveOffest.X;
            rectangle.Y -= moveOffest.Y;
            using (GraphicsPath path = CreateRoundedRectanglePath(rectangle, (int)(Size.Height / 2)))
            {
                g.FillPath(fillBrush, path);
                g.DrawPath(borderPen, path);
            }
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
            switch (Type)
            {
                case FCEndOrStartType.Start:
                    return "开始块";
                case FCEndOrStartType.End:
                    return "结束块";
                default:
                    return "开始或结束块";
            }
        }

        internal static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }
            // top left arc  
            path.AddArc(arc, 180, 90);
            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
    public enum FCEndOrStartType
    {
        None,
        Start,
        End,
    }
}
