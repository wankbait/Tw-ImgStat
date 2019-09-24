using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace ImgStat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            TweetGrabber.Init();
            while (true)
            {
                Console.WriteLine("Press (1) to grab tweets, (2) to process tweets. Press anything else to exit.");
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if(keyInfo.KeyChar == '1')
                {
                    Console.WriteLine("\nEnter max tweets: ");
                    int max = int.Parse(Console.ReadLine());
                    TweetGrabber.Fetch(max);
                }
                else if(keyInfo.KeyChar == '2')
                {
                    clInfo.Setup();
                    ImgParser parser = new ImgParser();
                    //TODO: Do something.
                    break;
                }
                else
                {
                    break;
                }
            }
            //Write app logs to a folder because I was bored & wanted to figure out how to save logs.
            //string logpath = assy.Location + @"\Logs\";
            //if (!Directory.Exists(logpath))
            //{
            //    string dir = Directory.CreateDirectory(logpath).FullName;
            //}
            //var logFile = File.CreateText($"{logpath}\\LOG_{System.DateTime.UtcNow}");
            //logFile.Write(Environment.CommandLine);


            Console.ReadKey();
        }

    }
}
