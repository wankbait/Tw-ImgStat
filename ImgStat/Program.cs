using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ImgStat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            //Initialize static classes & structs.
            //TweetGrabber.Init();
            clInfo.Setup();

            Assembly assy = Assembly.GetExecutingAssembly();
            ImgParser test = new ImgParser();
            test.GetStatGPU(@"C:\Users\rawr8\Pictures\Test\4_68.jpg");
            //Test workload; swap this for some sort of tweet buffer data later.
            //string[] files = Directory.GetFiles(@"C:\Users\rawr8\Pictures\Test\", "*.jpg");
            //Parallel.ForEach(files, (current) =>
            //{
            //    ImgParser parser = new ImgParser();
            //    //parser.GetStatGPU(current);
            //});

            //Write app logs to a folder because I was bored & wanted to figure out how to save logs.
            //string logpath = assy.Location + @"\Logs\";
            //if (!Directory.Exists(logpath))
            //{
            //    string dir = Directory.CreateDirectory(logpath).FullName;
            //}
            //var logFile = File.CreateText($"{logpath}\\LOG_{System.DateTime.UtcNow}");
            //logFile.Write(Environment.CommandLine);


            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadKey();
        }

    }
}
