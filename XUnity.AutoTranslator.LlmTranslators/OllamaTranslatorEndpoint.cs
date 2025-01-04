using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using XUnity.AutoTranslator.LlmTranslators.Config;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;

public class OllamaTranslatorEndpoint : HttpEndpoint
{
    public override string Id => "OllamaTranslate";
    public override string FriendlyName => "Ollama Translate";
    public override int MaxTranslationsPerRequest => 1;

    // Careful not to melt machines
    public override int MaxConcurrency => 5;
   
    private LlmConfig _config = new();

    public override void Initialize(IInitializationContext context)
    {
        string folder = Configuration.CalculateConfigFolder();
        var file = Path.Combine(folder, "Ollama.yaml");
        _config = Configuration.GetConfiguration(file);

        // Remove artificial delays
        context.SetTranslationDelay(0.1f);
        context.DisableSpamChecks();

        if (string.IsNullOrEmpty(_config.ApiKey) && _config.ApiKeyRequired)
            throw new Exception("The endpoint requires an API key which has not been provided.");
    } 

    public override void OnCreateRequest(IHttpRequestCreationContext context)
    {
        var requestData = GetRequestData(_config.SystemPrompt, _config.Model, context.UntranslatedTexts);

        var request = new XUnityWebRequest("POST", _config.Url, requestData);        
        request.Headers[HttpRequestHeader.ContentType] = "application/json";

        if (_config.ApiKeyRequired)
            request.Headers[HttpRequestHeader.Authorization] = $"Bearer {_config.ApiKey}";

        context.Complete(request);
    }

    public static string GetRequestData(string? systemPrompt, string? model, string[] texts)
    {
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var text in texts)
        {
            messages.Add(new { role = "user", content = text });
        }

        var requestBody = new
        {
            model,
            temperature = 0.1,
            max_tokens = 1000,
            top_p = 1,
            frequency_penalty = 0,
            presence_penalty = 0,
            stream = false,
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