using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using BrightMaster.data;
using System.Drawing;


namespace BrightMaster
{
    class SaveHelper
    {

        public static string CreateTodayFolder()
        {
            string todayFolder = FolderHelper.GetDefaultSaveFolder() + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(todayFolder))
                Directory.CreateDirectory(todayFolder);
            return todayFolder;
        }
        public void Save()
        {
            string todayFolder = CreateTodayFolder();
            string csvFile = todayFolder + GlobalVars.Instance.StartTime + " _all.csv";
            SaveWholePanelInfosCSV(csvFile);
            
        }

        private void SaveWholePanelInfosExcel(string excelFile, TestResult wholePanelResult,string imagePath)
        {
            var xlApp = new Excel.Application();
            string sFile = FolderHelper.GetTemplateFile();
            File.Copy(sFile, excelFile,true);
            Excel.Workbook xlWorkBook = xlApp.Workbooks.Open(excelFile);
            Excel.Worksheet xlWorkSheet = xlWorkBook.Sheets[1];
            xlWorkSheet.Cells[29, 1].Value = wholePanelResult.LAvg;
            xlWorkSheet.Cells[29, 2].Value = wholePanelResult.LMax;
            xlWorkSheet.Cells[29, 3].Value = wholePanelResult.LMin;
            xlWorkSheet.Cells[29, 4].Value = wholePanelResult.LCenter;
            xlWorkSheet.Cells[29, 5].Value = wholePanelResult.x;
            xlWorkSheet.Cells[29, 6].Value = wholePanelResult.y;
            xlWorkSheet.Cells[29, 7].Value = wholePanelResult.Uniform;
            xlWorkSheet.Shapes.AddPicture(imagePath, MsoTriState.msoFalse, MsoTriState.msoCTrue, 0, 130, 320, 240);
            xlWorkBook.Close(true);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private void SaveWholePanelInfosCSV(string csvFile)
        {
            string header = "年月日,流水号,中心亮度,x,y,全屏扫描数据,合格";
            int seqNo = 1;
            List<string> strs = new List<string>() { header };
            foreach (var info in GlobalVars.Instance.WholePanelHistoryInfoCollection.AllInfos)
            {
                string valid = info.IsOk ? "是" :"否";
                strs.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}", DateTime.Now.ToString("yyMMdd"), seqNo++, info.LCenter, info.x, info.y, info.Uniform,valid));
            }

            File.WriteAllLines(csvFile, strs,Encoding.Default);
        }

        internal void Save2Excel(Brightness brightness)
        {
            var wholePanelResult = TestResult.GetWholePanelResult(brightness);
            var todayFolder = CreateTodayFolder();
            string excelFile = todayFolder + GlobalVars.Instance.Barcode + ".xls";
            brightness.SaveImage();
            SaveWholePanelInfosExcel(excelFile, wholePanelResult, brightness.ImagePath);
        }

        internal void SaveRegionInfo(List<PixelInfo> pixelInfos)
        {
            
        }
    }
}
