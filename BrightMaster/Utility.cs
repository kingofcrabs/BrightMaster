﻿using System;
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
        static public T Load<T>(string sFile) where T : class
        {
            Object obj = new object();
            if (!File.Exists(sFile))
                throw new FileNotFoundException(string.Format("位于：{0}的配置文件不存在", sFile));
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            XmlSerializer xs = new XmlSerializer(typeof(T));
            obj = xs.Deserialize(stream) as T;
            stream.Close();
            return (T)obj;
        }

        static public void SaveSettings(Layout layout, string sFile)
        {
            int pos = sFile.LastIndexOf("\\");
            string sDir = sFile.Substring(0, pos);

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            if (File.Exists(sFile))
                File.Delete(sFile);

            XmlSerializer xs = new XmlSerializer(typeof(Layout));
            Stream stream = new FileStream(sFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, layout);
            stream.Close();
        }

        static public void LoadSettings(ref Layout layout, string sFile)
        {
            if (!File.Exists(sFile))
                throw new FileNotFoundException(string.Format("位于：{0}的配置文件不存在", sFile));
            Stream stream = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer xs = new XmlSerializer(typeof(Layout));
            layout = xs.Deserialize(stream) as Layout;
            stream.Close();
        }


    
      

    }

    class FolderHelper
    {
        static public string GetLayoutFolder()
        {
            
            string layoutFolder = GetExeParentFolder() + "Layouts\\";
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
    }


}
