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

            var searchParams = new SearchTweetsParameters("*#digitalart #portrait filter:media -filter:replies -filter:retweets") {
                SearchType = Tweetinvi.Models.SearchResultType.Mixed,
                MaximumNumberOfResults = num,
                Filters = TweetSearchFilters.Twimg
            };
            string csvPath = $@"{Environment.CurrentDirectory}\Tweets\";
            string csvFile = csvPath + $"tweets_{DateTime.UtcNow.ToOADate()}.csv";
            if (!Directory.Exists(csvPath)) {
                Directory.CreateDirectory(csvPath);
                File.Create(csvFile);
            }

            //Enumerate "tweets" with results from a search.
            IEnumerable<Tweetinvi.Models.ITweet> tweets = null;
            try { tweets = Search.SearchTweets(searchParams); }
            catch(Exception e)
            {
                //In the event of an exception, print to console with feedback; do not continue the script
                Console.Write($"E: Couldn't complete tweet search. \n \n Exception Msg: \n{e.ToString()}");
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
            
            //TODO/CLEANUP: Remove the stream code below.
            #region streams
            //var stream = Tweetinvi.Stream.CreateFilteredStream();
            //stream.AddTrack("#digitalart");

            //stream.StallWarnings = true;

            //stream.MatchingTweetReceived += (evnt, args) =>
            //{
            //    Console.WriteLine(args.Tweet);
            //};

            //stream.KeepAliveReceived += (sender, args) =>
            //{
            //    Console.WriteLine("WARN: KEEP ALIVE RECIEVED");
            //};

            //stream.DisconnectMessageReceived += (sender, args) =>
            //{
            //    Console.WriteLine("WARN: DisconnectMessageReceived " + args.DisconnectMessage);
            //};

            //stream.WarningFallingBehindDetected += (sender, args) =>
            //{
            //    Console.WriteLine("WARN: WarningFallingBehindDetected " + args.WarningMessage);
            //};

            //stream.StreamStopped += (sender, args) =>
            //{
            //    Console.WriteLine("StreamStopped " + args.DisconnectMessage);
            //};

            //stream.StartStreamMatchingAllConditions();
            #endregion
        }
        public static void Download()
        {
            Download(1);
        }

        public static void Download(int num)
        {
            Console.WriteLine("--downloading");
            string csvPath = $@"{Environment.CurrentDirectory}\Tweets\";
            string dlPath = $@"{Environment.CurrentDirectory}\Download\";
            try
            {
                foreach(string filePath in Directory.EnumerateFiles(csvPath))
                {
                    using (StreamReader streamReader = new StreamReader(filePath))
                    {
                        CsvReader csvReader = new CsvReader(streamReader);
                    
                    
                        //csvReader.Configuration.MemberTypes = CsvHelper.Configuration.MemberTypes.Properties;
                        //csvReader.Configuration.UnregisterClassMap(typeof(TweetMap));
                        //csvReader.Read();
                        //csvReader.ReadHeader();
                        using(WebClient webClient = new WebClient())
                        {
                            while (csvReader.Read())
                            {
                                string id = "", mediaUri = "";
                                for ( int i = 0; i < csvReader.FieldsCount; i++)
                                {
                                    var fr = csvReader[i];
                                    switch(i)
                                    {
                                        case 0:
                                            id = fr;
                                            break;
                                        case 1:
                                            Console.Write($"{i} {fr}");
                                            break;
                                        case 2:
                                            //Console.Write($"{i} {fr}");
                                            break;
                                        case 3:
                                           // Console.Write($"{i} {fr}");
                                            //Console.WriteLine(fieldRecord);
                                            break;
                                        case 4:
                                            //Console.Write($"{i} {fr}");
                                            break;
                                        case 5:
                                            //Console.Write($"{i} {fr}");
                                            break;
                                        case 6:
                                            mediaUri = fr;
                                            //Console.WriteLine(fr);
                                            break;
                                        default:
                                            break;
                                    }
                                    
                                    
                                }
                                //Console.Write($"downloading {id} from: {mediaUri} \n");
                                //UriBuilder uri = new UriBuilder(mediaUri);

                                //webClient.DownloadFile(uri.Uri, id);
                                //webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                                //{
                                //    Console.Write($"\r Downloading {id} : {e.ProgressPercentage}");
                                //};
                            }

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
