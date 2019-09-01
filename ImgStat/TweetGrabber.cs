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
            var stream = Tweetinvi.Stream.CreateFilteredStream();
            stream.ClearCustomQueryParameters();
            stream.AddCustomQueryParameter("art", "#digitalart");
            stream.MatchingTweetReceived += (sender, args) => {
                Console.WriteLine(args.Tweet);
                Console.WriteLine(args.Tweet.FavoriteCount);
            };
            stream.StartStreamMatchingAllConditionsAsync();
        }

        private static void Stream_TweetReceived(object sender, Tweetinvi.Events.TweetReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
