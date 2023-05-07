using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.Service.Abstractions.Services
{
    /// <summary>
    /// Event Producer and consumer
    /// </summary>
    public interface IEventMonitor
    {
        /// <summary>
        /// Notify the value change
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="value">value instance</param>
        void Notify<T>(T value) where T : notnull;
        /// <summary>
        /// Subscribe for value changes
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="onChange">callback when value change</param>
        /// <returns></returns>
        IDisposable Subscribe<T>(Func<T, Task> onChange);
    }

    /// <summary>
    /// Event listener for receiving 
    /// </summary>
    public interface IEventListener
    {
        Task OnReceive(object value);
    }

    /// <summary>
    /// Event monitor for producer and consumer implementation
    /// </summary>
    public class EventMonitor : IEventMonitor, IDisposable
    {
        private readonly struct Entry
        {
            public readonly object Value;
            public readonly Type Type;

            public Entry(object value, Type type)
            {
                Value = value;
                Type = type;
            }
        }

        private readonly ConcurrentDictionary<Type, List<IEventListener>> observers;

        private volatile int state;

        private bool isInitalizing;

        private Task processTask;

        private readonly BlockingCollection<Entry> events;

        private readonly ILogger<EventMonitor> _logger;

        public EventMonitor(ILogger<EventMonitor> logger)
        {
            _logger = logger;
            observers = new ConcurrentDictionary<Type, List<IEventListener>>();
            events = new BlockingCollection<Entry>(32);
        }

        public void Notify<T>(T value)
        {
            if (!events.IsAddingCompleted)
            {
                try
                {
                    events.Add(new Entry(value, typeof(T)));
                    return;
                }
                catch (InvalidOperationException) { }
            }
        }

        void UnSubscribe(Type type, IEventListener observer)
        {
            if (observers.TryGetValue(type, out var values))
            {
                values.Remove(observer);
            }
        }

        public IDisposable Subscribe<T>(Func<T, Task> onChange)
        {
            EnsureInitialzed();
            if (!observers.TryGetValue(typeof(T), out var values))
            {
                values = new List<IEventListener>();
                observers.TryAdd(typeof(T), values);
            }
            var observer = new Observer<T>(onChange);
            values.Add(new Observer<T>(onChange));
            return new UnSubscriber(this, typeof(T), observer);
        }

        void EnsureInitialzed()
        {
            if (state == 0)
            {
                if (isInitalizing)
                    return;
                isInitalizing = true;
                try
                {
                    processTask = Task.Factory.StartNew(StartProcess, state: this, creationOptions: TaskCreationOptions.LongRunning);
                    Interlocked.Exchange(ref state, 1);
                }
                finally
                {
                    isInitalizing = false;
                }
            }
        }

        static void StartProcess(object state)
        {
            ((EventMonitor)state).StartProcess();
        }

        async void StartProcess()
        {
            var e = events.GetConsumingEnumerable().GetEnumerator();
            try
            {
                while (e.MoveNext())
                {
                    var entry = e.Current;
                    if (observers.TryGetValue(entry.Type, out var subscribes))
                    {
                        try
                        {
                            for (int i = 0; i < subscribes.Count; i++)
                            {
                                await subscribes[i].OnReceive(entry.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }
                    }

                }
            }
            finally
            {
                e.Dispose();
            }
        }

        public void Dispose()
        {
            events.CompleteAdding();
            if (processTask != null)
            {
                processTask.Wait(100);
            }
        }

        private class UnSubscriber : IDisposable
        {
            private readonly EventMonitor dispatcher;

            private readonly Type Type;

            private readonly IEventListener Listener;

            private bool unsubscribed;

            public UnSubscriber(EventMonitor dispatcher, Type type, IEventListener listener)
            {
                this.dispatcher = dispatcher;
                Type = type;
                Listener = listener;
            }

            public void Dispose()
            {
                if (!unsubscribed)
                {
                    dispatcher.UnSubscribe(Type, Listener);
                    unsubscribed = true;
                }
            }
        }

        private class Observer<T> : IEventListener
        {
            private readonly Func<T, Task> onNext;

            public Observer(Func<T, Task> onNext)
            {
                this.onNext = onNext;
            }

            public Task OnReceive(object value)
            {
                return onNext((T)value);
            }
        }
    }
}
