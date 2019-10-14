﻿using Bullet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Bullet.Test
{
    public class BulletHttpClientTest
    {
        [Fact]
        public async void GetAsyncTest()
        {
            var url = "http://localhost:5000/";

            BulletHttpClient client = new BulletHttpClient(url);

            await client.GetAsync(url);
        }

        [Fact]
        public async void GetAsyncForOneSecondTest()
        {
            var url = "http://localhost:5000/";

            BulletHttpClient client = new BulletHttpClient(url);

            var duration = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            int index = 1;

            while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                await client.GetAsync(url);
                index++;
            }

            await client.GetData();
        }
    }
}