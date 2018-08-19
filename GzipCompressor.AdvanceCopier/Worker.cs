using System;
using System.Threading;

namespace GzipCompressor.AdvanceCopier
{
    class Worker
    {
        private readonly Thread thread;

        public Worker(Action mainAction, Action callBack)
        {
            thread = new Thread(() =>
            {
                mainAction();
                callBack();
            });
        }

        public Worker(Action mainAction)
        {
            thread = new Thread(() => { mainAction(); });
        }

        public void Start()
        {
            thread.Start();
        }

        public void Join()
        {
            thread.Join();
        }
    }
}