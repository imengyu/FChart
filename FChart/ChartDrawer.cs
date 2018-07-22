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
        }

        private void Blocks_OnBlockRemove(object sender, FCBlock b)
        {
            BlockRemoved?.Invoke(this, b);
        }
        private void Blocks_OnBlockAdd(object sender, FCBlock b)
        {
            b.Id = GlobalId;
            BlockAdded?.Invoke(this, b);
        }

        private void ChartDrawer_Load(object sender, EventArgs e)
        {
            RulerUnitsX = 50;
            RulerUnitsY = 20;
            dragPoint = Properties.Resources.drag_point;
        }

        public int RulerUnitsX { get; set; }
        public int RulerUnitsY { get; set; }
        public FCBlock SelectedBlock { get { return selectedBlock; } }
        public FCBlock EnteredBlock { get { return enteredBlock; } }
        private List<FCLine> Lines { get { return lines; } }
        private FCBlocksCollection Blocks { get { return blocks; } }
        private List<FCBlock> ShowedBlocks { get { return showedBlocks; } }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public long GlobalId
        {
            get
            {
                globalId++;
                return globalId;
            }
        }

        private long globalId = 0;
        private Image dragPoint = null;
        private Size puttingSizeOld = new Size();
        private Size puttingSize = new Size();
        private Point puttingStartPos = new Point();
        private Point puttingStartMousePos = new Point();
        private FCBlockType puttingBlockType = FCBlockType.BlockNone;
        private bool puttingBlockDowned = false;
        private bool puttingBlock = false;
        private bool moveing = false;
        private bool canMoveingBlock = false;
        private Point downMovePos = new Point();
        private bool opened = false;
        private Point offest = new Point(0, 0);
        private Point lastMousePos = new Point(0, 0);
        private List<FCLine> lines = new List<FCLine>();
        private FCBlocksCollection blocks = new FCBlocksCollection();
        private List<FCBlock> showedBlocks = new List<FCBlock>();
        private Point _RealLocation = new Point();
        private FCBlock selectedBlock = null;
        private FCBlock enteredBlock = null;

        public event EventHandler SelectedItemChanged;
        public event BlockChangedEventHandler BlockAdded;
        public event BlockChangedEventHandler BlockRemoved;

        public void SetPutBlock(FCBlockType fCBlockType)
        {
            puttingBlockType = fCBlockType;
            puttingBlock = true;
            Cursor = Cursors.Cross;
        }
        public void SetNoPutBlock()
        {
            puttingBlockType = FCBlockType.BlockNone;
            puttingBlock = false;
            Cursor = Cursors.Arrow;
        }
        public void SetOpened()
        {
            BackColor = Color.White;
            opened = true;
        }

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (opened)
            {
                DrawLines(e.Graphics, e.ClipRectangle);
                DrawBlocks(e.Graphics, e.ClipRectangle);
                DrawGrid(e.Graphics, e.ClipRectangle);
                DrawSelectedBlock(e.Graphics, e.ClipRectangle);
                DrawPutting(e.Graphics);
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
            showedBlocks.Clear();
            Rectangle rectangle = new Rectangle();
            foreach (FCBlock b in blocks)
            {
                rectangle = b.Rectangle;
                rectangle.Location = LocationToRealPos(rectangle.Location, offest);
                if (rectangle.IntersectsWith(refrc))
                {
                    showedBlocks.Add(b);
                    b.OnDraw(g, offest);
                }
            }
        }
        private void DrawGrid(Graphics g, Rectangle refrc)
        {
            if (refrc.Top < 20)
            {
                for (int x = -offest.X, max = Width + offest.X, n = 0; x <= max; x += RulerUnitsX, n += RulerUnitsX)
                {
                    g.DrawLine(Pens.Gray, x, 0, x, 10);
                    if (x != 0) g.DrawString(n.ToString(), Font, Brushes.Gray, x, 3);
                }
            }
            if (refrc.Left <= 15)
            {
                for (int y = -offest.Y, max = Height + offest.Y, n = 0; y <= max; y += RulerUnitsY, n += RulerUnitsY)
                {
                    g.DrawLine(Pens.Gray, 0, y, 10, y);
                    if (y != 0) g.DrawString(n.ToString(), Font, Brushes.Gray, 3, y + 2);
                }
            }
        }

        public FCBlock FindBlockInPoint(Point pos, bool checkSizeing = false)
        {
            Point point = RealPosToLocation(pos, offest);
            FCBlock rs = null;
            if (checkSizeing)
            {
                foreach (FCBlock b in blocks)
                {
                    if (b.IsSizeing || b.Rectangle.Contains(point))
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
                    if (b.Rectangle.Contains(point))
                    {
                        rs = b;
                        break;
                    }
                }
            }
            return rs;
        }
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
        public void InvalidBlock(FCBlock b)
        {
            if (b != null)
            {
                Rectangle r = b.GetRealRectangle(offest, true);
                r.X -= 2; r.Y -= 2; r.Width += 4; r.Height += 4;
                if (b == selectedBlock) { r.X -= 10; r.Y -= 10; r.Width += 20; r.Height += 20; }
                Invalidate(r);
            }
        }
        public void EditBlock(FCBlock b)
        {
            if (b != null && !b.Editing)
                b.BeginEdit();
        }
        public void MoveBlock(FCBlock b, Point newpos)
        {
            if (b != null && !b.Editing)
            {
                b.Location = newpos;
                InvalidBlock(b);
            }
        }

        private void ChartDrawer_MouseDown(object sender, MouseEventArgs e)
        {
            if (opened)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (puttingBlock)
                    {
                        puttingStartMousePos = e.Location;
                        puttingStartPos = RealPosToLocation(e.Location, offest);
                        puttingBlockDowned = true;
                    }
                    else if (!moveing)
                    {
                        lastMousePos = Point.Empty;
                        onSelectBlock(e);
                        if (selectedBlock != null)
                        {
                            selectedBlock.OnMouseEvent(FCBlock.MouseEvent.Down, e.Button, e.Location, offest);
                            if (selectedBlock.IsSizeing) { canMoveingBlock = false; moveing = false; }
                            else canMoveingBlock = true;
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

                    switch (puttingBlockType)
                    {
                        case FCBlockType.BlockProcess:
                            {
                                FCPROCBlock b = new FCPROCBlock(this);
                                b.Size = puttingSize;
                                b.Location = puttingStartPos;
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
                                SelectBlock(b);
                                break;
                            }
                        case FCBlockType.BlockStartOrEnd:
                            {
                                FCSEBlock b = new FCSEBlock(this);
                                b.Size = puttingSize;
                                b.Location = puttingStartPos;
                                blocks.Add(b);
                                SelectBlock(b);
                                break;
                            }
                        case FCBlockType.BlockCase:
                            {
                                FCCABlock b = new FCCABlock(this);
                                b.Size = puttingSize;
                                b.Location = puttingStartPos;
                                blocks.Add(b);
                                SelectBlock(b);
                                break;
                            }
                    }

                    SetNoPutBlock();
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
                        selectedBlock.OnMouseEvent(FCBlock.MouseEvent.Up, e.Button, e.Location, offest);
                    if (enteredBlock != null)
                        enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Up, e.Button, e.Location, offest);
                }
            }
        }
        private void ChartDrawer_MouseMove(object sender, MouseEventArgs e)
        {
            if (opened)
            {
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
                else {
                    onTestMouseIn(e);
                    if (enteredBlock != null)
                        if (!enteredBlock.OnMouseEvent(FCBlock.MouseEvent.Move, e.Button, e.Location, offest))
                            enteredBlock = null;
                }
            }
        }

        private void onDeleteBlock()
        {
            if (selectedBlock != null)
            {
                blocks.Remove(selectedBlock);
                InvalidBlock(selectedBlock);
                BlockRemoved?.Invoke(this, selectedBlock);
            }
        }
        private void onMoveBlock(MouseEventArgs e)
        {
            if (!selectedBlock.Changed) selectedBlock.Changed = true;
            Point p = RealPosToLocation(e.Location, offest), lastPos = LocationToRealPos(selectedBlock.Location, offest);


            selectedBlock.Location = new Point(p.X - downMovePos.X, p.Y - downMovePos.Y);
            InvalidBlock(selectedBlock);
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
                    downMovePos = RealPosToLocation(e.Location, offest);
                    downMovePos.X -= b.Location.X;
                    downMovePos.Y -= b.Location.Y;
                    moveing = false;
                    SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (selectedBlock != null)
                    {
                        if (selectedBlock.IsMouseInSizeing(e.Location, offest))
                        {
                            rs = true;
                            return rs;
                        }
                    }
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
                if (n.Name == "BLOCK")
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
                            block = new FCIOBlock(this);

                            break;
                        case FCBlockType.BlockStartOrEnd:
                            block = new FCSEBlock(this);
                            string tp = n.Attributes["se"].InnerText;
                            if (tp == "Start") ((FCSEBlock)block).Type = FCEndOrStartType.Start;
                            else if (tp == "End") ((FCSEBlock)block).Type = FCEndOrStartType.End;
                            break;
                        case FCBlockType.BlockCase:
                            block = new FCCABlock(this);

                            break;
                    }
                    if (block != null)
                    {
                        int x, y, w, h;
                        if (int.TryParse(n.Attributes["x"].InnerText, out x))
                            block.SetX(x);
                        if (int.TryParse(n.Attributes["y"].InnerText, out y))
                            block.SetY(y);
                        if (int.TryParse(n.Attributes["width"].InnerText, out w))
                            block.SetWidth(w);
                        if (int.TryParse(n.Attributes["height"].InnerText, out h))
                            block.SetHeight(h);
                        long id;
                        if (long.TryParse(n.Attributes["id"].InnerText, out id))
                            block.Id = id;
                        block.Text = n.Attributes["text"].InnerText;
                        blocks.Add(block);
                    }
                }
            }
            foreach (XmlNode n in lines_node)
            {

            }

            return true;
        }
        public void writeAllChanges(XmlDocument document, XmlNode root)
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

            foreach (FCBlock b in blocks)
            {
                XmlNode n = blocks_node.AppendChild(document.CreateNode(XmlNodeType.Element, "BLOCK", ""));

                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "type", "")).InnerText = b.BlockType.ToString();
                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "text", "")).InnerText = b.Text;
                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "id", "")).InnerText = b.Id.ToString();
                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "x", "")).InnerText = b.Location.X.ToString();
                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "y", "")).InnerText = b.Location.Y.ToString();
                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "width", "")).InnerText = b.Size.Width.ToString();
                n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "height", "")).InnerText = b.Size.Height.ToString();

                if (b.BlockType == FCBlockType.BlockStartOrEnd)
                {
                    n.AppendChild(document.CreateNode(XmlNodeType.Attribute, "se", "")).InnerText = ((FCSEBlock)b).Type.ToString();
                }
            }
        }

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
