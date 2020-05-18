using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Newtonsoft.Json;

namespace archilab.GitHub
{
    /// <summary>
    /// 
    /// </summary>
    public class Author
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("login")]
        public string Login { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("id")]
        public double Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Week
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("w")]
        public string W { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("a")]
        public double A { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("d")]
        public double D { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("c")]
        public double C { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Statistics
    {
        /// <summary>
        /// 
        /// </summary>
        public Author author { get; set; }

        /// <summary>
        /// The Total number of commits authored by the contributor.
        /// </summary>
        public double total { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Week> weeks { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Participation
    {
        /// <summary>
        /// Total commit count for all including owner in the last 52 weeks.
        /// </summary>
        public List<int> all { get; set; }

        /// <summary>
        /// Total commit count for owner only in the last 52 weeks.
        /// </summary>
        public List<int> owner { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class CommitActivity
    {
        /// <summary>
        /// group of commits per day, starting on Sunday.
        /// </summary>
        public List<int> days { get; set; }

        /// <summary>
        /// Total number of coomits per week.
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// This is a Unix timestamp also called Unix epoch or Unix time.
        /// It's the number of seconds that have elapsed since January 1, 1970 (midnight UTC/GMP), not counting leap seconds. 
        /// </summary>
        public int week { get; set; }
    }
}
