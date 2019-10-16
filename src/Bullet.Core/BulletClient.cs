using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bullet.Core
{
    public class BulletClient : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly int _index;
        private readonly Stopwatch _localStopWatch;
        //private readonly ConcurrentQueue<BulletRequest> _requests;
        //private CancellationTokenSource _ctk;
        private int _numberOfRequests;

        //public BulletRequest[] Requests => _requests.ToArray();

        //public event EventHandler OnSuccess;
        //public event EventHandler OnFailure;
        //public event EventHandler OnRequestStarted;
        //public event EventHandler OnRequestFinished;

        //public bool IsBusy { get; private set; }

        //public double AverageRequestPerSecond
        //{
        //    get
        //    {
        //        if (!_requests.Any())
        //            return 0;

        //        var successRequests = _requests.Where(a => a.Success);

        //        var totalMilliseconds = successRequests
        //            .Sum(a => a.ResponseTime);

        //        var totalSeconds = TimeSpan.FromMilliseconds(totalMilliseconds).TotalSeconds;

        //        return successRequests.Count() / totalSeconds;
        //    }
        //}

        //public int SuccessCount { get; private set; }
        //public int FailedCount { get; private set; }
        //public int CancelledCount { get; private set; }


        public BulletClient(int index,string url) 
        {
            _numberOfRequests = 0;
            _httpClient = new HttpClient();
            _localStopWatch = new Stopwatch();
            //_requests = new ConcurrentQueue<BulletRequest>();

            _url = url;
            _index = index;
        }

        public async ValueTask GetAsyncFast(CancellationToken ctk = default)
        {
            //bool success = false;

            //NumberOfRequests++;

            //OnRequestStarted?.Invoke(this, EventArgs.Empty);
            Interlocked.Increment(ref _numberOfRequests);

            _localStopWatch.Restart();
            
            using (var response = await _httpClient.GetAsync(_url,HttpCompletionOption.ResponseHeadersRead, ctk))
            {
                //var contentStream = await response.Content.ReadAsStreamAsync();
                //var length = contentStream.Length + response.Headers.ToString().Length;
                //var responseTime = (float) _localStopWatch.ElapsedTicks / Stopwatch.Frequency * 1000;

                //if (response.IsSuccessStatusCode)
                //    success = true;

                //_requests.Enqueue(new BulletRequest(responseTime, length, true));
            }

            //FinalizeRequest(success);
        }

        private void FinalizeRequest(bool success)
        {
           

            _localStopWatch.Reset();

            if (success)
            {
                //SuccessCount++;
                //OnSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                //FailedCount++;
                //OnFailure?.Invoke(this, EventArgs.Empty);
            }

            //OnRequestFinished?.Invoke(this, EventArgs.Empty);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
