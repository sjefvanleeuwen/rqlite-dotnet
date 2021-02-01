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

        public async Task<string> ExecuteAsync(string sqlStatement, params object[] parameters)
        {
            return await ExecuteAsync(new[] { sqlStatement }, RqLiteFlags.None, parameters);
        }

        public async Task<string> ExecuteAsync(string sqlStatement, RqLiteFlags flags, params object[] parameters)
        {

            return await ExecuteAsync(new[] { sqlStatement }, flags, parameters);
        }

        private StringContent createPayload(IEnumerable<string> sqlStatements, RqLiteFlags flags = 0, params object[] parameters)
        {
            StringContent content = null;
            if (parameters.Length == 0)
            {
                content = new StringContent(JsonSerializer.Serialize(sqlStatements), Encoding.UTF8, "application/json");
            }
            else
            {
                if (sqlStatements.Count() > 1)
                {
                    throw new Exception("This Rqlite client does not (yet) support multiple parameterized SQL statements in one transaction.");
                }
                else
                {
                    var parameterizedPayload = new object[parameters.Count() + 1];
                    parameterizedPayload[0] = sqlStatements.ElementAt(0);
                    for (int i = 0; i < parameters.Count(); i++)
                    {
                        parameterizedPayload[i + 1] = parameters[i];
                    }
                    content = new StringContent(JsonSerializer.Serialize(new[] { parameterizedPayload }), Encoding.UTF8, "application/json");

                }
            }
            return content;
        }

        public async Task<string> ExecuteAsync(IEnumerable<string> sqlStatements, RqLiteFlags flags = 0, params object[] parameters)
        {
            Exception ex = null;
            var content = createPayload(sqlStatements, flags, parameters);
            HttpResponseMessage response = null;
            for (int i = 0; i < endpoint_uri.Count(); i++)
            {
                try
                {
                    response = await client.PostAsync(raftLeaderEndpoint.ToString() + "db/execute?" + getFlagsQueryString(flags), content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                        continue;
                    throw new Exception(response.StatusCode.ToString());
                }
                catch (Exception ex1)
                {
                    switch (ex1.HResult)
                    {
                        case -2147467259: // Connection refused, select a new leader.
                            if (i==endpoint_uri.Length)
                                throw new Exception("No RqLite Leader Node found.", ex);
                            i++;
                            raftLeaderEndpoint = endpoint_uri[i];
                            response = await client.GetAsync(raftLeaderEndpoint.ToString() + "status");
                            break;
                        case -2146233088:
                            throw new RqLiteQueryException(sqlStatements.ElementAt(0), response);
                            break;
                        default:
                            throw ex1;
                    }
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

        public async Task<string> QueryAsync(string sqlStatement, params object[] parameters)
        {
            return QueryAsync(sqlStatement, 0, parameters).Result;
        }

        public async Task<string> QueryAsync(string sqlStatement, RqLiteFlags flags = 0, params object[] parameters)
        {
            Exception ex = null;
            var content = createPayload(new[] { sqlStatement }, flags, parameters);
            for (int i = 0; i < endpoint_uri.Count(); i++)
            {
                try
                {
                    string uri = null;
                    HttpResponseMessage response = null;
                    if (parameters.Length == 0)
                    {
                        uri = raftLeaderEndpoint.ToString() + "db/query?" + getFlagsQueryString(flags) + "&q=" + Uri.EscapeDataString(sqlStatement);
                        response = await client.GetAsync(uri);
                    }
                    else
                    {
                        response = await client.PostAsync(raftLeaderEndpoint.ToString() + "db/query?" + getFlagsQueryString(flags), content);
                    }
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
