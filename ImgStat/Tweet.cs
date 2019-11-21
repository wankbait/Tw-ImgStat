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
    public string ID { get; }
    public int Fav { get; }
    public int RT { get; }
    public int Followers { get; }
    public int? Replies { get; }
    public DateTime CreationTime { get; }
    public string Content { get; }
    public string MediaUrl { get; }
    public string TweetUrl { get; } //Should this be removed to preserve anonymity? Or will it not matter because it will not be included in the paper?
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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                this.MediaUrl = tweet.ExtendedTweet.ExtendedEntities.Medias[0].MediaURL;
            }
            catch (Exception e2)
            {
                Console.Error.WriteLine($"FOR SOME REASON THERE IS NO IMAGE LINK IN THIS IMAGE TWEET \n GO YELL AT TWITTER FOR THIS I GUESS \n \nSTACK TRACE: {e2}");
                throw e2;
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