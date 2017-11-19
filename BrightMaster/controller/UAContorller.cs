using System;
using System.Collections.Generic;
using System.Configuration;
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
        public event EventHandler<bool> onInitialFinished;
        public async Task Initialize()
        {
            await Task.Run(() =>
            {
                if (initialized)
                {
                    NotifyInitialFinish();
                    return;
                }
                    

                uaCore = new Ua.Core();
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
                NotifyInitialFinish();
            });
        }

        private void NotifyInitialFinish()
        {
            if (onInitialFinished != null)
                onInitialFinished(this,true);
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


        List<List<PixelInfo>> GetData(XYZImage img)
        {
            if (img.Y_ptr == IntPtr.Zero)
                throw new Exception("Invalid image.");
           
            float[] y_ptr = Ua.Utility.PtrToFloat(img.Y_ptr, img.size);
            float[] x_ptr = Ua.Utility.PtrToFloat(img.X_ptr, img.size);
            float[] z_ptr = Ua.Utility.PtrToFloat(img.Z_ptr, img.size);
                //System.IO.StreamWriter writer = new System.IO.StreamWriter("..\\out.csv", false);
            List<List<PixelInfo>> allPixelInfos = new List<List<PixelInfo>>();
            int ID = 1;
            for (int y = 0; y < img.height; y++)
            {
                List<PixelInfo> lineInfos = new List<PixelInfo>();
                for (int x = 0; x < img.width; x++)
                {
                    float Y = y_ptr[y * img.width + x];
                    float X = x_ptr[y * img.width + x];
                    float Z = z_ptr[y * img.width + x];
                    PixelInfo pixelInfo = new PixelInfo(ID++,X, Y, Z);
                    lineInfos.Add(pixelInfo);
                }
                allPixelInfos.Add(lineInfos);
            }
            return allPixelInfos;
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
            if(!GlobalVars.Instance.CameraSettings.AutoExposure)
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

        public List<List<PixelInfo>> Acquire()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (device_ptr == IntPtr.Zero)
                throw new Exception("Not initialized!");

            SetMannualMode();
            int average_count = 2;
            //uaCore.uaGetOptimumAverageCount(
            //    ref device, 0, 40, ref average_count);
           

            // Console.WriteLine("uaStartCapture");
            uaCore.uaStartCapture(ref device);
            double expo = device_property.exposure_time[0];
            // Console.WriteLine("uaCaptureImage");
            uaCore.uaCaptureImage(
                ref device, Ua.CaptureFilterType.UA_CAPTURE_FILTER_XYZ, average_count, ref capture_data);
            
            // Console.WriteLine("uaStopCapture");
            uaCore.uaStopCapture(ref device);
            //if( xyz_image_ptr != IntPtr.Zero)
            //    uaCore.uaDestroyXYZImage(xyz_image_ptr);
            
            uaCore.uaToXYZImage(ref device, ref capture_data, ref xyz_image);
            uaCore.uaDestroyCaptureData(capture_data_ptr);
            var allPixels = GetData(xyz_image);
            return allPixels;
           
        }

        public List<List<PixelInfo>> LoadXYZImage(string imgName)
        {
            uaCore.uaLoadMeasurementData(imgName,ref xyz_image_ptr,ref recipe_ptr);
            xyz_image = Ua.Utility.PtrToUaXYZImage(xyz_image_ptr);
            var allPixels = GetData(xyz_image);
            return allPixels;
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
