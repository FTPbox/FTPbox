/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Account.cs
 * The form used by the user to set the FTP/SFTP account info
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class Account : Form
    {
        static readonly string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);      

        public Account()
        {
            InitializeComponent();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Log.Write(l.Debug, AppPath);
            bool ftporsftp = cMode.SelectedIndex == 0;
            bool ftps = cMode.SelectedIndex == 0 && cEncryption.SelectedIndex != 0;
            bool ftpes = cEncryption.SelectedIndex == 1;

            Program.Account.AddAccount(tHost.Text, tUsername.Text, tPass.Text, Convert.ToInt32(nPort.Value));
            if (ftporsftp && ftps)
                Program.Account.Account.Protocol = FtpProtocol.FTPS;
            else if (ftporsftp)
                Program.Account.Account.Protocol = FtpProtocol.FTP;
            else
                Program.Account.Account.Protocol = FtpProtocol.SFTP;

            if (!ftps)
                Program.Account.Account.FtpsMethod = FtpsMethod.None;
            else if (ftpes)
                Program.Account.Account.FtpsMethod = FtpsMethod.Explicit;
            else
                Program.Account.Account.FtpsMethod = FtpsMethod.Implicit;
            
            try
            {
                Program.Account.Client.Connect();
                Log.Write(l.Debug, "Connected: {0}", Program.Account.Client.isConnected);

                Settings.AskForPassword = cAskForPass.Checked;

                Hide();
            }
            catch (Exception ex)
            {                
                MessageBox.Show("Could not connect to FTP server. Check your account details and try again."
                    + Environment.NewLine + " Error message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool just_password = false;
        private void Account_Load(object sender, EventArgs e)
        {
            cAskForPass.Checked = just_password;
            cEncryption.SelectedIndex = 0;
            cMode.SelectedIndex = 0;
            Set_Language(Settings.General.Language);

            if (just_password && Program.Account.isAccountSet)
            {
                tHost.Text = Program.Account.Account.Host;
                tUsername.Text = Program.Account.Account.Username;
                nPort.Value = Program.Account.Account.Port;
                cEncryption.SelectedIndex = (Program.Account.Account.Protocol != FtpProtocol.FTPS) ? 0 : (Program.Account.Account.FtpsMethod == FtpsMethod.Explicit ? 1 : 2);
                cMode.SelectedIndex = (Program.Account.Account.Protocol != FtpProtocol.SFTP) ? 0 : 1;

                ActiveControl = tPass;
            }
        }

        private void cMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cMode.SelectedIndex != 1)
            {
                nPort.Value = 21;
                cEncryption.Enabled = true;
            }
            else
            {
                nPort.Value = 22;
                cEncryption.SelectedIndex = 0;
                cEncryption.Enabled = false;
            }
        }

        /// <summary>
        /// set the language of the form's labels etc
        /// </summary>
        /// <param name="lan"></param>
        private void Set_Language(string lan)
        {
            Log.Write(l.Info, "Setting lang: {0}", lan);
            Text = "FTPbox | " + Common.Languages.Get(lan + "/new_account/new_ftp", "New FTP Account");
            gDetails.Text = Common.Languages.Get(lan + "/new_account/login_details", "FTP login details");
            labMode.Text = Common.Languages.Get(lan + "/main_form/mode", "Protocol") + ":";
            labEncryption.Text = Common.Languages.Get(lan + "/new_account/encryption", "Encryption") + ":";
            labHost.Text = Common.Languages.Get(lan + "/main_form/host", "Host") + ":";
            labPort.Text = Common.Languages.Get(lan + "/main_form/port", "Port") + ":";
            labUN.Text = Common.Languages.Get(lan + "/main_form/username", "Username") + ":";
            labPass.Text = Common.Languages.Get(lan + "/main_form/password", "Password") + ":";
            bDone.Text = Common.Languages.Get(lan + "/new_account/done", "Done");
            
            // Is this a right-to-left language?            
            RightToLeftLayout = new[] { "he" }.Contains(lan);
        }

        private void Account_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                ((fMain)Tag).ExitedFromTray = true;
                ((fMain)Tag).KillTheProcess();
            }
        }

        private void cAskForPass_CheckedChanged(object sender, EventArgs e)
        {
            just_password = cAskForPass.Checked;
        }

        private void Account_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            // Inherit manually
            gDetails.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            cAskForPass.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            // Relocate controls where necessary
            bDone.Location = RightToLeftLayout ? new Point(12, 206) : new Point(301, 206);
            cMode.Location = RightToLeftLayout ? new Point(210, 24) : new Point(100, 24);
            tHost.Location = RightToLeftLayout ? new Point(9, 78) : new Point(100, 78);
            tUsername.Location = RightToLeftLayout ? new Point(9, 104) : new Point(100, 104);
            tPass.Location = RightToLeftLayout ? new Point(9, 130) : new Point(100, 130);
            nPort.Location = RightToLeftLayout ? new Point(215, 156) : new Point(100, 157);
            bDone.Location = RightToLeftLayout ? new Point(12, 206) : new Point(301, 206);
        }
    }
}
