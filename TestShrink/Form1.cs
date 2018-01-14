using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            var lines = File.ReadAllLines("d:\\orgPts.txt").ToList();
            
            foreach(var line in lines)
            {
                //string s = txtNewPt.Text;
                if (line == "")
                    continue;
                string[] strs = line.Split(',');
                float x = float.Parse(strs[0]);
                float y = float.Parse(strs[1]);
                drawPts1.AddOrgPt(new PointF(x, y));
            }
            lines = File.ReadAllLines("d:\\newPts.txt").ToList();
            foreach (var line in lines)
            {
                if (line == "")
                    continue;
                //string s = txtNewPt.Text;
                string[] strs = line.Split(',');
                float x = float.Parse(strs[0]);
                float y = float.Parse(strs[1]);
                drawPts1.AddNewPt(new PointF(x, y));
            }
          
        }
    }
}
