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

        public async ValueTask StartGetAsync(int connections = 1, int durationInSeconds = 1)
        {
            Clients.Clear();

            for (int i = 0; i < connections; i++)
            {
                var client = CreateNewClient(i + 1);
                Clients.Add(client);
            }

            List<Task> tasks = new List<Task>();

            var sw = Stopwatch.StartNew();

            foreach (var item in Clients)
            {
                var task = StartGetClient(item, durationInSeconds);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            sw.Stop();

            Elapsed = sw.Elapsed;
        }

        private Task StartGetClient(BulletClient client, int durationInSeconds)
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
