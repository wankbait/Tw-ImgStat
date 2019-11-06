using System;
using System.Collections.Generic;
using System.Text;
using Tweetinvi;
using CsvHelper;
using CsvHelper.TypeConversion;
using CsvHelper.Configuration.Attributes;

    //Small tweet object to write to CSV (using Tweetinvi.Models.ITweet produced a stack overflow within VS, and was too slow anyway)
    public class Tweet
    {
    [Index(0)]
    public string ID { get; }
    [Index(1)]
    public int Fav { get; }
    [Index(2)]
    public int RT { get; }
    [Index(3)]
    public int Followers { get; }
    [Index(4)]
    public int? Replies { get; }
    [Index(5)]
    public DateTime CreationTime { get; }
    [Index(6)]
    public string Content { get; }
    [Index(7)]
    public string MediaUrl { get; }
    [Index(8)]
    public string TweetUrl { get; } //Should this be removed to preserve anonymity? Or will it not matter because it will not be included in the paper?
    [Index(9)]
    public float LikeFollowRatio { get; }

    public Tweet(Tweetinvi.Models.ITweet tweet)
        {
            this.ID = tweet.Id.ToString();
            this.Content = tweet.FullText;
            
            //Get media URL
            try
            {
                this.MediaUrl = tweet.Entities.Medias[0].MediaURL;
            }
            catch (Exception e)
            {
                try
                {
                    this.MediaUrl = tweet.ExtendedTweet.ExtendedEntities.Medias[0].MediaURL;
                }
                catch (Exception e2)
                {
                    Console.Error.WriteLine("FOR SOME REASON THERE IS NO IMAGE LINK IN THIS IMAGE TWEET \n GO YELL AT TWITTER FOR THIS I GUESS");
                    throw new Exception();
                    //TODO: handle this
                }
            }

            //Get reply count. tweet.replycount is nullable so replace the null with zero
            var temp = tweet.ReplyCount;
            if (temp == null)
            {
                this.Replies = 0;
            }
            else
            {
                this.Replies = temp;
            }
            this.Fav = tweet.FavoriteCount;
            this.CreationTime = tweet.CreatedAt;
            this.RT = tweet.RetweetCount;
            this.TweetUrl = tweet.Url;
            this.Followers = tweet.CreatedBy.FollowersCount;

            //Use this to take into account user popularity
            if (Fav != 0)
            {
                this.LikeFollowRatio = Followers / Fav;

            }
            else
            {

                this.LikeFollowRatio = -1;
            }

        }
        
    }

    