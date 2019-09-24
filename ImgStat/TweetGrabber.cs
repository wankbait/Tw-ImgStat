using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tweetinvi;
using Tweetinvi.Parameters;


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
            Console.Write($"Running Command: Fetch {num} \n");
            var searchParams = new SearchTweetsParameters("#digitalart") {
                SearchType = Tweetinvi.Models.SearchResultType.Mixed,
                MaximumNumberOfResults = num,
                Filters = TweetSearchFilters.Images
            };

            var tweets = Search.SearchTweets(searchParams);

            foreach (var t in tweets)
            {
                Console.Write($"{t.FullText}\n" +
                    $"Fav: {t.FavoriteCount} RT: {t.RetweetCount} \n" +
                    $"Img: {t.Media[0].ToString()}\n");
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
