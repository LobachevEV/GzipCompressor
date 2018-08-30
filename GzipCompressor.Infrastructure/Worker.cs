using System;
using System.Threading;

namespace GzipCompressor.Infrastructure
{
    internal enum WorkerState
    {
        Unstarted,
        Starting,
        Running,
        Waiting
    }

    internal class Worker : IDisposable
    {
        private readonly AutoResetEvent internalEvent = new AutoResetEvent(false);
        private readonly object sync = new object();
        private Action mainAction;
        private volatile WorkerState state = WorkerState.Unstarted;
        private volatile bool disposed;
        private Thread thread;

        public int ManagedId { get; set; }

        public void Dispose()
        {
            disposed = true;
            internalEvent.Set();
            thread.Join();
            ((IDisposable) internalEvent)?.Dispose();
        }

        public event Action OnComplete;

        public void Start(Action action)
        {
            if (state != WorkerState.Waiting && state != WorkerState.Unstarted)
                throw new Exception("Cannot start started thread");

            state = WorkerState.Starting;
            lock (sync)
            {
                mainAction = action;
            }

            if (thread == null)
            {
                thread = CreateThread();
                thread.Start();
            }
            else
            {
                internalEvent.Set();
            }
        }

        private Thread CreateThread()
        {
            return new Thread(() =>
            {
                while (!disposed)
                {
                    state = WorkerState.Running;
                    lock (sync)
                    {
                        mainAction?.Invoke();
                    }
                    state = WorkerState.Waiting;
                    OnComplete?.Invoke();
                    internalEvent.WaitOne();
                }
            });
        }
    }
}