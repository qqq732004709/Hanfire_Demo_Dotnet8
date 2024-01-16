using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Management;
using Hangfire.MemoryStorage;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(x =>
{
    x.UseMemoryStorage();
    x.UseDashboardMetric(DashboardMetrics.ServerCount)//����������
    .UseConsole()
    .UseManagementPages(Assembly.Load("Hangfire_Demo")); //���س���
});

builder.Services.AddHangfireServer();

var app = builder.Build();

app.MapGet("/", () => "Hello Hangfire!");

//����hangfire dashboard�м��
app.UseHangfireDashboard();

#region ��������
//var jobId = BackgroundJob.Schedule(

//() => Console.WriteLine($"Delayed at {DateTime.Now.ToString("G")}"),

//TimeSpan.FromSeconds(3));



//RecurringJob.AddOrUpdate(

//    "myrecurringjob",

//    () => Console.WriteLine("Recurring!"),

//    "0/2 * * * * ? ");  //Cron



//BackgroundJob.ContinueJobWith(

//    jobId,

//    () => Console.WriteLine($"Continuation at {DateTime.Now.ToString("G")}"));

#endregion

app.Run();
