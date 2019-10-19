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
        private int _totalRequests;
        private int _failedRequests;
        private string _url;

        public BulletClient[] Clients { get; private set; }

        public int Progress { get; private set; }

        public bool IsBusy { get; private set; }

        public int TotalRequests => _totalRequests;
        public int TotalFailedRequests => _failedRequests;
        public TimeSpan Elapsed { get; private set; }

        public double RequestPerSecond => TotalRequests / (Elapsed.TotalMilliseconds / 1000);

        public int Connections => Clients.Where(a => a.Responses.Any()).Count();

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
                IsBusy = true;

                await Task.Run(async () =>
                  {
                      GC.Collect();

                      OnStart?.Invoke(this, EventArgs.Empty);

                      SetupClients(connections);

                      var events = new List<ManualResetEventSlim>();
                      var threads = new Thread[connections];
                      var tasks = new Task[connections];
                      var duration = TimeSpan.FromSeconds(durationInSeconds);
                      var sw = new Stopwatch();
                      sw.Start();

                      Parallel.For(0, connections, (index, state) =>
                        {
                            tasks[index] = StartGetClient(Clients[index], duration, sw);

                        });

                      await Task.WhenAll(tasks);

                  });

            }
            catch (Exception)
            {

            }
            finally
            {
                IsBusy = false;
            }

            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        public async Task StartGetAsyncThreads(int connections = 1, int durationInSeconds = 1)
        {
            try
            {
                IsBusy = true;

                await Task.Run(() =>
                {
                    GC.Collect();

                    OnStart?.Invoke(this, EventArgs.Empty);

                    SetupClients(connections);

                    var events = new List<ManualResetEventSlim>();
                    var threads = new Thread[connections];
                    var tasks = new Task[connections];
                    var duration = TimeSpan.FromSeconds(durationInSeconds);
                    var sw = new Stopwatch();

                    for (int i = 0; i < connections; i++)
                    {
                        var thread = new Thread((index) => StartGetClient(Clients[(int)index], duration, sw));
                        threads[i] = thread;
                    }

                    sw.Start();

                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i].Start(i);
                    }

                    Parallel.ForEach(threads, (item) => item.Join());

                });
            }
            catch (Exception)
            {

            }
            finally
            {
                IsBusy = false;
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
                    client.Get();

                    Interlocked.Increment(ref _totalRequests);
                    
                    Elapsed = stopwatch.Elapsed;
                }

                stopwatch.Stop();
                Elapsed = stopwatch.Elapsed;
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
