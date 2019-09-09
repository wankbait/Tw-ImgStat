using System;
using System.IO;
using System.Drawing;
using Cloo;
namespace ImgStat
{
    class ImgParser
    {
        static string Kernel = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ImgStat.Kernel.cl")).ReadToEnd();
        
        /*OPENCL BOILERPLATE */
        private static ComputePlatform platform = ComputePlatform.Platforms[0];
        private static ComputeContext ctx =
            new ComputeContext(ComputeDeviceTypes.Gpu,
                new ComputeContextPropertyList(platform), null, IntPtr.Zero);
        private ComputeCommandQueue queue = new ComputeCommandQueue(ctx,
            ctx.Devices[0], ComputeCommandQueueFlags.None);
        public ImgParser()
        {
        }

        public void GetStatGPU(string file)
        {
            Console.WriteLine($"Parsing {file} on platform: {platform.Name}");
            


            Console.Read();
        }

        public void GetStat(string file)
        {
            Bitmap bitmap = new Bitmap(file);

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
            Console.Write($"Totals (HSV): {TotalHue}, {TotalSat}, {TotalVal}; \n" +
                $"Mean        : {TotalHue / (bitmap.Width * bitmap.Height)}, {TotalSat / (bitmap.Width * bitmap.Height)}, {TotalVal / (bitmap.Width * bitmap.Height)} \n" +
                $"Min         : {MinHue}, {MinSat}, {MinVal} \n" +
                $"Max         : {MaxHue}, {MaxSat}, {MaxVal} \n");

        }

    }
}
