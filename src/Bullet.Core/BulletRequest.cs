using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Core
{
    public class BulletRequest
    {
        public double ResponseTime { get; private set; }

        public bool Success { get; private set; }

        public long ResponseBytes { get; private set; }

        public BulletRequest(double responseTime,long length,bool success)
        {
            ResponseTime = responseTime;
            ResponseBytes = length;
            Success = success;
        }
    }
}
