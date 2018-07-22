using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChart.Chart.Blocks
{
    public class FCPROCBlock : FCBlock
    {
        public FCPROCBlock(ChartDrawer chartDrawer) : base(chartDrawer)
        {
            borderPen = new Pen(Color.FromArgb(notEnteredAlpha, 106, 0, 95), 2);
            fillBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            StringFormat.Trimming = StringTrimming.EllipsisCharacter;
            BlockType = FCBlockType.BlockProcess;
        }

        private Pen borderPen { get; set; }
        private SolidBrush fillBrush { get; set; }

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
            return "处理块";
        }
        protected override void OnDrawShape(Graphics g, Point moveOffest)
        {
            Rectangle rectangle = Rectangle;
            rectangle.X -= moveOffest.X;
            rectangle.Y -= moveOffest.Y;
            g.FillRectangle(fillBrush, rectangle);
            g.DrawRectangle(borderPen, rectangle);
        }
    }
}
