namespace ExAminoStreams
{
    public class AsyncUtil
    {
        private static readonly TaskFactory _taskFactory = new(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void RunSync(Func<Task> task)
        {
            _taskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> task)
        {
            return _taskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }
    }
}
