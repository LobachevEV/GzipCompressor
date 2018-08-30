using System;
using System.Threading;
using GzipCompressor.Infrastructure.Logging;

namespace GzipCompressor.Infrastructure
{
    enum WorkerState
    {
        Unstarted,
        Starting,
        Running,
        Waiting,
    }
    
    internal class Worker : IDisposable
    {
        private readonly AutoResetEvent internalEvent = new AutoResetEvent(false);
        private Thread thread;
        private Action mainAction;
        private volatile bool stopped;
        private readonly object sync = new object();
        private volatile WorkerState state = WorkerState.Unstarted;

        public WorkerState State
        {
            get => state;
            set => state = value;
        }

        public bool IsWaiting => State == WorkerState.Waiting; 

        public void Dispose()
        {
            Stop();
            ((IDisposable) internalEvent)?.Dispose();
        }

        public void Start(Action action)
        {
            if (State != WorkerState.Waiting && State != WorkerState.Unstarted)
            {
                throw new Exception("Cannot start started thread");
            }
            State = WorkerState.Starting;
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
                while (!stopped)
                {
                    State = WorkerState.Running;
                    lock (sync)
                    {
                        mainAction?.Invoke();
                    }
                    State = WorkerState.Waiting;
                    internalEvent.WaitOne();
                }
            });
        }

        private void Stop()
        {
            stopped = true;
            internalEvent.Set();
        }
    }
}