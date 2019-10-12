using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        public int NumberOfRequests { get; private set; } = 0;
        public int SuccessCount { get; private set; }
        public int FailedCount { get; private set; }
        public int CancelledCount { get; private set; }

        public BulletClient()
        {
            _client = new HttpClient();
        }

        public BulletClient(string url,CancellationTokenSource ctk = default) : this()
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


        public async Task GetAsync()
        {
            bool success = false;
            double totalSeconds = 0;
            var responseLength = 0;

            try
            {
                IsBusy = true;

                NumberOfRequests++;
                
                OnRequestStarted?.Invoke(this, EventArgs.Empty);

                _watch.Start();
                var response = await _client.GetAsync(_url,_ctk.Token).ConfigureAwait(false);
                _watch.Stop();

                var resultBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                responseLength = resultBytes.Length;

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

                totalSeconds = TimeSpan.FromMilliseconds(_watch.ElapsedMilliseconds).TotalSeconds;

                Requests.Add(new BulletRequest(totalSeconds, responseLength, success));

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
