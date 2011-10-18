using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FtpLib;
using System.Diagnostics;
using Tamir.SharpSsh.jsch;
using Utilities.Encryption;

namespace FTPbox
{
    public partial class NewFTP : Form
    {
        FtpConnection ftp;
        string decpass = "removed";
        string salt = "removed";
        
        public NewFTP()
        {
            InitializeComponent();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            bool ftporsftp;
            if (cMode.SelectedIndex == 0)
            {
                ftporsftp = true;
                Log.Write("FTP");
            }
            else
            {
                ftporsftp = false;
                Log.Write("SFTP");
            }

            try 
            {
                if (ftporsftp)
                {
                    ftp = new FtpConnection(tHost.Text, Convert.ToInt32(nPort.Value), tUsername.Text, tPass.Text);
                    ftp.Open();
                    ftp.Login();
                    ftp.Close();
                }
                else
                {
                    string pass = AESEncryption.Encrypt(tPass.Text, decpass, salt, "SHA1", 2, "OFRna73m*aze01xY", 256);
                    ((frmMain)this.Tag).SetPass(pass);
                    //FTPbox.Properties.Settings.Default.ftpPass = tPass.Text;
                    sftp_login();
                    //MessageBox.Show("SFTP Connected");
                    sftpc.quit();
                }
                
                string hostEncr = AESEncryption.Encrypt(tHost.Text, decpass, salt, "SHA1", 2, "OFRna73m*aze01xY", 256);
                string unEncr = AESEncryption.Encrypt(tUsername.Text, decpass, salt, "SHA1", 2, "OFRna73m*aze01xY", 256);
                string passEncr = AESEncryption.Encrypt(tPass.Text, decpass, salt, "SHA1", 2, "OFRna73m*aze01xY", 256);

                ((frmMain)this.Tag).UpdateAccountInfo(hostEncr, unEncr, passEncr, Convert.ToInt32(nPort.Value), "", ftporsftp);
                
                //FTPbox.Properties.Settings.Default.ftpHost = tHost.Text;
                //FTPbox.Properties.Settings.Default.ftpPort = Convert.ToInt32(nPort.Value);
                //FTPbox.Properties.Settings.Default.ftpUsername = tUsername.Text;
                //FTPbox.Properties.Settings.Default.ftpPass = tPass.Text;
                //FTPbox.Properties.Settings.Default.FTPorSFTP = ftporsftp;
                //FTPbox.Properties.Settings.Default.timedif = "";
                //FTPbox.Properties.Settings.Default.Save();

                
                Log.Write("got new ftp acccount details");
                ((frmMain)this.Tag).ClearLog();
                ((frmMain)this.Tag).UpdateDetails();
                //((frmMain)this.Tag).GetServerTime();
                ((frmMain)this.Tag).loggedIn = true;
                fNewDir fnewdir = new fNewDir();
                fnewdir.ShowDialog();
                this.Close();  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to FTP server. Check your account details and try again." + Environment.NewLine + " Error message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                     
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tHost.Text != "" && tUsername.Text != "" && tPass.Text != "")
            {
                bDone.Enabled = true;
            }
            else
            {
                bDone.Enabled = false;
            }
        }

        private void NewFTP_Load(object sender, EventArgs e)
        {
            tHost.Text = ((frmMain)this.Tag).ftpHost();
            tUsername.Text = ((frmMain)this.Tag).ftpUser();
            tPass.Text = ((frmMain)this.Tag).ftpPass();
            nPort.Value = 21;
            Set_Language(((frmMain)this.Tag).lang());

            if (((frmMain)this.Tag).FTP())
            {
                cMode.SelectedIndex = 0;
            }
            else
            {
                cMode.SelectedIndex = 1;
            }
        }

        private void Set_Language(string lan)
        {
            if (lan == "es")
            {
                this.Text = "FTPbox | Nueva cuenta FTP";
                gDetails.Text = "Datos de la cuenta FTP";
                labMode.Text = "Modo:";
                labHost.Text = "Host:";
                labPort.Text = "Puerto:";
                labUN.Text = "Usuario:";
                labPass.Text = "Contraseña";
                bDone.Text = "Listo";
            }
            else if (lan == "de")
            {
                this.Text = "FTPbox | Neuer FTP Account";
                gDetails.Text = "FTP login details";
                labMode.Text = "Modus:";
                labHost.Text = "Host:";
                labPort.Text = "Port:";
                labUN.Text = "Benutzername:";
                labPass.Text = "Passwort:";
                bDone.Text = "Fertig";

            }
            else if (lan == "fr")
            {
                this.Text = "FTPbox | Nouveau compte FTP";
                gDetails.Text = "Paramètres FTP";
                labMode.Text = "Mode:";
                labHost.Text = "Hôte:";
                labPort.Text = "Port:";
                labUN.Text = "Nom d'utilisateur:";
                labPass.Text = "Mot de passe:";
                bDone.Text = "Terminer";
            }
            else if (lan == "du")
            {
                this.Text = "FTPbox | Nieuwe FTP account";
                gDetails.Text = "FTP login details";
                labMode.Text = "Mode:";
                labHost.Text = "Host:";
                labPort.Text = "Poort:";
                labUN.Text = "Gebruikersnaam:";
                labPass.Text = "Wachtwoord:";
                bDone.Text = "Gereed";
            }
            else
            {
                this.Text = "FTPbox | New FTP Account";
                gDetails.Text = "FTP login details";
                labMode.Text = "Mode:";
                labHost.Text = "Host:";
                labPort.Text = "Port:";
                labUN.Text = "Username:";
                labPass.Text = "Password:";
                bDone.Text = "Done";
            }
        }

        ChannelSftp sftpc;
        private void sftp_login()
        {
            JSch jsch = new JSch();

            String host = tHost.Text;
            String user = tUsername.Text;

            Session session = jsch.getSession(user, host, 22);

            // username and password will be given via UserInfo interface.
            UserInfo ui = new MyUserInfo();

            session.setUserInfo(ui);

            session.setPort(Convert.ToInt32(nPort.Value));

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

        private void cMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cMode.SelectedIndex == 0)
            {
                nPort.Value = 21;
            }
            else
            {
                nPort.Value = 22;
            }
        }       

    }
}
