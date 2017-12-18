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
        string sTime;
        TestResult testResult = null;
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

        public float LAvg
        {
            get
            {
                return testResult.LAvg;
            }
        }
        public float LCenter
        {
            get
            {
                return testResult.LCenter;
            }
        }

        public float LMax
        {
            get
            {
                return testResult.LMax;
            }
        }
        public float LMin
        {
            get
            {
                return testResult.LMin;
            }
            
        }

        public float Uniform
        {
            get
            {
                return testResult.Uniform;
            }
           
        }


        public float x
        {
            get
            {
                return testResult.x;
            }
        }

        public float y
        {

            get
            {
                return testResult.y;
            }
        }
             
     
        public bool IsOk
        {
            get
            {
                return testResult.IsOk;
            }
        }

        public HistoryInfo(string _barcode, bool _isOk)
        {
            barcode = _barcode;
        }
        public HistoryInfo(string _barcode, Brightness brightness)
        {
            barcode = _barcode;

        }
        public HistoryInfo(string _barcode, TestResult testResult)
        {
            // TODO: Complete member initialization
            sTime = DateTime.Now.ToString("HHmmss");
            barcode = _barcode;
            if (barcode == "")
            {
                barcode = (GlobalVars.Instance.RegionsHistoryInfoCollection.AllInfos.Count + 1).ToString();
            }
        }
    }
}
