using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using System.Drawing;
using System.Text.RegularExpressions;
using SharpDX;

namespace ImgStat
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initialize static classes & structs.
            TweetGrabber.Init();
            clInfo.Init();

            Console.WriteLine("Hello World!");


            //Test workload; swap this for some sort of tweet buffer data later.
            string[] files = Directory.GetFiles(@"C:\Users\rawr8\Pictures\Test\", "*.jpg");
            Parallel.ForEach(files, (current) =>
            {
                ImgParser parser = new ImgParser();
                parser.GetStatGPU(current);
            });
            Console.WriteLine("Finished. Press any key to exit.");

            Console.ReadKey();
        }

    }
}
