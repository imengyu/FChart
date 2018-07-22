using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using FChart.Chart;
using FChart.Chart.Blocks;

namespace FChart
{
    public partial class ChartDrawer : UserControl
    {
        public ChartDrawer()
        {
            InitializeComponent();

            blocks.OnBlockAdd += Blocks_OnBlockAdd;
            blocks.OnBlockRemove += Blocks_OnBlockRemove;

            ctlTimer.Interval = 100;
            ctlTimer.Tick += CtlTimer_Tick;
            ctlTimer.Start();
        }

        private void CtlTimer_Tick(object sender, EventArgs e)
        {
            if(!ctlCan) ctlCan = true;
        }

        private void Blocks_OnBlockRemove(object sender, FCBlock b)
        {
            BlockRemoved?.Invoke(this, b);
        }
        private void Blocks_OnBlockAdd(object sender, FCBlock b)
        {
            if (b.Id == 0)
                b.Id = GlobalId;
            BlockAdded?.Invoke(this, b);
        }

        private void ChartDrawer_Load(object sender, EventArgs e)
        {
            RulerUnitsX = 50;
            RulerUnitsY = 20;
            dragPoint = Properties.Resources.drag_point;
        }

        /// <summary>
        /// X轴标尺单位
        /// </summary>
        public int RulerUnitsX { get; set; }
        /// <summary>
        /// Y轴标尺单位
        /// </summary>
        public int RulerUnitsY { get; set; }
        /// <summary>
        /// 选中的块
        /// </summary>
        public FCBlock SelectedBlock { get { return selectedBlock; } }
        /// <summary>
        /// 鼠标在其中的块
        /// </summary>
        public FCBlock EnteredBlock { get { return enteredBlock; } }
        /// <summary>
        /// 所有线条
        /// </summary>
        private List<FCLine> Lines { get { return lines; } }
        /// <summary>
        /// 所有块
        /// </summary>
        private FCBlocksCollection Blocks { get { return blocks; } }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public long GlobalId
        {
            get
            {
                globalId++;
                return globalId;
            }
        }

        public bool changed = false;
        private bool ctlCan = true;
        private Timer ctlTimer = new Timer();
        private long globalId = 0;
        private Image dragPoint = null;
        private Size puttingSizeOld = new Size();
        private Size puttingSize = new Size();
        private Point puttingStartPos = new Point();
        private Point puttingStartMousePos = new Point();
        private FCBlockType puttingBlockType = FCBlockType.BlockNone;
        private bool puttingBlockDowned = false;
        private bool puttingBlock = false;
        private bool moveingBlock = false;
        private bool moveing = false;
        private bool canMoveingBlock = false;
        private Point downMovePos = new Point();
        private bool opened = false;
        private Point offest = new Point(0, 0);
        private Point lastMousePos = new Point(0, 0);
        private List<CheckLine> drawCheckLineX = new List<CheckLine>();
        private List<CheckLine> drawCheckLineY = new List<CheckLine>();
        private List<FCLine> lines = new List<FCLine>();
        private FCBlocksCollection blocks = new FCBlocksCollection();
        private Point _RealLocation = new Point();
        private FCBlock selectedBlock = null;
        private FCBlock enteredBlock = null;
        private int drawCheckLineMaxY = 0;
        private int drawCheckLineMinY = 0;
        private int drawCheckLineMaxX = 0;
        private int drawCheckLineMinX = 0;



        /*public List<int> checkLineXL = new List<int>();
        public List<int> checkLineXC = new List<int>();
        public List<int> checkLineXR = new List<int>();
        public List<int> checkLineYT = new List<int>();
        public List<int> checkLineYC = new List<int>();
        public List<int> checkLineYB = new List<int>();*/

        #region Put&Open

        /// <summary>
        /// 开始放置块
        /// </summary>
        /// <param name="fCBlockType">需要放置块的类型</param>
        public void SetPutBlock(FCBlockType fCBlockType)
        {
            puttingBlockType = fCBlockType;
            puttingBlock = true;
            Cursor = Cursors.Cross;
        }
        /// <summary>
        /// 停止放置块
        /// </summary>
        public void SetNoPutBlock()
        {
            EndPutting?.Invoke(this, null);
            puttingBlockType = FCBlockType.BlockNone;
            puttingBlock = false;
            Cursor = Cursors.Arrow;
        }
        /// <summary>
        /// 设置已打开模式
        /// </summary>
        public void SetOpened()
        {
            BackColor = Color.White;
            opened = true;
        }
        /// <summary>
        /// 关闭打开的文件
        /// </summary>
        public void CloseAll()
        {
            BackColor = Color.Gray;
            opened = false;
            lines.Clear();
            blocks.Clear();
            selectedBlock = null;
            offest = new Point(0, 0);
            Invalidate();
        }

        #endregion

        private Point LocationToRealPos(Point pos, Point moveOffest)
        {
            _RealLocation.X = pos.X - moveOffest.X;
            _RealLocation.Y = pos.Y - moveOffest.Y;
            return _RealLocation;
        }
        private Point RealPosToLocation(Point pos, Point moveOffest)
        {
            pos.X += moveOffest.X;
            pos.Y += moveOffest.Y;
            return pos;
        }

        #region Draw

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (opened)
            {
                //e.Graphics.DrawRectangle(Pens.AliceBlue, e.ClipRectangle);
                DrawLines(e.Graphics, e.ClipRectangle);
                DrawBlocks(e.Graphics, e.ClipRectangle);
                DrawGrid(e.Graphics, e.ClipRectangle);
                DrawSelectedBlock(e.Graphics, e.ClipRectangle);
                DrawPutting(e.Graphics);
                DrawCheckLine(e.Graphics, e.ClipRectangle);
            }
        }

        private void DrawPutting(Graphics g)
        {
            if (puttingBlock)
                g.DrawRectangle(Pens.Gray, new Rectangle(puttingStartMousePos, new Size(puttingSize.Width - 1, puttingSize.Height - 1)));
        }
        private void DrawSelectedBlock(Graphics g, Rectangle refrc)
        {
            if (selectedBlock != null)
            {
                Rectangle rectangle = selectedBlock.Rectangle;
                rectangle.Location = LocationToRealPos(rectangle.Location, offest);
                rectangle.X -= 2; rectangle.Y -= 2;
                rectangle.Height += 2; rectangle.Width += 2;
                if (rectangle.IntersectsWith(refrc))
                {
                    rectangle.X += 2; rectangle.Y += 2;
                    rectangle.Height--; rectangle.Width--;
                    g.DrawRectangle(Pens.DodgerBlue, rectangle);
                    g.DrawImage(dragPoint, rectangle.X - 6, rectangle.Y - 6, 12, 12);//↖
                    g.DrawImage(dragPoint, rectangle.X + rectangle.Width / 2 - 6, rectangle.Y - 6, 12, 12);//↑
                    g.DrawImage(dragPoint, rectangle.X + rectangle.Width - 6, rectangle.Y - 6, 12, 12);//↗
                    g.DrawImage(dragPoint, rectangle.X + rectangle.Width - 6, rectangle.Y + rectangle.Height / 2 - 6, 12, 12);//→
                    g.DrawImage(dragPoint, rectangle.X + rectangle.Width - 6, rectangle.Y + rectangle.Height - 6, 12, 12);//↘
                    g.DrawImage(dragPoint, rectangle.X + rectangle.Width / 2 - 6, rectangle.Y + rectangle.Height - 6, 12, 12);//↓
                    g.DrawImage(dragPoint, rectangle.X - 6, rectangle.Y + rectangle.Height - 6, 12, 12);//↙
                    g.DrawImage(dragPoint, rectangle.X - 6, rectangle.Y + rectangle.Height / 2 - 6, 12, 12);//←
                }
            }
        }
        private void DrawLines(Graphics g, Rectangle refrc)
        {
            foreach (FCLine f in lines)
                f.OnDraw(g, offest, refrc);
        }
        private void DrawBlocks(Graphics g, Rectangle refrc)
        {
            Rectangle rectangle = new Rectangle();
            foreach (FCBlock b in blocks)
            {
                rectangle = b.GetRealRectangle(offest);
                if (rectangle.IntersectsWith(refrc))
                    b.OnDraw(g, offest);
            }
        }
        private void DrawGrid(Graphics g, Rectangle refrc)
        {
            if (refrc.Top < 20)
                for (int x = -offest.X, max = Width + offest.X, n = 0; x <= max; x += RulerUnitsX, n += RulerUnitsX)
                {
                    g.DrawLine(Pens.Gray, x, 0, x, 10);
                    if (x != 0) g.DrawString(n.ToString(), Font, Brushes.Gray, x, 3);
                }
            if (refrc.Left <= 15)
                for (int y = -offest.Y, max = Height + offest.Y, n = 0; y <= max; y += RulerUnitsY, n += RulerUnitsY)
                {
                    g.DrawLine(Pens.Gray, 0, y, 10, y);
                    if (y != 0) g.DrawString(n.ToString(), Font, Brushes.Gray, 3, y + 2);
                }
        }
        private void DrawCheckLine(Graphics g, Rectangle refrc)
        {
            if (drawCheckLineX.Count > 0)
            {
                foreach (CheckLine i in drawCheckLineX)
                    g.DrawLine(Pens.Orange, i.p, i.s, i.p, i.e);
                drawCheckLineX.Clear();
            }
            if (drawCheckLineY.Count > 0)
            {
                foreach (CheckLine i in drawCheckLineY)
                    g.DrawLine(Pens.Orange, i.s, i.p, i.e, i.p);
                drawCheckLineY.Clear();
            }
        }

        #endregion

        public bool IsBlockInRight(FCBlock b1, FCBlock b2)
        {
            return b1.Left > b2.Left;
        }
        public bool IsBlockAbove(FCBlock b1, FCBlock b2)
        {
            return b1.Top > b2.Top;
        }
        /// <summary>
        /// 寻找指定位置的块
        /// </summary>
        /// <param name="pos">指定位置</param>
        /// <param name="checkSizeing">是否检测正在调整大小的块</param>
        /// <returns></returns>
        public FCBlock FindBlockInPoint(Point pos, bool checkSizeing = false)
        {
            Point point = RealPosToLocation(pos, offest);
            Rectangle r = new Rectangle();
            FCBlock rs = null;
            if (checkSizeing)
            {
                foreach (FCBlock b in blocks)
                {
                    r = b.GetInflactedRectangle();
                    if (b == selectedBlock) r.X -= 10; r.Y -= 10; r.Width += 20; r.Height += 20;
                    if (b.IsSizeing || r.Contains(point))
                    {
                        rs = b;
                        break;
                    }
                }
            }
            else
            {
                foreach (FCBlock b in blocks)
                {
                    r = b.GetInflactedRectangle();
                    if (b == selectedBlock) r.X -= 10; r.Y -= 10; r.Width += 20; r.Height += 20;
                    if (r.Contains(point))
                    {
                        rs = b;
                        break;
                    }
                }
            }
            return rs;
        }
        /// <summary>
        /// 使块进入选中状态
        /// </summary>
        /// <param name="b"></param>
        public void SelectBlock(FCBlock b)
        {
            if (b == null)
            {
                if (selectedBlock != null)
                {
                    if (selectedBlock.Editing)
                        selectedBlock.EndEdit();
                    selectedBlock.Selected = false;
                    InvalidBlock(selectedBlock);
                    selectedBlock = null;
                }
            }
            else if (blocks.Contains(b))
            {
                if (selectedBlock != null)
                {
                    if (selectedBlock.Editing)
                        selectedBlock.EndEdit();
                    selectedBlock.Selected = false;
                    InvalidBlock(selectedBlock);
                }
                selectedBlock = b;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                if (selectedBlock != null)
                {
                    selectedBlock.Selected = true;
                    InvalidBlock(selectedBlock);
                }
            }
        }
        /// <summary>
        /// 刷新块
        /// </summary>
        /// <param name="b"></param>
        public void InvalidBlock(FCBlock b)
        {
            if (b != null)
            {
                Rectangle r = b.GetRealRectangle(offest, true);
                r.X -= 2; r.Y -= 2; r.Width += 4; r.Height += 4;
                if (b == selectedBlock) { r.X -= 10; r.Y -= 10; r.Width += 20; r.Height += 20; }
                if (drawCheckLineMinX != 0 && r.X > drawCheckLineMinX)
                {
                    int outy = r.Y - drawCheckLineMinX;
                    r.X = drawCheckLineMinX;
                    r.Width += outy;
                }
                if (drawCheckLineMinY != 0 && r.Y > drawCheckLineMinY)
                {
                    int outy = r.Y - drawCheckLineMinY;
                    r.Y = drawCheckLineMinY;
                    r.Height += outy;
                }
                if (drawCheckLineMaxX != 0 && r.Right < drawCheckLineMaxX)
                {
                    int outx = drawCheckLineMaxX - r.Right;
                    r.Width += outx;
                }
                if (drawCheckLineMaxY != 0 && r.Right < drawCheckLineMaxY)
                {
                    int outx = drawCheckLineMaxY - r.Right;
                    r.Height += outx;
                }

                drawCheckLineMinY = 0;
                Invalidate(r);
            }
        }
        /// <summary>
        /// 使块进入编辑模式
        /// </summary>
        /// <param name="b"></param>
        public void EditBlock(FCBlock b)
        {
            if (b != null && !b.Editing)
                b.BeginEdit();
        }
        /// <summary>
        /// 移动块
        /// </summary>
        /// <param name="b"></param>
        /// <param name="newpos"></param>
        public void MoveBlock(FCBlock b, Point newpos)
        {
            if (b != null && !b.Editing)
            {
                b.Location = newpos;
                InvalidBlock(b);
            }
        }

        private void onChanged()
        {
            if (!changed) changed = true;
        }
        private void onDeleteBlock()
        {
            if (selectedBlock != null)
            {
                onChanged();
                blocks.Remove(selectedBlock);
                FCBlock b = selectedBlock;
                BlockRemoved?.Invoke(this, selectedBlock);
                selectedBlock.Selected = false;
                selectedBlock = null;
                InvalidBlock(b);
            }
        }
        private void onMoveBlock(MouseEventArgs e)
        {
            onChanged();
            if (!selectedBlock.Changed) selectedBlock.Changed = true;

            Rectangle r = selectedBlock.GetInflactedRectangle();
            Point p = RealPosToLocation(e.Location, offest), lastPos = LocationToRealPos(selectedBlock.Location, offest);
            p = new Point(p.X - downMovePos.X, p.Y - downMovePos.Y);

            int xl = p.X, xc = p.X + r.Width / 2, xr = p.X + r.Width;
            int yt = p.Y, yc = p.Y + r.Height / 2, yb = p.Y + r.Height;
            
            p = onMoveBlockCheckLine(xl, xc, xr, yt, yc, yb, r.Size, selectedBlock,  p);

            selectedBlock.Location = p;

            Rectangle refeshrc = selectedBlock.GetRealRectangle(offest, true);
            if (lastPos.X > refeshrc.X)
                refeshrc.Width += lastPos.X - refeshrc.X;
            else
            {
                refeshrc.X = lastPos.X;
                refeshrc.Width += refeshrc.X - lastPos.X;
            }
            if (lastPos.Y > refeshrc.Y)
                refeshrc.Height += lastPos.Y - refeshrc.Y;
            else
            {
                refeshrc.Y = lastPos.Y;
                refeshrc.Height += refeshrc.Y - lastPos.Y;
            }

            refeshrc.X -= 10; refeshrc.Y -= 10; refeshrc.Width += 20; refeshrc.Height += 20;

            label1.Text = e.Location.ToString() + "\nlastPos : " + lastPos.ToString() + "\nnewPos :" + p.ToString();

            Invalidate(refeshrc);
        }
        private bool onSelectBlock(MouseEventArgs e)
        {
            bool rs = false;
            FCBlock b = FindBlockInPoint(e.Location, true);
            if (b == null)
            {
                if (selectedBlock != null)
                {
                    SelectBlock(null);
                    SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                }

                lastMousePos = e.Location;
                moveing = true;
                Cursor = Cursors.SizeAll;
            }
            else
            {
                if (selectedBlock != b)
                {
                    SelectBlock(b);
                    rs = true;
                    moveing = false;
                    SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    //if (selectedBlock != null)
                    //{
                    //    if (selectedBlock.IsMouseInSizeing(e.Location, offest))
                    //    {
                    //        rs = true;
                    //        return rs;
                    //    }
                    //}
                    if (selectedBlock == null)
                        moveing = true;
                }
                
            }
            return rs;
        }
        private void onTestMouseIn(MouseEventArgs e)
        {
            FCBlock b = FindBlockInPoint(e.Location, true);
            if (b == null)
            {
                if (enteredBlock != null)
                {
                    enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Leave);
                    InvalidBlock(enteredBlock);
                    enteredBlock = null;
                    Cursor = Cursors.Arrow;
                }
            }
            else
            {
                if (enteredBlock != null)
                {
                    enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Leave);
                    InvalidBlock(enteredBlock);
                    Cursor = Cursors.Arrow;
                }
                enteredBlock = b;
                enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Enter);
                InvalidBlock(enteredBlock);
            }
        }
        private void onPutBlock()
        {
            onChanged();
            switch (puttingBlockType)
            {
                case FCBlockType.BlockProcess:
                    {
                        FCPROCBlock b = new FCPROCBlock(this);
                        b.Size = puttingSize;
                        b.Location = puttingStartPos;
                        b.Changed = true;
                        blocks.Add(b);
                        SelectBlock(b);
                        break;
                    }
                case FCBlockType.BlockIO:
                    {
                        FCIOBlock b = new FCIOBlock(this);
                        b.Size = puttingSize;
                        b.Location = puttingStartPos;
                        blocks.Add(b);
                        b.Changed = true;
                        SelectBlock(b);
                        break;
                    }
                case FCBlockType.BlockStartOrEnd:
                    {
                        FCSEBlock b = new FCSEBlock(this);
                        b.Size = puttingSize;
                        b.Location = puttingStartPos;
                        b.Changed = true;
                        blocks.Add(b);
                        SelectBlock(b);
                        break;
                    }
                case FCBlockType.BlockCase:
                    {
                        FCCABlock b = new FCCABlock(this);
                        b.Size = puttingSize;
                        b.Location = puttingStartPos;
                        b.Changed = true;
                        blocks.Add(b);
                        SelectBlock(b);
                        break;
                    }
            }

            SetNoPutBlock();
        }

        #region CheckLines

        public struct CheckLine
        {
            public CheckLine(int p, int s, int e)
            {
                this.p = p;
                this.s = s;
                this.e = e;
                if (s > e)
                {
                    int e1 = this.e;
                    this.e = s;
                    this.s = e1;
                }
            }

            public int p;
            public int s;
            public int e;
        }

        public void AddCheckLineX(CheckLine i)
        {
            if (i.s < drawCheckLineMinY) drawCheckLineMinY = i.s;
            if (i.e > drawCheckLineMaxY) drawCheckLineMaxY = i.e;
            drawCheckLineX.Add(i);
        }
        public void AddCheckLineY(CheckLine i)
        {
            if (i.s < drawCheckLineMinX) drawCheckLineMinX = i.s;
            if (i.e > drawCheckLineMaxX) drawCheckLineMaxX = i.e;
            drawCheckLineY.Add(i);
        }

        private Point onMoveBlockCheckLine(int xl, int xc, int xr, int yt, int yc, int yb, Size sz, FCBlock block, Point p)
        {
            const int CHECKLINE_OFFEST = 2;
            foreach (FCBlock b in blocks)
            {
                if (b != block)
                {
                    int w = 0;

                    //x
                    if (checkXL(b, p, block))
                        p.X = b.Left;

                    w = b.Left + b.Width;
                    if (Math.Abs(w - xr) <= CHECKLINE_OFFEST)
                    {
                        p.X = w - block.Width;
                        if (IsBlockAbove(b, block))
                            AddCheckLineX(new CheckLine(w, p.Y, b.Bottom));
                        else AddCheckLineX(new CheckLine(w, b.Top, block.Bottom));
                    }
                    w = b.Left + b.Width / 2;
                    if (Math.Abs(w - xc) <= CHECKLINE_OFFEST)
                    {
                        p.X = w - block.Width / 2;
                        if (IsBlockAbove(b, block))
                            AddCheckLineX(new CheckLine(w, p.Y, b.Bottom));
                        else AddCheckLineX(new CheckLine(w, b.Top, block.Bottom));
                    }

                    //y
                    if (checkYT(b, p, block))
                        p.Y = b.Top;

                    w = b.Top + b.Height;
                    if (Math.Abs(w - yb) <= CHECKLINE_OFFEST)
                    {
                        p.Y = w - block.Height;
                        if (IsBlockInRight(b, block))
                            AddCheckLineY(new CheckLine(w, p.X, b.Right));
                        else AddCheckLineY(new CheckLine(w, b.Left, block.Right));
                    }
                    w = b.Top + b.Height / 2;
                    if (Math.Abs(w - yc) <= CHECKLINE_OFFEST)
                    {
                        p.Y = w - block.Height / 2;
                        if (IsBlockInRight(b, block))
                            AddCheckLineY(new CheckLine(w, p.X, b.Right));
                        else AddCheckLineY(new CheckLine(w, b.Left, block.Right));
                    }
                }
            }
            return p;
        }
        private const int CHECKLINE_OFFEST = 2;

        private bool checkXL(FCBlock b, Point p, FCBlock block)
        {
            if (Math.Abs(b.Left - p.X) <= CHECKLINE_OFFEST)
            {
                if (IsBlockAbove(b, block))
                    AddCheckLineX(new CheckLine(b.Left, p.Y, b.Bottom));
                else AddCheckLineX(new CheckLine(b.Left, b.Top, block.Bottom));
                return true;
            }
            return false;
        }
        private bool checkYT(FCBlock b, Point p, FCBlock block)
        {
            if (Math.Abs(b.Top - p.Y) <= CHECKLINE_OFFEST)
            {
                if (IsBlockInRight(b, block))
                    AddCheckLineY(new CheckLine(b.Top, p.X, b.Right));
                else AddCheckLineY(new CheckLine(b.Top, b.Left, block.Right));
                return true;
            }
            return false;
        }
        private bool checkW(FCBlock b, Point p, Size s, FCBlock block, int w)
        {
            if (Math.Abs(w - p.X + s.Width) <= CHECKLINE_OFFEST)
            {
                if (IsBlockAbove(b, block))
                    AddCheckLineX(new CheckLine(w, p.Y, b.Bottom));
                AddCheckLineX(new CheckLine(w, b.Top, block.Bottom));

                return true;
            }
            return false;
        }
        private bool checkH(FCBlock b, Point p, Size s, FCBlock block, int w)
        {
            if (Math.Abs(w - p.Y + s.Height) <= CHECKLINE_OFFEST)
            {
                s.Height = w - p.Y;
                if (IsBlockInRight(b, block))
                    AddCheckLineY(new CheckLine(w, p.X, b.Right));
                else AddCheckLineY(new CheckLine(w, b.Left, block.Right));
                return true; 
            }
            return false;
        }

        public Point onMoveBlockCheckLineXY(Point p, FCBlock block)
        {
            bool hasx = false, hasy = false;
            foreach (FCBlock b in blocks)
            {
                if (b != block)
                {
                    if (checkXL(b, p, block))
                    {
                        p.X = b.Left;
                        hasx = true;
                    }
                    if (checkYT(b, p, block))
                    {
                        p.Y = b.Top;
                        hasy = true;
                    }
                    if (hasx && hasy)
                        break;
                }
            }
            return p;
        }
        public Size onMoveBlockCheckLineWH(Point p, Size s, FCBlock block)
        {
            foreach (FCBlock b in blocks)
            {
                if (b != block)
                {
                    int w = b.Left + b.Width;
                    if (checkW(b, p, s, block, w))
                        s.Width = w - p.X;
                    w = b.Top + b.Height;
                    if (checkH(b, p, s, block, w))                   
                        s.Height = w - p.Y;
                }
            }
            return s;
        }

        #endregion

        #region File

        public bool resolveFile(XmlDocument document, XmlNode root)
        {
            XmlNode blocks_node = null, lines_node = null, data_node = null;
            foreach (XmlNode n in root)
            {
                if (n.Name == "BLOCKS")
                    blocks_node = n;
                else if (n.Name == "LINES")
                    lines_node = n;
                else if (n.Name == "DATA")
                    data_node = n;
            }

            if (blocks_node == null)
                blocks_node = root.AppendChild(document.CreateNode(XmlNodeType.Element, "BLOCKS", ""));
            if (lines_node == null)
                lines_node = root.AppendChild(document.CreateNode(XmlNodeType.Element, "LINES", ""));
            if (data_node == null)
                data_node = root.AppendChild(document.CreateNode(XmlNodeType.Element, "DATA", ""));

            foreach (XmlNode n in blocks_node)
            {
                if (n.Name.StartsWith("BLOCK_"))
                {
                    try
                    {
                        string typestr = n.Attributes["type"].InnerText;
                        FCBlockType type = (FCBlockType)Enum.Parse(typeof(FCBlockType), typestr);
                        FCBlock block = null;
                        switch (type)
                        {
                            case FCBlockType.BlockProcess:
                                block = new FCPROCBlock(this);

                                break;
                            case FCBlockType.BlockIO:
                                {
                                    block = new FCIOBlock(this);
                                    if (n.Attributes["io"] != null)
                                    {
                                        string tp = n.Attributes["io"].InnerText;
                                        ((FCIOBlock)block).IsIn = tp == "In";
                                    }
                                    break;
                                }
                            case FCBlockType.BlockStartOrEnd:
                                {
                                    block = new FCSEBlock(this);
                                    if (n.Attributes["io"] != null)
                                    {
                                        string tp = n.Attributes["se"].InnerText;
                                        if (tp == "Start") ((FCSEBlock)block).Type = FCEndOrStartType.Start;
                                        else if (tp == "End") ((FCSEBlock)block).Type = FCEndOrStartType.End;
                                    }
                                    break;
                                }
                            case FCBlockType.BlockCase:
                                block = new FCCABlock(this);

                                break;
                        }
                        if (block != null)
                        {
                            int x, y, w, h;
                            if (int.TryParse(n.Attributes["width"].InnerText, out w))
                                block.SetWidth(w);
                            if (int.TryParse(n.Attributes["height"].InnerText, out h))
                                block.SetHeight(h);
                            if (int.TryParse(n.Attributes["x"].InnerText, out x))
                                block.SetX(x);
                            if (int.TryParse(n.Attributes["y"].InnerText, out y))
                                block.SetY(y);
                            long id;
                            if (long.TryParse(n.Attributes["id"].InnerText, out id))
                            {
                                if (id > globalId) globalId = id + 1;
                                block.Id = id;
                            }
                            else block.Id = globalId;
                            block.Text = n.Attributes["text"].InnerText;
                            blocks.Add(block);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            foreach (XmlNode n in lines_node)
            {

            }

            return true;
        }
        public void writeAllChanges(string filepath, XmlDocument document, XmlNode root)
        {
            bool writeall = filepath == "new";

            XmlNode blocks_node = null, lines_node = null, data_node = null;
            foreach (XmlNode n in root)
            {
                if (n.Name == "BLOCKS")
                    blocks_node = n;
                else if (n.Name == "LINES")
                    lines_node = n;
                else if (n.Name == "DATA")
                    data_node = n;
            }

            if (blocks_node == null)
                blocks_node = root.AppendChild(document.CreateNode(XmlNodeType.Element, "BLOCKS", ""));
            if (lines_node == null)
                lines_node = root.AppendChild(document.CreateNode(XmlNodeType.Element, "LINES", ""));
            if (data_node == null)
                data_node = root.AppendChild(document.CreateNode(XmlNodeType.Element, "DATA", ""));

            foreach (FCBlock b in blocks)
            {
                if(writeall || b.Changed)
                {
                    string bknname = "BLOCK_" + b.Id;
                    XmlNode n = null;
                    foreach (XmlNode n1 in blocks_node)
                    {
                        if (n1.Name == bknname)
                        {
                            n = n1;
                            break;
                        }
                    }
                    if (n == null)
                    {
                        n = blocks_node.AppendChild(document.CreateElement(bknname));
                        n.Attributes.Append(document.CreateAttribute("type")).InnerText = b.BlockType.ToString();
                        n.Attributes.Append(document.CreateAttribute("id")).InnerText = b.Id.ToString();
                        n.Attributes.Append(document.CreateAttribute("x")).InnerText = b.Left.ToString();
                        n.Attributes.Append(document.CreateAttribute("y")).InnerText = b.Top.ToString();
                        n.Attributes.Append(document.CreateAttribute("width")).InnerText = b.Width.ToString();
                        n.Attributes.Append(document.CreateAttribute("height")).InnerText = b.Height.ToString();
                        n.Attributes.Append(document.CreateAttribute("text")).InnerText = b.Text;
                        if (b is FCIOBlock)
                        {
                            n.Attributes.Append(document.CreateAttribute("io")).InnerText = ((FCIOBlock)b).IsIn ? "In" : "Out";
                        }
                        else if (b is FCSEBlock)
                        {
                            n.Attributes.Append(document.CreateAttribute("se")).InnerText = ((FCSEBlock)b).Type == FCEndOrStartType.Start ? "Start" : "End";
                        }
                    }
                    else
                    {
                        XmlAttribute attr_id = n.Attributes["id"];
                        XmlAttribute attr_x = n.Attributes["x"];
                        XmlAttribute attr_y = n.Attributes["y"];
                        XmlAttribute attr_width = n.Attributes["width"];
                        XmlAttribute attr_height = n.Attributes["height"];
                        XmlAttribute attr_text = n.Attributes["text"];
                        XmlAttribute attr_type = n.Attributes["text"];

                        if (attr_type == null) attr_id = n.Attributes.Append(document.CreateAttribute("type"));
                        if (attr_id == null) attr_id = n.Attributes.Append(document.CreateAttribute("id"));
                        if (attr_x == null) attr_x = n.Attributes.Append(document.CreateAttribute("x"));
                        if (attr_y == null) attr_y = n.Attributes.Append(document.CreateAttribute("y"));
                        if (attr_width == null) attr_width = n.Attributes.Append(document.CreateAttribute("width"));
                        if (attr_height == null) attr_height = n.Attributes.Append(document.CreateAttribute("height"));
                        if (attr_text == null) attr_text = n.Attributes.Append(document.CreateAttribute("text"));

                        attr_type.InnerText = b.BlockType.ToString();
                        attr_id.InnerText = b.Id.ToString();
                        attr_x.InnerText = b.Left.ToString();
                        attr_y.InnerText = b.Top.ToString();
                        attr_width.InnerText = b.Width.ToString();
                        attr_height.InnerText = b.Height.ToString();
                        attr_text.InnerText = b.Text;

                        if (b is FCIOBlock)
                        {
                            XmlAttribute attr_io = n.Attributes["io"];
                            if (attr_io == null) attr_io = n.Attributes.Append(document.CreateAttribute("io"));
                            attr_io.InnerText = ((FCIOBlock)b).IsIn ? "In" : "Out";
                        }
                        else if (b is FCSEBlock)
                        {
                            XmlAttribute attr_se = n.Attributes["se"];
                            if (attr_se == null) attr_se = n.Attributes.Append(document.CreateAttribute("se"));
                            attr_se.InnerText = ((FCSEBlock)b).Type == FCEndOrStartType.Start ? "Start" : "End";
                        }
                    }
                    b.Changed = false;
                }
            }
        }

        #endregion

        #region Event

        private void ChartDrawer_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedBlock != null)
            {
                if (e.KeyCode == Keys.Delete)
                    if (!selectedBlock.Editing)
                        onDeleteBlock();
                if (selectedBlock.Editing)
                    selectedBlock.OnKeyEvent(FCBlock.KeyEvent.Down, e);
            }
        }
        private void ChartDrawer_KeyUp(object sender, KeyEventArgs e)
        {
            if (selectedBlock != null)
            {
                if (selectedBlock.Editing)
                    selectedBlock.OnKeyEvent(FCBlock.KeyEvent.Up, e);
                else
                {
                    switch(e.KeyCode)
                    {
                        case Keys.Left:
                            selectedBlock.Left -= 2;
                            InvalidBlock(selectedBlock);
                            break;
                        case Keys.Up:
                            selectedBlock.Top -= 2;
                            InvalidBlock(selectedBlock);
                            break;
                        case Keys.Right:
                            selectedBlock.Left += 2;
                            InvalidBlock(selectedBlock);
                            break;
                        case Keys.Down:
                            selectedBlock.Top += 2;
                            InvalidBlock(selectedBlock);
                            break;

                    }
                }
            }
        }

        private void ChartDrawer_MouseDown(object sender, MouseEventArgs e)
        {
            if (opened)
            {
                if (e.Button == MouseButtons.Left)
                {
                    moveing = false;
                    if (puttingBlock)
                    {
                        puttingStartMousePos = e.Location;
                        puttingStartPos = RealPosToLocation(e.Location, offest);
                        puttingBlockDowned = true;
                    }
                    else
                    {
                        lastMousePos = Point.Empty;
                        onSelectBlock(e);
                        if (selectedBlock != null)
                        {
                            selectedBlock.OnMouseEvent(FCBlock.MouseEvent.Down, e.Button, e.Location, offest);
                            canMoveingBlock = !selectedBlock.IsSizeing;
                            moveingBlock = canMoveingBlock;
                            downMovePos = RealPosToLocation(e.Location, offest);
                            downMovePos.X -= selectedBlock.Location.X;
                            downMovePos.Y -= selectedBlock.Location.Y;
                        }
                        else if (enteredBlock != null)
                        {
                            if (enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Down, e.Button, e.Location, offest))
                                moveing = false;
                        }
                    }
                }
            }
        }
        private void ChartDrawer_MouseUp(object sender, MouseEventArgs e)
        {
            if (opened)
            {
                if (puttingBlock)
                {
                    int x = puttingStartPos.X, y = puttingStartPos.Y;
                    if (puttingSize.Width < 0) { puttingStartPos.X += puttingSize.Width; puttingSize.Width = x - puttingStartPos.X; }
                    if (puttingSize.Height < 0) { puttingStartPos.Y += puttingSize.Height; puttingSize.Height = y - puttingStartPos.Y; }
                    if (puttingSize.Width <= 0) puttingSize.Width = 200;
                    if (puttingSize.Height <= 0) puttingSize.Height = 50;

                    onPutBlock();

                    puttingBlockDowned = false;
                    puttingBlock = false;
                }
                else if (moveing)
                {
                    Cursor = Cursors.Arrow;
                    moveing = false;
                }
                else
                {
                    if (selectedBlock != null)
                    {
                        if (moveingBlock)
                        {
                            if (drawCheckLineX.Count > 0) drawCheckLineX.Clear();
                            if (drawCheckLineY.Count > 0) drawCheckLineY.Clear();
                            moveingBlock = false;
                            Invalidate();
                        }
                        selectedBlock.OnMouseEvent(FCBlock.MouseEvent.Up, e.Button, e.Location, offest);
                    }
                    if (enteredBlock != null)
                        enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Up, e.Button, e.Location, offest);
                }
            }
        }
        private void ChartDrawer_MouseMove(object sender, MouseEventArgs e)
        {
            if (opened)
            {
                if (ctlCan)
                {
                    ctlCan = false;
                    if (puttingBlock && puttingBlockDowned)
                    {
                        Point mousepoint = RealPosToLocation(e.Location, offest);
                        puttingSizeOld.Width = puttingSize.Width;
                        puttingSizeOld.Height = puttingSize.Height;
                        puttingSize.Width = mousepoint.X - puttingStartPos.X;
                        puttingSize.Height = mousepoint.Y - puttingStartPos.Y;

                        int x = puttingStartPos.X, y = puttingStartPos.Y;
                        if (puttingSize.Width < 0) { puttingStartPos.X += puttingSize.Width; puttingSize.Width = x - puttingStartPos.X; }
                        if (puttingSize.Height < 0) { puttingStartPos.Y += puttingSize.Height; puttingSize.Height = y - puttingStartPos.Y; }

                        Rectangle r = new Rectangle(puttingStartMousePos, puttingSize);

                        if (puttingSize.Width < puttingSizeOld.Width) r.Width = puttingSizeOld.Width;
                        if (puttingSize.Height < puttingSizeOld.Height) r.Height = puttingSizeOld.Height;

                        Invalidate(r);

                        lastMousePos = e.Location;
                    }
                    else if (moveing)
                    {
                        if (!lastMousePos.IsEmpty)
                        {
                            offest.X -= e.X - lastMousePos.X;
                            offest.Y -= e.Y - lastMousePos.Y;

                            label1.Text = offest.ToString();

                            lastMousePos = e.Location;
                            Invalidate();
                        }
                        else moveing = false;
                    }
                    else if (selectedBlock != null)
                    {
                        if (e.Button == MouseButtons.Left && canMoveingBlock)
                            onMoveBlock(e);
                        else if (!selectedBlock.OnMouseEvent(FCBlock.MouseEvent.Move, e.Button, e.Location, offest))
                        {
                            if (Cursor != Cursors.Arrow) Cursor = Cursors.Arrow;

                        }
                    }
                    else
                    {
                        onTestMouseIn(e);
                        if (enteredBlock != null)
                            if (!enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Move, e.Button, e.Location, offest))
                            {
                                enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Leave);
                                InvalidBlock(enteredBlock);
                                enteredBlock = null;
                            }
                    }
                }
            }
        }

        #endregion

        public event EventHandler EndPutting;
        public event EventHandler SelectedItemChanged;
        public event BlockChangedEventHandler BlockAdded;
        public event BlockChangedEventHandler BlockRemoved;
    }

    public delegate void BlockChangedEventHandler(object sender, FCBlock b);

    public enum FCBlockType
    {
        BlockNone,
        BlockProcess,
        BlockIO,
        BlockStartOrEnd,
        BlockCase,
    }
    public class FCBlocksCollection : CollectionBase
    {
        public FCBlocksCollection()
        {

        }


        public event BlockChangedEventHandler OnBlockAdd;
        public event BlockChangedEventHandler OnBlockRemove;

        public bool Contains(FCBlock b)
        {
            return List.Contains(b);
        }
        public void Add(FCBlock b)
        {
            if (!Contains(b))
            {
                List.Add(b);
                OnBlockAdd?.Invoke(this, b);
            }
        }
        public void Remove(FCBlock b)
        {
            if (Contains(b))
            {
                List.Remove(b); OnBlockRemove?.Invoke(this, b);
            }
        }
    }
}
