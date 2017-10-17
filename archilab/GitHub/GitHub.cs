using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using RestSharp;

namespace archilab.GitHub
{
    /// <summary>
    /// Wrapper classes for GitHub interactions.
    /// </summary>
    public class GitHub
    {
        internal string BaseUrl { get; set; }
        internal RestClient Client { get; set; }
        internal string Owner { get; set; }
        internal string Repo { get; set; }

        private GitHub(string baseUrl, string owner, string repo)
        {
            InternalSetGitHub(baseUrl, owner, repo);
        }

        private void InternalSetGitHub(string baseUrl, string owner, string repo)
        {
            BaseUrl = baseUrl;
            Owner = owner;
            Repo = repo;
            Client = new RestClient(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="owner"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static GitHub CreateClient(string baseUrl, string owner, string repo)
        {
            return new GitHub(baseUrl, owner, repo);
        }

        /// <summary>
        /// Get contributors list with additions, deletions, and commit counts.
        /// </summary>
        /// <returns></returns>
        [MultiReturn("author", "total", "weeks")]
        public Dictionary<string, object> GET_Statistics()
        {
            var request = new RestRequest("/repos/" + Owner + "/" + Repo + "/stats/contributors", Method.GET);
            var response = Client.Execute<List<Statistics>>(request);

            if (response == null) return null;

            return new Dictionary<string, object>
            {
                { "author", response.Data.Select(x => x.author).ToList()},
                { "total", response.Data.Select(x => x.total).ToList()},
                { "weeks", response.Data.Select(x => x.weeks).ToList()}
            };
        }

        /// <summary>
        /// Returns the last year of commit activity grouped by week. The days array is a group of commits per day, starting on Sunday.
        /// </summary>
        /// <returns></returns>
        [MultiReturn("days", "timestamp", "total")]
        public Dictionary<string, object> GET_CommitActivity()
        {
            var request = new RestRequest("/repos/" + Owner + "/" + Repo + "/stats/commit_activity", Method.GET);
            var response = Client.Execute<List<CommitActivity>>(request);

            if (response == null) return null;

            return new Dictionary<string, object>
            {
                { "days", response.Data.Select(x => x.days).ToList()},
                { "timestamp", response.Data.Select(x => x.week).ToList()},
                { "total", response.Data.Select(x => x.total).ToList()}
            };
        }

        /// <summary>
        /// Get the number of additions and deletions per week.
        /// </summary>
        /// <returns></returns>
        [MultiReturn("timestamp", "additions", "deletions")]
        public Dictionary<string, List<int>> GET_CodeFrequency()
        {
            var request = new RestRequest("/repos/" + Owner + "/" + Repo + "/stats/code_frequency", Method.GET);
            var response = Client.Execute<List<List<int>>>(request);

            if (response == null) return null;

            var result = response.Data
                .SelectMany(inner => inner.Select((item, index) => new { item, index }))
                .GroupBy(i => i.index, i => i.item)
                .Select(g => g.ToList())
                .ToList();

            return new Dictionary<string, List<int>>
            {
                { "timestamp", result[0]},
                { "additions", result[1]},
                { "deletions", result[2]}
            };
        }

        /// <summary>
        /// Get the number of commits per hour in each day.
        /// </summary>
        /// <returns></returns>
        [MultiReturn("day", "hour", "commits")]
        public Dictionary<string, List<int>> GET_PunchCard()
        {
            var request = new RestRequest("/repos/" + Owner + "/" + Repo + "/stats/punch_card", Method.GET);
            var response = Client.Execute<List<List<int>>>(request);

            if (response == null) return null;

            var result = response.Data
                .SelectMany(inner => inner.Select((item, index) => new { item, index }))
                .GroupBy(i => i.index, i => i.item)
                .Select(g => g.ToList())
                .ToList();

            return new Dictionary<string, List<int>>
            {
                { "day", result[0]},
                { "hour", result[1]},
                { "commits", result[2]}
            };
        }

        /// <summary>
        /// Get the weekly commit count for the repository owner and everyone else. 
        /// Returns the total commit counts for the owner and total commit counts in all. 
        /// All is everyone combined, including the owner in the last 52 weeks. 
        /// If you'd like to get the commit counts for non-owners, you can subtract owner from all.
        /// The array order is oldest week (index 0) to most recent week.
        /// </summary>
        /// <returns></returns>
        [MultiReturn("all", "owner")]
        public Dictionary<string, List<int>> GET_Participation()
        {
            var request = new RestRequest("/repos/" + Owner + "/" + Repo + "/stats/participation", Method.GET);
            var response = Client.Execute<Participation>(request);

            if (response == null) return null;

            return new Dictionary<string, List<int>>
            {
                { "all", response.Data.all},
                { "owner", response.Data.owner}
            };
        }

        /// <summary>
        /// Converts Unix Epoch timestamp to human readable date/time.
        /// </summary>
        /// <param name="timestamp">Unix epoch timestamp.</param>
        /// <returns></returns>
        public static string TimestampToDate(int timestamp)
        {
            return EpochToString(timestamp);
        }

        #region Utilities

        private static string EpochToString(int epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch).ToShortDateString();
        }

        #endregion
    }
}
