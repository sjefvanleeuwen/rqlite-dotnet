using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RqLite.Client
{
    public class RqLiteClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Uri[] endpoint_uri;
        private Uri raftLeaderEndpoint;

        public RqLiteClient(string connectionString)
        {
            endpoint_uri = (from p in connectionString.Split(',') select new Uri(p)).ToArray();
            raftLeaderEndpoint = endpoint_uri[0];
        }

        private string getFlagsQueryString(RqLiteFlags mask)
        {
            var queryString = new StringBuilder();
            if ((mask & RqLiteFlags.Pretty) == RqLiteFlags.Pretty)
                queryString.Append("&pretty");
            if ((mask & RqLiteFlags.Timings) == RqLiteFlags.Timings)
                queryString.Append("&timings");
            if ((mask & RqLiteFlags.Transaction) == RqLiteFlags.Transaction)
                queryString.Append("&transaction");
            return queryString.ToString().TrimStart('&');
        }
        public async Task<string> ExecuteAsync(string sqlStatement, RqLiteFlags flags = 0)
        {
            return await ExecuteAsync(new[] { sqlStatement }, flags);
        }
        public string Execute(string sqlStatement, RqLiteFlags flags = 0)
        {
            return ExecuteAsync(new[] { sqlStatement }, flags).Result;
        }
        public string Execute(IEnumerable<string> sqlStatements, RqLiteFlags flags = 0)
        {
            return ExecuteAsync(sqlStatements, flags).Result;
        }

        public async Task<string> ExecuteAsync(IEnumerable<string> sqlStatements, RqLiteFlags flags = 0)
        {
            Exception ex = null;
            StringContent content = new StringContent(JsonSerializer.Serialize(sqlStatements), Encoding.UTF8, "application/json");
            for (int i = 0; i < endpoint_uri.Count(); i++)
            {
                try
                {
                    var response = await client.PostAsync(raftLeaderEndpoint.ToString() + "db/execute?" + getFlagsQueryString(flags), content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                        continue;
                    throw new Exception(response.StatusCode.ToString());
                }
                catch (Exception ex1)
                {
                    raftLeaderEndpoint = endpoint_uri[i];
                    ex = ex1;
                    continue;
                }
            }
            throw new Exception("No RqLite Leader Node found.",ex);
        }

        public string Query(string sqlStatement, RqLiteFlags flags = 0)
        {
            return QueryAsync(sqlStatement, flags).Result;
        }

        public async Task<string> QueryAsync(string sqlStatement, RqLiteFlags flags = 0)
        {
            Exception ex = null;
            for (int i = 0; i < endpoint_uri.Count(); i++)
            {
                try
                {
                    string uri = raftLeaderEndpoint.ToString() + "db/query?" + getFlagsQueryString(flags) + "&q=" + Uri.EscapeDataString(sqlStatement);
                    var response = await client.GetAsync(uri);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                        continue;
                    throw new Exception(response.StatusCode.ToString());
                }
                catch(Exception ex1)
                {
                    raftLeaderEndpoint = endpoint_uri[i];
                    ex = ex1;
                    continue;
                }
            }
            throw new Exception("No RqLite Leader Node found.",ex);
        }
    }
}
