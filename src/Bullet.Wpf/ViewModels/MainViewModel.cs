﻿using Bullet.Core;
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
        private int _totalSuccessRequest;
        private int _totalFailedRequest;
        private double _maxRequestPerSecond;
        private double _lowestRequestPerSecond;
        private double _averageRequestPerSecond;
        private CancellationTokenSource _ctk;
        private SemaphoreSlim _lock;

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

        public MainViewModel()
        {
            _lock = new SemaphoreSlim(1);
        }

        private async Task Start()
        {
            _ctk = new CancellationTokenSource();

            IsBusy = true;

            if (_manager != null)
                RemoveManagerEvents();

            Reset();

            AddManagerEvents();

            var duration = TimeSpan.FromSeconds(_duration);

            var progressTask = _manager.StartGetAsync(_numberOfConnections, _duration);
            var updatProgressTask = UpdateProgress(duration);

            await Task.WhenAll(updatProgressTask, progressTask);

            //await updatProgressTask;

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
            _manager = new BulletManager(Url);
        }

        private Task Cancel()
        {

            IsBusy = false;

            return Task.CompletedTask;
        }

        private void RemoveManagerEvents()
        {

        }

        private void AddManagerEvents()
        {
            _manager.OnFinished += OnManagerFinished;
        }

        private void OnManagerFinished(object sender, EventArgs e)
        {
            TotalRequest = _manager.TotalRequests;
        }

        private void OnClientFinished(object sender, int e)
        {
            //TotalRequest++;

        }

        private void OnClientStart(object sender, int e)
        {
            //TotalRequest++;
        }

        private async Task UpdateProgress(TimeSpan duration)
        {

            try
            {
                var sw = Stopwatch.StartNew();

                while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
                {

                    Progress = (sw.Elapsed.TotalMilliseconds / duration.TotalMilliseconds) * 100;
                    TotalRequest = _manager.TotalRequests;
                    await Task.Delay(30);
                }

            }
            catch (Exception ex)
            {

            }

        }
    }
}
