using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bullet.Client
{
    public class FastBulletClient
    {
        private readonly int _index;
        private readonly string _url;
        private readonly Stopwatch _stopwatch;
        private readonly Stopwatch _localStopwatch;
        private readonly HttpClient _httpClient;

        public FastBulletClient(string url)
        {
            _httpClient = new HttpClient();
            _url = url;
        }

        public async ValueTask GetAsync()
        {
            using (var response = await _httpClient.GetAsync(_url).ConfigureAwait(false))
            {
                var contentStream = await response.Content.ReadAsStreamAsync();
            }
        }
    }
}
