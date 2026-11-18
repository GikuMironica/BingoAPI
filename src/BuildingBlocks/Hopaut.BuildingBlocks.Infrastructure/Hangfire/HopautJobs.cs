using System.Linq.Expressions;
using Hangfire;

namespace Hopaut.BuildingBlocks.Infrastructure.Hangfire;

public sealed class HopautJobs : IHopautJobs
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HopautJobs(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
        => _backgroundJobClient.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
        => _backgroundJobClient.Enqueue(methodCall);

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
        => _backgroundJobClient.Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
        => _backgroundJobClient.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression)
        => _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);
}
