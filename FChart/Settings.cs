using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FChart
{
    class Settings
    {
        public bool[] lastFilesSaved = new bool[10];
        public string[] lastFiles = new string[10];
        public int lastX = -1;
        public int lastY = -1;
        public int lastW = -1;
        public int lastH = -1;
        public bool lastMax = false;

        private XmlDocument settingsFile = new XmlDocument();

        public int GetLastItemLastIndex()
        {
            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(lastFiles[i]))
                    return i;
            }
            return 1;
        }
        public bool ContainsLastItem(string s)
        {
            foreach (string sx in lastFiles)
            {
                if (sx == s)
                    return true;
            }
            return false;
        }
        public void AddLastItem(string s)
        {
            if (lastFiles[9] != null)
                lastFiles[9] = null;
            for(int i=8;i>=1;i--)
                lastFiles[i] = lastFiles[i - 1];
            lastFiles[0] = s;
        }

        public void Load()
        {
            try
            {
                settingsFile.Load(Application.StartupPath + "\\FChartSettings.xml");
                if (settingsFile.ChildNodes.Count > 0)
                {
                    XmlNode root = settingsFile.ChildNodes[0];
                    foreach(XmlNode n in root)
                    {
                        if (n.Name == "lastX")
                        {
                            int.TryParse(n.InnerText, out lastX);
                        }
                        else if (n.Name == "lastY")
                        {
                            int.TryParse(n.InnerText, out lastY);
                        }
                        else if (n.Name == "lastW")
                        {
                            int.TryParse(n.InnerText, out lastW);
                        }
                        else if (n.Name == "lastH")
                        {
                            int.TryParse(n.InnerText, out lastH);
                        }
                        else if (n.Name == "lastMax")
                        {
                            bool.TryParse(n.InnerText, out lastMax);
                        }
                        else if (n.Name == "lastFiles")
                        {
                            foreach (XmlNode n2 in n)
                            {
                                if (n2.Name.StartsWith("File_"))
                                {
                                    string index = n2.Name.Remove(0, 5);
                                    int i = -1;
                                    if (int.TryParse(index, out i))
                                    {
                                        if (i >= 0 && i < 10)
                                            lastFiles[i] = n2.InnerText;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public void Save()
        {
            XmlNode root = settingsFile.ChildNodes[0];
            if (root == null) root = settingsFile.AppendChild(settingsFile.CreateElement("Settings"));

            XmlNode set_lastX = null,
                set_lastY = null,
                set_lastW = null,
                set_lastH = null,
                set_lastMax = null,
                set_lastFiles = null;

            foreach (XmlNode n in root)
            {
                if (n.Name == "lastX")
                    set_lastX = n;
                else if (n.Name == "lastY")
                    set_lastX = n;
                else if (n.Name == "lastW")
                    set_lastX = n;
                else if (n.Name == "lastH")
                    set_lastX = n;
                else if (n.Name == "lastMax")
                    set_lastX = n;
                else if (n.Name == "lastFiles")
                    set_lastFiles = n;
            }

            if (set_lastX == null) set_lastX = root.AppendChild(settingsFile.CreateElement("lastX"));
            if (set_lastY == null) set_lastY = root.AppendChild(settingsFile.CreateElement("lastY"));
            if (set_lastW == null) set_lastW = root.AppendChild(settingsFile.CreateElement("lastW"));
            if (set_lastH == null) set_lastH = root.AppendChild(settingsFile.CreateElement("lastH"));
            if (set_lastMax == null) set_lastMax = root.AppendChild(settingsFile.CreateElement("lastMax"));
            if (set_lastFiles == null) set_lastFiles = root.AppendChild(settingsFile.CreateElement("lastFiles"));

            set_lastX.InnerText = lastX.ToString();
            set_lastY.InnerText = lastY.ToString();
            set_lastW.InnerText = lastW.ToString();
            set_lastH.InnerText = lastH.ToString();
            set_lastMax.InnerText = lastMax.ToString();

            foreach (XmlNode n in set_lastFiles)
            {
                if (n.Name.StartsWith("File_"))
                {
                    int i = 0;
                    if (int.TryParse(n.Name.Remove(0, 5), out i))
                    {
                        if (i >= 0 && i < 10)
                        {
                            n.InnerText = lastFiles[i];
                            lastFilesSaved[i] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < 10; i++)
            {
                if (!lastFilesSaved[i])
                {
                    set_lastFiles.AppendChild(settingsFile.CreateElement("File_" + i.ToString())).InnerText = lastFiles[i];
                    lastFilesSaved[i] = true;
                }
            }

            settingsFile.Save(Application.StartupPath + "\\FChartSettings.xml");
        }
    }
}
