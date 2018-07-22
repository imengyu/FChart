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
    /// 元件块基类
    /// </summary>
    public class FCBlock : FCObject
    {
        //
        // RealPos>Location +off
        // Location>RealPos -off
        //


        public FCBlock(ChartDrawer parent)
        {
            Parent = parent;
            brush = new SolidBrush(Color.Black);
            StringFormat = new StringFormat();
            StringFormat.Alignment = StringAlignment.Center;
            StringFormat.LineAlignment = StringAlignment.Center;
            Font = new Font("宋体", 10.5f);
            Text = "";
        }

        protected ChartDrawer Parent { get; private set; }

        [Category("引脚")]
        public FCPin PinTop { get; private set; }
        [Category("引脚")]
        public FCPin PinBottom { get; private set; }
        [Category("引脚")]
        public FCPin PinLeft { get; private set; }
        [Category("引脚")]
        public FCPin PinRight { get; private set; }

        [Browsable(false)]
        public bool Selected { get; set; }
        [Browsable(false)]
        public StringFormat StringFormat { get; private set; }
        [Description("文字颜色")]
        [Category("外观")]
        public Color ForeColor { get { return brush.Color; } set { brush.Color = value; } }
        [Description("文字字体")]
        [Category("外观")]
        public Font Font { get; set; }
        [Description("块的文字")]
        [Category("外观")]
        public string Text { get; set; }
        [Description("块的类型")]
        [Category("功能")]
        public FCBlockType BlockType { get; protected set; }
        [Browsable(false)]
        public bool Editing { get; private set; }

        private SolidBrush brush { get; set; }
        private int xl = 0;
        private int xc = 0;
        private int xr = 0;
        private int yt = 0;
        private int yc = 0;
        private int yb = 0;

        protected int notEnteredAlpha = 100;
        protected bool pinTop = false, pinBottom = false, pinLeft = false, pinRight = false;

        /// <summary>
        /// 添加引脚
        /// </summary>
        /// <param name="pin">引脚</param>
        /// <param name="direction">位置</param>
        public void AddPin(FCPin pin, FCPintDirection direction)
        {
            switch (direction)
            {
                case FCPintDirection.Left:
                    PinLeft = pin;
                    pinLeft = true;
                    break;
                case FCPintDirection.Top:
                    PinTop = pin;
                    pinTop = true;
                    break;
                case FCPintDirection.Right:
                    PinRight = pin;
                    pinRight = true;
                    break;
                case FCPintDirection.Bottom:
                    PinBottom = pin;
                    pinBottom = true;
                    break;
            }
        }
        /// <summary>
        /// 移除引脚
        /// </summary>
        /// <param name="direction">位置</param>
        public void RemovePin(FCPintDirection direction)
        {
            switch (direction)
            {
                case FCPintDirection.Left:
                    PinLeft = null;
                    pinLeft = false;
                    break;
                case FCPintDirection.Top:
                    PinTop = null;
                    pinTop = false;
                    break;
                case FCPintDirection.Right:
                    PinRight = null;
                    pinRight = false;
                    break;
                case FCPintDirection.Bottom:
                    pinBottom = false;
                    PinBottom = null;
                    break;
            }
        }

        public enum KeyEvent
        {
            None,
            Down,
            Up,
        }
        public enum MouseEvent
        {
            None,
            Down,
            Move,
            Up,
            Enter,
            Leave,
        }
        public override void OnDraw(Graphics g, Point moveOffest)
        {
            OnDrawShape(g, moveOffest);
            OnDrawPins(g, moveOffest);
            OnDrawText(g, moveOffest);
        }
        public virtual bool OnMouseEvent(MouseEvent e, MouseButtons btn, Point mousePoint, Point moveOffest)
        {
            Rectangle r = Rectangle;
            r.Location = LocationToRaelPos(moveOffest);
            if(IsSizeing || r.Contains(mousePoint))
            {
                switch (e)
                {
                    case MouseEvent.Move:
                        OnMouseMove(btn, mousePoint, moveOffest);
                        break;
                    case MouseEvent.Down:
                        OnMouseDown(btn, mousePoint, moveOffest);
                        break;
                    case MouseEvent.Up:
                        OnMouseUp(btn, mousePoint, moveOffest);
                        break;
                }
                return true;
            }
            return false;
        }
        public virtual void OnMouseEvent(MouseEvent e)
        {
            switch(e)
            {
                case MouseEvent.Enter:
                    OnMouseEnter();
                    break;
                case MouseEvent.Leave:
                    OnMouseLeave();
                    break;
            }
        }
        public virtual void OnKeyEvent(KeyEvent ev, KeyEventArgs e)
        {
            if (ev == KeyEvent.Down) OnKeyDown(e.KeyCode, e.Alt, e.Control, e.Shift);
            else if (ev == KeyEvent.Up) OnKeyUp(e.KeyCode, e.Alt, e.Control, e.Shift);
        }

        public void BeginEdit()
        {

            Editing = true;
        }
        public void EndEdit()
        {

            Editing = false;
        }




        protected virtual void OnKeyDown(Keys k, bool atl, bool ctl, bool shift)
        {

        }
        protected virtual void OnKeyUp(Keys k, bool atl, bool ctl, bool shift)
        {

        }
        protected virtual void OnMouseEnter()
        {
            MouseEntered = true;
        }
        protected virtual void OnMouseLeave()
        {
            MouseEntered = false;
        }
        protected virtual void OnMouseMove(MouseButtons btn, Point mousePoint, Point moveOffest)
        {
            if (Selected)
            {
                Point mousePointLocation = new Point();
                mousePointLocation.X = mousePoint.X + moveOffest.X;
                mousePointLocation.Y = mousePoint.Y + moveOffest.Y;
                if (Sizeing)
                {
                    //
                    resizeTargetPos.X = Left;
                    resizeTargetPos.Y = Top;
                    resizeTargetSize.Width = Width;
                    resizeTargetSize.Height = Height;
                    //获取调整大小类型
                    switch (SizeDre)
                    {
                        case SizeType.Left:
                            resizeTargetPos.X = mousePointLocation.X;
                            resizeTargetSize.Width = oldSizeRight - Left + ResizeWindth;
                            break;
                        case SizeType.Top:
                            resizeTargetPos.Y = mousePointLocation.Y;
                            resizeTargetSize.Height = oldSizeButtom - Top + ResizeWindth;
                            break;
                        case SizeType.Right:
                            resizeTargetSize.Width = mousePointLocation.X - Left + ResizeWindth;
                            break;
                        case SizeType.Buttom:
                            resizeTargetSize.Height = mousePointLocation.Y - Top + ResizeWindth;
                            break;
                        case SizeType.TopLeft:
                            resizeTargetPos.X = mousePointLocation.X;
                            resizeTargetSize.Width = oldSizeRight - Left + ResizeWindth;
                            resizeTargetPos.Y = mousePointLocation.Y;
                            resizeTargetSize.Height = oldSizeButtom - Top + ResizeWindth;
                            break;
                        case SizeType.TopRight:
                            resizeTargetPos.Y = mousePointLocation.Y;
                            resizeTargetSize.Height = oldSizeButtom - Top + ResizeWindth;
                            resizeTargetSize.Width = mousePointLocation.X + ResizeWindth;
                            break;
                        case SizeType.ButtomRight:
                            resizeTargetSize.Width = mousePointLocation.X - Left + ResizeWindth;
                            resizeTargetSize.Height = mousePointLocation.Y - Top + ResizeWindth;
                            break;
                        case SizeType.ButtomLeft:
                            resizeTargetSize.Height = mousePointLocation.Y - Top + ResizeWindth;
                            resizeTargetPos.X = mousePointLocation.X;
                            resizeTargetSize.Width = oldSizeRight - Left + ResizeWindth;
                            break;
                    }
                    if (!resizeTargetPos.IsEmpty)
                    {
                        resizeTargetPos = Parent.onMoveBlockCheckLineXY(resizeTargetPos, this);
                        Location = resizeTargetPos;
                    }
                    if (!resizeTargetSize.IsEmpty)
                    {
                        resizeTargetSize = Parent.onMoveBlockCheckLineWH(Location, resizeTargetSize, this);
                        Size = resizeTargetSize;
                    }
                    Invalidate();
                }
                else
                {
                    Point mousePointClient = new Point();
                    mousePointClient.X = mousePointLocation.X - Left;
                    mousePointClient.Y = mousePointLocation.Y - Top;

                    //ChartDrawer.ChartDrawerStatic.label1.Text = mousePointClient.ToString() + "\nmousePointLocation " + mousePointLocation.ToString() + "\nmoveOffest " + moveOffest.ToString();

                    //设置鼠标指针
                    if (mousePointClient.X > 0 && mousePointClient.X < SIZE_BORDER && mousePointClient.Y > SIZE_BORDER && mousePointClient.Y < Height - SIZE_BORDER)
                        Cursor = Cursors.SizeWE;
                    else if (mousePointClient.Y > 0 && mousePointClient.Y < SIZE_BORDER && mousePointClient.X > SIZE_BORDER && mousePointClient.X < Width - SIZE_BORDER)
                        Cursor = Cursors.SizeNS;
                    else if (mousePointClient.Y > Height - SIZE_BORDER && mousePointClient.Y < Height && mousePointClient.X > SIZE_BORDER && mousePointClient.X < Width - SIZE_BORDER)
                        Cursor = Cursors.SizeNS;
                    else if (mousePointClient.X > Width - SIZE_BORDER && mousePointClient.X < Width && mousePointClient.Y > SIZE_BORDER && mousePointClient.Y < Height - SIZE_BORDER)
                        Cursor = Cursors.SizeWE;
                    else if (mousePointClient.X > 0 && mousePointClient.X < SIZE_BORDER_2 && mousePointClient.Y < SIZE_BORDER_2 && mousePointClient.Y > 0)
                        Cursor = Cursors.SizeNWSE;
                    else if (mousePointClient.X > Width - SIZE_BORDER_2 && mousePointClient.X < Width && mousePointClient.Y < SIZE_BORDER_2 && mousePointClient.Y > 0)
                        Cursor.Current = Cursors.SizeNESW;
                    else if (mousePointClient.Y > Height - SIZE_BORDER_2 && mousePointClient.Y < Height && mousePointClient.X > Width - SIZE_BORDER_2 && mousePointClient.X < Width)
                        Cursor = Cursors.SizeNWSE;
                    else if (mousePointClient.X < SIZE_BORDER_2 && mousePointClient.X > 0 && mousePointClient.Y > Height - SIZE_BORDER_2 && mousePointClient.Y < Height)
                        Cursor = Cursors.SizeNESW;
                    else Cursor = Cursors.Arrow;
                }
            }
        }
        protected virtual void OnMouseUp(MouseButtons btn, Point mousePoint, Point moveOffest)
        {
            //是正在调整大小
            if (Sizeing)
            {
                //调整大小结束
                Sizeing = false;
                //应用调整

                resizeTargetPos = Point.Empty;
                resizeTargetSize = Size.Empty;

            }
        }
        protected virtual void OnMouseDown(MouseButtons btn, Point mousePoint, Point moveOffest)
        {
            if (btn == MouseButtons.Left && Selected)
                IsMouseInSizeing(mousePoint, moveOffest);
        }
        protected virtual void OnDrawShape(Graphics g, Point moveOffest)
        {
            base.OnDraw(g, moveOffest);
        }
        protected virtual void OnDrawText(Graphics g, Point moveOffest)
        {
            g.DrawString(Text, Font, brush, new Rectangle(LocationToRaelPos(moveOffest), Size), StringFormat);
        }
        protected virtual void OnDrawPins(Graphics g, Point moveOffest)
        {
            if (pinLeft) OnDrawPin(g, FCPintDirection.Left, moveOffest);
            if (pinTop) OnDrawPin(g, FCPintDirection.Top, moveOffest);
            if (pinRight) OnDrawPin(g, FCPintDirection.Right, moveOffest);
            if (pinBottom) OnDrawPin(g, FCPintDirection.Bottom, moveOffest);
        }
        protected virtual void OnDrawPin(Graphics g, FCPintDirection direction, Point moveOffest)
        {
            switch (direction)
            {
                case FCPintDirection.Left:
                    PinLeft.OnDraw(g, GetPinPosition(direction, PinLeft), moveOffest);
                    break;
                case FCPintDirection.Top:
                    PinTop.OnDraw(g, GetPinPosition(direction, PinTop), moveOffest);
                    break;
                case FCPintDirection.Right:
                    PinRight.OnDraw(g, GetPinPosition(direction, PinRight), moveOffest);
                    break;
                case FCPintDirection.Bottom:
                    PinBottom.OnDraw(g, GetPinPosition(direction, PinBottom), moveOffest);
                    break;
            }
        }

        public bool IsMouseInSizeing(Point mousePoint, Point moveOffest)
        {
            Point mousePointClient = new Point();
            mousePointClient.X = mousePoint.X + moveOffest.X - Left;
            mousePointClient.Y = mousePoint.Y + moveOffest.Y - Top;
            if (mousePointClient.X > 0 && mousePointClient.X < SIZE_BORDER && mousePointClient.Y > SIZE_BORDER && mousePointClient.Y < Height - SIZE_BORDER)
            {
                Sizeing = true;
                SizeDre = SizeType.Left;
                oldSizeRight = Right;
                oldSizeButtom = Bottom;
            }
            else if (mousePointClient.Y > 0 && mousePointClient.Y < SIZE_BORDER && mousePointClient.X > SIZE_BORDER && mousePointClient.X < Width - SIZE_BORDER)
            {
                Sizeing = true;
                SizeDre = SizeType.Top;
                oldSizeRight = Right;
                oldSizeButtom = Bottom;
            }
            else if (mousePointClient.Y > Height - SIZE_BORDER && mousePointClient.Y < Height && mousePointClient.X > SIZE_BORDER && mousePointClient.X < Width - SIZE_BORDER)
            {
                Sizeing = true;
                SizeDre = SizeType.Buttom;
            }
            else if (mousePointClient.X > Width - SIZE_BORDER && mousePointClient.X < Width && mousePointClient.Y > SIZE_BORDER && mousePointClient.Y < Height - SIZE_BORDER)
            {
                Sizeing = true;
                SizeDre = SizeType.Right;
            }
            else if (mousePointClient.X > 0 && mousePointClient.X < SIZE_BORDER && mousePointClient.Y < SIZE_BORDER && mousePointClient.Y > 0)
            {
                Sizeing = true;
                SizeDre = SizeType.TopLeft;
                oldSizeRight = Right;
                oldSizeButtom = Bottom;
            }
            else if (mousePointClient.X > Width - SIZE_BORDER && mousePointClient.X < Width && mousePointClient.Y < SIZE_BORDER && mousePointClient.Y > 0)
            {
                Sizeing = true;
                SizeDre = SizeType.TopRight;
                oldSizeRight = Right;
                oldSizeButtom = Bottom;
            }
            else if (mousePointClient.Y > Height - SIZE_BORDER_2 && mousePointClient.Y < Height && mousePointClient.X > Width - SIZE_BORDER_2 && mousePointClient.X < Width)
            {
                Sizeing = true;
                SizeDre = SizeType.ButtomRight;
            }
            else if (mousePointClient.X < SIZE_BORDER_2 && mousePointClient.X > 0 && mousePointClient.Y > Height - SIZE_BORDER_2 && mousePointClient.Y < Height)
            {
                Sizeing = true;
                SizeDre = SizeType.ButtomLeft;
                oldSizeRight = Right;
                oldSizeButtom = Bottom;
            }
            else
            {
                SizeDre = SizeType.None;
                Sizeing = false;
            }
            return Sizeing;
        }

        /// <summary>
        /// 获取 引脚绘画位置
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns></returns>
        protected virtual Point GetPinPosition(FCPintDirection direction, FCPin pin)
        {
            if (pin.DrawLocation == Point.Empty)
            {
                switch (direction)
                {
                    case FCPintDirection.Left:
                        pin.DrawLocation = new Point(Size.Width / 2 - PinLeft.Size.Width, Size.Height / 2 - PinLeft.Size.Height);
                        break;
                    case FCPintDirection.Top:
                        pin.DrawLocation = new Point(Size.Width / 2 - PinTop.Size.Width, Size.Height / 2 - PinTop.Size.Height);
                        break;
                    case FCPintDirection.Right:
                        pin.DrawLocation = new Point(Size.Width / 2 - PinRight.Size.Width, Size.Height / 2 - PinRight.Size.Height);
                        break;
                    case FCPintDirection.Bottom:
                        pin.DrawLocation = new Point(Size.Width / 2 - PinBottom.Size.Width, Size.Height / 2 - PinBottom.Size.Height);
                        break;
                }
            }
            return pin.DrawLocation;
        }
        /// <summary>
        /// 获取块类型名字
        /// </summary>
        /// <returns></returns>
        protected virtual string GetBlockTypeString() { return "块"; }

        public virtual Padding GetEditPadding()
        {
            return new Padding(2);
        }
        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Text != "") return GetBlockTypeString() + "：" + Text;
            return GetBlockTypeString() + "：" + Id;
        }
        /// <summary>
        /// 重绘
        /// </summary>
        public void Invalidate()
        {
            Parent.InvalidBlock(this);
        }
        /// <summary>
        /// 获取或设置鼠标指针
        /// </summary>
        public Cursor Cursor
        {
            get { return Parent.Cursor; }
            set { Parent.Cursor = value; }
        }

        #region ReSize

        //调整大小的类型
        public enum SizeType
        {
            None,
            TopLeft,
            Top,
            TopRight,
            Right,
            ButtomRight,
            Buttom,
            ButtomLeft,
            Left,
        }

        public bool IsSizeing { get { return Sizeing; } }

        //是否正在调整大小
        bool Sizeing = false;
        int oldSizeRight = 0;
        int oldSizeButtom = 0;
        Point resizeTargetPos = new Point();
        Size resizeTargetSize = new Size();
        SizeType SizeDre = SizeType.None;

        public SizeType SizeingType { get { return SizeDre; } }

        //调整大小的边距
        //鼠标距离边框<SIZE_BORDER大小就认为可以调节大小
        const int SIZE_BORDER = 5;
        const int SIZE_BORDER_2 = 10;
        const int ResizeWindth = 2;


        #endregion

        protected override void OnLocationChanged()
        {
            UpdateAllCheckLines(true);
            base.OnLocationChanged();
        }
        protected override void OnSizeChanged()
        {
            UpdateAllCheckLines(false);
            base.OnSizeChanged();
        }

        private void UpdateAllCheckLines(bool xy)
        {
            /*
            if (xy)
            {
                if (Parent.checkLineXL.Contains(xl)) Parent.checkLineXL.Remove(xl);
                if (Parent.checkLineYT.Contains(yt)) Parent.checkLineYT.Remove(yt);

                xl = Left;
                yt = Top;

                if (!Parent.checkLineXL.Contains(xl)) Parent.checkLineXL.Add(xl);
                if (!Parent.checkLineYT.Contains(yt)) Parent.checkLineYT.Add(yt);
            }

            if (Parent.checkLineXR.Contains(xr)) Parent.checkLineXR.Remove(xr);
            if (Parent.checkLineYB.Contains(yb)) Parent.checkLineYB.Remove(yb);
            if (Parent.checkLineXC.Contains(xc)) Parent.checkLineXC.Remove(xc);
            if (Parent.checkLineYC.Contains(yc)) Parent.checkLineYC.Remove(yc);

            xr = Left + Width;
            yb = Top + Height;
            xc = Left + Width / 2;
            yc = Top + Height / 2;

            if (!Parent.checkLineXR.Contains(xr)) Parent.checkLineXR.Add(xr);
            if (!Parent.checkLineYB.Contains(yb)) Parent.checkLineYB.Add(yb);
            if (!Parent.checkLineXC.Contains(xc)) Parent.checkLineXC.Add(xc);
            if (!Parent.checkLineYC.Contains(yc)) Parent.checkLineYC.Add(yc);
            */
        }
    }

    public enum FCPintDirection
    {
        Left,
        Top,
        Right,
        Bottom,
    }
}
