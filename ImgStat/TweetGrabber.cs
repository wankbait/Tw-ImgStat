using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;

using CsvHelper;
using System.Linq;

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
            var searchParams = new SearchTweetsParameters("*#digitalart #portrait") {
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

            //using the "using" command disposes of and flushes both the StreamWriter and CsvWriter at the end; no need to do that manually
            using(StreamWriter streamWriter = new StreamWriter(csvFile))
            using(CsvWriter csv = new CsvWriter(streamWriter))
            {
                csv.WriteHeader(typeof(Tweet));
                //Loop through the tweet results & add them to a CSV document.
                foreach (Tweetinvi.Models.ITweet t in tweets)
                {
                    //create a "slim" tweet object with only a few fields
                    //using the ITweet object returned from tweetinvi results in a stack overflow error.
                    Tweet slim = new Tweet(t);

                    //Write record to file
                    csv.NextRecord();
                    csv.WriteRecord(slim);

                    //provide some feedback in the console
                    Console.WriteLine($"Wrote tweet with ID: {slim.Id} to file {csvFile} \n \n");
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
    }
}
