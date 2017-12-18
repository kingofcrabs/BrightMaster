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
        public void Save()
        {
            string todayFolder = FolderHelper.GetDefaultSaveFolder() + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(todayFolder))
                Directory.CreateDirectory(todayFolder);
            string csvFile = todayFolder + GlobalVars.Instance.Barcode + ".csv";
            SaveWholePanelInfosCSV(csvFile);
            
        }

        private void SaveWholePanelInfosExcel(string excelFile, TestResult wholePanelResult,string imagePath)
        {
            var xlApp = new Excel.Application();
            string sFile = FolderHelper.GetTemplateFile();
            File.Copy(sFile, excelFile);
            Excel.Workbook xlWorkBook = xlApp.Workbooks.Open(excelFile);
            Excel.Worksheet xlWorkSheet = xlWorkBook.Sheets[1];
            xlWorkSheet.Cells["A29"] = wholePanelResult.LAvg;
            xlWorkSheet.Cells["B29"] = wholePanelResult.LMax;
            xlWorkSheet.Cells["C29"] = wholePanelResult.LMin;
            xlWorkSheet.Cells["D29"] = wholePanelResult.LCenter;
            xlWorkSheet.Cells["E29"] = wholePanelResult.x;
            xlWorkSheet.Cells["F29"] = wholePanelResult.y;
            xlWorkSheet.Cells["G29"] = wholePanelResult.Uniform;
            xlWorkSheet.Shapes.AddPicture(imagePath, MsoTriState.msoFalse, MsoTriState.msoCTrue, 0, 155, 460, 440);
            xlWorkBook.Close(true);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private void SaveWholePanelInfosCSV(string csvFile)
        {
          
            string header = "年月日,流水号,中心亮度,x,y,全屏扫描数据";
            int seqNo = 1;
            List<string> strs = new List<string>() { header };
            foreach (var info in GlobalVars.Instance.WholePanelHistoryInfoCollection.AllInfos)
            {
                strs.Add(string.Format("{0},{1},{2},{3},{4},{5}", DateTime.Now.ToString("yyMMdd"), seqNo++, info.LCenter, info.x, info.y, info.Uniform));
            }

            File.WriteAllLines(csvFile, strs);
        }



        internal void Save2Excel(TestResult wholePanelResult,Brightness brightness)
        {
            string todayFolder = FolderHelper.GetDefaultSaveFolder() + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(todayFolder))
                Directory.CreateDirectory(todayFolder);
            string excelFile = todayFolder + GlobalVars.Instance.Barcode + ".xls";
            brightness.SaveImage();
            SaveWholePanelInfosExcel(excelFile, wholePanelResult, brightness.ImagePath);
        }
    }
}
