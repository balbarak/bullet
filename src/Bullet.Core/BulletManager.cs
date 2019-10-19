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
        private Stopwatch _localStopwatch;
        private System.Timers.Timer _timer;

        public BulletClient[] Clients { get; private set; }

        public int Progress { get; private set; }

        public bool IsBusy { get; private set; }

        public int TotalRequests => _totalRequests;
        public int TotalFailedRequests => _failedRequests;

        public TimeSpan Duration { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public double RequestPerSecond
        {
            get
            {
                if (Duration == null)
                    return 0;

                return TotalRequests / (Duration.TotalMilliseconds / 1000.0);
            }
        }

        public int Connections => Clients.Where(a => a.Responses.Any()).Count();

        public event EventHandler OnStart;
        public event EventHandler OnFinished;

        public BulletManager(string url)
        {
            _url = url;
            _localStopwatch = new Stopwatch();
            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public async Task StartGetAsync(int connections = 1, int durationInSeconds = 1)
        {
            try
            {
                IsBusy = true;

                await Task.Run(() =>
                  {
                      GC.Collect();

                      var proccessCount = Environment.ProcessorCount;

                      if (connections > proccessCount)
                          connections = proccessCount;

                      var events = new List<ManualResetEventSlim>();
                      var threads = new Thread[connections];
                      var duration = TimeSpan.FromSeconds(durationInSeconds);
                      var sw = new Stopwatch();


                      SetupClients(connections);

                      OnStart?.Invoke(this, EventArgs.Empty);

                      for (int i = 0; i < connections; i++)
                      {
                          var thread = new Thread(async (index) => await StartGetClient(Clients[(int)index], duration, sw));
                          threads[i] = thread;
                      }

                      sw.Start();

                      for (int i = 0; i < threads.Length; i++)
                      {
                          threads[i].Start(i);
                      }

                      foreach (var item in threads)
                      {
                          item.Join();
                      }

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

        //Best senario
        public async Task StartGetAsyncSeparateTimer(int connections = 1, int durationInSeconds = 1, CancellationToken token = default)
        {
            try
            {
                var ctk = token;

                IsBusy = true;

                _localStopwatch = new Stopwatch();

                await Task.Run(() =>
                {
                    GC.Collect();

                    Duration = TimeSpan.FromSeconds(durationInSeconds);

                    var proccessCount = Environment.ProcessorCount;
                    var events = new List<ManualResetEventSlim>();
                    var threads = new Thread[connections];


                    SetupClients(connections);

                    OnStart?.Invoke(this, EventArgs.Empty);

                    for (int i = 0; i < connections; i++)
                    {
                        var thread = new Thread(async (index) => await StartGetClient(Clients[(int)index], Duration,_localStopwatch, ctk));
                        threads[i] = thread;
                    }

                    Parallel.ForEach(threads, (thread) =>
                    {
                        var index = Array.IndexOf(threads, thread);

                        thread.Start(index);
                    });
                    
                    _localStopwatch.Start();

                    foreach (var item in threads)
                    {
                        item.Join();
                    }
                });

                _localStopwatch.Stop();

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
                        var thread = new Thread(async (index) => await StartGetClient(Clients[(int)index], duration, sw));
                        threads[i] = thread;
                    }

                    sw.Start();

                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i].Start(i);
                    }

                    foreach (var item in threads)
                    {
                        item.Join();
                    }

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

        private Task StartGetClient(BulletClient client, TimeSpan duration, Stopwatch stopwatch = null, CancellationToken token = default)
        {
            try
            {
                var ctk = token;

                if (stopwatch == null)
                    stopwatch = Stopwatch.StartNew();

                while (!ctk.IsCancellationRequested && duration.TotalMilliseconds > stopwatch.Elapsed.TotalMilliseconds)
                {
                    client.Get();

                    Interlocked.Increment(ref _totalRequests);
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


        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_localStopwatch != null)
                Elapsed = _localStopwatch.Elapsed;
        }

    }
}
