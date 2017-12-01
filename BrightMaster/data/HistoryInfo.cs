using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.data
{
    class HistoryInfoCollection:BindableBase
    {
        ObservableCollection<HistoryInfo> allInfos = new ObservableCollection<HistoryInfo>();
        int ngCount;
        int okCount;
        int totalCount;

        private HistoryInfo selected;
        public HistoryInfo Selected
        {
            get
            {
                return selected;
            }
            set
            {
                SetProperty(ref selected, value);
            }
        }


        public int NGCnt
        {
            get
            {
                return ngCount;
            }
            set
            {
                SetProperty(ref ngCount, value);
            }
        }

        public int OkCnt
        {
            get
            {
                return okCount;
            }
            set
            {
                SetProperty(ref okCount, value);
            }
        }

        public int TotalCnt
        {
            get
            {
                return totalCount;
            }
            set
            {
                SetProperty(ref totalCount, value);
            }
        }



        public ObservableCollection<HistoryInfo> AllInfos
        {
            get
            {
                return allInfos;
            }
            set
            {
                SetProperty(ref allInfos, value);
            }
        }


        public void AddNew(HistoryInfo newInfo)
        {
            for(int i = 0; i< allInfos.Count; i++)
            {
                if(allInfos[i].Barcode == newInfo.Barcode )
                {
                    allInfos.RemoveAt(i);
                    break;
                }
            }
            allInfos.Add(newInfo);
            UpdateCnts();
        }

        private void UpdateCnts()
        {
            TotalCnt = allInfos.Count;
            OkCnt = allInfos.Count(x => x.IsOk);
            NGCnt = allInfos.Count - okCount;
        }

        public void Remove()
        {
            if (selected != null)
            {
                allInfos.Remove(selected);
                if (allInfos.Count > 0)
                    Selected = allInfos[allInfos.Count - 1];
                else
                    selected = null;
            }
            UpdateCnts();
        }


    }


    class HistoryInfo:BindableBase
    {
        string barcode;
        bool isOk;
        string sTime;
        float uniform;
        float lMax;
        float lMin;
        public string Barcode
        {
            get
            {
                return barcode;
            }
            set
            {
                SetProperty(ref barcode, value);
            }
        }

        public string Time
        {
            get
            {
                return sTime;
            }
            set
            {
                SetProperty(ref sTime, value);
            }
        }

        public float LMax
        {
            get
            {
                return lMax;
            }
            set
            {
                SetProperty(ref lMax, value);
            }
        }
        public float LMin
        {
            get
            {
                return lMax;
            }
            set
            {
                SetProperty(ref lMin, value);
            }
        }

        public float Uniform
        {
            get
            {
                return uniform;
            }
            set
            {
                SetProperty(ref uniform, value);
            }
        }

        float Keep4Valid(float num)
        {
            if (num == 0)
                return 0;
            float value = num;
            int count = 0;
            if (num < 1000)
            {
                while (num < 1000)
                {
                    count++;
                    num *= 10;
                }
            }
            value = (float)Math.Round(value, count);
            return value;
        }

        public bool IsOk
        {
            get
            {
                return isOk;
            }
            set
            {
                SetProperty(ref isOk, value);
            }
        }

        public HistoryInfo(string _barcode, bool _isOk)
        {
            isOk = _isOk;
            barcode = _barcode;
        }

        public HistoryInfo(string _barcode, List<PixelInfo> results)
        {
            // TODO: Complete member initialization
            barcode = _barcode;
            isOk = PixelInfo.GetResult(results).IsOk;
            var LArray = results.Select(x => x.Y).ToList();
            lMax = Keep4Valid(LArray.Max());
            lMin = Keep4Valid(LArray.Min());
            uniform = Keep4Valid(lMin * 100 / lMax);
            sTime = DateTime.Now.ToString("HHmmss");
        }
    }
}
