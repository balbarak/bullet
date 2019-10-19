using Bullet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bullet.Wpf
{
    public class MainViewModel : BaseViewModel
    {
        private int _numberOfConnections = 1;
        private string _url = "http://localhost:5000/";
        private int _duration = 1;
        private BulletManager _manager;
        private double _progress;
        private int _totalRequest;
        private int _totalFailedRequest;
        private double _requestsPerSecond;
        private int _connections;
        private bool _useThreadPerConnection;
        private bool _useOptimazedNumberOfThreads = true;
        private CancellationTokenSource _ctk;
        private TimeSpan _durationSpan;
        private double _elapsed;
        private Stopwatch _stopwatch;
        System.Windows.Threading.DispatcherTimer _timer;
        public double Elapsed
        {
            get { return _elapsed; }
            set { SetProperty(ref _elapsed, value); }
        }

        public bool UseOptimazedNumberOfThreads
        {
            get { return _useOptimazedNumberOfThreads; }
            set { SetProperty(ref _useOptimazedNumberOfThreads, value); UseThreadPerConnection = !value; }
        }
        public bool UseThreadPerConnection
        {
            get { return _useThreadPerConnection; }
            set { SetProperty(ref _useThreadPerConnection, value); }
        }
        public int Connections
        {
            get { return _connections; }
            set { SetProperty(ref _connections, value); }
        }
        public double RequestsPerSecond
        {
            get { return _requestsPerSecond; }
            set { SetProperty(ref _requestsPerSecond, value); }
        }
        public string Url
        {
            get { return _url; }
            set { SetProperty<string>(ref _url, value); }
        }
        public int TotalFailedRequest
        {
            get { return _totalFailedRequest; }
            set { SetProperty(ref _totalFailedRequest, value); }
        }
        public int TotalRequest
        {
            get { return _totalRequest; }
            set { SetProperty(ref _totalRequest, value); }
        }
        public int NumberOfConnections
        {
            get { return _numberOfConnections; }
            set { SetProperty(ref _numberOfConnections, value); }
        }
        public int Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        public ICommand StartCommand => new AsyncCommand(Start);
        public ICommand CancelCommand => new AsyncCommand(Cancel);

        public MainViewModel()
        {
            _durationSpan = new TimeSpan();

            _timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _manager = new BulletManager();

            SetupManagerEvents();
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await UpdateProgress();
        }

        private async Task Start()
        {
            _ctk = new CancellationTokenSource();

            IsBusy = true;

            Reset();


            _durationSpan = TimeSpan.FromSeconds(_duration);

            _stopwatch = Stopwatch.StartNew();

            await _manager.StartGetAsyncSystemDate(Url,_numberOfConnections, _duration, _ctk.Token);

            _stopwatch.Stop();

            //if (UseOptimazedNumberOfThreads)
            //    await _manager.StartGetAsync(_numberOfConnections, _duration);
            //else
            //    await _manager.StartGetAsyncThreads(_numberOfConnections, _duration);


            IsBusy = false;
        }

        private void Reset()
        {
            Progress = 0;
            TotalRequest = 0;
            TotalFailedRequest = 0;
            RequestsPerSecond = 0;
            Connections = 0;
        }

        private Task Cancel()
        {

            IsBusy = false;

            return Task.CompletedTask;
        }

        private void SetupManagerEvents()
        {
            _manager.OnFinished += OnManagerFinished;
        }

        private void OnManagerFinished(object sender, EventArgs e)
        {
            TotalRequest = _manager.TotalRequests;
            RequestsPerSecond = _manager.RequestPerSecond;
            TotalFailedRequest = _manager.TotalFailedRequests;
            Connections = _manager.Connections;
        }

        private Task UpdateProgress()
        {

            if (IsBusy && _manager != null)
            {
                Progress = (_manager.Elapsed.TotalMilliseconds / _durationSpan.TotalMilliseconds) * 100.0;
                TotalRequest = _manager.TotalRequests;
                Elapsed = _manager.Elapsed.TotalSeconds;
            }

            return Task.CompletedTask;
        }
    }
}
