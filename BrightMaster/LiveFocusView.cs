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
    public partial class LivewFocusView : Form
    {
        private const string parameter_directory_ = "..\\..\\param";

        private bool grab_images_;
        private BackgroundWorker grab_thread_;

        private Ua.Core uaCore_ = null;
        private IntPtr system_ptr_;
        private IntPtr device_ptr_;
        private Ua.Device device_;
        private IntPtr recipe_ptr_;
        private Ua.Recipe recipe_;
        private Ua.DeviceProperty device_property_;
        private IntPtr capture_data_ptr_;
        private Ua.CaptureData capture_data_;

        private Bitmap bitmap_;

        public LivewFocusView()
        {
            InitializeComponent();
        }

        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
            pictureBox1.Image = bitmap_;
            pictureBox1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                uaCore_ = new Ua.Core();

                system_ptr_ = uaCore_.uaInitialize(parameter_directory_);

                Ua.System ua_system = Ua.Utility.PtrToUaSystem(system_ptr_);
                Ua.Configuration[] configuration = { ua_system.ua_10, ua_system.ua_200, ua_system.ua_200A };

                device_ptr_ = IntPtr.Zero;

                for (int i = 0; i < configuration.Length; i++)
                {
                    for (int j = 0; j < configuration[i].num_connected; j++)
                    {
                        device_ptr_ = uaCore_.uaOpenDevice(configuration[i].connected_product_ids[j]);

                        if (device_ptr_ != IntPtr.Zero)
                        {
                            break;
                        }
                    }
                }

                if (device_ptr_ != IntPtr.Zero)
                {
                    device_ = Ua.Utility.PtrToUaDevice(device_ptr_);

                    if (uaCore_.uaIsUA200A(device_.type) == Ua.Constants.UA_TRUE)
                    {
                        Ua.Constants.Binning2Size.UaSize size = Ua.Constants.Binning2Size.UA_200A[0];

                        bitmap_ = new Bitmap(size.width, size.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    else if (uaCore_.uaIsUA200(device_.type) == Ua.Constants.UA_TRUE)
                    {
                        Ua.Constants.Binning2Size.UaSize size = Ua.Constants.Binning2Size.UA_200[0];

                        bitmap_ = new Bitmap(size.width, size.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                    else
                    {
                        Ua.Constants.Binning2Size.UaSize size = Ua.Constants.Binning2Size.UA_10[0];

                        bitmap_ = new Bitmap(size.width, size.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }

                    recipe_ptr_ = uaCore_.uaCreateRecipe(device_.type);

                    recipe_ = Ua.Utility.PtrToUaRecipe(recipe_ptr_);

                    device_property_ = Ua.Utility.PtrToUaDeviceProperty(recipe_.property_ptr);

                    uaCore_.uaGetDeviceProperty(ref device_, ref device_property_);

                    capture_data_ptr_ = uaCore_.uaCreateCaptureData(device_.type);

                    capture_data_ = Ua.Utility.PtrToUaCaptureData(capture_data_ptr_);

                    // set capture mode parameter
                    device_property_.capture_mode = Ua.CaptureMode.UA_CAPTURE_FOCUS;

                    // Focus Rect
                    Ua.UaRect roi;
                    roi.width = capture_data_.width / 4;
                    roi.height = capture_data_.height / 4;
                    roi.x = capture_data_.width / 2 - roi.width / 2;
                    roi.y = capture_data_.height / 2 - roi.height / 2;

                    device_property_.focus_roi = roi;

                    // set device Property
                    int ret = uaCore_.uaSetDeviceProperty(ref device_, ref device_property_);

                    Ua.UaError err = uaCore_.uaGetError();

                    ret = uaCore_.uaStartCapture(ref device_);

                    err = uaCore_.uaGetError();

                    StartGrabLoop();
                }
            }
            catch (System.DllNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Ua.DllVersionException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device_ptr_ != IntPtr.Zero)
            {
                try
                {
                    grab_thread_.CancelAsync();
                }
                catch (Exception)
                {
                }
                System.Threading.Thread.Sleep(100);
            }

            try
            {
                lock (this)
                {
                    if (uaCore_ != null)
                    {
                        if (device_ptr_ != IntPtr.Zero)
                        {
                            uaCore_.uaStopCapture(ref device_);
                            uaCore_.uaDestroyCaptureData(capture_data_ptr_);
                            uaCore_.uaDestroyRecipe(recipe_ptr_);
                            uaCore_.uaCloseDevice(device_ptr_);
                            device_ptr_ = IntPtr.Zero;
                        }
                        uaCore_.uaFinalize(system_ptr_);
                    }
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

            Ua.UaRect roi = device_property_.focus_roi;

            while (grab_images_)
            {
                try
                {

                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        break;
                    }

                    lock (this)
                    {


                        if (device_ptr_ == IntPtr.Zero)
                        {
                            break;
                        }


                        uaCore_.uaCaptureImage(
                            ref device_, Ua.CaptureFilterType.UA_CAPTURE_FILTER_XYZ, 1, ref capture_data_);

                        ushort[] x_ptr = Ua.Utility.PtrToUshort(capture_data_.X_ptr, capture_data_.size);
                        ushort[] y_ptr = Ua.Utility.PtrToUshort(capture_data_.Y_ptr, capture_data_.size);
                        ushort[] z_ptr = Ua.Utility.PtrToUshort(capture_data_.Z_ptr, capture_data_.size);

                        for (int y = 0; y < capture_data_.height; y++)
                        //   for (int y = roi.y; y < roi.y + roi.height; y++)
                        {
                            for (int x = 0; x < capture_data_.width; x++)
                            //      for (int x = roi.x; x < roi.x + roi.width; x++)
                            {
                                int r = x_ptr[y * capture_data_.width + x] / 256;
                                int g = y_ptr[y * capture_data_.width + x] / 256;
                                int b = z_ptr[y * capture_data_.width + x] / 256;
                                bitmap_.SetPixel(x, y, Color.FromArgb(r, g, b));
                            }
                        }

                        Graphics bitmap_graphics = Graphics.FromImage(bitmap_);

                        bitmap_graphics.DrawRectangle(pRectPen,
                            roi.x, roi.y, roi.width, roi.height);

                        int posx = 0;
                        int posy = 0;
                        float fValue = 0;

                        // get focus value
                        if (uaCore_.uaGetFocusValue(ref capture_data_, ref fValue, ref posx, ref posy) !=
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

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
