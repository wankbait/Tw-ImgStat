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
            FileMgr.Init();
            //TweetGrabber.Fetch(100);
            //TweetGrabber.Download();

            ImgParser imgParser = new ImgParser();
            var downloads = Directory.EnumerateFiles(FileMgr.DLPath);
            int count = 1;
            Parallel.ForEach(downloads, file =>
            {
                Console.Write("\n" + count + ": ");
                Console.WriteLine($"STAT:{file}\n");
                imgParser.GetStat(file);
            });
            

            //while (true)
            //{
            //    Console.WriteLine("Press (1) to grab tweets, (2) to process tweets. Press anything else to exit.");
            //    ConsoleKeyInfo keyInfo = Console.ReadKey();
            //    if(keyInfo.KeyChar == '1')
            //    {
            //        Console.WriteLine("\nEnter max tweets: ");
            //        int max = int.Parse(Console.ReadLine());
            //        TweetGrabber.Fetch(max);
            //        Console.ReadKey();
            //    }
            //    else if(keyInfo.KeyChar == '2')
            //    {
            //        clInfo.Setup();
            //        ImgParser parser = new ImgParser();
            //        //TODO: Do something.
            //        break;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}

            Console.ReadKey();
        }

    }
}
