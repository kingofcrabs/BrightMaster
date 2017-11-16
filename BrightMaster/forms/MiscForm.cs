using BrightMaster.settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrightMaster.forms
{
    public partial class MiscForm : Form
    {
        public MiscForm()
        {
            InitializeComponent();
            txtSaveFolder.Text = GlobalVars.Instance.MiscSettings.SaveFolder;
            chkAutoFindBound.Checked = GlobalVars.Instance.MiscSettings.AutoFindBoundary;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDlg = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(folderBrowserDlg.SelectedPath == "")
                {
                    return;
                }
                GlobalVars.Instance.MiscSettings.SaveFolder = folderBrowserDlg.SelectedPath;
            }
            txtSaveFolder.Text = GlobalVars.Instance.MiscSettings.SaveFolder;
            GlobalVars.Instance.MiscSettings.Save();
        }

        private void chkAutoFindBound_CheckedChanged(object sender, EventArgs e)
        {
            GlobalVars.Instance.MiscSettings.AutoFindBoundary = chkAutoFindBound.Checked;
            GlobalVars.Instance.MiscSettings.Save();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
