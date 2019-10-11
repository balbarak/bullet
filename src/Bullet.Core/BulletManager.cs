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
        private string _url;
        private List<BulletClient> _clients = new List<BulletClient>();
        private readonly Stopwatch _watch = new Stopwatch();
        private CancellationTokenSource _ctk = new CancellationTokenSource();

        private readonly int _durationInSeconds = 5;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly int _numberOfConnections;
        private DateTime _startTime;
        private int _successCount = 0;
        private int _faildCount = 0;

        public event EventHandler OnSecondElapsed;

        public double TotalSeconds { get; private set; }

        public BulletManager(string url,int numberConnections = 125,int durationInSeconds = 10)
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
            var tasks =_clients.Select(a => a.GetAsync());

            _startTime = DateTime.Now;
            _timer.Start();
            _watch.Start();
            
            while (_watch.Elapsed.TotalSeconds < _durationInSeconds)
            {
                await Task.WhenAll(tasks);
            }

            _watch.Stop();
            _watch.Reset();

            _timer.Stop();
        }

        public void Cancell()
        {
            _ctk.Cancel();
        }

        private void SetupClients()
        {
            for (int i = 0; i < _numberOfConnections; i++)
            {
                var client = new BulletClient(_url, _ctk);

                client.OnSuccess += OnClientSuccess;
                client.OnFailure += OnClientFailed;
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

        private void OnClientFailed(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _faildCount);
        }

        private void OnClientSuccess(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _successCount);
        }

        private void OnClientRequestStarted(object sender, EventArgs e)
        {

        }

        private void OnClientRequestFinished(object sender, EventArgs e)
        {

        }
    }
}
