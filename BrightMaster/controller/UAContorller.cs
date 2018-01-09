using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ua;

namespace BrightMaster
{
    class UAContorller
    {
        Ua.Core uaCore = null;
        IntPtr device_ptr = IntPtr.Zero;
        IntPtr system_ptr = IntPtr.Zero;
        IntPtr recipe_ptr = IntPtr.Zero;
        Device device;
        Recipe recipe;
        IntPtr xyz_image_ptr = IntPtr.Zero;
        Ua.XYZImage xyz_image;
        DeviceProperty device_property;
        bool initialized = false;

        IntPtr capture_data_ptr;
        Ua.CaptureData capture_data;
        public  delegate void DelegateInitFinish(bool bok, string errMsg);
        public event DelegateInitFinish onInitialFinished;
        List<string> validSNs = new List<string>()
        {
            "11130020","10770200"
        };
        public async Task Initialize()
        {
            await Task.Run(() =>
            {
                if (initialized)
                {
                    NotifyInitialFinish(true);
                    return;
                }
                    
                try{
                    uaCore = new Ua.Core();
                    string keyFolder = GlobalVars.Instance.ParamPath + "UA-10\\SL";
                    var allSns = Directory.EnumerateDirectories(keyFolder);
                    if (allSns.Count() == 0 || IsInvalid(validSNs, allSns.First()))
                        throw new Exception("初始化失败！");
                    
                    
                    system_ptr = uaCore.uaInitialize(GlobalVars.Instance.ParamPath);
                    Ua.System ua_system = Ua.Utility.PtrToUaSystem(system_ptr);
                    Ua.Configuration[] configuration = { ua_system.ua_10 };
                    for (int i = 0; i < configuration.Length; i++)
                    {
                        for (int j = 0; j < configuration[i].num_connected; j++)
                        {
                            device_ptr = uaCore.uaOpenDevice(configuration[i].connected_product_ids[j]);
                            if (device_ptr != IntPtr.Zero)
                            {
                                break;
                            }
                        }
                    }
                    Ua.UaError n_err_code = uaCore.uaGetError();
                    if (n_err_code != Ua.UaError.UA_NO_ERROR)
                    {
                        throw new Exception(uaCore.uaGetErrorString(n_err_code));
                    }
                    device = Ua.Utility.PtrToUaDevice(device_ptr);
                    recipe_ptr = uaCore.uaCreateRecipe(device.type);
                    recipe = Ua.Utility.PtrToUaRecipe(recipe_ptr);

                    //uaCore.uaDestroyRecipe(recipe_ptr);
                    device_property = Ua.Utility.PtrToUaDeviceProperty(recipe.property_ptr);
                    uaCore.uaGetDeviceProperty(ref device, ref device_property);
                    //复用一个xyz_image
                    xyz_image_ptr = uaCore.uaCreateXYZImage(device.type, Ua.DataType.UA_DATA_TRISTIMULUS_XYZ);
                    xyz_image = Ua.Utility.PtrToUaXYZImage(xyz_image_ptr);
                
                    initialized = true;
                    NotifyInitialFinish(true);
                }
                catch(Exception ex)
                {
                    NotifyInitialFinish(false, ex.Message);
                }
                
            });
        }

        private bool IsInvalid(List<string> validSNs, string curSN)
        {
            FileInfo fileInfo = new FileInfo(curSN);
            return !validSNs.Contains(fileInfo.Name);
        }

        private void NotifyInitialFinish(bool bok,string errMsg = "")
        {
            if (onInitialFinished != null)
                onInitialFinished(bok,errMsg);
        }

        
        public void SetLiveviewMode()
        {
            capture_data_ptr = uaCore.uaCreateCaptureData(device.type);
            capture_data = Ua.Utility.PtrToUaCaptureData(capture_data_ptr);
            // set capture mode parameter
          
            device_property.capture_mode = Ua.CaptureMode.UA_CAPTURE_FOCUS;
            // Focus Rect
            Ua.UaRect roi;
            roi.width = capture_data.width / 4;
            roi.height = capture_data.height / 4;
            roi.x = capture_data.width / 2 - roi.width / 2;
            roi.y = capture_data.height / 2 - roi.height / 2;
            device_property.focus_roi = roi;

            // set device Property
            int ret = uaCore.uaSetDeviceProperty(ref device, ref device_property);
     
            Ua.UaError err = uaCore.uaGetError();
       
        }


        public void UnInitialize()
        {
            if (uaCore == null)
                return;

            if (xyz_image_ptr != IntPtr.Zero)
                uaCore.uaDestroyXYZImage(xyz_image_ptr);
            if (recipe_ptr != IntPtr.Zero)
                uaCore.uaDestroyRecipe(recipe_ptr);

            if(device_ptr != IntPtr.Zero)
            {
                uaCore.uaCloseDevice(device_ptr);
            }
            uaCore.uaFinalize(system_ptr);
            initialized = false;
        }


        LightPixelInfo[,] GetData(XYZImage img)
        {
            if (img.Y_ptr == IntPtr.Zero)
                throw new Exception("Invalid image.");
           
            float[] y_ptr = Ua.Utility.PtrToFloat(img.Y_ptr, img.size);
            float[] x_ptr = Ua.Utility.PtrToFloat(img.X_ptr, img.size);
            float[] z_ptr = Ua.Utility.PtrToFloat(img.Z_ptr, img.size);
            LightPixelInfo[,] allPixelInfos = new LightPixelInfo[img.height, img.width];
            int ID = 1;
            int height = img.height;
            int width = img.width;

            //unsafe
            {
                Parallel.Invoke(() =>
                {
                    GetPixelInfos(allPixelInfos, x_ptr, y_ptr, z_ptr, 0, height / 4, width, height);
                },
                () =>
                {
                    GetPixelInfos(allPixelInfos, x_ptr, y_ptr, z_ptr, height / 4, height / 2, width, height);
                },
                () =>
                {
                    GetPixelInfos(allPixelInfos, x_ptr, y_ptr, z_ptr, height / 2, height * 3 / 4, width, height);
                },
                () =>
                {
                    GetPixelInfos(allPixelInfos, x_ptr, y_ptr, z_ptr, height * 3 / 4, height, width, height);
                });
                //Parallel.For(0, height, y =>
                //{
                //    List<PixelInfo> lineInfos = new List<PixelInfo>();
                //    int startPos = y * width;
                //    for (int x = 0; x < width; x++)
                //    {
                //        int curIndex = startPos + x;
                //        allPixelInfos[y, x] = new PixelInfo(ID++, x_ptr[curIndex], y_ptr[curIndex], z_ptr[curIndex]);
                //    }
                //});
            }
          
           
            return allPixelInfos;
        }

        private void GetPixelInfos(LightPixelInfo[,] allPixelInfos, 
            float[] x_ptr, 
            float[] y_ptr, 
            float[] z_ptr, 
            int startY, 
            int endY,
            int width, int height)
        {
            int ID = startY * width +1;

            for( int y = startY; y < endY; y++)
            {
                List<LightPixelInfo> lineInfos = new List<LightPixelInfo>();
                int startPos = y * width;
                for (int x = 0; x < width; x++)
                {
                    int curIndex = startPos + x;
                    allPixelInfos[y, x] = new LightPixelInfo(x_ptr[curIndex], y_ptr[curIndex], z_ptr[curIndex]);
                }
            }
        }

        

        public void SetMannualMode()
        {
            Ua.OptimizationCondition cond = Ua.OptimizationCondition.UA_OPTIMIZE_COND_GAIN_FIX_ND_FIX;
            if (uaCore.uaIsUA10(device.type) == Ua.Constants.UA_TRUE)
            {
                cond = Ua.OptimizationCondition.UA_OPTIMIZE_COND_GAIN_FIX_ND_FIX;
            }
            // Console.WriteLine("uaOptimizeDeviceProperty");
            uaCore.uaOptimizeDeviceProperty(
                ref device, cond, ref device_property);
            
            device_property.capture_mode = Ua.CaptureMode.UA_CAPTURE_MANUAL;
           
            //set distance & exposure time
            
            //uaCore.uaGetOptimumAverageCount(
            //    ref device, 0, 40, ref average_count);
            if (!GlobalVars.Instance.CameraSettings.AutoExposure)
            {
                for (int i = 0; i < device_property.exposure_time.Count(); i++)
                    device_property.exposure_time[i] = GlobalVars.Instance.CameraSettings.ExposureTime;
            }

            device_property.measurement_distance = GlobalVars.Instance.CameraSettings.WorkingDistance;

            // Console.WriteLine("uaSetDeviceProperty");
            uaCore.uaSetDeviceProperty(ref device, ref device_property);
            capture_data_ptr = uaCore.uaCreateCaptureData(device.type);
            capture_data = Ua.Utility.PtrToUaCaptureData(capture_data_ptr);
        }

        public LightPixelInfo[,] Acquire()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (device_ptr == IntPtr.Zero)
                throw new Exception("Not initialized!");

            SetMannualMode();
         
            Stopwatch watch = new Stopwatch();
            watch.Start();
            uaCore.uaStartCapture(ref device);
            string sAvgCnt = ConfigurationManager.AppSettings["AvgCount"];
            int average_count = int.Parse(sAvgCnt);
            double expo = device_property.exposure_time[0];
            uaCore.uaCaptureImage(ref device,
                Ua.CaptureFilterType.UA_CAPTURE_FILTER_XYZ, 
                average_count, 
                ref capture_data);
            
            // Console.WriteLine("uaStopCapture");
            uaCore.uaStopCapture(ref device);
            Debug.WriteLine("used time:" + watch.ElapsedMilliseconds+"ms");
            
            uaCore.uaToXYZImage(ref device, ref capture_data, ref xyz_image);
            //double buildInRatio = 0.718;
            uaCore.uaCorrectColor(ref device, ref xyz_image, Ua.ColorCorrectionType.UA_COLOR_CORRECTION_LED);

            //User color correction
            uaCore.uaCorrectXYZImageLevel(ref xyz_image, GlobalVars.Instance.AdjustRatio.XRatio, GlobalVars.Instance.AdjustRatio.YRatio, GlobalVars.Instance.AdjustRatio.ZRatio);

            uaCore.uaDestroyCaptureData(capture_data_ptr);
            var allPixels = GetData(xyz_image);
            Debug.WriteLine("used time 2:" + watch.ElapsedMilliseconds + "ms");
            watch.Stop();
            return allPixels;
           
        }

        public LightPixelInfo[,] LoadXYZImage(string imgName)
        {
            uaCore.uaLoadMeasurementData(imgName,ref xyz_image_ptr,ref recipe_ptr);
            xyz_image = Ua.Utility.PtrToUaXYZImage(xyz_image_ptr);
            return GetData(xyz_image);
        }

        public bool HaveXYZImage
        {
            get
            {
                return xyz_image_ptr != IntPtr.Zero;
            }
        }
        public void SaveXYZImage(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            uaCore.uaSaveMeasurementData(fileInfo.DirectoryName, fileInfo.Name, ref xyz_image, ref recipe);

        }

        public Device  Device
        {
            get
            {
                return device;
            }
        }
       
        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public Core UACore
        {
            get
            {
                return uaCore;
            }
        }

        public CaptureData CaptureData
        {
            get
            {
                return capture_data;
            }
        }

        internal void StopCapture()
        {
            uaCore.uaStopCapture(ref device);
            //uaCore.uaDestroyCaptureData(capture_data_ptr);
            Ua.UaError n_err_code = uaCore.uaGetError();
            if (n_err_code != Ua.UaError.UA_NO_ERROR)
            {
                throw new Exception(uaCore.uaGetErrorString(n_err_code));
            }
        }
    }
}
