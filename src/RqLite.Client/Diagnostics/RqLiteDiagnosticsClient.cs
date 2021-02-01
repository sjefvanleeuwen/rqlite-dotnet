using RqLite.Client.Diagnostics.Model;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RqLite.Client.Diagnostics
{
    public class RqLiteDiagnosticsClient
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<RqLiteDiagnosticsConfiguration> GetDiagnostics(string uri)
        {
            var response = await client.GetAsync(uri + "/status");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return  RqLiteDiagnosticsSerializer.FromJson(await response.Content.ReadAsStringAsync());
            throw new Exception(response.StatusCode.ToString());
        }
    }
}
