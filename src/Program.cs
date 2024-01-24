using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var deploymentName = "";
var endpoint = "";
var apiKey = "";

var myHandler = new AzureOpenAIFixFunctionCallingHttpMessageHandler()
{
    InnerHandler = new HttpClientHandler()
};
var myHttpClient = new HttpClient(myHandler);

var builder = Kernel.CreateBuilder()
                    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey, httpClient: myHttpClient);

builder.Plugins.AddFromType<HelloPlugin>();

var kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var chatMessages = new ChatHistory("Do what you can to achieve user's goal");
chatMessages.AddUserMessage("What's my name and the weather condition today?");

var executionSettings = new OpenAIPromptExecutionSettings()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    ResultsPerPrompt = 1,
    Temperature = 0
};

try
{
    var result = await chatCompletionService.GetChatMessageContentAsync(
                chatMessages,
                executionSettings: executionSettings,
                kernel: kernel);

    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

