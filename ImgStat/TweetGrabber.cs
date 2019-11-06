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
            string authFile = $"{Environment.CurrentDirectory}\\Auth.txt";
            string cToken = "", cSecret = "", aToken = "", aSecret = "";
            try
            {
                using (StreamReader r = new StreamReader(authFile))
                {
                    string line;
                    while((line = r.ReadLine()) != null)
                    {
                        string[] token = line.Split('=');
                        if(token[0] == "CONSUMER_TOKEN")
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
                Console.Error.WriteLine($"{authFile} could not be read.");
                Console.Error.WriteLine(e.Message);
            }
            Auth.SetUserCredentials(cToken, cSecret, aToken, aSecret);
            
        }
        public static void Fetch(int num)
        {
            Console.Write($"Fetching {num} tweets... \n");


            //Create a directory for Tweet data to be stored.
            string csvPath = $@"{Environment.CurrentDirectory}\Tweets\";
            string csvFile = csvPath + $"tweets_{DateTime.UtcNow.ToOADate()}.csv";
            if (!Directory.Exists(csvPath)) {
                Directory.CreateDirectory(csvPath);
                File.Create(csvFile);
            }


            //Create parameters for the Twitter search.
            var searchParams = new SearchTweetsParameters("*#digitalart #portrait filter:media -filter:replies -filter:retweets") {
                SearchType = Tweetinvi.Models.SearchResultType.Mixed,
                MaximumNumberOfResults = num,
                Until = DateTime.Today.AddDays(-5.0),
                Filters = TweetSearchFilters.Twimg
            };


            //Enumerate "tweets" with results from a search.
            IEnumerable<Tweetinvi.Models.ITweet> tweets = null;
            try { tweets = Search.SearchTweets(searchParams); }
            catch(Exception e)
            {
                //In the event of an exception, print to console with feedback; do not continue the script
                Console.Write($"E: Couldn't complete tweet search. \n \n Exception Msg: \n{e.ToString()}");
                return;
            }
            //Stop early in case we don't get shit.
            if (tweets.Count() <= 0)
            {
                Console.WriteLine("Search returned 0 results.");
                return;
            }

            //Using the "using" command disposes of and flushes both the StreamWriter and CsvWriter at the end; no need to do that manually
            using (StreamWriter streamWriter = new StreamWriter(csvFile))
            {
                CsvWriter csv = new CsvWriter(streamWriter);
                
                //csv.WriteHeader(typeof(CTweet));
                //Loop through the tweet results & add them to a CSV document.
                foreach (Tweetinvi.Models.ITweet t in tweets)
                {
                    //create a "slim" tweet object with only a select few fields
                    //using the ITweet object returned from tweetinvi results in a stack overflow error.
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
                }
            }
        }
        
        //Download images based on CSV output
        public static void Download()
        {
            Console.WriteLine("--downloading");
            string csvPath = $@"{Environment.CurrentDirectory}\Tweets\";
            string dlPath = $@"{Environment.CurrentDirectory}\Download\";
            if (!Directory.Exists(dlPath))
            {
                Directory.CreateDirectory(dlPath);
            }
            try
            {
                foreach(string filePath in Directory.EnumerateFiles(csvPath))
                {
                    using (StreamReader streamReader = new StreamReader(filePath))
                    {
                        CsvReader csvReader = new CsvReader(streamReader);
                    
                        using(WebClient webClient = new WebClient())
                        {
                            csvReader.Read();
                            while (csvReader.Read())
                            {
                                string id = "", mediaUri = "";
                                for ( int i = 0; i < csvReader.FieldsCount; i++)
                                {
                                    var fr = csvReader[i];

                                    //Console.WriteLine(i);
                                    //Console.WriteLine(fr);

                                    switch (i)
                                    {
                                        case 0:
                                            id = fr;
                                            break;
                                        case 1:
                                            break;
                                        case 2:
                                            break;
                                        case 3:
                                            break;
                                        case 4:
                                            
                                            break;
                                        case 5:
                                            //Console.WriteLine(i);
                                            //Console.WriteLine(fr + "\n");
                                            mediaUri = fr;
                                            break;
                                        case 6:
                                            break;
                                        case 7:
                                            break;
                                        default:
                                            break;
                                    }
                                    
                                    
                                }
                                Console.Write($"downloading {id} from: {mediaUri} \n");
                                UriBuilder uri = new UriBuilder(mediaUri);

                                webClient.DownloadFile(uri.Uri, $"{dlPath}{id}.jpg");
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
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                //Fetch(num);
                //Download(num);
                return;
            }
            
        }
    }
}
