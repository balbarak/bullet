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

        public int Count => Clients.SelectMany(a => a.Responses).Count();

        public List<BulletClient> Clients { get; private set; } = new List<BulletClient>();

        public double TotalSeconds => TimeSpan.FromMilliseconds(Clients.SelectMany(a => a.Responses).Sum(a => a.Duration)).TotalSeconds;

        public int TotalRequests => Clients.SelectMany(a => a.Responses).Count();

        public TimeSpan Elapsed { get; private set; }

        public double RequestPerSecond => Count / (Elapsed.TotalMilliseconds / 1000);

        public BulletManager(string url)
        {
            _url = url;
        }

        public Task StartGetAsync(int connections = 1, int durationInSeconds = 1)
        {
            GC.Collect();

            return Task.Run(() =>
            {
                Clients.Clear();

                for (int i = 0; i < connections; i++)
                {
                    var client = CreateNewClient(i + 1);
                    Clients.Add(client);
                }

                var events = new List<ManualResetEventSlim>();
                var threads = new List<Thread>();

                var sw = Stopwatch.StartNew();

                foreach (var item in Clients)
                {
                    var resetEvent = new ManualResetEventSlim(false);

                    Thread thread;

                    thread = new Thread((index) => StartGetClient(item, durationInSeconds,sw, resetEvent));
                    
                    thread.Start();

                    events.Add(resetEvent);
                }

                for (var i = 0; i < events.Count; i += 50)
                {
                    var group = events.Skip(i).Take(50).Select(r => r.WaitHandle).ToArray();
                    WaitHandle.WaitAll(group);
                }

                Elapsed = sw.Elapsed;
            });
            
        }

        private Task StartGetClientAsync(BulletClient client, int durationInSeconds)
        {
            return Task.Run(() =>
            {
                var duration = TimeSpan.FromSeconds(durationInSeconds);
                var sw = Stopwatch.StartNew();

                while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
                {
                    client.Get();
                }
            });

        }

        private void StartGetClient(BulletClient client, int durationInSeconds,Stopwatch stopwatch,ManualResetEventSlim resetEventSlim)
        {
            try
            {
                var duration = TimeSpan.FromSeconds(durationInSeconds);

                while (duration.TotalMilliseconds > stopwatch.Elapsed.TotalMilliseconds)
                {
                    client.Get();
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                resetEventSlim.Set();
            }
        }

        private BulletClient CreateNewClient(int index)
        {
            var result = new BulletClient(_url, index);

            result.OnRequestBegin += OnClientRequestBeginInternal;
            result.OnRequestEnd += OnClientRequestEndInternal;

            return result;
        }

        private void OnClientRequestEndInternal(object sender, int e)
        {

        }

        private void OnClientRequestBeginInternal(object sender, int e)
        {

        }
    }
}
