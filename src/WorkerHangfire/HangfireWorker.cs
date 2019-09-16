using System;
using Hangfire;
using Hangfire.Storage;

namespace StandardDot.Worker.Hangfire
{
	public class HangfireWorker
	{
		public void test()
		{
			BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget"));
			// BackgroundJob.Schedule(() => {Console.WriteLine("test")}, )

			// RecurringJob.AddOrUpdate()
			// Hangfire.
			// Hangfire.JobStorage.Current.GetConnection().GetRecurringJobs();
			StorageConnectionExtensions.GetRecurringJobCount(null);
			// Hangfire.JobStorageConnection.Current
			// var stuff = Hangfire.Storage.Curr
			var stuff = JobStorage.Current.GetConnection().GetRecurringJobs();
			foreach (var thing in stuff)
			{
				// thing.Id;
			}
		}

		// enqueue
		// dequeue
		// requeue
		// schedule
		// continuewith

		// addorupdate (cron string)
		// remove
		// manualrun

		// batches

		// https://github.com/marcoCasamento/Hangfire.Redis.StackExchange
	}
}
