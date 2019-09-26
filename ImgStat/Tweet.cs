using System;
using System.Collections.Generic;
using System.Text;
using Tweetinvi;
using CsvHelper.Expressions;
namespace ImgStat
{
    //Small tweet object to write to CSV (using Tweetinvi.Models.ITweet produced a stack overflow within VS, and was too slow anyway)
    public class Tweet
    {
        public long Id { get; }
        public int Favorites { get; }
        public int RTCount { get; }
        public int? Replies { get; }
        public DateTime CreationTime { get; }
        public string Content { get;  }
        public string MediaUrl { get; }
        //Should this be removed to preserve anonymity? Or will it not matter because it will not be included in the paper?
        public string TweetUrl { get; }
        public Tweet(Tweetinvi.Models.ITweet tweet)
        {
            this.Id = tweet.Id;
            this.Content = tweet.FullText;
            try
            {
                this.MediaUrl = tweet.Entities.Medias[0].DisplayURL;
            }
            catch (Exception e)
            {
                try
                {
                    this.MediaUrl = tweet.ExtendedTweet.ExtendedEntities.Medias[0].DisplayURL;
                }
                catch (Exception e2)
                {
                    //TODO
                }
            }
            this.Favorites = tweet.FavoriteCount;
            this.CreationTime = tweet.CreatedAt;
            this.RTCount = tweet.RetweetCount;
            var temp = tweet.ReplyCount;
            if (temp == null)
            {
                this.Replies = 0;
            }
            else
            {
                this.Replies = temp;
            }
            this.TweetUrl = tweet.Url;
        }
    }
}
