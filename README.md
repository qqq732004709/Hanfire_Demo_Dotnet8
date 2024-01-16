# Part 1 .NET Core

## 前言

[.Net 8 已经发布, 接下来我们将基于这个版本，写一个Hangfire的web api应用示例](https://devblogs.microsoft.com/dotnet-ch/%E5%AE%98%E5%AE%A3-net-8%E7%9A%84%E5%8F%91%E5%B8%83/)

## 创建 Asp.Net Core Web 应用

```Shell
dotnet new sln -n HangfireDemo

dotnet new web -n Hangfire.Server

dotnet new console -n Hangfire.Client

dotnet sln add Hangfire.Client Hangfire.Server

```

### 项目结构

### Program.cs 概述

- 代码简洁的原因：

`顶级语句` => （C# 9 ）省略了 Main 方法的主入口

`全局Using` => （C# 10）省略了 Using 命令

`Minimal API` => Minimal API 是.Net 6 中新增的模板，借助 C# 10 的一些特性以最少的代码运行一个 Web 服务。

- 这个文件中做了什么:

```C#
var builder = WebApplication.CreateBuilder(args); 

//初始化一个有默认配置的WebApplicationBuilder实例

//WebApplicationBuilder用来构建web应用程序和服务



builder.Services.AddControllers();  //将服务注册到容器中



var app = builder.Build(); // 创建 WebApplication. 



app.MapGet("/", () => "Hello World!"); // 匹配指定模式的HTTP GET请求。



app.UseRouting(); //配置请求管道的中间件



app.Run(); //运行应用程序

```

用一张图片说明

![](https://img2022.cnblogs.com/blog/2518219/202210/2518219-20221028181447195-1081837784.png)

[WebApplication](https%3A%2F%2Fwww.dotnetdeveloper.cn%2Fdotnet-guide%2Fdotnet-6-webapplication-quickstart)

- 添加了一个 `Host` 类型的字段 `_Host`, 该字段用于直接使用 `host` 实例。
- 创建了一个 `EndpointDataSource` 的列表，用于配置 `endpoint`
- 创建一个 `ApplicationBuilder` 的实例，这个实例主要用于构建 middleware pipeline.

[WebApplicationBuilder](https%3A%2F%2Fwww.dotnetdeveloper.cn%2Fdotnet-guide%2Fdotnet-6-deep-learning-webapplication-builder)

- `IWebHostEnvironment`: 取得环境变量，`ContentRoot` 以及其他的各种类似的变量。
- `IServiceCollection`: 用于注册 `DI` 服务，它的作用和老版的 `ConfigureService` 是一致的。
- `ConfigurationManager`: 配置信息，关于这个类，可以看之前的文章了解更多。
- `ILoggingBuilder`: 用户注册日志提供者，和 `Generic Host` 上的 `ConfigureLogging()` 是一样的功能。
- `WebHost` 和 `Host` 分别代表了类 `ConfigureWebHostBuilder` 和 `ConfigureHostBuilder`,之前的很多扩展方法现在可以直接在这两个属性上使用

对 Asp.Net Core 的项目结构有了一定了解后，我们接下来就开始使用这个项目模板搭建 Hangfire 应用。

# Part 2 Hangfire

一句话概括，Hangfire 是开源的.NET **非同步任务调度框架**，并且还有丰富的扩展支持。

**hangfire 的架构**

[Getting Started — Hangfire Documentation](https://docs.hangfire.io/en/latest/getting-started/index.html)

![](https://img2022.cnblogs.com/blog/2518219/202210/2518219-20221028181502023-956432183.png)

## 快速开始

先来搭建一个最基本的 Hangfire Server

### Hangfire Server

使用 nuget 安装 Hangfire package

```Shell
Install-Package Hangfire

```

存储层方面，Hangfire 支持多种数据库，这里我们使用内存缓存。

```Shell
Install-Package Hangfire.MemoryStorage

```

Program.cs 中注册 hangfire 服务

```C#
var builder = WebApplication.CreateBuilder(args);



//The `Hangfire.AspNetCore` integration package adds an extension method to register all the services, their implementation, as well as logging and a job activator

builder.Services.AddHangfire(x =>
{
    x.UseMemoryStorage();
});


// Add the processing server as IHostedService

builder.Services.AddHangfireServer();



var app = builder.Build();



app.Run();

```

至此，一个基本的 hangfire server 就已经实现。接下来我们再简单实现一个 Hangfire Client。

### Hangfire Client

安装 nuget package

```Shell
Install-Package Hangfire.Core

Install-Package Hangfire.MemoryStorage

```

配置存储层地址

```C#
GlobalConfiguration.Configuration.UseRedisStorage("127.0.0.1:6379", new Hangfire.Redis.RedisStorageOptions

{

    Db = 11,

    Prefix = "Hangfire:"

});

```

便可以愉快地开始创建 Job，让 Hangfire 服务器执行任务：

```C#
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

```

完成创建任务后，我们可以看到控制台中输出了完成任务的情况。

## 配置 UI 界面

可视化管理界面是 Hangfire 不同于其他任务调度框架的亮点之一。

### Hangfire Dashboard

```C#
var builder = WebApplication.CreateBuilder(args);



//Add Hangfire services.

builder.Services.AddHangfire(x => 

{

    x.UseRedisStorage("127.0.0.1:6379",new Hangfire.Redis.RedisStorageOptions

    {

        Db=11,

        Prefix = "Hangfire:"

    });

});



// Add the processing server as IHostedService

builder.Services.AddHangfireServer();



var app = builder.Build();



//启用hangfire dashboard中间件

app.UseHangfireDashboard();


app.Run();

```

再次使用 Client 创建任务，现在我们可以在 Dashboard 中看到任务的执行情况。并且，还可以使用 `.UseDashboardMetric()` 去添加需要的报表。

### 可视化创建任务

除此之外我们还将添加两个扩展 Hangfire.Console、Hangfire.Dashboard.Management

```Shell
Install-Package Hangfire.Console

Install-Package Hangfire.Dashboard.Management

```

`.UseConsole()` ：可以输出 Console.Write 的 log 到 Hangfire 的任务页面，支持文案颜色的修改。还支持任务进度条的展示。

![](https://img2022.cnblogs.com/blog/2518219/202210/2518219-20221028181518943-951742969.png)

![](https://img2022.cnblogs.com/blog/2518219/202210/2518219-20221028181533270-225135702.png)

`.UseManagementPages()` ：可视化创建任务的关键扩展，在默认仪表板中提供一个 Management 页面，可以手动在管理界面创建任务。

![](https://img2022.cnblogs.com/blog/2518219/202210/2518219-20221028181551889-1780106795.png)

先在 server 中开启这两个扩展

```C#
builder.Services.AddHangfire(x =>

{

    var connectionString = builder.Configuration.GetConnectionString("Redis");

    x.UseRedisStorage(connectionString, redisOptions);

    x.UseDashboardMetric(DashboardMetrics.ServerCount)//服务器数量

        .UseConsole()

        .UseManagementPages(Assembly.Load("Hangfire_Demo")); //加载程序集

});

```

想要使用 ManageMentPage 需要实现对应的配置类，如下：

```C#
 [ManagementPage("侧边菜单栏的标题")]

    public class TestJobC

    {

        [Hangfire.Dashboard.Management.Support.Job] //必须要声明这个属性 

        [DisplayName("任务标题")]

        [Description("任务描述")]

        [AutomaticRetry(Attempts = 0)] //重试次数

        [DisableConcurrentExecution(90)]//禁用并行执行

        public void Test(PerformContext context, IJobCancellationToken token,

            [DisplayData("Output Text", "Enter text to output.")] string outputText,

            [DisplayData("Bool类型参数", "Bool展示名称")] bool repeat,

            [DisplayData("Test Date", "Enter date")] DateTime testDate)

        {

            context.WriteLine(testDate + ":" + outputText);

        }

    }

```

使用这个可视化 UI，除了看到任务执行的统计信息外，我们还能在 management 页面去执行任务，这就是 Hangfire 的方便之处。

此外，GitHub 上还提供了许多开源扩展：[Extensions — Hangfire Core](https://www.hangfire.io/extensions.html), 可以按需加入项目中。
