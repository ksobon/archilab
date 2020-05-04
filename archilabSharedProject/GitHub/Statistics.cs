using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

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
        public string login { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string avatar_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string gravatar_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string html_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string followers_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string following_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string gists_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string starred_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string subscriptions_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string organizations_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string repos_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string events_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string received_events_url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool site_admin { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Week
    {
        /// <summary>
        /// 
        /// </summary>
        public string w { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double a { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double d { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double c { get; set; }
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
