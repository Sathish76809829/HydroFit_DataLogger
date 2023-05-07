using System;
using System.Threading;
using System.Threading.Tasks;

namespace RMS.SocketServer.Net
{
    /// <summary>
    /// Task helper for running background task
    /// </summary>
    public static class TaskFx
    {
        public static Task Start(Action action, CancellationToken cancellationToken)
        {
            var task = new Task(action, cancellationToken, TaskCreationOptions.LongRunning);
            task.ConfigureAwait(false);
            task.Start();
            return task;
        }
    }
}
