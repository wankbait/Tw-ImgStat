using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;

//using CsvHelper;
using NReco.Csv;
using System.Linq;
using System.Net;

namespace ImgStat
{
    static class TweetGrabber
    {
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
            Auth.SetUserCredentials(cToken, cSecret, aToken, aSecret);

        }
        public static void Fetch(int num)
        {
            Console.Write($"Fetching {num} tweets... \n");


            string csvFile = FileMgr.CSVPath + $"{DateTime.Now.ToOADate()}_tweet.csv";
            if (!Directory.Exists(FileMgr.CSVPath))
            {
                Directory.CreateDirectory(FileMgr.CSVPath);   
            }
            if (!File.Exists(csvFile))
            {
                File.WriteAllText(csvFile, "");
            }
            //Create parameters for the Twitter search.
            var searchParams = new SearchTweetsParameters("* #digitalart #portrait filter:media -filter:replies -filter:retweets")
            {
                SearchType = Tweetinvi.Models.SearchResultType.Mixed,
                //MaximumNumberOfResults = num,
                Until = DateTime.Today.AddDays(-5.0),
                Filters = TweetSearchFilters.Twimg
            };


            //Enumerate "tweets" with results from a search.
            IEnumerable<Tweetinvi.Models.ITweet> tweets = null;
            try { tweets = Search.SearchTweets(searchParams); }
            catch (Exception e)
            {
                //In the event of an exception, print to console with feedback; do not continue the script
                Console.Write($"E: Couldn't complete tweet search. \n \n Exception Msg: \n{e.ToString()}");
                return;
            }
            //Stop early in case we don't get anything.
            if (tweets.Count() <= 0)
            {
                Console.WriteLine("Search returned 0 results.");
                return;
            }

            List<string> skipID = new List<string>();
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            foreach (string file in Directory.EnumerateFiles(FileMgr.CSVPath))
            {
                using (FileStream fileStream = new FileStream(file,FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    Console.WriteLine($"Gathering old IDs from {file}...");
                    var csvReader = new CsvReader(streamReader, ",");
                    while (csvReader.Read())
                    {
                        skipID.Add(csvReader[0]);
                    }
                    
                }
            }
            Console.ResetColor();

            //create a "slim" tweet object with only a select few fields
            //using the ITweet object returned from tweetinvi results in a stack overflow error.
            using (FileStream fileStream = new FileStream(csvFile,FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {

                CsvWriter csv = new CsvWriter(streamWriter);

                csv.NextRecord();
                
                //Loop through the tweet results & add them to a CSV document.
                foreach (Tweetinvi.Models.ITweet t in tweets)
                {
                    foreach (string id in skipID)
                    {
                        if (id == t.IdStr)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine($"SKIPPING TWEET WITH ID: {id}");
                            Console.ResetColor();
                            goto Skip;
                        }
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

                    //Write record to file
                    csv.NextRecord();

                    //provide some feedback in the console
                    Console.WriteLine($"Wrote tweet with ID: {slim.ID} to file {csvFile} \n");
                    continue;
                    Skip: {
                        continue;
                    }
                }
            }

            //Using the "using" command disposes of and flushes both the StreamWriter and CsvWriter at the end; no need to do that manually



        }

        //Download images based on CSV output
        public static void Download()
        {
            Console.WriteLine("--downloading");

            try
            {
                foreach (string filePath in Directory.EnumerateFiles(FileMgr.CSVPath))
                {
                    using (StreamReader streamReader = new StreamReader(filePath))
                    {
                        CsvReader csvReader = new CsvReader(streamReader);

                        using (WebClient webClient = new WebClient())
                        {
                            while (csvReader.Read())
                            {
                                string id = "", mediaUri = "";
                                id = csvReader[0];
                                mediaUri = csvReader[5];
                                
                                Console.Write($"downloading {id} from: {mediaUri} \n");
                                UriBuilder uri = new UriBuilder(mediaUri);

                                webClient.DownloadFile(uri.Uri, $"{FileMgr.DLPath}{id}.jpg");
                                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                                {
                                    Console.Write($"\r Downloading {id} : {e.ProgressPercentage}");
                                };
                            }
                            Console.WriteLine("Done.");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //Fetch(num);
                //Download(num);
                return;
            }

        }
    }
}
