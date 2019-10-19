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
        private System.Timers.Timer _timer;

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
            _timer = new System.Timers.Timer
            {
                Interval = 500
            };
            _timer.Elapsed += OnTimer;
            _timer.Start();
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {

            UpdateProgress();

        }

        private async Task Start()
        {
            _ctk = new CancellationTokenSource();

            IsBusy = true;

            if (_manager != null)
                RemoveManagerEvents();

            Reset();

            AddManagerEvents();

            _durationSpan = TimeSpan.FromSeconds(_duration);

            if (UseOptimazedNumberOfThreads)
                await _manager.StartGetAsync(_numberOfConnections, _duration);
            else
                await _manager.StartGetAsyncThreads(_numberOfConnections, _duration);


            IsBusy = false;
        }

        private void Reset()
        {
            Progress = 0;
            TotalRequest = 0;
            TotalFailedRequest = 0;
            RequestsPerSecond = 0;
            _manager = new BulletManager(Url);
        }

        private Task Cancel()
        {

            IsBusy = false;

            return Task.CompletedTask;
        }

        private void RemoveManagerEvents()
        {
            _manager.OnFinished -= OnManagerFinished;
        }

        private void AddManagerEvents()
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

        private void UpdateProgress()
        {
            if (_manager != null && _manager.IsBusy)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Progress = (_manager.Elapsed.TotalMilliseconds / _durationSpan.TotalMilliseconds ) * 100.0;
                    TotalRequest = _manager.TotalRequests;
                },System.Windows.Threading.DispatcherPriority.Render);
                
            }
        }
    }
}
