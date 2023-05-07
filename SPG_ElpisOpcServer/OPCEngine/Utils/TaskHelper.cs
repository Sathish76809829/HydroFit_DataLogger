using NDI.SLIKDA.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elpis.Windows.OPC.Server
{
    public class TaskHelper
    {
        public static bool isTaskRunning = true;
        public static async Task<T> RetryAsyncF<T>(Func<T> func, CancellationToken token, int retryCount, int delay)
        {
            int count = retryCount;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();

                }

                try
                {
                    Task.Delay(delay).Wait();
                    var result = await Task.Factory.StartNew(func, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

                }
                catch
                {
                    if (retryCount == 0)
                    {
                        Thread.Sleep(5000);
                        retryCount = count;

                    }


                    retryCount--;
                }
            }
        }

        public static async Task RetryAsync(Action func, CancellationToken token, int retryCount, int delay)
        {
            int count = retryCount;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();

                }

                try
                {
                    Task.Delay(delay).Wait();
                    await Task.Factory.StartNew(func, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                    //Console.WriteLine(func.Method.Name + " " + delay + " " + result);
                }
                catch
                {
                    if (retryCount == 0)
                    {
                        Thread.Sleep(5000);
                        retryCount = count;
                        //Console.WriteLine("Resetting to 3");
                    }

                    //Console.WriteLine(retryCount);
                    retryCount--;
                }
            }
        }

        public static async Task<T> RetryWithParamsAsyncF<T>(Func<T[], T> func, T[] inpValues, int retryCount, int delay)
        {
            int count = retryCount;
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            while (true)
            {

                try
                {
                    Task.Delay(delay).Wait();
                    var result = await Task.Factory.StartNew((a) => func(inpValues), inpValues, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    //Console.WriteLine(func.Method.Name + " " + delay + " " + result);
                }
                catch
                {
                    if (retryCount == 0)
                    {
                        Thread.Sleep(5000);
                        retryCount = count;
                        //Console.WriteLine("Resetting to 3");
                    }

                    // Console.WriteLine(retryCount);
                    retryCount--;
                }
            }
        }

        public static async Task<T> RetryWithParamsAsyncA<T>(Action<List<T>> func, List<T> inpValues, CancellationToken token, int retryCount, int delay)
        {
            int count = retryCount;


            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();

                }

                try
                {
                    System.Diagnostics.Trace.TraceInformation("delay-" + delay);
                    Task.Delay(delay).Wait();
                    await Task.Factory.StartNew((a) => func(inpValues), inpValues, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

                    //Console.WriteLine(func.Method.Name + " " + delay);
                }
                catch (Exception)
                {
                    if (retryCount == 0)
                    {
                        Thread.Sleep(5000);
                        retryCount = count;
                        //Console.WriteLine("Resetting to 3");
                    }

                    //Console.WriteLine(retryCount);
                    retryCount--;

                }
            }
        }

        public static async Task RunPeriodically<T, T2, T3, T4>(Action<List<T>, T2, T3, T4> action, List<T> inpValues, T2 protocolConn, T3 tagCollection, T4 groupCollection, TimeSpan interval, CancellationToken token)
        {
            //int retryCount = 3;
            // bool isRunningTask1 = true;

            while (isTaskRunning)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                try
                {
                    Debug.WriteLine("Tag Subscription Start");
                    action(inpValues, protocolConn, tagCollection, groupCollection);
                    await Task.Delay(interval, token);
                    Debug.WriteLine("Tag Subscription End");
                }
                catch (Exception ex)
                {
                    //isRunningTask = false;
                    //Task.Delay(3000).Wait();
                    throw ex; //3                  
                }
            }
        }

        internal static async Task RunPeriodically1<T1, T2, T3, T4, T5>(Action<List<T1>, T2, T3, T4, object> action, List<T1> slikdaTagsList, T2 connectorConn, T3 deviceBaseObject, T4 tagsCollection, T5 groupsCollection, TimeSpan timeSpan, CancellationToken token)
        {
            while (isTaskRunning)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                try
                {

                    Debug.WriteLine("Tag Subscription Start" + deviceBaseObject.ToString());
                    await Task.Run(() => action(slikdaTagsList, connectorConn, deviceBaseObject, tagsCollection, groupsCollection));
                    await Task.Delay(timeSpan, token);
                    Debug.WriteLine("Tag Subscription End");
                }
                catch (Exception)
                {
                    isTaskRunning = false;
                    //Task.Delay(3000).Wait();
                    //throw ex; //3                  
                }

            }
        }

       
        internal static async Task RunPeriodicallySerial<T1, T2, T3,T4>(Action<T1, T2, T3,T4> action, T1 tagMapped, T2 connectorConn, T3 deviceBaseObject, T4 scanRate, TimeSpan timeSpan, CancellationToken token)
        {
            while (isTaskRunning)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                try
                {

                    Debug.WriteLine("Tag Subscription Start" + deviceBaseObject.ToString());
                    await Task.Run(() => action(tagMapped, connectorConn, deviceBaseObject, scanRate));
                    await Task.Delay(timeSpan, token);
                    Debug.WriteLine("Tag Subscription End");
                }
                catch (Exception)
                {
                    isTaskRunning = false;
                    //Task.Delay(3000).Wait();
                    //throw ex; //3                  
                }

            }
        }
    }
}
