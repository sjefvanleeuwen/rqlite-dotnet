using RqLite.Client.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace RqLite.Client.Tests
{
    public class DiagnosticsClientTests
    {
        private static string rqLiteEndpoint = "http://127.0.0.1:4003";

        [Theory]
        [InlineData("{\"build\":{\"branch\":\"unknown\",\"build_time\":\"unknown\",\"commit\":\"unknown\",\"version\":\"5\"},\"http\":{\"addr\":\"127.0.0.1:4003\",\"auth\":\"disabled\",\"redirect\":\"localhost:4003\"},\"node\":{\"start_time\":\"2021-01-31T22:30:23.6977071+01:00\",\"uptime\":\"11h47m37.0858631s\"},\"runtime\":{\"GOARCH\":\"amd64\",\"GOMAXPROCS\":16,\"GOOS\":\"windows\",\"num_cpu\":16,\"num_goroutine\":18,\"version\":\"go1.13.3\"},\"store\":{\"addr\":\"127.0.0.1:4004\",\"apply_timeout\":\"10s\",\"db_conf\":{\"DSN\":\"\",\"Memory\":true},\"dir\":\"C:\\\\rqlite\\\\~\\\\node2\",\"election_timeout\":\"1s\",\"heartbeat_timeout\":\"1s\",\"leader\":{\"addr\":\"127.0.0.1:4004\",\"node_id\":\"2\"},\"metadata\":{\"1\":{\"api_addr\":\"localhost:4001\",\"api_proto\":\"http\"},\"2\":{\"api_addr\":\"localhost:4003\",\"api_proto\":\"http\"},\"3\":{\"api_addr\":\"localhost:4005\",\"api_proto\":\"http\"}},\"node_id\":\"2\",\"nodes\":[{\"id\":\"1\",\"addr\":\"127.0.0.1:4002\"},{\"id\":\"2\",\"addr\":\"127.0.0.1:4004\"},{\"id\":\"3\",\"addr\":\"127.0.0.1:4006\"}],\"raft\":{\"applied_index\":\"20034\",\"commit_index\":\"20034\",\"fsm_pending\":\"0\",\"last_contact\":\"0\",\"last_log_index\":\"20034\",\"last_log_term\":\"5\",\"last_snapshot_index\":\"20031\",\"last_snapshot_term\":\"2\",\"latest_configuration\":\"[{Suffrage:Voter ID:1 Address:127.0.0.1:4002} {Suffrage:Voter ID:2 Address:127.0.0.1:4004} {Suffrage:Voter ID:3 Address:127.0.0.1:4006}]\",\"latest_configuration_index\":\"0\",\"log_size\":\"8388608\",\"num_peers\":\"2\",\"protocol_version\":\"3\",\"protocol_version_max\":\"3\",\"protocol_version_min\":\"0\",\"snapshot_version_max\":\"1\",\"snapshot_version_min\":\"0\",\"state\":\"Leader\",\"term\":\"5\"},\"request_marshaler\":{\"compression_batch\":5,\"compression_size\":150,\"force_compression\":false},\"snapshot_interval\":30000000000,\"snapshot_threshold\":8192,\"sqlite3\":{\"db_size\":98304,\"dsn\":\"\",\"fk_constraints\":\"disabled\",\"path\":\":memory:\",\"version\":\"3.34.0\"},\"trailing_logs\":10240}}")]
        public async Task CanDeserializeJsonDiagnosticsPayload(string jsonPayload)
        {
            var target = RqLiteDiagnosticsSerializer.FromJson(jsonPayload);
            Assert.True(target.Store.Nodes.Length == 3);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanRetrieveDiagnosticsFromHttp()
        {
            var client = new RqLiteDiagnosticsClient();
            var result = await client.GetDiagnostics(rqLiteEndpoint);
            Assert.Contains(result.Http.Addr, rqLiteEndpoint);
        }
    }
}
