using Bullet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Bullet.Test.CoreTest
{
    public class BulletManagerTest
    {
        [Fact]
        public async void StartGetAsyncTest()
        {
            var url = "http://localhost:5000/";

            var manager = new BulletManager(url);

            await manager.StartGetAsync(125, 1);

            var totalRequests = manager.TotalRequests;
            var rps = manager.RequestPerSecond;

            var totalRealRequests = manager.Clients.SelectMany(a => a.Responses).Where(a => a.StatusCode == 200).Count();
        }
    }
}
