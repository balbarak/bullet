using Bullet.Core;
using System;
using Xunit;

namespace Bullet.Test
{
    public class BulletClientTest
    {
        [Fact]
        public async void GetTest()
        {
            var url = "http://localhost:5000/";

            BulletClient client = new BulletClient(1,url);

            

        }
    }
}
