using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FChart.Chart
{
    /// <summary>
    /// 流程图元件基类
    /// </summary>
    public class FCObject
    {
        public FCObject()
        {

        }

        protected Size _Size = new Size();
        protected Point _Location = new Point();
        private Point _RealLocation = new Point();
        private Rectangle _Rectangle = new Rectangle();
        private Rectangle _RealRectangle = new Rectangle();

        [Browsable(false)]
        public bool MouseEntered { get; set; }
        [Browsable(false)]
        public bool Changed { get; set; }
        [Description("文字颜色")]
        [Category("功能")]
        /// <summary>
        /// id
        /// </summary>
        public long Id { get; set; }
        [Description("块的大小")]
        [Category("位置")]
        /// <summary>
        /// 元件大小
        /// </summary>
        public Size Size
        {
            get { return _Size; } set
            {
                _Size = value;
                OnSizeChanged();
            }
        }
        [Description("块的位置坐标")]
        [Category("位置")]
        /// <summary>
        /// 元件位置
        /// </summary>
        public Point Location
        {
            get { return _Location; }
            set { _Location = value; OnLocationChanged(); }
        }
        [Description("块的宽度")]
        [Category("大小")]
        public int Width
        {
            get { return _Size.Width; }
            set { SetWidth(value); }
        }
        [Description("块的高度")]
        [Category("大小")]
        public int Height
        {
            get { return _Size.Height; }
            set { SetHeight(value); }
        }

        [Browsable(false)]
        public int Right
        {
            get { return _Rectangle.Right; }
        }
        [Browsable(false)]
        public int Bottom
        {
            get { return _Rectangle.Bottom; }
        }

        [Description("块的 X 坐标")]
        [Category("位置")]
        public int Left
        {
            get { return _Location.X; }
            set { SetX(value); }
        }
        [Description("块的 Y 坐标")]
        [Category("位置")]
        public int Top
        {
            get { return _Location.Y; }
            set { SetY(value); }
        }

        public void SetWidth(int width)
        {
            _Size.Width = width;
        }
        public void SetHeight(int height)
        {
            _Size.Height = height;
        }
        public void SetX(int x)
        {
            _Location.X = x;
        }
        public void SetY(int y)
        {
            _Location.Y = y;
        }
        [Description("块以矩形表示的位置和大小")]
        [Category("位置")]
        /// <summary>
        /// 元件矩形
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                _Rectangle.Size = Size;
                _Rectangle.Location = _Location;
                return _Rectangle;
            }
        }
        /// <summary>
        /// 元件真实矩形
        /// </summary>
        public Rectangle GetRealRectangle(Point moveOffest, bool inflacted=false)
        {
            _RealRectangle.Size = _Size;
            _RealRectangle.Location = LocationToRaelPos(moveOffest);
            if(inflacted)
            {
                Padding p = GetRedrawPadding();
                if (p.All != 0)
                {
                    _Rectangle.X -= p.Left; _Rectangle.Y -= p.Top;
                    _Rectangle.Width += p.Left + p.Right;
                    _Rectangle.Height += p.Top + p.Bottom;
                }
            }
            return _RealRectangle;
        }
        public Rectangle GetInflactedRectangle()
        {
            Padding p = GetRedrawPadding();
            _RealRectangle.Size = _Size;
            _RealRectangle.Location = _Location;
            if (p.All != 0)
            {
                _Rectangle.X -= p.Left; _Rectangle.Y -= p.Top;
                _Rectangle.Width += p.Left + p.Right;
                _Rectangle.Height += p.Top + p.Bottom;
            }
            return _RealRectangle;
        }
        /// <summary>
        /// 元件真实位置
        /// </summary>
        /// <param name="moveOffest"></param>
        /// <returns></returns>
        public Point LocationToRaelPos(Point moveOffest)
        {
            _RealLocation.X = Location.X - moveOffest.X;
            _RealLocation.Y = Location.Y - moveOffest.Y;
            return _RealLocation; 
        }

        public virtual void OnDraw(Graphics g, Point moveOffest)
        {
            if (MouseEntered)
                g.DrawRectangle(Pens.Black, new Rectangle(LocationToRaelPos(moveOffest), Size));
            else g.DrawRectangle(Pens.Gray, new Rectangle(LocationToRaelPos(moveOffest), Size));
        }

        public virtual Padding GetRedrawPadding()
        {
            return new Padding(2);
        }

        protected virtual void OnSizeChanged() { }
        protected virtual void OnLocationChanged() { }
    }
}
