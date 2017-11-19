using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrightMaster
{
    public partial class QueryBarcode : Form
    {
        public QueryBarcode()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(txtBarcode.Text == "")
            {
                MessageBox.Show("条码不能为空！");
                return;
            }
            GlobalVars.Instance.Barcode = txtBarcode.Text;
            this.Close();
        }
    }
}
