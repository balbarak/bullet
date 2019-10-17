using Bullet.Core;
using Bullet.Core.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Bullet.Test.CoreTest
{
    public class BulletHttpClientTest
    {
        [Fact]
        public async void GetAsyncTest()
        {
            var url = "http://btx-web/";

            BulletHttpClient client = new BulletHttpClient(url);

            var data = await client.GetAsync();

        }

        [Fact]
        public async void GetAsyncForOneSecondTest()
        {
            var url = "http://localhost:5000/";

            BulletHttpClient client = new BulletHttpClient(url);

            var duration = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            int index = 1;

            List<BulletHttpResponse> results = new List<BulletHttpResponse>();

            while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                var data = await client.GetAsync();
                index++;

                if (data != null)
                    results.Add(data);
            }


        }
    }
}
