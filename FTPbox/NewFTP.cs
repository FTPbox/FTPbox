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
            try {
            ftp = new FtpConnection(tHost.Text, Convert.ToInt32(nPort.Value), tUsername.Text, tPass.Text);
            ftp.Open();
            ftp.Login();

            FTPbox.Properties.Settings.Default.ftpHost = tHost.Text;
            FTPbox.Properties.Settings.Default.ftpPort = Convert.ToInt32(nPort.Value);
            FTPbox.Properties.Settings.Default.ftpUsername = tUsername.Text;
            FTPbox.Properties.Settings.Default.ftpPass = tPass.Text;
            FTPbox.Properties.Settings.Default.Save();

            }
            catch
            {
                MessageBox.Show("Could not connect to FTP server. Check your account details and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ((frmMain)this.Tag).UpdateDetails();
            ((frmMain)this.Tag).loggedIn = true;
            //((fMain)this.Tag).Update_Acc_info(tHost.Text, tUsername.Text, tPass.Text, Convert.ToInt32(nPort.Value));

            ftp.Close();

            this.Close();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tHost.Text != "" && tUsername.Text != "" && tPass.Text != "" && nPort.Value > 0)
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
            tHost.Text = FTPbox.Properties.Settings.Default.ftpHost;
            tUsername.Text = FTPbox.Properties.Settings.Default.ftpUsername;
            tPass.Text = FTPbox.Properties.Settings.Default.ftpPass;
            nPort.Value = Convert.ToDecimal(FTPbox.Properties.Settings.Default.ftpPort);
        }

    }
}
