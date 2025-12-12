namespace spinner;

public interface ITask
{
    Task<TaskResult> Run();
}

public abstract class TaskResultBase
{
    public bool Success = true;
}

public class TaskResult : TaskResultBase
{
    public string Error = "";
}

public class TaskResultSet : TaskResult
{
    public TaskResult[] Results = [];
}

public abstract class BaseTask
{
    protected List<Func<Task<TaskResult>>> _tasks = [];
    public event EventHandler<TaskResultBase>? OnTaskFinished;

    public virtual async Task<TaskResultBase> Run()
    {
        TaskResultBase result = new TaskResult();
        NotifyTaskDone(result);
        await Task.CompletedTask;
        return result;
    }

    public virtual void SetTasks(List<Func<Task<TaskResult>>> tasks)
    {
        _tasks = tasks;
    }

    public void Add(Func<Task<TaskResult>> task)
    {
        _tasks.Add(task);
    }

    protected void NotifyTaskDone(TaskResultBase result)
    {
        OnTaskFinished?.Invoke(this, result);
    }
}

public class TaskSequence : BaseTask
{
    CancellationToken _token = new();

    public override async Task<TaskResultBase> Run()
    {
        TaskResultBase result = await ProcessFrom(0);
        try
        {
            return result;
        }
        finally
        {
            NotifyTaskDone(result);
        }
    }

    private async Task<TaskResult> Process(int idx)
    {
        if (idx >= _tasks.Count || idx < 0)
        {
            return new TaskResult() { Success = false };
        }

        try
        {
            _token = new();
            return await Task.Run(_tasks[idx]);
        }
        catch (Exception ex)
        {
            return new TaskResult() { Success = false, Error = ex.Message };
            // TODO retry policie
        }
    }

    private async Task<TaskResultSet> ProcessFrom(int idx)
    {
        List<TaskResult> res = [];
        res.Add(await Process(idx));
        TaskResultSet r = await ProcessFrom(idx + 1);
        foreach (var item in r.Results)
        {
            res.Add(item);
        }

        return new TaskResultSet() { Results = res.ToArray() };
    }
}

public class TaskBatch : BaseTask
{
    private async Task<TaskResultSet> Process()
    {
        try
        {
            TaskResult[] res = await Task.WhenAll(
                _tasks.Select(async v =>
                {
                    var res = await Task.Run(v);
                    return res;
                })
            );
            return new() { Results = res };
        }
        catch (Exception)
        {
            return new();
            // TODO retry policie
        }
    }

    public override async Task<TaskResultBase> Run()
    {
        TaskResultBase result = await Process();
        try
        {
            return result;
        }
        finally
        {
            NotifyTaskDone(result);
        }
    }
}

public class TaskManager
{
    OwnedSemaphore _taskLock = new(1, 1);
    CancellationTokenSource _tokenSource = new(TimeSpan.FromMinutes(10));
    Queue<BaseTask> _taskList = [];

    public async Task ScheduleTask(BaseTask task)
    {
        int randomId = Random.Shared.Next(1000, 10000);
        await _taskLock.WaitAsync(randomId);
        _taskList.Enqueue(task);
        _taskLock.Release(randomId);
    }

    public void Start()
    {
        Stop();
        _tokenSource = new(TimeSpan.FromMinutes(10));
        _ = Run(_tokenSource.Token);
    }

    public void Stop()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }

    private async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            for (int i = 0; i < 2 && _taskList.Count > 0; i++)
            {
                int randomId = Random.Shared.Next(1000, 10000);
                await _taskLock.WaitAsync(randomId);
                _ = RunTask(_taskList.Dequeue());
                _taskLock.Release(randomId);
            }

            await Task.Delay(5000);
        }
    }

    private async Task RunTask(BaseTask task)
    {
        TaskResultBase res = await task.Run();
    }
}
