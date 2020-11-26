using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using NReco.Csv;

namespace ImgStat
{
    class Program
    {
        //TODO: Make this more user friendly for standalone build.
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            TweetGrabber.Init();
            FileMgr.Init();

            //TweetGrabber.Fetch(11);
            TweetGrabber.UpdateAll();
            TweetGrabber.Download();

            ImgParser imgParser = new ImgParser();
            var downloads = Directory.EnumerateFiles(FileMgr.DLPath);
            int count = 1;
            
            using (StreamWriter streamWriter = new StreamWriter(FileMgr.OutFile))
            {
                Parallel.ForEach(Directory.EnumerateFiles(FileMgr.CSVPath), filePath =>
                {
                    using (StreamReader streamReader = new StreamReader(filePath))
                    {
                        CsvReader csvReader = new CsvReader(streamReader);
                        CsvWriter csv = new CsvWriter(streamWriter);
                        while (csvReader.Read())
                        {
                            //Get statistics of the image based on tweet ID.
                            var stat = imgParser.GetStat(FileMgr.DLPath + $"{csvReader[0]}.jpg");

                            //Copy over existing data.
                            for(int i = 0; i < csvReader.FieldsCount; i++)
                            {
                                csv.WriteField(csvReader[i]);
                            }
                            //csv.WriteField(csvReader[0]);
                            //csv.WriteField(csvReader[1]);
                            //csv.WriteField(csvReader[2]);
                            //csv.WriteField(csvReader[3]);
                            //csv.WriteField(csvReader[4]);
                            //csv.WriteField(csvReader[5]);
                            //csv.WriteField(csvReader[6]);
                            //csv.WriteField(csvReader[7]);
                            //csv.WriteField(csvReader[8]);
                            //csv.WriteField(csvReader[9]);
                            //csv.WriteField(csvReader[10]);
                            //csv.WriteField(csvReader[11]);
                            //csv.WriteField(csvReader[12]);

                            //Write averages (saturation, value, hue)
                            csv.WriteField(stat.MeanSat.ToString());    //N
                            csv.WriteField(stat.MeanVal.ToString());    //O
                            csv.WriteField(stat.MeanHue.ToString());    //P

                            //Write maximums
                            csv.WriteField(stat.MaxSat.ToString());     //Q
                            csv.WriteField(stat.MaxVal.ToString());     //R
                            csv.WriteField(stat.MaxHue.ToString());     //S
                            
                            //Minimums
                            csv.WriteField(stat.MinSat.ToString());     //T
                            csv.WriteField(stat.MinVal.ToString());     //U
                            csv.WriteField(stat.MinHue.ToString());     //V

                            //Write record to file
                            csv.NextRecord();
                            Console.Write($"\n Wrote record {csvReader[0]} to {FileMgr.OutFile}");
                        }
                        count++;


                    }
                });
            }

            Console.WriteLine("\n Done.");

            //Console.WriteLine($"AVG TIME: {ImgParser.totalElapsedMS / count}ms");
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
