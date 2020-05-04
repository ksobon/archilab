using System;
using RestSharp;

namespace archilab.Http
{
    /// <summary>
    /// 
    /// </summary>
    public class Request
    {
        internal string RequestString { get; set; }
        internal Method RequestType { get; set; }
        internal RestRequest InternalRequest { get; set; }

        private Request(string requestString, string requestType)
        {
            InternalSetRequest(requestString, requestType);
        }

        private void InternalSetRequest(string requestString, string requestType)
        {
            RequestString = requestString;
            RequestType = (Method)Enum.Parse(typeof(Method), requestType);
            InternalRequest = new RestRequest(requestString, RequestType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestString"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        public static Request ByStringAndType(string requestString, string requestType)
        {
            return new Request(requestString, requestType);
        }
    }
}
