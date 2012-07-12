using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FTPbox.Forms
{
    public partial class selective_sync : Form
    {
        List<string> list = new List<string>();

        public selective_sync(List<string> l)
        {
            InitializeComponent();
            list = l;
        }

        private void selective_sync_Load(object sender, EventArgs e)
        {                        
            foreach (string s in list)
            {
                Log.Write(l.Debug, s);
                if (!s.Contains("/"))
                {
                    TreeNode ParentNode = new TreeNode();
                    ParentNode.Checked = true;
                    ParentNode.Text = s;
                    treeView1.Nodes.Add(ParentNode);                    
                }
                else
                {
                    int i = s.Split('/').Length - 1;
                    string[] p = s.Split('/');
                    string name = p[i];
                    Log.Write(l.Debug, treeView1.Nodes.Count.ToString());
                    foreach (TreeNode t in treeView1.Nodes)
                    {                        
                        Log.Write(l.Debug, "FP: " + t.FullPath + " - " + s.Substring(0, s.LastIndexOf('/')));
                        if (t.FullPath.Replace('\\', '/') == s.Substring(0, s.LastIndexOf('/')))
                        {
                            TreeNode n = new TreeNode();
                            n.Text = name;
                            n.Checked = true;
                            t.Nodes.Add(n);
                        }
                        else
                            RecursiveCheck(t, s, name);
                    }
                    
                }
            }
        }

        private void RecursiveCheck(TreeNode p, string s, string name)
        {
            foreach (TreeNode t in p.Nodes)
            {
                if (t.FullPath.Replace('\\', '/') == s.Substring(0, s.LastIndexOf('/')))
                {
                    TreeNode n = new TreeNode();
                    n.Text = name;
                    n.Checked = true;
                    t.Nodes.Add(n);
                }
                else
                {
                    if (t.Nodes.Count > 0)
                        RecursiveCheck(t, s, name);
                }
            }
        }

        List<string> notSelected = new List<string>();
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (TreeNode t in treeView1.Nodes)
            {
                if (!t.Checked)
                    notSelected.Add(t.FullPath.Replace(@"\", @"/"));
                if (t.Nodes.Count > 0)
                    RecursiveLookForUnchecked(t);
            }

            Log.Write(l.Debug, "Unselected:");
            foreach (string s in notSelected)
            {
                Log.Write(l.Debug, s);
            }

            //((fMain)this.Tag).GetUnselectedFolders(notSelected);
        }

        private void RecursiveLookForUnchecked(TreeNode p)
        {
            foreach (TreeNode t in p.Nodes)
            {
                if (!t.Checked)
                    notSelected.Add(t.FullPath.Replace(@"\", @"/"));
                if (t.Nodes.Count > 0)
                    RecursiveLookForUnchecked(t);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
