using RqLite.Client.Diagnostics;
using RqLite.Client.Diagnostics.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RqLite.Client.Raft
{
    /// <summary>
    /// The raft client sees after connection QOS.
    /// It automatically discovers the RAF network, but querying one available node.
    /// </summary>
    public class RqliteRaftClient
    {
        private static HttpClient http { get; set; } = new HttpClient();

        //private List<RqLiteDiagnosticsClient> diagnosticsClients {get;set;}
        private List<Uri> diagnosticsEndPoints { get; set; }
        public ConcurrentDictionary<string, RqLiteDiagnosticsConfiguration> Nodes {get;set;}

        public RqliteRaftClient(string connectionString)
        {
            diagnosticsEndPoints = (from p in connectionString.Split(',') select new Uri(p)).ToList();
            Nodes = new ConcurrentDictionary<string, RqLiteDiagnosticsConfiguration>();
        }

        /// <summary>
        /// Tunnel client requests through the RaftClient
        /// </summary>
        /// <param name="pathAndQuery">i.e.: /db/query?pretty</param>
        /// <param name="content">The Payload to post (!note: rqlite accepts Application/Json format)</param>
        /// <returns></returns>
        public async Task PostAsync(string pathAndQuery, StringContent content)
        {
            
        }

        public async Task GetAsync(string pathAndQuery)
        {
            http.GetAsync(pathAndQuery);
        }

        private string currentLeaderNode
        {
            get;set;
        }

        /// <summary>
        /// Returns the last known leader node. Which is usually the current leader node
        /// but does not query the raft quorum.
        /// </summary>
        public string LastKnownLeaderNode
        {
            get
            {
                return currentLeaderNode;
            }
        }

        /// <summary>
        /// Gets the new leader node. If the leader node endpoint is down, it will return the new leader node.
        /// Or it will fail with an exception.
        /// </summary>
        /// <returns>new leader node</returns>
        private string getNewLeaderNodeEndpoint()
        {
            return null;
        }

        /// <summary>
        /// Connects to the leader and follow nodes, then maintains health inspection.
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, RqLiteDiagnosticsConfiguration>> ConnectAsync()
        {
            Nodes.Clear();
            Parallel.For(0, diagnosticsEndPoints.Count(), (i) =>
            {
                try
                {
                    var result = RqLiteDiagnosticsClient.GetDiagnostics(diagnosticsEndPoints[i].ToString()).Result;
                    // get node id from diagnostics.
                    Nodes.TryAdd(diagnosticsEndPoints[i].ToString(), result);
                }
                catch (AggregateException ae)
                {
                    Nodes.TryAdd(diagnosticsEndPoints[i].ToString(), 
                        new RqLiteDiagnosticsConfiguration() { 
                            Exceptions = new List<Exception>() { ae } 
                        });
                }
            });

            // set current leader node by redirect
            try
            {
                // demoted raft quorum with single "leader"?
                currentLeaderNode = (from p in Nodes where p.Value.Http != null && p.Value.Http.Redirect == string.Empty select p).First().Value.Http.Addr;
            }
            catch (InvalidOperationException io)
            {
                currentLeaderNode = (from p in Nodes where p.Value.Http != null && p.Value.Http.Redirect != string.Empty select p).First().Value.Http.Redirect;
            }
            return await Task.FromResult(Nodes);
        }
    }
}
