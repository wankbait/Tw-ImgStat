using System;
using System.Collections.Generic;
using System.Text;
using Tweetinvi;
using CsvHelper;
using CsvHelper.TypeConversion;
using CsvHelper.Configuration.Attributes;

    //Small tweet object to write to CSV (using Tweetinvi.Models.ITweet produced a stack overflow within VS, and was too slow anyway)
    public class CTweet : MTweet
    {
        [Index(0)]
    new public long ID { get; }
        [Index(1)]
    new public int Fav { get; }
        [Index(2)]
    new public int RT { get; }
        [Index(3)]
    new public int? Replies { get; }
        [Index(4)]
    new public DateTime CreationTime { get; }
        [Index(5)]
    new public string Content { get; }
        [Index(6)]
    new public string MediaUrl { get; }
        [Index(7)]
    new public string TweetUrl { get; } //Should this be removed to preserve anonymity? Or will it not matter because it will not be included in the paper?

        public Type tweet = null;
    public CTweet(Tweetinvi.Models.ITweet tweet)
        {
            this.ID = tweet.Id;
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
            this.Fav = tweet.FavoriteCount;
            this.CreationTime = tweet.CreatedAt;
            this.RT = tweet.RetweetCount;
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

    //Minimal Tweet (no constructor)
    public class MTweet
    {
        [Index(0)]
        public long ID { get; }
        [Index(1)]
        public int Fav { get; }
        [Index(2)]
        public int RT { get; }
        [Index(3)]
        public int? Replies { get; }
        [Index(4)]
        public DateTime CreationTime { get; }
        [Index(5)]
        public string Content { get; }
        [Index(6)]
        public string MediaUrl { get; }
        [Index(7)]
        public string TweetUrl { get; } //Should this be removed to preserve anonymity? Or will it not matter because it will not be included in the paper?
    }


    public class TweetMap : CsvHelper.Configuration.ClassMap<MTweet>
    {
        public TweetMap()
        {
            AutoMap();
            Map(m => m.ID).Name("ID");
            Map(m => m.Fav).Name("Fav");
            Map(m => m.RT).Name("RT");
            Map(m => m.Replies).Name("Replies");
            Map(m => m.CreationTime).Name("CreationTIme");
            Map(m => m.Content).Name("Content");
            Map(m => m.MediaUrl).Name("MediaUrl");
            Map(m => m.TweetUrl).Name("TweetUrl");

            Map(m => m.ID).Index(0);
            Map(m => m.Fav).Index(1);
            Map(m => m.RT).Index(2);
            Map(m => m.Replies).Index(3);
            Map(m => m.CreationTime).Index(4);
            Map(m => m.Content).Index(5);
            Map(m => m.MediaUrl).Index(6);
            Map(m => m.TweetUrl).Index(7);
        }
    }

