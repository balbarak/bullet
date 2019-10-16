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
        private readonly HttpClient _httpClient;

        public FastBulletClient()
        {
            _httpClient = new HttpClient();
            
        }

        public async ValueTask GetAsync(string url)
        {
            using (var response = await _httpClient.GetAsync(url,HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                //var contentStream = await response.Content.ReadAsStreamAsync();
            }
        }
    }
}
