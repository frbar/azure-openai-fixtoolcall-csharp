Disclaimer: not for production use, only for demonstration purposes. Code is crap and not reliable. See it as a workaround to keep doing your Semantic Kernel experiments when Azure OpenAI is the only LLM you can get.

Relates to: https://github.com/microsoft/semantic-kernel/issues/4694

Please monitor this issue to know when situation is resolved or to get official solution: https://github.com/microsoft/semantic-kernel/issues/4556

Without the "fix" (Semantic Kernel 1.0.1, with 1.1.0 no error but no content):

```
Invalid value for 'content': expected a string, got null.
Status: 400 (model_error)

Content:
{
  "error": {
    "message": "Invalid value for 'content': expected a string, got null.",
    "type": "invalid_request_error",
    "param": "messages.[2].content",
    "code": null
  }
}
```

With the "fix", 2 function calls are done succesfully:

```
Your name is Brian and the weather condition today is sunny.
```