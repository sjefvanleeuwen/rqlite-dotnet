using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RqLite.Client.Benchmarks
{
    class Program
    {
        private static void writeJs(List<string> series, List<double> opssec, int concurrent)
        {
            File.WriteAllText("output/benchmark.js", @$"var data = [{{
  x: {JsonSerializer.Serialize(series)},
  y: {JsonSerializer.Serialize(opssec)},
  type: 'line',
  name: 'ops/sec concurreny: {concurrent}'
}}
];");
        }

        static async Task Main(string[] args)
        {
            List<string> series = new List<string>();
            List<double> opssecs = new List<double>();
            List<double> maxC = new List<double>();
            var results = new ConcurrentBag<string>();
            var concurrent = 25;
            var tasks = new Task[concurrent];
            var semaphore = new SemaphoreSlim(concurrent, concurrent);
            var  connectionString = "http://localhost:4001,http://localhost:4003,http://localhost:4005";
            Console.WriteLine("Hello World!");
            RqLiteFlags maskDefault = (RqLiteFlags.Pretty | RqLiteFlags.Timings | RqLiteFlags.Transaction);
            RqLiteFlags maskTransaction = (RqLiteFlags.Transaction);
            var client = new RqLiteClient(connectionString);
           // var dropTable = client.Execute("DROP TABLE FOO");
            //var createTable = client.Execute("CREATE TABLE FOO (age int)");
            // Assert.Equal(dropTable, createTable);
            int max = 0;
            int sampleSize = 25/concurrent;
            object sync = new object();
            for (int j = 0; j < 10;j++) {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                tasks = new Task[concurrent];
                try
                {
                    for (int i = 0; i < sampleSize; i++)
                    {
                        for (int k = 0; k < concurrent; k++)
                        {
                            tasks[k] = Task.Run(async() =>
                            {
                                
                                try
                                {
                                    await semaphore.WaitAsync();
                                    var client1 = new RqLiteClient(connectionString);
                                    results.Add(await client1.ExecuteAsync("INSERT INTO FOO VALUES(?)", (i + 1) * (j + 1), (i + 1) * (j + 1)));
                                }
                                catch (Exception ex)
                                {

                                }
                                finally
                                {
                                    semaphore.Release(1);
                                }
                            });
                            
                        }
                    }
                }
                catch (RqLiteQueryException ex)
                {
                   // Console.WriteLine(ex.Message);
                }
               // Thread.Sleep(1000);
                Task.WaitAll(tasks);
                sw.Stop();
                double opssec = Math.Round((double)sampleSize*concurrent / sw.ElapsedMilliseconds * 1000, 2);
                var total = client.Execute("SELECT COUNT(*) FROM foo");
                Console.WriteLine(total);
                Console.WriteLine($"avg: {sw.ElapsedMilliseconds / (sampleSize * concurrent)}ms concurrent: {concurrent} ops/sec: {opssec} total:{concurrent*sampleSize} time:{sw.ElapsedMilliseconds}");
                series.Add($"run {j}");
                opssecs.Add(opssec);
                maxC.Add(max);
                writeJs(series, opssecs, concurrent);
                max = 0;
               // semaphore.Release(concurrent);
            }
            File.WriteAllText(@"./results.json", JsonSerializer.Serialize(results));

            Console.ReadLine();
        }
    }
}
