using System;
using System.Drawing;
using System.Windows.Forms;

namespace FTPbox.Forms
{
    public partial class trayFormListItem : UserControl
    {
        public trayFormListItem()
        {
            InitializeComponent();
            
            this.BackColor = Color.Transparent;
        }

        /// <summary>
        /// The name of the file this recent item refers to
        /// </summary>
        public string FileNameLabel
        {
            set
            {
                lFileName.Text = value;
                pbFileIcon.Image = Win32.GetFileIcon(value).ToBitmap();
            }
        }

        /// <summary>
        /// The status of the item, will either include transfer status or last modified time
        /// </summary>
        public string FileStatusLabel { set { lStatusLabel.Text = value; } }        

        /// <summary>
        /// The format of the status label when the item is being transfered, ie 'Downloading {0}'
        /// </summary>
        public string SubTitleFormat { get; set; }

        /// <summary>
        /// Make sure Click is raised no matter where the user clicks
        /// </summary>
        public new event EventHandler Click
        {
            add
            {
                base.Click += value;
                foreach (Control control in Controls)
                    control.Click += value;
            }
            remove
            {
                base.Click -= value;
                foreach (Control control in Controls)
                    control.Click -= value;
            }
        }
    }
}
