using System.Text;
using Newtonsoft.Json.Linq;
/// <summary>
/// Disclaimer: 
/// - crappy code ahead 
/// - if the LLM returns an error, the code will crash
/// - there is no model version checking
/// - there is no support for multiple choices in the response
/// </summary>
internal class AzureOpenAIFixFunctionCallingHttpMessageHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await FixRequest(request);

        var response = await base.SendAsync(request, cancellationToken);

        await FixResponse(response);

        return response;
    }

    private async Task FixRequest(HttpRequestMessage request)
    {
        var requestAsString = await request.Content.ReadAsStringAsync();
        requestAsString = requestAsString.Replace("\"tools\"", "\"functions\"");
        requestAsString = requestAsString.Replace("\"function\":{", "");
        requestAsString = requestAsString.Replace("},\"type\":\"function\"", "");
        requestAsString = requestAsString.Replace(",\"tool_choice\":\"auto\"", "");

        var jsonObject = JObject.Parse(requestAsString);
        var messages = (JArray)jsonObject["messages"]!;
        for (var i = 0; i < messages.Count; i++)
        {
            var message = messages[i];

            if (message["tool_calls"] != null)
            {
                var firstToolCall = (JObject)message["tool_calls"]![0]!;

                var func = new JObject()
                    {
                        { "name", firstToolCall["name"]  },
                        { "arguments", firstToolCall["arguments"] }
                    };

                firstToolCall["function"] = func;

                messages[i] = new JObject()
                    {
                        {"content", null },
                        {"role", "assistant" },
                        {"function_call", func }
                    };
            }

            if ((string)message["role"] == "tool")
            {
                var newFunctionMessage = new JObject
                    {
                        { "role", "function" },
                        { "content", message["content"] },
                        { "name", (string)messages[i - 1]["function_call"]["name"] }
                    };

                messages[i] = newFunctionMessage;
            }
        }

        requestAsString = jsonObject.ToString();
        request.Content = new StringContent(requestAsString, Encoding.UTF8, "application/json");
    }

    private async Task FixResponse(HttpResponseMessage response)
    {
        var responseAsString = await response.Content.ReadAsStringAsync();
        var jsonObject = JObject.Parse(responseAsString);

        var choicesArray = (JArray)jsonObject["choices"];

        if (choicesArray != null && choicesArray.Count > 0)
        {
            var firstChoice = (JObject)choicesArray[0];

            if ((string)firstChoice["finish_reason"] == "function_call")
            {
                var functionObject = (JObject)firstChoice.SelectToken("message.function_call");

                var callId = "call_" + DateTime.UtcNow.Ticks;

                var newToolCall = new JObject
                        {
                            { "id", callId },
                            { "type", "function" },
                            { "function", functionObject }
                        };

                firstChoice["finish_reason"] = "tool_calls";
                firstChoice["message"]["tool_calls"] = new JArray(newToolCall);

                string modifiedJson = jsonObject.ToString();
                response.Content = new StringContent(modifiedJson, Encoding.UTF8, "application/json");
            }
        }
    }
}