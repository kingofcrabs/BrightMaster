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
using Microsoft.Office.Interop.Excel;


namespace BrightMaster
{
    class SaveHelper
    {
        public static string newFilePath = "";
        private static bool isFirst = true;
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
            Worksheet wkSheet;
            Excel.Workbook xlWorkBook;
            if (isFirst)
            {
                isFirst = false;
                File.Copy(sFile, excelFile, true);
                xlWorkBook = xlApp.Workbooks.Open(excelFile);
            }
            else
            {
                xlWorkBook = xlApp.Workbooks.Open(excelFile);
                xlWorkBook.Sheets.Add(After: xlWorkBook.Sheets[xlWorkBook.Sheets.Count]);
                Excel.Range from = xlWorkBook.Worksheets[1].Range("A1:L5");
                Excel.Range to = xlWorkBook.Worksheets[xlWorkBook.Sheets.Count].Range("A1:L5");
                from.Copy(to);

                from = xlWorkBook.Worksheets[1].Range("A27:G28");
                to = xlWorkBook.Worksheets[xlWorkBook.Sheets.Count].Range("A27:G28");
                from.Copy(to);
            }
            wkSheet = xlWorkBook.Worksheets[xlWorkBook.Sheets.Count];
            wkSheet.Name = GetSheetName(xlWorkBook);
            wkSheet.Cells[29, 1].Value = wholePanelResult.LAvg;
            wkSheet.Cells[29, 2].Value = wholePanelResult.LMax;
            wkSheet.Cells[29, 3].Value = wholePanelResult.LMin;
            wkSheet.Cells[29, 4].Value = wholePanelResult.LCenter;
            wkSheet.Cells[29, 5].Value = wholePanelResult.x;
            wkSheet.Cells[29, 6].Value = wholePanelResult.y;
            wkSheet.Cells[29, 7].Value = wholePanelResult.Uniform;
            wkSheet.Shapes.AddPicture(imagePath, MsoTriState.msoFalse, MsoTriState.msoCTrue, 0, 120, 320, 240);
            xlWorkBook.Close(true);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private string GetSheetName(Workbook xlWorkBook)
        {
            List<string> existSheetNames = new List<string>();
            for(int i = 0; i< xlWorkBook.Worksheets.Count; i++)
            {
                existSheetNames.Add(xlWorkBook.Worksheets[i + 1].Name);
            }
            if (!existSheetNames.Contains(GlobalVars.Instance.Barcode))
                return GlobalVars.Instance.Barcode;
            int curSuffixID = 1;
            for(int i = 0; i< 255;i++)
            {
                string nameWithSuffix = GlobalVars.Instance.Barcode + string.Format("({0})", curSuffixID++);
                if (!existSheetNames.Contains(nameWithSuffix))
                    return nameWithSuffix;
            }
            throw new Exception("无法找到合适的Sheet名！");
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
            brightness.SaveImage();
            SaveWholePanelInfosExcel(newFilePath, wholePanelResult, brightness.ImagePath);
        }

        internal static void CreateNewFile(string newFile)
        {
            isFirst = true;
            newFilePath = newFile;
        }
    }
}
