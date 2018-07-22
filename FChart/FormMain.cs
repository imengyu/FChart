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
        }

        private void toolStripButtonStartOrEnd_Click(object sender, EventArgs e)
        {
            if(opened_file)
            chartDrawer.SetPutBlock(FCBlockType.BlockStartOrEnd);
        }
        private void toolStripButtonProcess_Click(object sender, EventArgs e)
        {
            if (opened_file)
                chartDrawer.SetPutBlock(FCBlockType.BlockProcess);
        }
        private void toolStripButtonInOut_Click(object sender, EventArgs e)
        {
            if (opened_file)
                chartDrawer.SetPutBlock(FCBlockType.BlockIO);
        }
        private void toolStripButtonCase_Click(object sender, EventArgs e)
        {
            if (opened_file)
                chartDrawer.SetPutBlock(FCBlockType.BlockCase);
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
        private bool opened_file_saved = false;
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
                    closeFileHandle();
                    openFileUdateTitle();
                }
            }
        }
        private void saveFile()
        {
            writeFile(opened_file_path);
            opened_file_saved = true;
        }
        private void saveFileAs(string path)
        {
            writeFile(path);
            opened_file_saved = true;
        }
        private void openFile(string path)
        {
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
                }
                else
                {
                    opened_file = loadFile(path);
                    if (opened_file)
                    {
                        chartDrawer.SetOpened();
                        opened_file_path = path;
                        opened_file_name = Path.GetFileNameWithoutExtension(opened_file_path);
                    }
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
                if (!opened_file_saved)
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
                    if (document.ChildNodes.Count > 0)
                        root = document.ChildNodes[0];
                    else
                        root = document.CreateNode(XmlNodeType.Element, "fc", "");
                }
                catch (Exception e)
                {
                    MessageBox.Show("无法打开：" + path + "\n" + e.Message, "打开文件错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            chartDrawer.writeAllChanges(document, root);
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
                        }
                        else return false;
                    }
                    writeAllChanges();
                    document.Save(path);
                    return true;
                }
                catch(Exception e)
                {
                    MessageBox.Show("无法保存至：" + path + "\n" + e.Message, "保存文件错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return false;
        }


        private void FormMain_Load(object sender, EventArgs e)
        {

        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !openFileCheckSaved();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (chartDrawer.SelectedBlock != null)
            {
                chartDrawer.SelectedBlock.Changed = true;
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
    }

}
