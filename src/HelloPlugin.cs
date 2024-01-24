using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

public class HelloPlugin
{
    [KernelFunction]
    public string SayHello(Kernel kernel, ILogger logger, string userName)
    {
        return $"Hello {userName}!";
    }

    [KernelFunction]
    public string GetUserName(Kernel kernel, ILogger logger)
    {
        return $"Brian";
    }

    [KernelFunction]
    public string GetWeather(Kernel kernel, ILogger logger)
    {
        return $"Sunny";
    }
}