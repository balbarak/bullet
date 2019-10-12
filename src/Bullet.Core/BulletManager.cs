using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Net.Http;

namespace Bullet.Core
{
    public class BulletManager
    {
        private readonly Stopwatch _watch = new Stopwatch();
        private readonly int _durationInSeconds = 5;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly int _numberOfConnections;

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

        public double MaxRequestPerSecond => _clients.SelectMany(a => a.Requests).Where(a => a.Success).Max(p => p.TotalSeconds);

        public double LowsetRequestPerSecond => _clients.SelectMany(a => a.Requests).Where(a => a.Success).Min(p => p.TotalSeconds);

        public double AverageRequestPerSecond
        {
            get
            {
                var requests = _clients
                    .SelectMany(a => a.Requests)
                    .Where(p=> p.Success);

                var totalSeconds = requests.Sum(a => a.TotalSeconds);

                return requests.Count() / totalSeconds;

            }
        }

        public BulletClient[] Clientes => _clients.ToArray();

        public BulletManager(string url, int numberConnections = 125, int durationInSeconds = 10)
        {
            _timer.Interval = 1000;
            _timer.Elapsed += OnTimerElapsed;

            _url = url;
            _durationInSeconds = durationInSeconds;
            _numberOfConnections = numberConnections;

            SetupClients();
        }

        public async Task StartGetRequests()
        {
            var tasks = _clients.Select(a => a.GetAsync());

            _startTime = DateTime.Now;
            _timer.Start();
            _watch.Start();

            while (_watch.Elapsed.TotalSeconds < _durationInSeconds)
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            _watch.Stop();
            _watch.Reset();
            _timer.Stop();

            OnCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void Cancell()
        {
            _ctk.Cancel();
            _timer.Stop();
        }

        private void SetupClients()
        {
            for (int i = 0; i < _numberOfConnections; i++)
            {
                var client = new BulletClient(_url, _ctk);

                client.OnSuccess += OnClientSuccessInternal;
                client.OnFailure += OnClientFailedInternal;
                client.OnRequestFinished += OnClientRequestFinished;
                client.OnRequestStarted += OnClientRequestStarted;
                _clients.Add(client);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnSecondElapsed?.Invoke(this, EventArgs.Empty);

            var seconds = (e.SignalTime - _startTime).TotalSeconds;

            if (seconds > _durationInSeconds)
                Cancell();
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
