using Xunit;

namespace RqLite.Client.Tests
{
    public class ClientTests
    {
        public static string connectionString = "http://localhost:4001,http://localhost:4003,http://localhost:4005";

        [Fact]
        [Trait("Category", "Integration")]
        public void ShouldPerformBasicFunctions()
        {
            RqLiteFlags maskDefault = (RqLiteFlags.Pretty | RqLiteFlags.Timings);
            RqLiteFlags maskTransaction = (RqLiteFlags.Transaction);
            var client = new RqLiteClient(connectionString);
            var dropTable = client.Execute("DROP TABLE foo");
            var createTable = client.Execute("CREATE TABLE foo (id integer not null primary key, name text)");
            Assert.Equal(dropTable, createTable);
            var createFiona = client.Execute("INSERT INTO foo(name) VALUES(\"fiona\")");
            Assert.Equal("{\"results\":[{\"last_insert_id\":1,\"rows_affected\":1}]}", createFiona);
            var readFiona = client.Query("SELECT * FROM FOO WHERE name=\"fiona\"");
            Assert.Equal("{\"results\":[{\"columns\":[\"id\",\"name\"],\"types\":[\"integer\",\"text\"],\"values\":[[1,\"fiona\"]]}]}", readFiona);
            // Check timings
            Assert.DoesNotContain("time", readFiona);
            // Check json pretty print
            Assert.DoesNotContain("\n", readFiona);
            // Do multiple inserts in a single transaction.
            var transaction = client.Execute(new string[] {
                "INSERT INTO foo(name) VALUES(\"gary\")",
                "INSERT INTO foo(name) VALUES(\"fred\")"
            }, maskTransaction);
            Assert.Equal("{\"results\":[{\"last_insert_id\":2,\"rows_affected\":1},{\"last_insert_id\":3,\"rows_affected\":1}]}", transaction);
            var count = client.Query("SELECT COUNT(*) FROM foo");
            Assert.Equal("{\"results\":[{\"columns\":[\"COUNT(*)\"],\"types\":[\"\"],\"values\":[[3]]}]}", count);
            // Check flags
            readFiona = client.Query("SELECT * FROM FOO WHERE name=\"fiona\"", maskDefault);
            // Check timings
            //Assert.Contains("time", readFiona); // timing seem to be broken in newer version rqlite.
            // Check json pretty print
            Assert.Contains("\n", readFiona);
        }

        [Fact]
        [Trait("Category","Integration")]
        public async void ShouldPerformBasicFunctionsAsync()
        {
            RqLiteFlags maskDefault = (RqLiteFlags.Pretty | RqLiteFlags.Timings | RqLiteFlags.Transaction);
            RqLiteFlags maskTransaction = (RqLiteFlags.Transaction);
            var client = new RqLiteClient(connectionString);
            var dropTable = await client.ExecuteAsync("DROP TABLE foo");
            var createTable = await client.ExecuteAsync("CREATE TABLE foo (id integer not null primary key, name text)");
            Assert.Equal(dropTable, createTable);
            var createFiona = await client.ExecuteAsync("INSERT INTO foo(name) VALUES(\"fiona\")");
            Assert.Equal("{\"results\":[{\"last_insert_id\":1,\"rows_affected\":1}]}", createFiona);
            var readFiona = await client.QueryAsync("SELECT * FROM FOO WHERE name=\"fiona\"");
            Assert.Equal("{\"results\":[{\"columns\":[\"id\",\"name\"],\"types\":[\"integer\",\"text\"],\"values\":[[1,\"fiona\"]]}]}", readFiona);
            // Check timings
            Assert.DoesNotContain("time", readFiona);
            // Check json pretty print
            Assert.DoesNotContain("\n", readFiona);
            // Do multiple inserts in a single transaction.
            var transaction = await client.ExecuteAsync(new string[] {
                "INSERT INTO foo(name) VALUES(\"gary\")",
                "INSERT INTO foo(name) VALUES(\"fred\")"
            }, maskTransaction);
            Assert.Equal("{\"results\":[{\"last_insert_id\":2,\"rows_affected\":1},{\"last_insert_id\":3,\"rows_affected\":1}]}", transaction);
            var count = await client.QueryAsync("SELECT COUNT(*) FROM foo");
            Assert.Equal("{\"results\":[{\"columns\":[\"COUNT(*)\"],\"types\":[\"\"],\"values\":[[3]]}]}", count);
            // Check flags
            readFiona = await client.QueryAsync("SELECT * FROM FOO WHERE name=\"fiona\"",maskDefault);
            // Check timings
            // Assert.Contains("time", readFiona); // Timings seem to be broken in rqlite latest version
            // Check json pretty print
            Assert.Contains("\n", readFiona);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async void ShouldPerformParameterizedWriteAsync()
        {
            RqLiteFlags maskDefault = (RqLiteFlags.Pretty | RqLiteFlags.Timings | RqLiteFlags.Transaction);
            var client = new RqLiteClient(connectionString);
            var dropTable = await client.ExecuteAsync("DROP TABLE foo");
            var createTable = await client.ExecuteAsync("CREATE TABLE foo (id integer not null primary key, name text, age int)");
            Assert.Equal(dropTable, createTable);
            var parameters = new object[] { "fiona", 20 };
            var createFiona = await client.ExecuteAsync("INSERT INTO foo(name,age) VALUES(?,?)", parameters);
            Assert.Contains("\"last_insert_id\":1,", createFiona);
            var readFiona = await client.QueryAsync("SELECT * FROM FOO WHERE name=?", parameters[0] );
            Assert.Contains("fiona", readFiona);
            Assert.Contains("20", readFiona);
        }
    }
}