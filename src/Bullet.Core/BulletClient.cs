using System;
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
        private readonly HttpClient _client;
        private readonly string _url;
        private readonly Stopwatch _watch = new Stopwatch();
        private CancellationTokenSource _ctk;

        public event EventHandler OnSuccess;
        public event EventHandler OnFailure;
        public event EventHandler OnRequestStarted;
        public event EventHandler OnRequestFinished;

        public List<BulletRequest> Requests { get; private set; } = new List<BulletRequest>();
        public long ContentLength { get; private set; } = 0;
        public bool IsBusy { get; private set; }

        public double AverageRequestPerSecond
        {
            get
            {
                if (!Requests.Any())
                    return 0;

                var successRequests = Requests.Where(a => a.Success);

                var totalMilliseconds = successRequests
                    .Sum(a => a.TotalMilliseconds);

                var totalSeconds = TimeSpan.FromMilliseconds(totalMilliseconds).TotalSeconds;

                return successRequests.Count() / totalSeconds;
            }
        }

        public int NumberOfRequests { get; private set; } = 0;
        public int SuccessCount { get; private set; }
        public int FailedCount { get; private set; }
        public int CancelledCount { get; private set; }

        public BulletClient()
        {
            _client = new HttpClient();
        }

        public BulletClient(string url, CancellationTokenSource ctk = default) : this()
        {
            _url = url;
            _ctk = ctk;
        }

        public BulletClient(string url, HttpClient client, CancellationTokenSource ctk = default)
        {
            _url = url;
            _ctk = ctk;
            _client = client;
        }


        public async Task GetAsyncFast()
        {
            
            bool success = false;
            double totalMilliseconds = 0;
            var responseLength = 0;

            try
            {
                _watch.Start();
                var response = await _client.GetAsync(_url, _ctk.Token).ConfigureAwait(false);
                _watch.Stop();

                totalMilliseconds = _watch.Elapsed.TotalMilliseconds;

            }
            catch (OperationCanceledException)
            {
                success = true;

                CancelledCount++;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                _watch.Stop();

                Requests.Add(new BulletRequest(totalMilliseconds, responseLength, success));

                //FinalizeRequest(success);

                //OnRequestFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task GetAsync()
        {
            bool success = false;
            double totalMilliseconds = 0;
            var responseLength = 0;

            try
            {
                IsBusy = true;

                NumberOfRequests++;

                OnRequestStarted?.Invoke(this, EventArgs.Empty);

                _watch.Start();
                var response = await _client.GetAsync(_url, _ctk.Token).ConfigureAwait(false);
                _watch.Stop();

                totalMilliseconds = _watch.Elapsed.TotalMilliseconds;

                //var resultBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                //responseLength = resultBytes.Length;

                if (response.IsSuccessStatusCode)
                    success = true;

            }
            catch (OperationCanceledException)
            {
                success = true;

                CancelledCount++;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                _watch.Stop();


                Requests.Add(new BulletRequest(totalMilliseconds, responseLength, success));

                FinalizeRequest(success);

                OnRequestFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        private void FinalizeRequest(bool success)
        {
            IsBusy = false;

            _watch.Reset();

            if (success)
            {
                SuccessCount++;
                OnSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                FailedCount++;
                OnFailure?.Invoke(this, EventArgs.Empty);
            }
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
