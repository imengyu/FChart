using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FChart
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chartDrawer_SelectedItemChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = chartDrawer.SelectedBlock;
            comboBoxBlocks.SelectedItem = chartDrawer.SelectedBlock;
            if (chartDrawer.SelectedBlock != null)
                SetStautsText("选中：" + chartDrawer.SelectedBlock.ToString());
            else SetStautsText(null);
        }

        private void toolStripButtonStartOrEnd_Click(object sender, EventArgs e)
        {
            if (opened_file)
            {
                chartDrawer.SetPutBlock(FCBlockType.BlockStartOrEnd);
                SetStautsText("在绘制区域中拖动来创建一个新的 开始/结束块");
            }
            else SetStautsText("请打开或新建一个文件");
        }
        private void toolStripButtonProcess_Click(object sender, EventArgs e)
        {
            if (opened_file)
            {
                chartDrawer.SetPutBlock(FCBlockType.BlockProcess);
                SetStautsText("在绘制区域中拖动来创建一个新的 处理块");
            }
            else SetStautsText("请打开或新建一个文件");
        }
        private void toolStripButtonInOut_Click(object sender, EventArgs e)
        {
            if (opened_file)
            {
                chartDrawer.SetPutBlock(FCBlockType.BlockIO);
                SetStautsText("在绘制区域中拖动来创建一个新的 输入/输出块");
            }
            else SetStautsText("请打开或新建一个文件");
        }
        private void toolStripButtonCase_Click(object sender, EventArgs e)
        {
            if (opened_file)
            {
                chartDrawer.SetPutBlock(FCBlockType.BlockCase);
                SetStautsText("在绘制区域中拖动来创建一个新的 判断块");
            }
            else SetStautsText("请打开或新建一个文件");
        }
        private void toolStripMenuLastFileItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if(item!=null)
            {
                string s = item.Text.Remove(0, 3);
                openFile(s);
            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                openFile(openFileDialog.FileName);
        }
        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile("new");
        }
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = opened_file_name;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                saveFileAs(saveFileDialog.FileName);
        }
        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeFile();
        }

        private string opened_file_name = "";
        private string opened_file_path = "";

        private bool _opened_file_saved = false;
        private bool opened_file_saved {
            get { return _opened_file_saved; }
            set { _opened_file_saved = value; chartDrawer.changed = false; }
        }
        private bool opened_file = false;

        private void closeFile()
        {
            if (opened_file)
            {
                if (openFileCheckSaved())
                {
                    opened_file = false;
                    chartDrawer.CloseAll();
                    propertyGrid.SelectedObject = null;
                    comboBoxBlocks.Items.Clear();
                    closeFileHandle();
                    openFileUdateTitle();
                    SetStautsText("就绪 没有打开的文件");
                }
            }
            else SetStautsText("没有打开的文件");
        }
        private void saveFile()
        {
            if (writeFile(opened_file_path))
            {
                opened_file_saved = true;
                SetStautsText("文件已保存至：" + opened_file_path);
            }
            else SetStautsText("文件保存失败，详情请查看输出窗口");           
        }
        private void saveFileAs(string path)
        {
            if (writeFile(opened_file_path))
            {
                opened_file_saved = true;
                SetStautsText("文件已保存至：" + path);
            }
            else SetStautsText("文件保存失败，详情请查看输出窗口");
        }
        private void openFile(string path)
        {
            if (path == opened_file_path) return;
            if (openFileCheckSaved())
            {
                openFileUdateTitle();
                if (path == "new")
                {
                    opened_file_name = "new_chart";
                    opened_file_path = "new";
                    opened_file_saved = false;
                    opened_file = true;
                    chartDrawer.SetOpened();
                    newFile();
                    SetStautsText("新建文件成功");
                }
                else
                {
                    opened_file = loadFile(path);
                    if (opened_file)
                    {
                        chartDrawer.SetOpened();
                        opened_file_path = path;
                        opened_file_name = Path.GetFileNameWithoutExtension(opened_file_path);
                        SetStautsText("文件打开成功");
                    }
                    else SetStautsText("新建文件失败");
                }
            }
        }
        private void openFileUdateTitle()
        {
            if (opened_file)
                Text = "Folw chart - " + opened_file_name + "    - " + opened_file_path;
            else Text = "Folw chart";
        }
        private bool openFileCheckSaved()
        {
            if (opened_file)
            {               
                if (!opened_file_saved || chartDrawer.changed)
                {
                    DialogResult rs = MessageBox.Show("您的文档还没有保存，您想保存吗？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (rs == DialogResult.Yes)
                        saveFile();
                    else if (rs == DialogResult.Cancel) return false;
                }
            }
            return true;
        }

        private XmlDocument document;
        private XmlNode root = null;

        private void SetStautsText(string s)
        {
            if(string.IsNullOrEmpty(s))
                toolStripStatus.Text = "就绪";
            else toolStripStatus.Text = s;
        }
        private void SetStautsProgress(int v)
        {
            if(v==0)
            {
                if (toolStripProgressBar.Visible)
                    toolStripProgressBar.Visible = false;
                toolStripProgressBar.Value = 0;
            }
            else
            {
                if (!toolStripProgressBar.Visible)
                    toolStripProgressBar.Visible = true;
                toolStripProgressBar.Value = v;
            }
        }

        private void closeFileHandle()
        {
            document = null;
        }
        private void newFile()
        {
            document = new XmlDocument();
            root = document.AppendChild(document.CreateNode(XmlNodeType.Element, "fc", ""));
        }
        private bool loadFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    document = new XmlDocument();
                    document.Load(path);
                    addCloseItems(path);
                    if (document.ChildNodes.Count > 0)
                    {
                        root = document.ChildNodes[0];
                        opened_file_saved = true;
                    }
                    else root = document.AppendChild(document.CreateNode(XmlNodeType.Element, "fc", ""));
                }
                catch (Exception e)
                {
                    MessageBox.Show("无法打开：" + path + "\n" + e.Message + "\n\n堆栈位置：" + e.StackTrace, "打开文件错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if(resolveFile())
                {
                    return true;
                }
            }
            else MessageBox.Show("无法打开：" + path + "\n文件不存在。", "打开文件错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        private bool resolveFile()
        {
            return chartDrawer.resolveFile(document, root);
        }
        private void writeAllChanges()
        {
            chartDrawer.writeAllChanges(opened_file_path, document, root);
        }
        private bool writeFile(string path)
        {
            if (document != null)
            {
                try
                {
                    if (path == "new")
                    {
                        saveFileDialog.FileName = opened_file_name;
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            path = saveFileDialog.FileName;
                            opened_file_path = path;
                            addCloseItems(path);
                        }
                        else return false;
                    }
                    writeAllChanges();
                    document.Save(path);
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("无法保存至：" + path + "\n" + e.Message + "\n\n堆栈位置：" + e.StackTrace, "保存文件错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return false;
        }

        private Settings settings;

        private void loadSetting()
        {
            settings = new Settings();
            settings.Load();

            if (settings.lastMax) WindowState = FormWindowState.Maximized;
            else
            {
                if (settings.lastX > 0) Left = settings.lastX;
                if (settings.lastY > 0) Top = settings.lastY;
                if (settings.lastW > 0) Width = settings.lastW;
                if (settings.lastH > 0) Height = settings.lastH;
            }

            loadCloseItems();
        }
        private void saveSetting()
        {
            settings.Save();
        }
        private void loadCloseItems()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!string.IsNullOrEmpty(settings.lastFiles[i]))
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(i + (i > 9 ? " " : "  ") + settings.lastFiles[i]);
                    item.Click += toolStripMenuLastFileItem_Click;
                    toolStripMenuItemCloseItem.DropDownItems.Add(item);
                }
            }
            //
        }
        private void addCloseItems(string s)
        {
            if (!settings.ContainsLastItem(s))
            {
                int i = settings.GetLastItemLastIndex();
                ToolStripMenuItem item = new ToolStripMenuItem(i + (i > 9 ? " " : "  ") + s);
                item.Click += toolStripMenuLastFileItem_Click;
                toolStripMenuItemCloseItem.DropDownItems.Add(item);

                settings.AddLastItem(s);
            }
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            loadSetting();
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !openFileCheckSaved();
            if (!e.Cancel) saveSetting();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (chartDrawer.SelectedBlock != null)
            {
                chartDrawer.SelectedBlock.Changed = true;
                if (!chartDrawer.changed) chartDrawer.changed = true;
                chartDrawer.Invalidate();
            }
        }
        private void comboBoxBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBoxBlocks.SelectedItem is Chart.FCBlock)
            chartDrawer.SelectBlock(comboBoxBlocks.SelectedItem as Chart.FCBlock);
        }

        private void chartDrawer_BlockAdded(object sender, Chart.FCBlock b)
        {
            comboBoxBlocks.Items.Add(b);
        }
        private void chartDrawer_BlockRemoved(object sender, Chart.FCBlock b)
        {
            comboBoxBlocks.Items.Remove(b);
        }

        private void chartDrawer_EndPutting(object sender, EventArgs e)
        {
            SetStautsText("");
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAbout().ShowDialog();
        }
    }

}
