using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using XUnity.AutoTranslator.LlmTranslators.Config;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;

public class ChatGptTranslatorEndpoint : HttpEndpoint
{
    public override string Id => "ChatGPTTranslate";
    public override string FriendlyName => "ChatGPT Translate";
    public override int MaxTranslationsPerRequest => 1;
    public override int MaxConcurrency => 15;

    private LlmConfig _config = new();

    public override void Initialize(IInitializationContext context)
    {
        string folder = Configuration.CalculateConfigFolder();
        var file = Path.Combine(folder, "ChatGPT.yaml");
        _config = Configuration.GetConfiguration(file);

        // Remove artificial delays
        context.SetTranslationDelay(0.1f);
        context.DisableSpamChecks();

        if (string.IsNullOrEmpty(_config.ApiKey))
            throw new Exception("The endpoint requires an API key which has not been provided.");
    }

    public override void OnCreateRequest(IHttpRequestCreationContext context)
    {
        var requestData = GetRequestData(context);

        var request = new XUnityWebRequest("POST", _config.Url, requestData);
        request.Headers[HttpRequestHeader.Authorization] = $"Bearer {_config.ApiKey}";
        request.Headers[HttpRequestHeader.ContentType] = "application/json";

        context.Complete(request);
    }

    private string GetRequestData(IHttpRequestCreationContext context)
    {
        var messages = new List<object>
        {
            new { role = "system", content = _config.SystemPrompt }
        };

        foreach (var text in context.UntranslatedTexts)
        {
            messages.Add(new { role = "user", content = text });
        }

        var requestBody = new
        {
            model = _config.Model,
            temperature = 0.1,
            max_tokens = 1000,
            top_p = 1,
            frequency_penalty = 0,
            presence_penalty = 0,
            messages
        };

        return JsonConvert.SerializeObject(requestBody);
    }

    public override void OnExtractTranslation(IHttpTranslationExtractionContext context)
    {
        var data = context.Response.Data;

        var jsonResponse = JObject.Parse(data);
        if (MaxTranslationsPerRequest == 1)
            context.Complete(GetTranslatedText(jsonResponse, 0));
    }

    private static string GetTranslatedText(JObject jsonResponse, int index)
    {
        var rawString = jsonResponse["choices"]?[index]?["message"]?["content"]?.ToString() ?? string.Empty;
        return rawString.Trim();
    }
}