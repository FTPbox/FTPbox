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

        ChannelSftp sftpc;
        
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
            if (!System.IO.Directory.Exists(tPath.Text))
                System.IO.Directory.CreateDirectory(tPath.Text);

            ((frmMain)this.Tag).UpdatePaths(tFullDir.Text, tPath.Text, tParent.Text);
            
            //FTPbox.Properties.Settings.Default.rPath = tFullDir.Text;
            //FTPbox.Properties.Settings.Default.lPath = tPath.Text;
            //FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            //FTPbox.Properties.Settings.Default.Save();

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
                host = ((frmMain)this.Tag).ftpHost();
                UN = ((frmMain)this.Tag).ftpUser();
                pass = ((frmMain)this.Tag).ftpPass();
                port = ((frmMain)this.Tag).ftpPort();
                ftporsftp = ((frmMain)this.Tag).FTP();
                ((frmMain)this.Tag).SetParent(host);

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

                if (((frmMain)this.Tag).ftpParent() == "")
                    tParent.Text = ((frmMain)this.Tag).ftpHost();
                else
                    tParent.Text = ((frmMain)this.Tag).ftpParent();

                tPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FTPbox";

                Log.Write(((frmMain)this.Tag).ftpParent() + " " + ((frmMain)this.Tag).ftpHost());

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
                Set_Language(((frmMain)this.Tag).lang());
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
            tParent.Text = ((frmMain)this.Tag).ftpParent() + path;
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.ToString().Replace('\\', '/');

            if (e.Node.Nodes.Count > 0)
            {
                int i = e.Node.Index;                

                foreach (TreeNode tn in e.Node.Nodes)
                {
                    try
                    {
                        treeView1.Nodes[i].Nodes.Remove(tn);                  
                    }
                    catch (Exception ex){
                        MessageBox.Show(ex.Message);
                    }
                }
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
                expandSFTP(path, e);
            }
        }

        private void expandSFTP(string path, TreeViewEventArgs e)
        {
            try
            {
                SftpGoToRoot();
                string fpath = sftproot + path;
                Log.Write(fpath);
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
            catch (Exception ex)
            {
                sftpc.quit();
                sftp_login();
                expandSFTP(path, e);
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
            //((frmMain)this.Tag).SetParent(tParent.Text);
            //FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            //FTPbox.Properties.Settings.Default.Save();
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
            else if (lan == "fr")
            {
                this.Text = "Ajouter un nouveau répertoire";
                labSelect.Text = "Sélectionner un répertoire:";
                labFullPath.Text = "Chemin complet:";
                labLocal.Text = "Répertoire local:";
                labParent.Text = "Chemin complet:";
                bBrowse.Text = "Parcourir";
                bDone.Text = "Terminer";
            }
            else if (lan == "du")
            {
                this.Text = "voeg een map toe";
                labSelect.Text = "selecteer een map:";
                labFullPath.Text = "Volledig pad:";
                labLocal.Text = "lokale map:";
                labParent.Text = "Account's full path:";
                bBrowse.Text = "Zoeken";
                bDone.Text = "Gereed";
            }
            else
            {
                this.Text = "Add a new directory";
                labSelect.Text = "Select directory:";
                labFullPath.Text = "Full path:";
                labLocal.Text = "Local folder:";
                labParent.Text = "Volledig account pad:";
                bBrowse.Text = "Browse";
                bDone.Text = "Done";
            }
        }

        private void sftp_login()
        {
            JSch jsch = new JSch();

            host = ((frmMain)this.Tag).ftpHost();
            UN = ((frmMain)this.Tag).ftpUser();
            port = ((frmMain)this.Tag).ftpPort();

            Session session = jsch.getSession(UN, host, 22);

            // username and password will be given via UserInfo interface.
            UserInfo ui = new MyUserInfo();

            session.setUserInfo(ui);

            session.setPort(port);

            session.connect();

            Channel channel = session.openChannel("sftp");
            channel.connect();
            sftpc = (ChannelSftp)channel;

        }

        public class MyUserInfo : UserInfo
        {
            FTPbox.Classes.Settings sets = new FTPbox.Classes.Settings();

            public String getPassword()
            {
                return Decrypt(sets.Get("Account/Password", ""),
                "removed",
                "removed",
                "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            public bool promptYesNo(String str)
            {
                DialogResult returnVal = MessageBox.Show(
                    str,
                    "SharpSSH",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                return (returnVal == DialogResult.Yes);
            }

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

            #region Decrypt Method
            public static string Decrypt(string CipherText, string Password,
              string Salt = "Kosher", string HashAlgorithm = "SHA1",
              int PasswordIterations = 2, string InitialVector = "OFRna73m*aze01xY",
              int KeySize = 256)
            {
                if (string.IsNullOrEmpty(CipherText))
                    return "";
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(InitialVector);
                byte[] SaltValueBytes = Encoding.ASCII.GetBytes(Salt);
                byte[] CipherTextBytes = Convert.FromBase64String(CipherText);
                System.Security.Cryptography.PasswordDeriveBytes DerivedPassword = new System.Security.Cryptography.PasswordDeriveBytes(Password, SaltValueBytes, HashAlgorithm, PasswordIterations);
                byte[] KeyBytes = DerivedPassword.GetBytes(KeySize / 8);
                System.Security.Cryptography.RijndaelManaged SymmetricKey = new System.Security.Cryptography.RijndaelManaged();
                SymmetricKey.Mode = System.Security.Cryptography.CipherMode.CBC;
                byte[] PlainTextBytes = new byte[CipherTextBytes.Length];
                int ByteCount = 0;
                using (System.Security.Cryptography.ICryptoTransform Decryptor = SymmetricKey.CreateDecryptor(KeyBytes, InitialVectorBytes))
                {
                    using (System.IO.MemoryStream MemStream = new System.IO.MemoryStream(CipherTextBytes))
                    {
                        using (System.Security.Cryptography.CryptoStream CryptoStream = new System.Security.Cryptography.CryptoStream(MemStream, Decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                        {

                            ByteCount = CryptoStream.Read(PlainTextBytes, 0, PlainTextBytes.Length);
                            MemStream.Close();
                            CryptoStream.Close();
                        }
                    }
                }
                SymmetricKey.Clear();
                return Encoding.UTF8.GetString(PlainTextBytes, 0, ByteCount);
            }
            #endregion
        }

        public void SftpGoToRoot()
        {
            while (!sftpc.pwd().equals(sftproot))
            {
                try
                {
                    sftpc.cd("..");
                }
                catch { 
                    Log.Write("errrrror");
                    sftpc.quit();
                    sftp_login();
                }
            }
        }

        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            int i = e.Node.Nodes.Count;
            Log.Write(i.ToString());

            Log.Write(e.Node.FullPath);
            
            int ind = treeView1.Nodes.IndexOf(e.Node);            

            while (i != 0)
            {
                treeView1.Nodes.RemoveAt(i);
                i -= 1;
                Log.Write(i.ToString());
            }
            
        }
    }
}
