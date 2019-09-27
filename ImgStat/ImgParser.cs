using System;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCL.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ImgStat
{
    //
    public struct clInfo {
        public static string KernelSrc = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ImgStat.Kernel.cl")).ReadToEnd();
        
        /*OPENCL BOILERPLATE */
        #region OCLNet

        public static Context _context;
        public static Device _device;
        public static CommandQueue _queue;
        public static OpenCL.Net.Program _program;

        public static void CheckErr(ErrorCode err, string name)
        {
            if (err != ErrorCode.Success)
            {
                Console.WriteLine($"OPENCL ERROR: {name}- {err.ToString()}");
            }
        }
        public static void ContextNotify(string errINfo, byte[] data, IntPtr cb, IntPtr userData)
        {
            Console.WriteLine($"OpenCl Notification: {errINfo}");
        }

        public static void Setup()
        {
            ErrorCode err;
            Platform[] platforms = Cl.GetPlatformIDs(out err);
            List<Device> CLDevices = new List<Device>();

            CheckErr(err, "Cl.GetPlatformIDs");

            foreach(Platform p in platforms)
            {
                string platformName = Cl.GetPlatformInfo(p, PlatformInfo.Name, out err).ToString();

                Console.WriteLine($"Platform: {platformName}");
                CheckErr(err, "Cl.GetPlatformInfo");

                //Limit platforms to GPU-based platforms
                foreach (Device d in Cl.GetDeviceIDs(p, DeviceType.Gpu, out err))
                {
                    CheckErr(err, "Cl.GetDeviceIDs");
                    Console.WriteLine("Device: " + d.ToString());
                    CLDevices.Add(d);
                }
            }

            if (CLDevices.Count <= 0)
            {
                Console.Error.WriteLine("No suitable OpenCL devices found.");
                return;
            }

            _device = CLDevices[0];

            if (Cl.GetDeviceInfo(_device, DeviceInfo.ImageSupport, out err).CastTo<Bool>() == Bool.False)
            {
                Console.Error.WriteLine($"Selected CL device {_device.ToString()} does not have image support.");
                return;
            }

            _context = Cl.CreateContext(null, 1, new Device[] { _device }, ContextNotify, IntPtr.Zero, out err);
            CheckErr(err, "Cl.CreateContext");
        } 
        #endregion

    }
    class ImgParser
    {
        public ImgParser()
        {
        }

        public void GetStatGPU(string file)
        {

            ErrorCode err;

            using (OpenCL.Net.Program program = Cl.CreateProgramWithSource(clInfo._context, 1, new[] { clInfo.KernelSrc }, null, out err))
            {
                clInfo.CheckErr(err, "Cl.CreateProgramWithSource");

                //Compile Kernels & check errors.
                err = Cl.BuildProgram(program, 1, new[] { clInfo._device }, string.Empty, null, IntPtr.Zero);
                clInfo.CheckErr(err, "Cl.BuildProgram");
                if (Cl.GetProgramBuildInfo(program, clInfo._device, ProgramBuildInfo.Status, out err).CastTo<BuildStatus>() != BuildStatus.Success)
                {
                    clInfo.CheckErr(err, "Cl.GetProgramBuildInfo");
                    Console.WriteLine("Cl.GetProgramBuildInfo != Success");
                    Console.WriteLine(Cl.GetProgramBuildInfo(program, clInfo._device, ProgramBuildInfo.Log, out err));
                    return;
                }

                //Specify the specific kernel for use
                Kernel kernel = Cl.CreateKernel(program, "satAvg", out err);
                clInfo.CheckErr(err, "Cl.CreateKernel");

                int intPtrSize = 0;
                intPtrSize = Marshal.SizeOf(typeof(IntPtr));

                IMem image2DBuffer;
                OpenCL.Net.ImageFormat imageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);
                int imageWidth, imageHeight, imageBytesSize, imageStride;

                using (FileStream imgStream = new FileStream(file, FileMode.Open))
                {
                    Image image = Image.FromStream(imgStream);

                    if (image == null)
                    {
                        Console.Error.WriteLine($"failed to open file {file}");
                        return;
                    }
                    imageWidth = image.Width;
                    imageHeight = image.Height;

                    //Create a bitmap from the file with the correct channel settings.
                    Bitmap inputBitmap = new Bitmap(image);
                    BitmapData bitmapDat = inputBitmap.LockBits(new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height),
                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    imageStride = bitmapDat.Stride;
                    imageBytesSize = imageStride * bitmapDat.Height;

                    byte[] inputByteArray = new byte[imageBytesSize];
                    Marshal.Copy(bitmapDat.Scan0, inputByteArray, 0, imageBytesSize);

                    //Create & populate memory buffer for use in Kernel
                    image2DBuffer = Cl.CreateImage2D(clInfo._context, 
                        MemFlags.CopyHostPtr | MemFlags.ReadOnly, imageFormat, 
                        (IntPtr)inputBitmap.Width, 
                        (IntPtr)inputBitmap.Height, 
                        (IntPtr)0, inputByteArray, out err);

                    clInfo.CheckErr(err, "Cl.CreateImage2D output");
                }
                //Create an output buffer for the average saturation of the image.
                float saturation = 0;
                IMem sat = Cl.CreateBuffer(clInfo._context, MemFlags.CopyHostPtr | MemFlags.WriteOnly, (IntPtr)intPtrSize, (object)saturation, out err);
                clInfo.CheckErr(err, "Cl.CreateBuffer saturation");

                //Passing buffers to the CL kernel
                err = Cl.SetKernelArg(kernel, 0, (IntPtr)intPtrSize, image2DBuffer);
                //TODO: Figure out how to pass a float to the kernel
                //err |= Cl.SetKernelArg(kernel, 1, (IntPtr)intPtrSize, sat );
                clInfo.CheckErr(err, "Cl.SetKernelArg");

                //Create & queue commands
                CommandQueue cmdQueue = Cl.CreateCommandQueue(clInfo._context, clInfo._device, CommandQueueProperties.None, out err);
                clInfo.CheckErr(err, "Cl.CreateCommandQueue");

                Event @event;

                //Transfer image data to kernel buffers
                IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
                IntPtr[] regionPtr = new IntPtr[] { (IntPtr)imageWidth, (IntPtr)imageHeight, (IntPtr)1 };
                IntPtr[] workGroupPtr = new IntPtr[] { (IntPtr)imageWidth, (IntPtr)imageHeight, (IntPtr)1 };

                err = Cl.EnqueueReadImage(cmdQueue, image2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, null, 0, null, out @event);
                clInfo.CheckErr(err, "Cl.EnqueueReadImage");

                //Execute the kerenl
                err = Cl.EnqueueNDRangeKernel(cmdQueue, kernel, 2, null, workGroupPtr, null, 0, null, out @event);
                clInfo.CheckErr(err, "Cl.EnqueueNDRangeKernel");

                err = Cl.Finish(cmdQueue);
                clInfo.CheckErr(err, "Cl.Finish");

                Cl.ReleaseKernel(kernel);
                Cl.ReleaseCommandQueue(cmdQueue);

                Cl.ReleaseMemObject(image2DBuffer);

            }

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
