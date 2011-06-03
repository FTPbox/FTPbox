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
        }

        private void bDownload_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://sourceforge.net/projects/ftpbox");
            }
            catch { }
            this.Close();
            Application.Exit();
        }

        private void bLearnMore_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://sharpmindprojects.com/project/ftpbox");
            }
            catch { }
            this.Close();
            Application.Exit();
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
