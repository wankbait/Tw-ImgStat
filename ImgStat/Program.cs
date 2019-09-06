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
            TweetGrabber.Init();
            //TweetGrabber.Fetch(1);
            string[] files = Directory.GetFiles(@"C:\Users\rawr8\Pictures\Test\", "*.jpg");
            Console.WriteLine("Hello World!");

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
