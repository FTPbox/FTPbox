using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace FTPbox
{
    public partial class newversion : Form
    {
        string newvers;

        public newversion(string newv)
        {
            InitializeComponent();
            newvers = newv;
        }

        private void newversion_Load(object sender, EventArgs e)
        {
            label3.Text = Application.ProductVersion.Substring(0, 5);
            label5.Text = newvers.Substring(0, 5);
            Set_Language(FTPbox.Properties.Settings.Default.lan);
        }

        private void bDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string fpathtoexe = Application.StartupPath + @"\updater.exe";
                Process.Start(fpathtoexe);
            }
            catch { }
            //this.Close();
            try
            {
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
            catch { }
            Application.Exit();
        }

        private void bLearnMore_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://ftpbox.org/changelog/");
            }
            catch { }
            this.Close();
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Set_Language(string lan)
        {
            string qmark = "?";
            if (lan == "el") qmark = ";";

            this.Text = "FTPbox | " + ((frmMain)this.Tag).languages.Get(lan + "/new_version/update_available", "Update Available");
            labInfo.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/new_v_available", "New version of FTPbox is available");
            labCurVer.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/current_version", "Current Version") + ":";
            labNewVer.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/new_ver", "New Version") + ":";
            labQuest.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/wanna_download", "Do you want to download the new version now") + qmark;
            bDownload.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/download", "Update Now");
            bLearnMore.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/learn_more", "Learn More");
            bClose.Text = ((frmMain)this.Tag).languages.Get(lan + "/new_version/remind_me_next_time", "Not this time");                 
        }
    }
}
