using System;
using System.Threading;

namespace GzipCompressor.Infrastructure
{
    internal enum WorkerState
    {
        NotStarted,
        Running,
        Waiting
    }

    public class Worker : IDisposable
    {
        private readonly object actionSync = new object();
        private readonly AutoResetEvent internalEvent = new AutoResetEvent(false);
        private readonly object stateSync = new object();
        private Action action;
        private volatile bool disposed;
        private volatile WorkerState state = WorkerState.NotStarted;
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

        public void Start(Action newAction)
        {
            switch (state)
            {
                case WorkerState.NotStarted:
                    StateRunning(newAction);
                    InitThread();
                    break;
                case WorkerState.Running:
                    throw new Exception("Cannot start running worker");
                case WorkerState.Waiting:
                    StateRunning(newAction);
                    internalEvent.Set();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StateWaiting()
        {
            SetState(WorkerState.Waiting);
            OnComplete?.Invoke();
            internalEvent.WaitOne();
        }

        private void StateRunning(Action newAction)
        {
            SetState(WorkerState.Running);
            SetAction(newAction);
        }

        private void SetState(WorkerState newState)
        {
            lock (stateSync)
            {
                state = newState;
            }
        }

        private void SetAction(Action newAction)
        {
            lock (actionSync)
            {
                action = newAction;
            }
        }

        private void InitThread()
        {
            thread = new Thread(() =>
            {
                while (!disposed)
                {
                    lock (actionSync)
                    {
                        action?.Invoke();
                    }

                    StateWaiting();
                }
            });
            thread.Start();
        }
    }
}