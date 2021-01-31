using System;
using System.Net.Http;

namespace RqLite.Client
{
    public class RqLiteQueryException : Exception
    {
        public RqLiteQueryException(string query, HttpResponseMessage response) : base(response.ReasonPhrase)
        {
            Query = query;
            Response = response;
        }

        public string ResponseContent { get { return Response.Content.ReadAsStringAsync().Result; } }

        public string Query { get; }
        public HttpResponseMessage Response { get; }
    }
}
