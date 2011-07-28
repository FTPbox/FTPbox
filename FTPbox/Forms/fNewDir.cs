using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FtpLib;
using Tamir.SharpSsh.jsch;

namespace FTPbox
{
    public partial class fNewDir : Form
    {
        //variables used
        string host;
        string UN;
        string pass;
        int port;
        bool ftporsftp;

        FtpConnection ftp;

        string sftproot = "/home";

        public fNewDir()
        {
            InitializeComponent();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            fbd.ShowDialog();
            tPath.Text = fbd.SelectedPath;
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.rPath = tFullDir.Text;
            FTPbox.Properties.Settings.Default.lPath = tPath.Text;
            FTPbox.Properties.Settings.Default.delRem = !chkDelRem.Checked;
            FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            FTPbox.Properties.Settings.Default.Save();

            ((frmMain)this.Tag).ClearLog();
            ((frmMain)this.Tag).UpdateDetails();
            ((frmMain)this.Tag).SetLocalWatcher();
            ((frmMain)this.Tag).gotpaths = true;

            if (ftporsftp)
            {
                try
                {
                    ftp.Close();
                }
                catch { }
            }
            else
            {
                try
                {
                    sftpc.quit();
                }
                catch { }
            }
            this.Close();
        }       

        private void fNewDir_Load(object sender, EventArgs e)
        {
            try
            {
                host = FTPbox.Properties.Settings.Default.ftpHost;
                UN = FTPbox.Properties.Settings.Default.ftpUsername;
                pass = FTPbox.Properties.Settings.Default.ftpPass;
                port = FTPbox.Properties.Settings.Default.ftpPort;
                ftporsftp = FTPbox.Properties.Settings.Default.FTPorSFTP;

                if (ftporsftp)
                {
                    ftp = new FtpConnection(host, port, UN, pass);
                    ftp.Open();
                    ftp.Login();
                }
                else
                {
                    sftp_login();
                    sftproot = sftpc.pwd();
                }

                tParent.Text = FTPbox.Properties.Settings.Default.ftpHost;

                treeView1.Nodes.Clear();

                TreeNode first = new TreeNode();
                first.Text = "/";
                treeView1.Nodes.Add(first);

                if (ftporsftp)
                {
                    foreach (FtpDirectoryInfo dir in ftp.GetDirectories())
                    {
                        if (dir.Name != "." && dir.Name != "..")
                        {
                            TreeNode ParentNode = new TreeNode();
                            ParentNode.Text = dir.Name.ToString();
                            treeView1.Nodes.Add(ParentNode);

                            TreeNode ChildNode = new TreeNode();
                            ChildNode.Text = dir.Name.ToString();
                            ParentNode.Nodes.Add(ChildNode);
                        }

                    }
                }
                else
                {
                    foreach (ChannelSftp.LsEntry lse in sftpc.ls("."))
                    {
                        SftpATTRS attrs = lse.getAttrs();
                        if (lse.getFilename() != "." && lse.getFilename() != ".." && attrs.getPermissionsString().StartsWith("d"))
                        {
                            TreeNode ParentNode = new TreeNode();
                            ParentNode.Text = lse.getFilename();
                            treeView1.Nodes.Add(ParentNode);

                            TreeNode ChildNode = new TreeNode();
                            ChildNode.Text = lse.getFilename();
                            ParentNode.Nodes.Add(ChildNode);
                        }
                    }
                }
                treeView1.SelectedNode = first;
                Set_Language(FTPbox.Properties.Settings.Default.lan);
            }
            catch { this.Close(); }

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.ToString().Replace('\\', '/');
            if (path.EndsWith(".."))
            {
                path = path.Substring(0, path.Length - 2);
            }
            else if (path.EndsWith("."))
            {
                path = path.Substring(0, path.Length - 1);
            }
            else if (path.EndsWith("//"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            tFullDir.Text = path;
            tParent.Text = FTPbox.Properties.Settings.Default.ftpHost +path;
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.ToString().Replace('\\', '/');

            foreach (TreeNode tn in e.Node.Nodes)
            {
                tn.Remove();
            }

            if (ftporsftp)
            {
                foreach (FtpDirectoryInfo dir in ftp.GetDirectories(path))
                {
                    if (dir.Name != "." && dir.Name != "..")
                    {
                        TreeNode ParentNode = new TreeNode();
                        ParentNode.Text = dir.Name.ToString();
                        e.Node.Nodes.Add(ParentNode);

                        TreeNode ChildNode = new TreeNode();
                        ChildNode.Text = dir.Name.ToString();
                        ParentNode.Nodes.Add(ChildNode);
                    }
                }
            }
            else
            {
                SftpGoToRoot();
                string fpath = sftproot + path;
                sftpc.cd(fpath);
                foreach (ChannelSftp.LsEntry lse in sftpc.ls("."))
                {
                    SftpATTRS attrs = lse.getAttrs();
                    if (lse.getFilename() != "." && lse.getFilename() != ".." && attrs.getPermissionsString().StartsWith("d"))
                    {
                        TreeNode ParentNode = new TreeNode();
                        ParentNode.Text = lse.getFilename();
                        e.Node.Nodes.Add(ParentNode);

                        TreeNode ChildNode = new TreeNode();
                        ChildNode.Text = lse.getFilename();
                        ParentNode.Nodes.Add(ChildNode);
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tFullDir.Text != "" && tPath.Text != "")
            {
                bDone.Enabled = true;
            }
            else
            {
                bDone.Enabled = false;
            }
        }        

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            FTPbox.Properties.Settings.Default.Save();
        }

        private void Set_Language(string lan)
        {
            if (lan == "es")
            {
                this.Text = "Añade un nuevo directorio";
                labSelect.Text = "Selecciona la dirección:";
                labFullPath.Text = "Dirección completa:";
                labLocal.Text = "Carpeta local:";
                labParent.Text = "Dirección completa de la cuent:";
                bBrowse.Text = "Explorar";
                bDone.Text = "Listo";
            }
            else if (lan == "de")
            {
                this.Text = "Ein Verzeichnis erstellen";
                labSelect.Text = "Vollständigen Ordnerpfad auswählen:";
                labFullPath.Text = "Vollständiger Pfad:";
                labLocal.Text = "Lokaler Ordner:";
                labParent.Text = "Vollständiger Kontopfad:";
                bBrowse.Text = "Durchsuchen";
                bDone.Text = "Fertig";
            }
            else
            {
                this.Text = "Add a new directory";
                labSelect.Text = "Select directory:";
                labFullPath.Text = "Full path:";
                labLocal.Text = "Local folder:";
                labParent.Text = "Account's full path:";
                bBrowse.Text = "Browse";
                bDone.Text = "Done";
            }
        }
        
        ChannelSftp sftpc;
        private void sftp_login()
        {
            JSch jsch = new JSch();

            String host = FTPbox.Properties.Settings.Default.ftpHost;
            String user = FTPbox.Properties.Settings.Default.ftpUsername;

            Session session = jsch.getSession(user, host, 22);

            // username and password will be given via UserInfo interface.
            UserInfo ui = new MyUserInfo();

            session.setUserInfo(ui);

            session.connect();

            Channel channel = session.openChannel("sftp");
            channel.connect();
            sftpc = (ChannelSftp)channel;

        }

        public class MyUserInfo : UserInfo
        {
            public String getPassword() { return passwd; }
            public bool promptYesNo(String str)
            {
                /*
                DialogResult returnVal = MessageBox.Show(
                    str,
                    "SharpSSH_",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                return (returnVal == DialogResult.Yes); */
                return true;
            }

            String passwd = FTPbox.Properties.Settings.Default.ftpPass;

            public String getPassphrase() { return null; }
            public bool promptPassphrase(String message) { return true; }
            public bool promptPassword(String message) { return true; }

            public void showMessage(String message)
            {
                MessageBox.Show(
                    message,
                    "SharpSSH",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        public void SftpGoToRoot()
        {
            while (!sftpc.pwd().equals(sftproot))
            {
                sftpc.cd("..");
            }
        }
    }
}
