using Bullet.Core.Http;
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
        private readonly BulletHttpClient _client;

        public List<BulletHttpResponse> Responses { get; private set; } = new List<BulletHttpResponse>();

        public int Index { get; private set; } = 1;

        public event EventHandler<int> OnRequestBegin;
        public event EventHandler<int> OnRequestEnd;

        public BulletClient(string url, int? index = null)
        {
            _client = new BulletHttpClient(url);

            if (index.HasValue)
                Index = index.Value;
        }

        public async ValueTask GetAsync()
        {
            OnRequestBegin?.Invoke(this, Index);

            var result = await _client.GetAsync();

            if (result != null)
                Responses.Add(result);

            OnRequestEnd?.Invoke(this, Index);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
