using Bullet.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bullet.Wpf
{
    public class MainViewModel : BaseViewModel
    {
        private int _numberOfConnections = 125;
        private string _url = "http://localhost:5000/";
        private int _duration = 10;
        private BulletManager _manager;
        private double _progress;
        private int _secondElapsed = 1;
        private int _totalRequest;
        private int _totalSuccessRequest;
        private int _totalFailedRequest;
        private double _maxRequestPerSecond;
        private double _lowestRequestPerSecond;
        private double _averageRequestPerSecond;

        public double LowsetRequestPerSecond
        {
            get { return _lowestRequestPerSecond; }
            set { SetProperty(ref _lowestRequestPerSecond, value); }
        }

        public double AverageRequestPerSecond
        {
            get { return _averageRequestPerSecond; }
            set { SetProperty(ref _averageRequestPerSecond, value); }
        }

        public double MaxRequestPerSecond
        {
            get { return _maxRequestPerSecond; }
            set { SetProperty(ref _maxRequestPerSecond, value); }
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
        public int TotalSuccessRequest
        {
            get { return _totalSuccessRequest; }
            set { SetProperty(ref _totalSuccessRequest, value); }
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


        private async Task Start()
        {

            IsBusy = true;

            if (_manager != null)
                RemoveManagerEvents();

            Reset();

            AddManagerEvents();

            await Task.Run(() =>
            {
                return _manager.StartGetRequests();
            });


            IsBusy = false;
        }

        private void Reset()
        {
            Progress = 0;
            TotalRequest = 0;
            TotalSuccessRequest = 0;
            TotalFailedRequest = 0;
            AverageRequestPerSecond = 0;
            MaxRequestPerSecond = 0;
            LowsetRequestPerSecond = 0;
            _secondElapsed = 1;
            _manager = new BulletManager(Url, NumberOfConnections, Duration);
        }

        private Task Cancel()
        {
            _manager?.Cancell();

            IsBusy = false;

            return Task.CompletedTask;
        }

        private void RemoveManagerEvents()
        {
            _manager.OnClientBeginRequest -= OnClientBeginRequest;
            _manager.OnSecondElapsed -= OnManagerTimerElapsed;
        }

        private void AddManagerEvents()
        {
            _manager.OnClientBeginRequest += OnClientBeginRequest;
            _manager.OnSecondElapsed += OnManagerTimerElapsed;
            _manager.OnClientSuccess += OnClientSuccess;
            _manager.OnClientFailed += OnClientFailed;
            _manager.OnCompleted += OnManagerCompleted;
        }

        private void OnClientFailed(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _totalFailedRequest);

            OnPropertyChanged(nameof(TotalFailedRequest));
        }

        private void OnClientSuccess(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _totalSuccessRequest);

            OnPropertyChanged(nameof(TotalSuccessRequest));
        }

        private void OnManagerTimerElapsed(object sender, EventArgs e)
        {
            _secondElapsed++;

            double current = (double)_secondElapsed / (double)Duration;

            Progress = current * 100.0;

        }
        private void OnManagerCompleted(object sender, EventArgs e)
        {
            var manager = sender as BulletManager;

            MaxRequestPerSecond = manager.MaxRequestPerSecond;
            LowsetRequestPerSecond = manager.LowsetRequestPerSecond;
            AverageRequestPerSecond = manager.TotalAverageRequestPerSecond;

            IsBusy = false;
        }

        private void OnClientBeginRequest(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _totalRequest);

            OnPropertyChanged(nameof(TotalRequest));
        }
    }
}
