using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrightMaster
{
    public partial class LiveFocusView : Form
    {
        

        private bool grab_images_;
        private BackgroundWorker grab_thread_;

        private Ua.Core uaCore = null;
  
        private Ua.Device device;
        private Ua.DeviceProperty device_property;
        Ua.CaptureData capture_data;
        private Bitmap bitmap_;
        Point ptStart, ptEnd;
        AutoResetEvent finishEvt = new AutoResetEvent(false);
        bool pause = false;
        public LiveFocusView()
        {
            InitializeComponent();
            this.Load += LivewFocusView_Load;
            this.FormClosing += LivewFocusView_FormClosing;
            this.MouseDown +=LivewFocusView_MouseDown;
           
            ptStart = ptEnd = new Point(-1, -1);
        }

      

        void LivewFocusView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                return;

            if(ptStart.X != -1 &&ptEnd.X != -1)
                ptStart = ptEnd = new Point(-1, -1);

            if (ptStart.X == -1)
                ptStart = e.Location;
            else
                ptEnd = e.Location;

            Invalidate();
        }

        void StartLiveview()
        {
            //GlobalVars.Instance.UAController.Initialize();
            GlobalVars.Instance.UAController.SetLiveviewMode();

            //get all variables
            device = GlobalVars.Instance.UAController.Device;
            //device_property = GlobalVars.Instance.UAController.DeviceProperty;
            uaCore = GlobalVars.Instance.UAController.UACore;
            capture_data = GlobalVars.Instance.UAController.CaptureData;

            //create bitmap
            Ua.Constants.Binning2Size.UaSize size = Ua.Constants.Binning2Size.UA_10[0];
            bitmap_ = new Bitmap(size.width, size.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //start
            uaCore.uaStartCapture(ref device);
            StartGrabLoop();
        }



        void LivewFocusView_Load(object sender, EventArgs e)
        {
            try
            {
                StartLiveview();
            }    
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

          
        }

        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
            pictureBox1.Image = bitmap_;
            pictureBox1.Invalidate();
        }



        private void LivewFocusView_FormClosing(object sender, FormClosingEventArgs e)
        {
           
            try
            {
                grab_thread_.CancelAsync();
            }
            catch (Exception)
            {
            }
            finishEvt.WaitOne();
           
            try
            {
                lock (this)
                {
                    GlobalVars.Instance.UAController.StopCapture();
                    //GlobalVars.Instance.UAController.UnInitialize();
                }
            }
            catch (Exception)
            {
            }
        }

        private void StartGrabLoop()
        {
            grab_images_ = true;
            grab_thread_ = new BackgroundWorker();
            grab_thread_.ProgressChanged += new ProgressChangedEventHandler(UpdateUI);
            grab_thread_.DoWork += new DoWorkEventHandler(GrabLoop);
            grab_thread_.WorkerReportsProgress = true;
            grab_thread_.WorkerSupportsCancellation = true;
            grab_thread_.RunWorkerAsync();
        }

        private void GrabLoop(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Pen pRectPen = new Pen(Color.FromArgb(255, Color.White), 4);
            Pen pFocusPen = new Pen(Color.FromArgb(255, Color.Red), 4);

            Ua.UaRect roi = device_property.focus_roi;

            while (grab_images_)
            {
                try
                {
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        finishEvt.Set();
                        break;
                    }

                    if(pause)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    lock (this)
                    {

                        uaCore.uaCaptureImage(
                            ref device, Ua.CaptureFilterType.UA_CAPTURE_FILTER_XYZ, 1, ref capture_data);

                        BitmapPlus bmpP = new BitmapPlus(bitmap_);
                        bmpP.BeginAccess();
                        unsafe
                        {
                            // If I can use unsafe. This code is more fast
                            ushort* x_ptr = (ushort*)capture_data.X_ptr;
                            ushort* y_ptr = (ushort*)capture_data.Y_ptr;
                            ushort* z_ptr = (ushort*)capture_data.Z_ptr;

                            // If I can not use unsafe.
                            //ushort[] x_ptr = Ua.Utility.PtrToUshort(capture_data_.X_ptr, capture_data_.size);
                            //ushort[] y_ptr = Ua.Utility.PtrToUshort(capture_data_.Y_ptr, capture_data_.size);
                            //ushort[] z_ptr = Ua.Utility.PtrToUshort(capture_data_.Z_ptr, capture_data_.size);

                            for (int y = 0; y < capture_data.height; y++)
                            {
                                for (int x = 0; x < capture_data.width; x++)
                                {
                                    int r = x_ptr[y * capture_data.width + x] / 256;
                                    int g = y_ptr[y * capture_data.width + x] / 256;
                                    int b = z_ptr[y * capture_data.width + x] / 256;
                                    bmpP.SetPixel(x, y, Color.FromArgb(r, g, b));
                                }
                            }
                        }
                        bmpP.EndAccess();



                        Graphics bitmap_graphics = Graphics.FromImage(bmpP.Bitmap);
                        if(ptStart.X != -1)
                        {
                            if(ptEnd.X == -1) //only start
                            {
                                bitmap_graphics.DrawEllipse(new Pen(Brushes.Red, 1), ptStart.X - 1, ptStart.Y - 1, ptStart.X + 1, ptStart.Y + 1);
                            }
                            else
                                bitmap_graphics.DrawRectangle(new Pen(Brushes.Red, 1), ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y);
                        }
                     
                        
                        bitmap_graphics.DrawRectangle(pRectPen,
                            roi.x, roi.y, roi.width, roi.height);

                        int posx = 0;
                        int posy = 0;
                        float fValue = 0;

                        // get focus value
                        if (uaCore.uaGetFocusValue(ref capture_data, ref fValue, ref posx, ref posy) !=
                          Ua.Constants.UA_FAILURE)
                        {
                            // Draw focus value  
                            bitmap_graphics.DrawEllipse(pFocusPen, posx - 2, posy - 2, 4, 4);
                        }

                        bitmap_graphics.Dispose();

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    continue;
                }

                worker.ReportProgress(0);

            }

            pRectPen.Dispose();
            pFocusPen.Dispose();
        }

        //private void btnPause_Click(object sender, EventArgs e)
        //{
        //    Debug.WriteLine("pause");
        //    pause = !pause;
        //    btnPause.Text = pause ? "恢复" : "暂停";

        //}

    }
}
