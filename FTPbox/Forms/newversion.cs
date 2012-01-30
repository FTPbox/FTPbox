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
            if (lan == "es")
            {
                this.Text = "FTPbox | Actualización disponible";
                labInfo.Text = "Una nueva versión de FTPbox está disponible";
                labCurVer.Text = "Versión actual";
                labNewVer.Text = "Nueva versión";
                labQuest.Text = "¿Deseas descargar la nueva versión ahora?";
                bDownload.Text = "Actualizar";
                bLearnMore.Text = "Conoce más";
                bClose.Text = "No esta vez";
            }
            else if (lan == "de")
            {
                this.Text = "FTPbox | Update verfuegbar";
                labInfo.Text = "Neue Version von FTPBox ist verfügbar";
                labCurVer.Text = "Aktuelle Version:";
                labNewVer.Text = "Neue Version:";
                labQuest.Text = "Wollen Sie die neue Version jetzt herunterladen?";
                bDownload.Text = "Aktuallisieren";
                bLearnMore.Text = "Mehr erfahren";
                bClose.Text = "Nicht dieses Mal";
            }
            else if (lan == "fr")
            {
                this.Text = "FTPbox | Mise à jour disponible";
                labInfo.Text = "Une nouvelle version de FTPbox est disponible";
                labCurVer.Text = "Version actuelle:";
                labNewVer.Text = "Nouvelle version:";
                labQuest.Text = "Souhaitez-vous télécharger la nouvelle version maintenant?";
                bDownload.Text = "Télécharger";
                bLearnMore.Text = "Plus d'informations";
                bClose.Text = "Pas maintenant";
            }
            else if (lan == "du")
            {
                this.Text = "FTPbox | Update beschikbaar";
                labInfo.Text = "Een nieuwe versie van FTPbox is beschikbaar";
                labCurVer.Text = "Huidige versie:";
                labNewVer.Text = "Nieuwe versie:";
                labQuest.Text = "Wilt u de nieuwste versie downloaden?";
                bDownload.Text = "Download";
                bLearnMore.Text = "Lees meer";
                bClose.Text = "Niet nu";
            }
            else
            {
                this.Text = "FTPbox | Update Available";
                labInfo.Text = "New version of FTPbox is available";
                labCurVer.Text = "Current Version:";
                labNewVer.Text = "New Version:";
                labQuest.Text = "Do you want to download the new version now?";
                bDownload.Text = "Update Now";
                bLearnMore.Text = "Learn More";
                bClose.Text = "Not this time";
            }
        }
    }
}
