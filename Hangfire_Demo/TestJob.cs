using Hangfire.Dashboard.Management.Metadata;
using Hangfire.Server;
using Hangfire;
using System.ComponentModel;
using Hangfire.Console;

namespace Hangfire_Demo;

[ManagementPage("侧边菜单栏的标题")]

public class TestJob
{

    [Hangfire.Dashboard.Management.Support.Job] //必须要声明这个属性 

    [DisplayName("任务标题")]

    [Description("任务描述")]

    [AutomaticRetry(Attempts = 0)] //重试次数

    [DisableConcurrentExecution(90)]//禁用并行执行

    public void Test(PerformContext context, 

        [DisplayData("Output Text", "Enter text to output.")] string outputText,

        [DisplayData("Bool类型参数", "Bool展示名称")] bool repeat = false)

    {

        context.WriteLine(repeat + ":" + outputText);

    }

}
