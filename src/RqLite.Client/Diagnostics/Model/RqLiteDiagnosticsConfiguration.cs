namespace RqLite.Client.Diagnostics.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public partial class RqLiteDiagnosticsConfiguration
    {
        [JsonPropertyName("build")]
        public Build Build { get; set; }

        [JsonPropertyName("http")]
        public Http Http { get; set; }

        [JsonPropertyName("node")]
        public Node Node { get; set; }

        [JsonPropertyName("runtime")]
        public Runtime Runtime { get; set; }

        [JsonPropertyName("store")]
        public Store Store { get; set; }
    }

    public partial class Build
    {
        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("build_time")]
        public string BuildTime { get; set; }

        [JsonPropertyName("commit")]
        public string Commit { get; set; }

        [JsonPropertyName("version")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Version { get; set; }
    }

    public partial class Http
    {
        [JsonPropertyName("addr")]
        public string Addr { get; set; }

        [JsonPropertyName("auth")]
        public string Auth { get; set; }

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; }
    }

    public partial class Node
    {
        [JsonPropertyName("start_time")]
        public DateTimeOffset StartTime { get; set; }

        [JsonPropertyName("uptime")]
        public string Uptime { get; set; }
    }

    public partial class Runtime
    {
        [JsonPropertyName("GOARCH")]
        public string Goarch { get; set; }

        [JsonPropertyName("GOMAXPROCS")]
        public long Gomaxprocs { get; set; }

        [JsonPropertyName("GOOS")]
        public string Goos { get; set; }

        [JsonPropertyName("num_cpu")]
        public long NumCpu { get; set; }

        [JsonPropertyName("num_goroutine")]
        public long NumGoroutine { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public partial class Store
    {
        [JsonPropertyName("addr")]
        public string Addr { get; set; }

        [JsonPropertyName("apply_timeout")]
        public string ApplyTimeout { get; set; }

        [JsonPropertyName("db_conf")]
        public DbConf DbConf { get; set; }

        [JsonPropertyName("dir")]
        public string Dir { get; set; }

        [JsonPropertyName("election_timeout")]
        public string ElectionTimeout { get; set; }

        [JsonPropertyName("heartbeat_timeout")]
        public string HeartbeatTimeout { get; set; }

        [JsonPropertyName("leader")]
        public Leader Leader { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, Metadatum> Metadata { get; set; }

        [JsonPropertyName("node_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long NodeId { get; set; }

        [JsonPropertyName("nodes")]
        public NodeElement[] Nodes { get; set; }

        [JsonPropertyName("raft")]
        public Raft Raft { get; set; }

        [JsonPropertyName("request_marshaler")]
        public RequestMarshaler RequestMarshaler { get; set; }

        [JsonPropertyName("snapshot_interval")]
        public long SnapshotInterval { get; set; }

        [JsonPropertyName("snapshot_threshold")]
        public long SnapshotThreshold { get; set; }

        [JsonPropertyName("sqlite3")]
        public Sqlite3 Sqlite3 { get; set; }

        [JsonPropertyName("trailing_logs")]
        public long TrailingLogs { get; set; }
    }

    public partial class DbConf
    {
        [JsonPropertyName("DSN")]
        public string Dsn { get; set; }

        [JsonPropertyName("Memory")]
        public bool Memory { get; set; }
    }

    public partial class Leader
    {
        [JsonPropertyName("addr")]
        public string Addr { get; set; }

        [JsonPropertyName("node_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long NodeId { get; set; }
    }

    public partial class Metadatum
    {
        [JsonPropertyName("api_addr")]
        public string ApiAddr { get; set; }

        [JsonPropertyName("api_proto")]
        public string ApiProto { get; set; }
    }

    public partial class NodeElement
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonPropertyName("addr")]
        public string Addr { get; set; }
    }

    public partial class Raft
    {
        [JsonPropertyName("applied_index")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long AppliedIndex { get; set; }

        [JsonPropertyName("commit_index")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long CommitIndex { get; set; }

        [JsonPropertyName("fsm_pending")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long FsmPending { get; set; }

        [JsonPropertyName("last_contact")]
        public string LastContact { get; set; }

        [JsonPropertyName("last_log_index")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LastLogIndex { get; set; }

        [JsonPropertyName("last_log_term")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LastLogTerm { get; set; }

        [JsonPropertyName("last_snapshot_index")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LastSnapshotIndex { get; set; }

        [JsonPropertyName("last_snapshot_term")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LastSnapshotTerm { get; set; }

        /// <summary>
        /// Not in json format, issue posted at: https://github.com/rqlite/rqlite/issues/748
        /// </summary>
        [JsonPropertyName("latest_configuration")]
        public string LatestConfigurationAsString { get; set; }

        [JsonPropertyName("latest_configuration_index")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LatestConfigurationIndex { get; set; }

        [JsonPropertyName("log_size")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LogSize { get; set; }

        [JsonPropertyName("num_peers")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long NumPeers { get; set; }

        [JsonPropertyName("protocol_version")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ProtocolVersion { get; set; }

        [JsonPropertyName("protocol_version_max")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ProtocolVersionMax { get; set; }

        [JsonPropertyName("protocol_version_min")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ProtocolVersionMin { get; set; }

        [JsonPropertyName("snapshot_version_max")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long SnapshotVersionMax { get; set; }

        [JsonPropertyName("snapshot_version_min")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long SnapshotVersionMin { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("term")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Term { get; set; }
    }

    public partial class RequestMarshaler
    {
        [JsonPropertyName("compression_batch")]
        public long CompressionBatch { get; set; }

        [JsonPropertyName("compression_size")]
        public long CompressionSize { get; set; }

        [JsonPropertyName("force_compression")]
        public bool ForceCompression { get; set; }
    }

    public partial class Sqlite3
    {
        [JsonPropertyName("db_size")]
        public long DbSize { get; set; }

        [JsonPropertyName("dsn")]
        public string Dsn { get; set; }

        [JsonPropertyName("fk_constraints")]
        public string FkConstraints { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
