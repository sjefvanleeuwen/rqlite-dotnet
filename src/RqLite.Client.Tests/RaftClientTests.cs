using RqLite.Client.Raft;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RqLite.Client.Tests
{
    public class RaftClientTests
    {
        public static string connectionString = "http://localhost:4001,http://localhost:4003,http://localhost:4005";

        [Fact]
        [Trait("Category", "Integration")]
        public async void CanConnectToCluster()
        {
            var target = new RqliteRaftClient(connectionString);
            var nodes = await target.ConnectAsync();
#if CLUSTERTEST
            Assert.Equal(3, nodes.Count());
#else
            Assert.Equal(1, nodes.Count());
#endif
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanDiscoverCurrentLeaderNode()
        {
            var target = new RqliteRaftClient(connectionString);
            var nodes = await target.ConnectAsync();
            Assert.Equal("localhost:4001", target.LastKnownLeaderNode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanConnectToClusterWithFailingNode()
        {
            // Arrange, add a failing node.
            var target = new RqliteRaftClient($"{connectionString},http://127.0.0.2:4005");
            var nodes = await target.ConnectAsync();
#if CLUSTERTEST
            Assert.Equal(4, nodes.Count());
#else
            Assert.Equal(2, nodes.Count());
#endif
            // Expect exactly one failing node with an exception.
            Assert.Single(nodes.Where(p=>p.Value.Exceptions != null && p.Value.Exceptions.Count() == 1));
            // Expect it to be a HTTPRequestException
            Assert.Single(from p in nodes where 
                          p.Value.Exceptions != null && 
                          p.Value.Exceptions.Count() != 0 && 
                          p.Value.Exceptions[0].InnerException.GetType()==typeof(HttpRequestException)
                          select p.Value.Exceptions[0].InnerException);
        }
    }
}
