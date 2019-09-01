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
            TweetGrabber.Fetch(1);
            string[] files = Directory.GetFiles(@"C:\Users\rawr8\Pictures\Test\", "*.jpg");
            Console.WriteLine("Hello World!");
            /*
            Parallel.ForEach(files, (current) =>{
                Console.WriteLine($"FILE: {current} on thread: {Thread.CurrentThread.ManagedThreadId}");
                
                Bitmap bitmap = new Bitmap(current);

                float TotalHue = 0.0f;
                float TotalSat = 0.0f;
                float TotalVal = 0.0f;

                float MaxHue = 0f;
                float MaxSat = 0f;
                float MaxVal = 0f;

                float MinHue = 100f;
                float MinSat = 100f;
                float MinVal = 100f;

                char[] processChar = { '-', '\\', '|', '/' };
                float processCharIndex = 0;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var Pixel = bitmap.GetPixel(x, y);
                        float hue = Pixel.GetHue();
                        float value = Pixel.GetBrightness();
                        float sat = bitmap.GetPixel(x, y).GetSaturation();


                        TotalHue += hue;
                        TotalSat += sat;
                        TotalVal += value;

                        //Set the 
                        if (hue > MaxHue)
                            MaxHue = hue;
                        if (sat > MaxSat)
                            MaxSat = sat;
                        if (value > MaxVal)
                            MaxVal = value;

                        if (hue < MinHue)
                            MinHue = hue;
                        if (sat < MinSat)
                            MinSat = sat;
                        if (value < MinVal)
                            MinVal = value;

                        //Just so I could make the cool little spinny thing
                        //Also because I need to know if the program is working or not.
                        Console.Write("\r" + processChar[(int)processCharIndex]);
                        processCharIndex += 0.001f;
                        if (processCharIndex >= processChar.Length)
                        {
                            processCharIndex = 0;
                        }
                    }
                }
                Console.Write("\n");
                Console.Write( $"Totals (HSV): {TotalHue}, {TotalSat}, {TotalVal}; \n" +
                    $"Mean        : {TotalHue / (bitmap.Width * bitmap.Height)}, {TotalSat / (bitmap.Width * bitmap.Height)}, {TotalVal / (bitmap.Width * bitmap.Height)} \n" +
                    $"Min         : {MinHue}, {MinSat}, {MinVal} \n" +
                    $"Max         : {MaxHue}, {MaxSat}, {MaxVal} \n");


            });
            ImgParser parser = new ImgParser();
            Console.WriteLine("Finished. Press any key to exit.");
            */
            Console.ReadKey();
        }

    }
}
