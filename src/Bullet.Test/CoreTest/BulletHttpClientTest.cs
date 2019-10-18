using Bullet.Core;
using Bullet.Core.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            int index = 1;

            List<BulletHttpResponse> results = new List<BulletHttpResponse>();
            var tasks = new List<Task>();
            var processCount = Environment.ProcessorCount;

            var sw = Stopwatch.StartNew();

            //for (int i = 0; i < processCount; i++)
            //{
            //    ThreadPool.QueueUserWorkItem<int>((item) => tasks.Add(client.GetAsync()), 1, false);
            //    index++;
            //}

            Parallel.For(0, processCount, async (index) =>
            {
                while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
                {
                   tasks.Add(client.GetAsync());
                    Interlocked.Increment(ref index);
                };
            });
        }


        [Fact]
        public void GetForOneSecondTest()
        {
            var url = "http://localhost:5000/";

            BulletHttpClient client = new BulletHttpClient(url);

            var duration = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            int index = 1;

            List<BulletHttpResponse> results = new List<BulletHttpResponse>();

            while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                var result = client.Get();

                results.Add(result);

                index++;
            }


        }
    }
}
