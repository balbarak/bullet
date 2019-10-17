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
        public List<BulletClient> Clients { get; private set; } = new List<BulletClient>();

        public double TotalSeconds => TimeSpan.FromMilliseconds(Clients.SelectMany(a => a.Responses).Sum(a => a.Duration)).TotalSeconds;

        private int _totalRequests;

        public int TotalRequests => _totalRequests;
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
            await Task.Run(async () =>
            {
                GC.Collect();

                Clients.Clear();

                OnStart?.Invoke(this, EventArgs.Empty);

                SetupClients(connections);

                var events = new List<ManualResetEventSlim>();
                var threads = new List<Thread>();

                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 20
                };

                var duration = TimeSpan.FromSeconds(durationInSeconds);

                var sw = Stopwatch.StartNew();

                Parallel.ForEach(Clients, options, (item) =>
                {
                    var resetEvent = new ManualResetEventSlim(false);

                    StartGetClient(item, duration, sw, resetEvent);

                    events.Add(resetEvent);
                });

                await WaitThreads(events);

            });

            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        private void SetupClients(int connections)
        {
            for (int i = 0; i < connections; i++)
            {
                var client = CreateNewClient(i + 1);
                Clients.Add(client);
            }
        }

        private Task WaitThreads(List<ManualResetEventSlim> events)
        {
            for (var i = 0; i < events.Count; i += 60)
            {
                var group = events.Skip(i).Take(50).Select(r => r.WaitHandle).ToArray();
                WaitHandle.WaitAll(group);
            }

            return Task.CompletedTask;
        }

        private void StartGetClient(BulletClient client, TimeSpan duration, Stopwatch stopwatch, ManualResetEventSlim resetEventSlim)
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

            }
            finally
            {

                resetEventSlim?.Set();
            }
        }

        private BulletClient CreateNewClient(int index)
        {
            var result = new BulletClient(_url, index);

            return result;
        }

        private int GetTotalRequest()
        {
            int result = 0;
            
            for (int i = 0; i < Clients.Count; i++)
            {
                for (int y = 0; y < Clients[i].Responses.Count; y++)
                {
                    result++;
                }
            }

            return result;
        }
    }
}
