using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bullet.Core
{
    public class BulletClient : IAsyncDisposable
    {
        private readonly HttpClient _client;
        private readonly string _url;
        private readonly Stopwatch _watch = new Stopwatch();

        public event EventHandler OnSuccess;
        public event EventHandler OnFailure;
        public bool IsBusy { get; private set; }
        public double TotalSeconds { get; private set; } = 0;
        public int NumberOfRequests { get; private set; } = 0;

        public BulletClient()
        {
            _client = new HttpClient();
        }

        public BulletClient(string url) : this()
        {
            _url = url;
        }

        public BulletClient(string url,HttpClient client) : this(url)
        {
            _client = client;
        }

        public async Task GetAsync()
        {
            bool success = false;

            try
            {
                IsBusy = true;
                
                _watch.Start();
                
                NumberOfRequests++;

                var response = await _client.GetAsync(_url);

                if (response.IsSuccessStatusCode)
                    success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                _watch.Stop();
                
                TotalSeconds = TimeSpan.FromMilliseconds(_watch.ElapsedMilliseconds).TotalSeconds;

                IsBusy = false;

                _watch.Reset();

                if (success)
                    OnSuccess?.Invoke(this, EventArgs.Empty);
                else
                    OnFailure?.Invoke(this, EventArgs.Empty);
            }
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
