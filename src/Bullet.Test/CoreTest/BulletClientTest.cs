using Bullet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bullet.Test.CoreTest
{
    public class BulletClientTest
    {
        [Fact]
        public void GetAsyncForOneSecondTest()
        {
            var url = "http://localhost:5000/";

            BulletClient client = new BulletClient(url);
            var duration = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            int index = 1;

            while (duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                client.Get();

                index++;
            }
        }

    }
}
