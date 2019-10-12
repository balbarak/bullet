﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Core
{
    public class BulletRequest
    {
        public double TotalSeconds { get; private set; }

        public bool Success { get; private set; }

        public int ResponseContentLength { get; private set; }

        public BulletRequest(double totalSeconds,int length,bool success)
        {
            TotalSeconds = totalSeconds;
            ResponseContentLength = length;
            Success = success;
        }
    }
}