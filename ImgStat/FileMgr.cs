using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImgStat
{
    public static class FileMgr
    {
        public static string DLPath = $@"{Environment.CurrentDirectory}\Download\";
        public static string CSVPath = $@"{Environment.CurrentDirectory}\Tweets\";
        public static string AuthFile = $"{Environment.CurrentDirectory}\\Auth.txt";
        public static string OutFile = $@"{Environment.CurrentDirectory}\Output.csv";

        public static void Init()
        {
            if (!Directory.Exists(DLPath))
            {
                Directory.CreateDirectory(DLPath);
            }
        }
    }
}
