﻿using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace BrightMaster
{
    public class SerializeHelper
    {

       
        static public void Save<T>(T setting, string sFile) where T : class
        {
            int pos = sFile.LastIndexOf("\\");
            string sDir = sFile.Substring(0, pos);

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            if (File.Exists(sFile))
                File.Delete(sFile);

            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream stream = new FileStream(sFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, setting);
            stream.Close();
        }
        static public T Load<T>(string sFile) where T : class,new()
        {
            Object obj = new object();
            if (!File.Exists(sFile))
                return new T();
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            XmlSerializer xs = new XmlSerializer(typeof(T));
            obj = xs.Deserialize(stream) as T;
            stream.Close();
            return (T)obj;
        }

        static public void SaveSettings(Recipe recipe, string sFile)
        {
            int pos = sFile.LastIndexOf("\\");
            string sDir = sFile.Substring(0, pos);

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            if (File.Exists(sFile))
                File.Delete(sFile);

            XmlSerializer xs = new XmlSerializer(typeof(Recipe));
            Stream stream = new FileStream(sFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, recipe);
            stream.Close();
        }

        static public void LoadSettings(ref Recipe recipe, string sFile)
        {
            if (!File.Exists(sFile))
                throw new FileNotFoundException(string.Format("位于：{0}的配置文件不存在", sFile));
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer xs = new XmlSerializer(typeof(Recipe));
            recipe = xs.Deserialize(stream) as Recipe;
            stream.Close();
        }


    
      

    }

    class FolderHelper
    {
        static public string GetRecipeFolder()
        {
            
            string layoutFolder = GetExeParentFolder() + "Recipes\\";
            if (!Directory.Exists(layoutFolder))
                Directory.CreateDirectory(layoutFolder);
            return layoutFolder;
        }

        static public string GetImageFolder()
        {
            string acquiredImagesFolder = GetExeParentFolder() + "AcquiredImages\\";
            if (!Directory.Exists(acquiredImagesFolder))
                Directory.CreateDirectory(acquiredImagesFolder);
            return acquiredImagesFolder;
        }


        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        internal static string GetMiscFolder()
        {
            string miscFolder = GetExeParentFolder() + "Misc\\";
            if (!Directory.Exists(miscFolder))
                Directory.CreateDirectory(miscFolder);
            return miscFolder;
        }

        internal static string GetDefaultSaveFolder()
        {
            string saveFolder = GetExeParentFolder() + "Save\\";
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);
            return saveFolder;
        }

        internal static string GetTemplateFile()
        {
            string templateFolder = GetExeParentFolder() + "Template\\";
            if (!Directory.Exists(templateFolder))
                throw new Exception("No Template folder!");
            string file = templateFolder + "wholePanel.xlsx";
            if(!File.Exists(file))
                throw new Exception("No Template file!");
            return file;
        }
        internal static string GetCorrectionFactorFolder()
        {
            string correctionFactorFolder = GetExeParentFolder() + "CorrectionFactor\\";
            if (!Directory.Exists(correctionFactorFolder))
                Directory.CreateDirectory(correctionFactorFolder);
            return correctionFactorFolder;
        }
    }


}
