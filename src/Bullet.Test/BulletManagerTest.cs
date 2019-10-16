using Bullet.Client;
using Bullet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bullet.Test
{
    public class BulletManagerTest
    {
        [Fact]
        public async void StartTest()
        {
            var url = "http://localhost:5000/";

            var manager = new BulletManager(url);

            var duration = TimeSpan.FromSeconds(1);

            await manager.StartDuration(duration,1);

            var x = 1;
        }

        [Fact]
        public async void LoopCount()
        {
            int index = 1;

            var duration = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();
            //var client = new FastBulletClient("http://localhost:5000/");

            //while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            //{
            //    await client.GetAsync();
            //    index++;
            //}

        }
    }
}
