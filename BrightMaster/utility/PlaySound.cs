using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.utility
{
    class PlaySound
    {
        public static void OnError()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(FolderHelper.GetExeFolder());
            var wavFiles = dirInfo.EnumerateFiles("*.wav");
            if (wavFiles.Count() == 0)
                return;
            string fileName = wavFiles.First().FullName;
            System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
            sp.SoundLocation = fileName;
            sp.Play();
        }
    }
}
