using Bullet.Client;
using Bullet.Core;
using System;
using System.Diagnostics;
using Xunit;

namespace Bullet.Test
{
    public class BulletClientTest
    {
        [Fact]
        public async void GetTest()
        {
            var url = "http://localhost:5000/";

            FastBulletClient client = new FastBulletClient();

            var duration = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            int index = 1;

            while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                await client.GetAsync(url);
                index++;
            }

        }
    }
}
