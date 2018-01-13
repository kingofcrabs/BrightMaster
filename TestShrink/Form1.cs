using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestShrink
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string s = txtNewPt.Text;
            string[] strs = s.Split(',');
            float x = float.Parse(strs[0]);
            float y = float.Parse(strs[1]);
            drawPts1.AddPt(new PointF(x,y));
          
        }
    }
}
