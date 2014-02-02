/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* fAccountDetails.cs
 * Display account details in this form.
 * TODO: Allow profile editing
 */

using System;
using System.Linq;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class fAccountDetails : Form
    {
        public fAccountDetails()
        {
            InitializeComponent();
        }

        private void fAccountDetails_Load(object sender, EventArgs e)
        {
            Set_Language(Settings.General.Language);

            this.Text = Settings.ProfileTitles[Settings.General.DefaultProfile];

            lHost.Text = Program.Account.Account.Host;
            lUsername.Text = Program.Account.Account.Username;
            lPort.Text = Program.Account.Account.Port.ToString();
            lMode.Text = (Program.Account.Account.Protocol != FtpProtocol.SFTP) ? "FTP" : "SFTP";

            lRemPath.Text = Program.Account.Paths.Remote;
            lLocPath.Text = Program.Account.Paths.Local;
            tParent.Text = Program.Account.Paths.Parent;
        }

        private void Set_Language(string lan)
        {
            gAccount.Text = Common.Languages[UiControl.Account];
            labUN.Text = Common.Languages[UiControl.Username];
            labPort.Text = Common.Languages[UiControl.Port];
            labMode.Text = Common.Languages[UiControl.Protocol];

            gPaths.Text = Common.Languages[UiControl.Details];
            labRemPath.Text = Common.Languages[UiControl.RemotePath] + ":";
            labLocPath.Text = Common.Languages[UiControl.LocalFolder] + ":";
            labFullPath.Text = Common.Languages[UiControl.FullAccountPath];

            bDone.Text = Common.Languages[UiControl.Finish];

            // Is this a right-to-left language?
            RightToLeftLayout = new[] { "he" }.Contains(lan);
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            Program.Account.Paths.Parent = tParent.Text;
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Settings.SaveProfile();
            Hide();
        }

        private void fAccountDetails_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            gAccount.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            gPaths.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
        }
    }
}
