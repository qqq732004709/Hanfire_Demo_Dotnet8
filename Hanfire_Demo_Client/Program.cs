using Hangfire;
using Hangfire.MemoryStorage;

GlobalConfiguration.Configuration.UseMemoryStorage();

BackgroundJob.Enqueue(

    () => Console.WriteLine($"Fire-and-forget at {DateTime.Now.ToString("G")}"));



var jobId = BackgroundJob.Schedule(

   () => Console.WriteLine($"Delayed at {DateTime.Now.ToString("G")}"),

   TimeSpan.FromSeconds(3));



RecurringJob.AddOrUpdate(

    "myrecurringjob",

    () => Console.WriteLine("Recurring!"),

    "0/2 * * * * ? ");  //Cron



BackgroundJob.ContinueJobWith(

    jobId,

    () => Console.WriteLine($"Continuation at {DateTime.Now.ToString("G")}"));