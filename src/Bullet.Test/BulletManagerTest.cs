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

            var manager = new BulletManager(url, 125,10);

            await manager.StartGetRequests();

           // await Task.Delay(1000);

            //manager.Cancell();

            var seconds = manager.TotalSeconds;

        }
    }
}
