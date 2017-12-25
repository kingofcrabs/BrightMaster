using BrightMaster.data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrightMaster
{
    public partial class ColumnVisibilityForm : Form
    {
        public ColumnVisibilityForm()
        {
            InitializeComponent();
            this.Load += ColumnVisibilityForm_Load;
        }

        void ColumnVisibilityForm_Load(object sender, EventArgs e)
        {

            chkID.Checked = GlobalVars.Instance.GridColumnWidth.ID != 0;
            chkX.Checked  = GlobalVars.Instance.GridColumnWidth.X  != 0;
            chkY.Checked  = GlobalVars.Instance.GridColumnWidth.Y !=0;
            chkZ.Checked = GlobalVars.Instance.GridColumnWidth.Z != 0;
            chk_x.Checked = GlobalVars.Instance.GridColumnWidth.x != 0;
            chk_y.Checked = GlobalVars.Instance.GridColumnWidth.y != 0;
            chku.Checked = GlobalVars.Instance.GridColumnWidth.u != 0;
            chkv.Checked = GlobalVars.Instance.GridColumnWidth.v != 0;
            chkL.Checked = GlobalVars.Instance.GridColumnWidth.L != 0;

        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            GlobalVars.Instance.GridColumnWidth.ID = chkID.Checked ? 40 : 0;
            GlobalVars.Instance.GridColumnWidth.X = chkX.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.Y = chkY.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.Z = chkZ.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.x = chk_x.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.y = chk_y.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.u = chku.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.v = chkv.Checked ? 60 : 0;
            GlobalVars.Instance.GridColumnWidth.L = chkL.Checked ? 60 : 0;
            this.Close();
        }
    }
}
