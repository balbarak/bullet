using Bullet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Bullet.Test
{
    public class BulletManagerTest
    {
        [Fact]
        public async void StartTest()
        {
            var url = "http://localhost:5000/";

            var manager = new BulletManager(url, 200);

            await manager.Start();

            var seconds = manager.TotalSeconds;

        }
    }
}
