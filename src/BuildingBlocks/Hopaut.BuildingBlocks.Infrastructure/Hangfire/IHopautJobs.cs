using System.Linq.Expressions;

namespace Hopaut.BuildingBlocks.Infrastructure.Hangfire;

/// <summary>
/// Thin wrapper around Hangfire job scheduling.
/// Modules use this interface to enqueue/schedule jobs without depending on Hangfire directly.
/// </summary>
public interface IHopautJobs
{
    /// <summary>Enqueue a fire-and-forget job.</summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>Enqueue a fire-and-forget job with a typed service.</summary>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>Schedule a delayed job.</summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>Schedule a delayed job with a typed service.</summary>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>Add or update a recurring job.</summary>
    void AddOrUpdateRecurring(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression);
}
