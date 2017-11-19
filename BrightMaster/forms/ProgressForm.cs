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
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            Application.EnableVisualStyles();
            InitializeComponent();
            this.Load += ProgressForm_Load;
        }

        void ProgressForm_Load(object sender, EventArgs e)
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            
        }
    }
}
