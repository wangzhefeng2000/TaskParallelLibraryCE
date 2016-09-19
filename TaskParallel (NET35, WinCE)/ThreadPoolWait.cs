﻿using System.Collections.Generic;

namespace System.Threading
{
#if NETFX_CE
    /// <summary>
    /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
    /// </summary>
    /// <param name="state">
    /// An object containing information to be used by the callback method each time it executes.
    /// </param>
    /// <param name="timedOut">
    /// true if the System.Threading.WaitHandle timed out; false if it was signaled.
    /// </param>
    public delegate void WaitOrTimerCallback(object state, bool timedOut);
#endif

    /// <summary>
    /// Provides a thread that can be used to wait on behalf of other threads, and process timers.
    /// </summary>
    public static class ThreadPoolWait
    {
        static readonly Thread _thread = new Thread(WaitsHandler);
        static readonly List<WaitEntry> _registeredWaits = new List<WaitEntry>();
        static readonly ManualResetEvent _addEvent = new ManualResetEvent(true);

        private static void WaitsHandler(object stateObject)
        {
            if (!(stateObject is WaitEntry))
                throw new InvalidOperationException("Invalid WaitHandle to register waiting");

            WaitEntry firstHandle = (WaitEntry)stateObject;
            _registeredWaits.Add(firstHandle);
            var expiredList = new List<int>();

            while (true)
            {
                Monitor.Enter(_registeredWaits);
                var now = DateTime.Now;
                for (int i = 0; i < _registeredWaits.Count; i++)
                {
                    var currentWait = _registeredWaits[i];
                    var signalReceived = currentWait.WaitObject.WaitOne(0, false);
                    var timedOut = now >= currentWait.ExpireAt;

                    if (signalReceived)
                        timedOut = false;

                    if (signalReceived || timedOut)
                    {
                        ThreadPool.QueueUserWorkItem(WaitHandlerCallback,
                            new WaitCallbackArgs(currentWait.Callback, currentWait.State, timedOut));

                        if (currentWait.ExecuteOnlyOnce)
                            expiredList.Add(i);
                        else
                            currentWait.Reset();
                    }
                }

                for (int i = expiredList.Count - 1; i >= 0; i--)
                    _registeredWaits.RemoveAt(expiredList[i]);

                if (_registeredWaits.Count == 0)
                    _addEvent.Reset();

                expiredList.Clear();
                Monitor.Exit(_registeredWaits);

                if (!_addEvent.WaitOne(0, false))
                    _addEvent.WaitOne();
                else
                    Thread.Sleep(1);
            }
        }

        private static void WaitHandlerCallback(object stateObject)
        {
            if (!(stateObject is WaitCallbackArgs))
                throw new InvalidOperationException("Invalid stateObject, should be WaitCallbackArgs");

            WaitCallbackArgs waitEntry = (WaitCallbackArgs)stateObject;
            waitEntry.Callback(waitEntry.State, waitEntry.TimedOut);
        }

        /// <summary>
        /// Registers a delegate to wait for a WaitHandle, specifying a 32-bit
        /// signed integer for the time-out in milliseconds.
        /// </summary>
        /// <param name="waitObject">The <see cref="WaitHandle"/> to register. Use a <see cref="WaitHandle"/> other than <see cref="Mutex"/>.</param>
        /// <param name="callBack">The <see cref="WaitOrTimerCallback"/> delegate to call when the <paramref name="waitObject"/> parameter is signaled.</param>
        /// <param name="state">The object that is passed to the delegate.</param>
        /// <param name="millisecondsTimeOutInterval">
        /// The time-out in milliseconds. If the <paramref name="millisecondsTimeOutInterval"/> parameter is 0 (zero), the function tests the object's state
        /// and returns immediately. If <paramref name="millisecondsTimeOutInterval"/> is -1, the function's time-out interval never elapses.
        /// </param>
        /// <param name="executeOnlyOnce">
        /// true to indicate that the thread will no longer wait on the <paramref name="waitObject"/> parameter after the delegate has been called;
        /// false to indicate that the timer is reset every time the wait operation completes until the wait is unregistered.
        /// </param>
        public static void RegisterWaitForSingleObject(
            WaitHandle waitObject, WaitOrTimerCallback callBack, object state,
            int millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            var entry = new WaitEntry(waitObject, callBack, state,
                millisecondsTimeOutInterval, executeOnlyOnce);

            if (_thread.ThreadState == ThreadState.Unstarted)
            {
                _thread.IsBackground = true;
                _thread.Priority = ThreadPriority.BelowNormal;
                _thread.Start(entry);
                return;
            }

            Monitor.Enter(_registeredWaits);
            _registeredWaits.Add(entry);
            Monitor.Exit(_registeredWaits);
            _addEvent.Set();
        }

        private class WaitEntry
        {
            public WaitHandle WaitObject { get; set; }
            public WaitOrTimerCallback Callback { get; set; }
            public object State { get; set; }
            public int Timeout { get; set; }
            public bool ExecuteOnlyOnce { get; set; }
            public DateTime ExpireAt { get; set; }

            public WaitEntry(
                WaitHandle waitObject, WaitOrTimerCallback callback, object state,
                int timeout, bool executeOnlyOnce)
            {
                WaitObject = waitObject;
                Callback = callback;
                State = state;
                Timeout = timeout;
                ExecuteOnlyOnce = executeOnlyOnce;
                ExpireAt = new DateTime();

                Reset();
            }

            public void Reset()
            {
                switch (Timeout)
                {
                    case -1:
                        ExpireAt = DateTime.Now.AddYears(1000);
                        break;
                    case 0:
                        ExpireAt = DateTime.Now;
                        break;
                    default:
                        ExpireAt = DateTime.Now.AddMilliseconds(Timeout);
                        break;
                }
            }
        }

        private struct WaitCallbackArgs
        {
            public WaitOrTimerCallback Callback { get; set; }
            public object State { get; set; }
            public bool TimedOut { get; set; }

            public WaitCallbackArgs(WaitOrTimerCallback callback, object state, bool timedOut)
            {
                Callback = callback;
                State = state;
                TimedOut = timedOut;
            }
        }
    }
}
