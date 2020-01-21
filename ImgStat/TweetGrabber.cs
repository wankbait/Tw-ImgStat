using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Streaming;
using Tweetinvi.Streams;
using Tweetinvi.Parameters;

//using CsvHelper;
using NReco.Csv;
using System.Linq;
using System.Net;
using System.Threading;
using Tweetinvi.Models;

namespace ImgStat
{
    static class TweetGrabber
    {
        static TwitterCredentials twitterCredentials;
        public static void Init()
        {
            ExceptionHandler.SwallowWebExceptions = false;
            string cToken = "", cSecret = "", aToken = "", aSecret = "";
            try
            {
                using (StreamReader r = new StreamReader(FileMgr.AuthFile))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] token = line.Split('=');
                        if (token[0] == "CONSUMER_TOKEN")
                        {
                            cToken = token[1];
                        }
                        else if (token[0] == "ACCESS_TOKEN")
                        {
                            aToken = token[1];
                        }
                        else if (token[0] == "CONSUMER_SECRET")
                        {
                            cSecret = token[1];
                        }
                        else if (token[0] == "ACCESS_SECRET")
                        {
                            aSecret = token[1];
                        }
                        else
                        {
                            //todo: provide some error in this situation
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{FileMgr.AuthFile} could not be read.");
                Console.Error.WriteLine(e.Message);
            }
            twitterCredentials = new TwitterCredentials(cToken, cSecret, aToken, aSecret);
            Auth.SetUserCredentials(cToken, cSecret, aToken, aSecret);

        }

        internal static void UpdateAll()
        {
            IEnumerable<string> files = Directory.EnumerateFiles(FileMgr.CSVPath);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"UPDATE: {Enumerable.Count(files)} DOCUMENTS");
            Console.ResetColor();

            string bulkCsvFile = FileMgr.CSVPath + $"{DateTime.Now.ToOADate()}_blktweet.csv";
            if (!Directory.Exists(FileMgr.CSVPath))
            {
                Directory.CreateDirectory(FileMgr.CSVPath);
            }
            if (!File.Exists(bulkCsvFile))
            {
                File.WriteAllText(bulkCsvFile, "");
            }


            List<long> ids = new List<long>();
            Parallel.ForEach(files, file => {
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    Console.WriteLine($"Gathering IDs from File: {file}...");
                    var csvReader = new CsvReader(streamReader, ",");
                    while (csvReader.Read())
                    {
                        long record_id = long.Parse(csvReader[0]);
                        if (!ids.Contains(record_id))
                            ids.Add(record_id);
                    }

                }

                //Get 'em out of here.
                try
                {
                    File.Move(file, FileMgr.CSVBackup + file.Split("\\")[file.Split("\\").Length - 1]);
                }catch(Exception e)
                {
                    
                    File.Move(file, FileMgr.CSVBackup + (new Random().Next()) + file.Split("\\")[file.Split("\\").Length - 1]);
                }
                Console.WriteLine($"Moved old file {file} to \n{FileMgr.CSVBackup}");
            });


            Console.ResetColor();
            using (StreamWriter streamWriter = new StreamWriter(bulkCsvFile))
            {
                Console.WriteLine("Fetching new data from Twitter...");
                CsvWriter csv = new CsvWriter(streamWriter);
                var newTweets = Tweetinvi.Tweet.GetTweets(ids.ToArray());
                foreach (ITweet t in newTweets)
                {
                    try
                    {
                        if(t.Media[0].MediaType != "photo")
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine($"SKIPPING {t.Id}; Does not contain photo media.");
                            Console.ResetColor();

                            continue;
                        }

                    }catch(Exception e)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"SKIPPING {t.Id}; Does not contain embedded media.");
                        Console.ResetColor();

                        continue;

                    }

                    Tweet slim = new Tweet(t);

                    csv.WriteField(slim.ID.ToString());//0
                    csv.WriteField(slim.Fav.ToString());//1
                    csv.WriteField(slim.RT.ToString());//2
                    csv.WriteField(slim.Replies.ToString());//3
                    csv.WriteField(slim.Followers.ToString());//4
                    csv.WriteField(slim.MediaUrl.ToString());//5
                    csv.WriteField(slim.TweetUrl.ToString());//6
                    csv.WriteField(slim.Content.ToString());//7
                    csv.WriteField(slim.CreationTime.ToString());//8
                    csv.WriteField(slim.LikeFollowRatio.ToString());//9

                    //Write record to file
                    csv.NextRecord();

                    //provide some feedback in the console
                    Console.WriteLine($"Updated ID: {slim.ID} to file {bulkCsvFile} \n");
                }
            }
            
        }

        public static void Fetch(int num)
        {
            //Console.Write($"Fetching {num} tweets... \n

            string csvFile = FileMgr.CSVPath + $"{DateTime.Now.ToOADate()}_tweet.csv";
            if (!Directory.Exists(FileMgr.CSVPath))
            {
                Directory.CreateDirectory(FileMgr.CSVPath);   
            }
            if (!File.Exists(csvFile))
            {
                File.WriteAllText(csvFile, "");
            }
            

            int count = 0;
            using (FileStream fileStream = new FileStream(csvFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                CsvWriter csv = new CsvWriter(streamWriter);

                var stream = Tweetinvi.Stream.CreateFilteredStream(credentials: twitterCredentials);
                stream.AddTrack("#digitalart");
                //stream.AddTrack("#digitalart #portrait has:images -filter:replies");
                //stream.AddTrack("github");
                stream.MatchingTweetReceived += (sender, args) =>
                {
                    Console.WriteLine(args.Tweet.Id);
                    if (!args.Tweet.IsRetweet && args.Tweet.Media.Count != 0 && !args.Tweet.FullText.Contains("#portrait"))
                    {
                        Tweet slim = new Tweet(args.Tweet);

                        csv.WriteField(slim.ID.ToString());//0
                        csv.WriteField(slim.Fav.ToString());//1
                        csv.WriteField(slim.RT.ToString());//2
                        csv.WriteField(slim.Replies.ToString());//3
                        csv.WriteField(slim.Followers.ToString());//4
                        csv.WriteField(slim.MediaUrl.ToString());//5
                        csv.WriteField(slim.TweetUrl.ToString());//6
                        csv.WriteField(slim.Content.ToString());//7
                        csv.WriteField(slim.CreationTime.ToString());//8
                        csv.WriteField(slim.LikeFollowRatio.ToString());//9

                        //Write record to file
                        csv.NextRecord();

                        //provide some feedback in the console
                        Console.WriteLine($"Wrote tweet with ID: {slim.ID} to file {csvFile} \n");
                        count++;
                        Console.WriteLine(count);
                    }
                };

                stream.StreamStopped += (sender, args) =>
                {
                    Console.WriteLine(stream.StreamState);
                    Console.WriteLine(args.Exception);
                    Console.WriteLine(args.DisconnectMessage.Reason);
                };

                //Start stream, stop when receiving the amount
                stream.StartStreamMatchingAllConditions();
                if (count >= num)
                {
                    stream.StopStream();
                }
            }
                


            //List<string> skipID = new List<string>();
            //Console.BackgroundColor = ConsoleColor.DarkMagenta;
            //foreach (string file in Directory.EnumerateFiles(FileMgr.CSVPath))
            //{
            //    using (FileStream fileStream = new FileStream(file,FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //    using (StreamReader streamReader = new StreamReader(fileStream))
            //    {
            //        Console.WriteLine($"Gathering old IDs from {file}...");
            //        var csvReader = new CsvReader(streamReader, ",");
            //        while (csvReader.Read())
            //        {
            //            skipID.Add(csvReader[0]);
            //        }
                    
            //    }
            //}
            //Console.ResetColor();

            //create a "slim" tweet object with only a select few fields
            //using the ITweet object returned from tweetinvi results in a stack overflow error.
            //using (FileStream fileStream = new FileStream(csvFile,FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //using (StreamWriter streamWriter = new StreamWriter(fileStream))
            //{

            //    CsvWriter csv = new CsvWriter(streamWriter);

            //    csv.NextRecord();
                
            //    //Loop through the tweet results & add them to a CSV document.
            //    foreach (Tweetinvi.Models.ITweet t in tweets)
            //    {
            //        foreach (string id in skipID)
            //        {
            //            if (id == t.IdStr)
            //            {
            //                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            //                Console.WriteLine($"SKIPPING TWEET WITH ID: {id}");
            //                Console.ResetColor();
            //                goto Skip;
            //            }
            //        }

            //        Tweet slim = new Tweet(t);

            //        csv.WriteField(slim.ID.ToString());//0
            //        csv.WriteField(slim.Fav.ToString());//1
            //        csv.WriteField(slim.RT.ToString());//2
            //        csv.WriteField(slim.Replies.ToString());//3
            //        csv.WriteField(slim.Followers.ToString());//4
            //        csv.WriteField(slim.MediaUrl.ToString());//5
            //        csv.WriteField(slim.TweetUrl.ToString());//6
            //        csv.WriteField(slim.Content.ToString());//7
            //        csv.WriteField(slim.CreationTime.ToString());//8

            //        //Write record to file
            //        csv.NextRecord();

            //        //provide some feedback in the console
            //        Console.WriteLine($"Wrote tweet with ID: {slim.ID} to file {csvFile} \n");
            //        continue;
            //        Skip: {
            //            continue;
            //        }
            //    }
            //}

            //Using the "using" command disposes of and flushes both the StreamWriter and CsvWriter at the end; no need to do that manually



        }

        private static void Stream_StreamStarted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        //Download images based on CSV output
        public static void Download()
        {
            Console.WriteLine("--downloading");
            string id = "", mediaUri = "";
            int count = 1;

            //Surround in try/catch block in case the download fails
            try
            {
                //Loop through each file in the CSV folder
                foreach (string filePath in Directory.EnumerateFiles(FileMgr.CSVPath))
                {
                    //Read each file, download the attached media using the saved URI and the tweet ID as a filename.
                    using (StreamReader streamReader = new StreamReader(filePath))
                    {
                        CsvReader csvReader = new CsvReader(streamReader);

                        using (WebClient webClient = new WebClient())
                        {
                            while (csvReader.Read())
                            {
                                id = csvReader[0];
                                mediaUri = csvReader[5];
                                
                                Console.Write($"{count}: Downloading {id} from: {mediaUri} \n");
                                UriBuilder uri = new UriBuilder(mediaUri);

                                webClient.DownloadFile(uri.Uri, $"{FileMgr.DLPath}{id}.jpg");
                                
                            }
                            Console.WriteLine("Done.");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine($"ID:{id}");
                Console.WriteLine($"URI:{mediaUri}");
                //Fetch(num);
                //Download(num);
                Thread.Sleep(5000);
                return;
            }

        }
    }
}
