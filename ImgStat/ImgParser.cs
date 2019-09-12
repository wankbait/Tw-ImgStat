using System;
using System.IO;
using System.Drawing;
using Cloo;
using OpenCL.Net;
namespace ImgStat
{
    //
    public struct clInfo {
        static string clKernels = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ImgStat.Kernel.cl")).ReadToEnd();

        /*OPENCL BOILERPLATE */
        public static ComputePlatform platform = ComputePlatform.Platforms[0];

        public static ComputeContext ctx =
            new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, IntPtr.Zero);

        public static ComputeCommandQueue queue = new ComputeCommandQueue(ctx,
            ctx.Devices[0], ComputeCommandQueueFlags.None);

        public static ComputeProgram program = new ComputeProgram(ctx, clKernels);

        public static bool isInitialized = false;

        public static void Init()
        {
            isInitialized = true;
            Console.WriteLine(ctx.Devices.ToString());
            program.Build(null, null, null, IntPtr.Zero);
        }

    }
    class ImgParser
    {
        public ImgParser()
        {
        }

        public void GetStatGPU(string file)
        {
            Console.WriteLine($"Parsing {file} on platform: {clInfo.platform.Name}");

            ComputeKernel kernel = clInfo.program.CreateKernel("helloWorld");
            // create a ten integer array and its length
            int[] message = new int[] { 1, 2, 3, 4, 5 };
            int messageSize = message.Length;

            // allocate a memory buffer with the message (the int array)
            ComputeBuffer<int> messageBuffer = new ComputeBuffer<int>(clInfo.ctx,
            ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, message);

            ComputeBuffer<ComputeImageFormat> computeBuffer = new ComputeBuffer<ComputeImageFormat>(clInfo.ctx, ComputeMemoryFlags.ReadOnly, 1L, new Bitmap(file).GetHbitmap());


            kernel.SetMemoryArgument(0, messageBuffer); // set the integer array
            kernel.SetValueArgument(1, messageSize); // set the array size

            kernel.SetMemoryArgument(2, computeBuffer);

            // execute kernel
            clInfo.queue.ExecuteTask(kernel, null);
            clInfo.queue.Finish();
            Console.Read();
        }

        /* Fallback CPU processing */
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
