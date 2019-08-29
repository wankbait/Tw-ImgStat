using System;
using System.Drawing;

namespace ImgStat
{
    class ImgParser
    {
        static int threads = 0;
        Bitmap bitmap = new Bitmap(@"C:\Users\rawr8\Pictures\Daddy.jpg");
        public ImgParser(string path)
        {
            if (path != null)
            {
                bitmap = new Bitmap(@"" + path);
            }
        }
        public ImgParser()
        {
            bitmap = new Bitmap(@"C:\Users\rawr8\Pictures\Daddy.jpg");
        }

        public string GetStat()
        {

            float TotalHue = 0.0f;
            float TotalSat = 0.0f;
            float TotalVal = 0.0f;

            float MaxHue = 0f;
            float MaxSat = 0f;
            float MaxVal = 0f;

            float MinHue = 100f;
            float MinSat = 100f;
            float MinVal = 100f;

            char[] c = { '-', '\\', '|', '/' };
            float ind = 0;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var Pixel   = bitmap.GetPixel(x, y);
                    float hue   = Pixel.GetHue();
                    float value = Pixel.GetBrightness();
                    float sat   = bitmap.GetPixel(x, y).GetSaturation();


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
                    Console.Write("\r" + c[(int)ind]);
                    ind+=0.001f;
                    if (ind >= c.Length)
                    {
                        ind = 0;
                    }
                }
            }
            Console.Write("\n");
            threads--;
            return $"Totals (HSV): {TotalHue}, {TotalSat}, {TotalVal}; \n" +
                $"Mean        : {TotalHue/(bitmap.Width * bitmap.Height)}, {TotalSat / (bitmap.Width * bitmap.Height)}, {TotalVal / (bitmap.Width * bitmap.Height)} \n" +
                $"Min         : {MinHue}, {MinSat}, {MinVal} \n" +
                $"Max         : {MaxHue}, {MaxSat}, {MaxVal}";

        }
            
    }
}
