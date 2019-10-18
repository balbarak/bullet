using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Net.Http;
using System.Collections.Concurrent;

namespace Bullet.Core
{
    public class BulletManager
    {
        private string _url;

        public BulletClient[] Clients { get; private set; }

        public double TotalSeconds => TimeSpan.FromMilliseconds(Clients.SelectMany(a => a.Responses).Sum(a => a.Latency)).TotalSeconds;

        private int _totalRequests;
        private int _failedRequests;

        public int TotalRequests => _totalRequests;
        public int TotalFailedRequests => _failedRequests;
        public TimeSpan Elapsed { get; private set; }

        public double RequestPerSecond => TotalRequests / (Elapsed.TotalMilliseconds / 1000);

        public event EventHandler OnStart;
        public event EventHandler OnFinished;
        
        public BulletManager(string url)
        {
            _url = url;
        }

        public async Task StartGetAsync(int connections = 1, int durationInSeconds = 1)
        {
            try
            {
                await Task.Run(async () =>
                  {
                      GC.Collect();

                      OnStart?.Invoke(this, EventArgs.Empty);

                      SetupClients(connections);

                      var events = new List<ManualResetEventSlim>();
                      var threads = new List<Thread>();
                      var tasks = new Task[connections];
                      var duration = TimeSpan.FromSeconds(durationInSeconds);
                      var sw = Stopwatch.StartNew();

                      Parallel.For(0,connections,(index) =>
                      {
                          tasks[index] = StartGetClient(Clients[index], duration, sw);
                      });

                      Elapsed = sw.Elapsed;

                      await Task.WhenAll(tasks);

                  });
            }
            catch (Exception)
            {

            }

            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        
        private void SetupClients(int connections)
        {
            Clients = new BulletClient[connections];

            for (int i = 0; i < connections; i++)
            {
                var client = CreateNewClient(i + 1);

                Clients[i] = client;
            }
        }

        private Task StartGetClient(BulletClient client, TimeSpan duration, Stopwatch stopwatch)
        {
            try
            {
                while (duration.TotalMilliseconds > stopwatch.Elapsed.TotalMilliseconds)
                {
                    Interlocked.Increment(ref _totalRequests);

                    client.Get();
                }

            }
            catch (Exception)
            {
                Interlocked.Increment(ref _failedRequests);
            }
            finally
            {

            }

            return Task.CompletedTask;
        }

        private BulletClient CreateNewClient(int index)
        {
            var result = new BulletClient(_url, index);

            return result;
        }

    }
}
