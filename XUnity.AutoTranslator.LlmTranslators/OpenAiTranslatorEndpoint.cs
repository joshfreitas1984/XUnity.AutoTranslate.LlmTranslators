using SimpleJSON;
using System.Net;
using XUnity.AutoTranslator.LlmTranslators.Behavior;
using XUnity.AutoTranslator.LlmTranslators.Config;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;

public class OpenAiTranslatorEndpoint : HttpEndpoint
{
    public override string Id => "OpenAiTranslate";
    public override string FriendlyName => "OpenAi Translate";
    public override int MaxTranslationsPerRequest => 1;
    public override int MaxConcurrency => 15;

    private LlmConfig _config = new();

    public override void Initialize(IInitializationContext context)
    {
        string folder = Configuration.CalculateConfigFolder();
        var file = Path.Combine(folder, "OpenAi.yaml");
        _config = Configuration.GetConfiguration(file);
        Configuration.LoadGlossary(_config, "OpenAi-Glossary.yaml");

        // Remove artificial delays
        context.SetTranslationDelay(0.1f);
        context.DisableSpamChecks();

        if (string.IsNullOrEmpty(_config.ApiKey))
            throw new Exception("The endpoint requires an API key which has not been provided.");
    }

    public override void OnCreateRequest(IHttpRequestCreationContext context)
    {
        var requestData = BaseEndpointBehavior.GetRequestData(_config, context.UntranslatedText);
        //File.WriteAllText($@"C:\Debug\{DateTime.Now.ToString("yyyy-MM-dd-hh-mm")}-{Guid.NewGuid()}-request.txt", requestData);

        var request = new XUnityWebRequest("POST", _config.Url, requestData);
        request.Headers[HttpRequestHeader.Authorization] = $"Bearer {_config.ApiKey}";
        request.Headers[HttpRequestHeader.ContentType] = "application/json";

        context.Complete(request);
    }

    public override void OnExtractTranslation(IHttpTranslationExtractionContext context)
    {
        var data = context.Response.Data;

        var jsonResponse = JSON.Parse(data);
        var result = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
        result = BaseEndpointBehavior.ValidateAndCleanupTranslation(context.UntranslatedText, result, _config);

        if (MaxTranslationsPerRequest == 1)
            context.Complete(result);
    }
}