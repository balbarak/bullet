using System;
using System.Collections.Generic;
using System.Text;

namespace Bullet.Core.Http
{
    public class BulletHttpResponse
    {
        public int StatusCode { get; private set; }
        public int Length { get; private set; }
        public double Latency { get; private set; }

        public BulletHttpResponse(int statusCode, int length, double latency)
        {
            StatusCode = statusCode;
            Length = length;
            Latency = latency;
        }

    }
}
