namespace SatelliteRpc.Shared.Tests;

public class MethodInvokerTests
{
    private class TestService
    {
        public void VoidMethod()
        {
        }

        public Task TaskMethod() => Task.CompletedTask;
        public Task<int> TaskTMethod() => Task.FromResult(42);
    }

    [Fact]
    public void Test_Void_Method()
    {
        var type = typeof(TestService);
        var methodInfo = type.GetMethod(nameof(TestService.VoidMethod));
        var invoker = MethodInvoker.CreateInvoker(type, methodInfo!);

        var service = new TestService();
        var result = invoker(service, Array.Empty<object>());

        Assert.Null(result);
    }

    [Fact]
    public void Test_Task_Method()
    {
        var type = typeof(TestService);
        var methodInfo = type.GetMethod(nameof(TestService.TaskMethod));
        var invoker = MethodInvoker.CreateInvoker(type, methodInfo!);

        var service = new TestService();
        var result = invoker(service, Array.Empty<object>());

        Assert.IsAssignableFrom<Task>(result);
        Assert.True(((Task)result).IsCompleted);
    }


    [Fact]
    public async Task Test_Task_TMethod()
    {
        var type = typeof(TestService);
        var methodInfo = type.GetMethod(nameof(TestService.TaskTMethod));
        var invoker = MethodInvoker.CreateInvoker(type, methodInfo!);

        var service = new TestService();
        var result = invoker(service, Array.Empty<object>());

        await Assert.IsType<Task<int>>(result);
        var taskResult = await (Task<int>)result;
        Assert.Equal(42, taskResult);
    }
}