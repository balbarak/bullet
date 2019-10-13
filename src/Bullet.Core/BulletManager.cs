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
        private readonly Stopwatch _watch = new Stopwatch();
        private readonly int _durationInSeconds = 5;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly int _numberOfConnections;
        private readonly BulletClient _client;
        private ConcurrentQueue<BulletClient> _bulletClients;
        private string _url;

        private List<BulletClient> _clients = new List<BulletClient>();
        private CancellationTokenSource _ctk = new CancellationTokenSource();
        private DateTime _startTime;
        private int _successCount = 0;
        private int _faildCount = 0;

        public event EventHandler OnSecondElapsed;
        public event EventHandler OnClientBeginRequest;
        public event EventHandler OnClientSuccess;
        public event EventHandler OnClientFailed;
        public event EventHandler OnCompleted;

        public double TotalSeconds { get; private set; }

        public double MaxRequestPerSecond => 0; //_client.Requests.Where(a => a.Success).Max(p => p.ResponseTime);

        public double LowsetRequestPerSecond => 0; //_client.Requests.Where(a => a.Success).Min(p => p.ResponseTime);

        public double TotalAverageRequestPerSecond
        {
            get
            {
                //var successRequests = _client.Requests.Where(a => a.Success);

                //var count = successRequests.Count();

                //var totalRequestMillSeconds = successRequests.Sum(a => a.ResponseTime);

                //var totalSeconds = TimeSpan.FromMilliseconds(totalRequestMillSeconds).TotalSeconds;

                return 0;

                //return count / totalSeconds;
            }
        }

        public BulletClient[] Clientes => _clients.ToArray();


        public BulletManager(string url)
        {
            _url = url;
            _client = new BulletClient(1, _url);
            _bulletClients = new ConcurrentQueue<BulletClient>();

            SetupClients();
        }

        public BulletManager(string url, int numberConnections = 125, int durationInSeconds = 10) : this(url)
        {
            _timer.Interval = 1000;
            _timer.Elapsed += OnTimerElapsed;

            _url = url;
            _durationInSeconds = durationInSeconds;
            _numberOfConnections = numberConnections;

        }

       

        public Task StartDuration(TimeSpan duration,int threads,CancellationToken ctk =default)
        {
            var events = new List<ManualResetEventSlim>();
            
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < threads; i++)
            {
                var resetEvent = new ManualResetEventSlim(false);

                Thread thread;

                thread = new Thread(async (index) => await StartuDurationInternal(duration,sw,resetEvent,(int)index,ctk));

                thread.Start(i);
                events.Add(resetEvent);
            }

            for (var i = 0; i < events.Count; i += 50)
            {
                var group = events.Skip(i).Take(50).Select(r => r.WaitHandle).ToArray();
                WaitHandle.WaitAll(group);
            }

            OnCompleted?.Invoke(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        private async Task StartuDurationInternal(TimeSpan duration,Stopwatch sw, ManualResetEventSlim resetEvent,int index,CancellationToken ctk =default)
        {
            var localCtk = ctk;

            //var bulletClient = new BulletClient(index, _url);
            int count = 1;

            while (!ctk.IsCancellationRequested && duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                try
                {
                    await _client.GetAsyncFast(localCtk);
                }
                catch (Exception)
                {

                }

                count++;
            }
            
            //_bulletClients.Enqueue(bulletClient);

            resetEvent.Set();
        }

        public void Cancell()
        {
            _ctk.Cancel();
            _timer.Stop();
        }

        private void SetupClients()
        {
            //var httpClient = new HttpClient();

            //for (int i = 0; i < _numberOfConnections; i++)
            //{
            //    var client = new BulletClient(i,_url);

            //    client.OnSuccess += OnClientSuccessInternal;
            //    client.OnFailure += OnClientFailedInternal;
            //    client.OnRequestFinished += OnClientRequestFinished;
            //    client.OnRequestStarted += OnClientRequestStarted;
            //    _clients.Add(client);
            //}

            //_client.OnSuccess += OnClientSuccessInternal;
            //_client.OnFailure += OnClientFailedInternal;
            //_client.OnRequestFinished += OnClientRequestFinished;
            //_client.OnRequestStarted += OnClientRequestStarted;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnSecondElapsed?.Invoke(this, EventArgs.Empty);

            var seconds = (e.SignalTime - _startTime).TotalSeconds;

            //if (seconds > _durationInSeconds)
            //    Cancell();
        }

        private void OnClientFailedInternal(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _faildCount);

            OnClientFailed?.Invoke(this, EventArgs.Empty);
        }

        private void OnClientSuccessInternal(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _successCount);

            OnClientSuccess?.Invoke(this, EventArgs.Empty);
        }

        private void OnClientRequestStarted(object sender, EventArgs e)
        {
            OnClientBeginRequest?.Invoke(sender, EventArgs.Empty);
        }

        private void OnClientRequestFinished(object sender, EventArgs e)
        {

        }
    }
}
