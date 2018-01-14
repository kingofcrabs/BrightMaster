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
    public partial class DrawPts : Control
    {
        List<PointF> orgPts = new List<PointF>();
        List<PointF> newPts = new List<PointF>();
        public DrawPts()
        {
            InitializeComponent();
        }

        public void AddOrgPt(PointF orgPt)
        {
            orgPts.Add(orgPt);
            Invalidate();
        }

        public void AddNewPt(PointF newPt)
        {
            newPts.Add(newPt);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < orgPts.Count; i++)
            {
                SolidBrush brush = new SolidBrush(Color.Blue);
                pe.Graphics.DrawEllipse(new Pen(brush, 2), new Rectangle((int)orgPts[i].X, (int)orgPts[i].Y, 1, 1));

            }

            for (int i = 0; i < newPts.Count; i++)
            {
                SolidBrush brush = new SolidBrush(Color.Red);
                pe.Graphics.DrawEllipse(new Pen(brush, 2), new Rectangle((int)newPts[i].X, (int)newPts[i].Y, 1, 1));

            }
        }
    }
}
