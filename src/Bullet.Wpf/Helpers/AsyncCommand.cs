using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bullet.Wpf
{
    public class AsyncCommand : BulletCommand
    {
        public AsyncCommand(Func<Task> execute) : base(() => execute())
        {

        }

        public AsyncCommand(Func<object, Task> execute) : base(() => execute(null))
        {

        }
    }
}
