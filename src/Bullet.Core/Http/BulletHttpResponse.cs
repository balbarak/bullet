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

        public double Duration { get; set; }

        public BulletHttpResponse(int statusCode, int length, double latency,double duration)
        {
            StatusCode = statusCode;
            Length = length;
            Latency = latency;
            Duration = duration;
        }

    }
}
