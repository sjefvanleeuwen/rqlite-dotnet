![rqlite-dotnet](doc/img/rqlite-dotnet.svg)

[![nuget](https://img.shields.io/nuget/v/RqLite.Client)](https://www.nuget.org/packages/RqLite.Client/)
# What is it

A lightweight database HTTP API client for rqlite. rqlite is a lightweight, distributed relational database, which uses SQLite as its storage engine. Forming a cluster is very straightforward, it gracefully handles leader elections, and tolerates failures of machines, including the leader. rqlite is available for Linux, OSX, and Microsoft Windows.

rqlite uses Raft to achieve consensus across all the instances of the SQLite databases, ensuring that every change made to the system is made to a quorum of SQLite databases, or none at all.

# Quick Start

## Install NuGet

```
dotnet add package RqLite.Client --version 0.0.1-alpha
```

or visit the nuget for other options on: https://www.nuget.org/packages/RqLite.Client/


## Initialize a client

You can initialize the client with a single uri (single node operation), or if running a cluster you can provide multiple endpoints as illustrated below. The client will automatically look for the RAFT leader node.

```csharp
 var client = new RqLiteClient("http://localhost:4001,http://localhost:4002,http://localhost:4003");
```

## Perform an Execution

Executions can be used to execute sql statements such as "INSERT, "DROP TABLE foo" and "CREATE TABLE foo":

```csharp
var createTable = client.Execute("CREATE TABLE foo (id integer not null primary key, name text)");
var createFiona = await client.ExecuteAsync("INSERT INTO foo(name) VALUES(\"fiona\")");
```

## Perform a query

Queries are used for sql statements such as the "SELECT" query. You can either use sync or async calls:

```csharp
var readFiona = client.Query("SELECT * FROM FOO WHERE name=\"fiona\"");
var readFiona = await client.QueryAsync("SELECT * FROM FOO WHERE name=\"fiona\"");
```

## Setting multiple flags

rqlite provides three flags. 

* Flag for timings,showing execution time in the json query result
* Flag for pretty print, return pretty print json in the query result
* Flag for transaction, execution of multiple sql statements in one execution (as shown above)

These can be passed by the `RqLiteFlags` enum as a mask. To Enable all flags:

```csharp
RqLiteFlags maskAll =  (RqLiteFlags.Pretty | RqLiteFlags.Timings | RqLiteFlags.Transaction);
```

Then pass the mask into the Query or Execution path.

## Perform a transaction

You can perform multiple sql statements in one transaction. Please use the flag option:

```csharp
RqLiteFlags maskTransaction = RqLiteFlags.Transaction;
var transaction = await client.ExecuteAsync(new string[] {
    "INSERT INTO foo(name) VALUES(\"gary\")",
    "INSERT INTO foo(name) VALUES(\"fred\")"
}, maskTransaction);
```