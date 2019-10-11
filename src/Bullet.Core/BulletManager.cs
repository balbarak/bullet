using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Net.Http;

namespace Bullet.Core
{
    public class BulletManager
    {
        private string _url;
        private List<BulletClient> _clients = new List<BulletClient>();
        private readonly Stopwatch _watch = new Stopwatch();
        private int _successCount = 0;
        private int _faildCount = 0;

        public double TotalSeconds { get; private set; }


        public BulletManager(string url,int numberConnections = 2)
        {
            _url = url;
            
            for (int i = 0; i < numberConnections; i++)
            {
                var client = new BulletClient(_url);
                
                client.OnSuccess += OnClientSuccess;
                client.OnFailure += OnClientFailed;

                _clients.Add(client);
            }
        }

        public async Task Start()
        {
            var tasks =_clients.Select(a => a.GetAsync());

            _watch.Start();

            await Task.WhenAll(tasks);

            _watch.Stop();

            TotalSeconds = TimeSpan.FromMilliseconds(_watch.ElapsedMilliseconds).TotalSeconds;
        }

        private void OnClientFailed(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _faildCount);
        }

        private void OnClientSuccess(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _successCount);
        }
    }
}
