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

namespace FTPbox
{
    public partial class NewFTP : Form
    {
        FtpConnection ftp;
        
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
                    ((frmMain)this.Tag).SetPass(tPass.Text);
                    //FTPbox.Properties.Settings.Default.ftpPass = tPass.Text;
                    sftp_login();
                    //MessageBox.Show("SFTP Connected");
                    sftpc.quit();
                }
                ((frmMain)this.Tag).UpdateAccountInfo(tHost.Text, tUsername.Text, tPass.Text, Convert.ToInt32(nPort.Value), "", ftporsftp);
                
                //FTPbox.Properties.Settings.Default.ftpHost = tHost.Text;
                //FTPbox.Properties.Settings.Default.ftpPort = Convert.ToInt32(nPort.Value);
                //FTPbox.Properties.Settings.Default.ftpUsername = tUsername.Text;
                //FTPbox.Properties.Settings.Default.ftpPass = tPass.Text;
                //FTPbox.Properties.Settings.Default.FTPorSFTP = ftporsftp;
                //FTPbox.Properties.Settings.Default.timedif = "";
                //FTPbox.Properties.Settings.Default.Save();

            }
            catch
            {
                MessageBox.Show("Could not connect to FTP server. Check your account details and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Log.Write("got new ftp acccount details");
            ((frmMain)this.Tag).ClearLog();
            ((frmMain)this.Tag).UpdateDetails();
            //((frmMain)this.Tag).GetServerTime();
            ((frmMain)this.Tag).loggedIn = true;
            fNewDir fnewdir = new fNewDir();
            fnewdir.ShowDialog();
            this.Close();           
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

            session.connect();

            Channel channel = session.openChannel("sftp");
            channel.connect();
            sftpc = (ChannelSftp)channel;

        }

        public class MyUserInfo : UserInfo
        {
            FTPbox.Classes.Settings sets = new FTPbox.Classes.Settings();
            public String getPassword() { return sets.Get("Account/Password", ""); }
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

        private void cMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cMode.SelectedIndex == 0)
            {
                nPort.Enabled = true;
                nPort.Value = 21;
            }
            else
            {
                nPort.Enabled = false;
                nPort.Value = 22;
            }
        }       

    }
}
