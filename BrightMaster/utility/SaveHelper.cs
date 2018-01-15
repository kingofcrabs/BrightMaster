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

        private Boolean IsEmptySheet(Worksheet wksheet)
        {
            int row;
            int col;
            row = wksheet.UsedRange.Rows.Count; //xlWorkSheet is Excel.Worksheet object
            col = wksheet.UsedRange.Columns.Count;
            return row == 1 && col == 1;
        }

        private void SaveWholePanelInfosExcel(string dstExcelFile, TestResult wholePanelResult,string imagePath)
        {
            var xlApp = new Excel.Application();
            string srcFile = FolderHelper.GetTemplateFile();
            Worksheet wkSheet;
            Excel.Workbook dstWorkBook;
            Excel.Workbook templateWorkBook;
             bool isCopied = false;
            if (isFirst) //find whether need to copy
            {
                isFirst = false;
                FileInfo fileInfo = new FileInfo(dstExcelFile);
                if (!File.Exists(dstExcelFile))
                {
                    File.Copy(srcFile, dstExcelFile);
                    isCopied = true;
                }
                else if(fileInfo.Length < 9*1000)
                {
                    File.Copy(srcFile, dstExcelFile,true);
                    isCopied = true;
                }
            }
            templateWorkBook = xlApp.Workbooks.Open(srcFile);
            dstWorkBook = xlApp.Workbooks.Open(dstExcelFile);
            if(!isCopied) //append new worksheet
            {
                AddNewSheetThenCopy(templateWorkBook, dstWorkBook);
            }
            wkSheet = dstWorkBook.Worksheets[dstWorkBook.Sheets.Count];
            wkSheet.Name = GetSheetName(dstWorkBook);
            wkSheet.Cells[29, 1].Value = wholePanelResult.LAvg;
            wkSheet.Cells[29, 2].Value = wholePanelResult.LMax;
            wkSheet.Cells[29, 3].Value = wholePanelResult.LMin;
            wkSheet.Cells[29, 4].Value = wholePanelResult.LCenter;
            wkSheet.Cells[29, 5].Value = wholePanelResult.x;
            wkSheet.Cells[29, 6].Value = wholePanelResult.y;
            wkSheet.Cells[29, 7].Value = wholePanelResult.Uniform;
            wkSheet.Shapes.AddPicture(imagePath, MsoTriState.msoFalse, MsoTriState.msoCTrue, 0, 120, 320, 240);
            templateWorkBook.Close(true);
            dstWorkBook.Close(true);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private void AddNewSheetThenCopy(Workbook templateWorkBook, Workbook dstWorkBook)
        {
            dstWorkBook.Sheets.Add(After: dstWorkBook.Sheets[dstWorkBook.Sheets.Count]);
            Excel.Range from = templateWorkBook.Worksheets[1].Range("A1:L5");
            Excel.Range to = dstWorkBook.Worksheets[dstWorkBook.Sheets.Count].Range("A1:L5");
            from.Copy(to);

            from = templateWorkBook.Worksheets[1].Range("A27:G28");
            to = dstWorkBook.Worksheets[dstWorkBook.Sheets.Count].Range("A27:G28");
            from.Copy(to);
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
            string header = "年月日,条码,中心亮度,x,y,全屏扫描数据,合格";
            List<string> strs = new List<string>() { header };
            foreach (var info in GlobalVars.Instance.WholePanelHistoryInfoCollection.AllInfos)
            {
                string valid = info.IsOk ? "是" :"否";
                strs.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}", DateTime.Now.ToString("yyMMdd"), info.Barcode, info.LCenter, info.x, info.y, info.Uniform,valid));
            }

            File.WriteAllLines(csvFile, strs,Encoding.Default);
        }

        internal void Save2Excel(Brightness brightness)
        {
            var wholePanelResult = TestResult.GetWholePanelResult(brightness);
            brightness.SaveImage();
            if (newFilePath == "")
                throw new Exception("未设置保存路径！");
            SaveWholePanelInfosExcel(newFilePath, wholePanelResult, brightness.ImagePath);
        }

        internal static void CreateNewFile(string newFile)
        {
            isFirst = true;
            newFilePath = newFile;
        }
    }
}
