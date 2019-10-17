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

        public BulletClient(string url)
        {
            _client = new BulletHttpClient(url);
        }

        public async ValueTask GetAsync()
        {
            var result = await _client.GetAsync();

            Responses.Add(result);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
