using SimpleJSON;
using System.Net;
using XUnity.AutoTranslator.LlmTranslators.Behavior;
using XUnity.AutoTranslator.LlmTranslators.Config;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;

public class LmStudioTranslatorEndpoint : HttpEndpoint
{
    public override string Id => "LmStudioTranslate";
    public override string FriendlyName => "LM Studio Translate";
    public override int MaxTranslationsPerRequest => 1;

    // Local models can saturate quickly; keep this closer to Ollama's default.
    public override int MaxConcurrency => 5;

    private LlmConfig _config = new();

    public override void Initialize(IInitializationContext context)
    {
        string folder = Configuration.CalculateConfigFolder(context.TranslatorDirectory);
        var file = Path.Combine(folder, "LmStudio.yaml");
        _config = Configuration.GetConfiguration(file);

        // Remove artificial delays
        context.SetTranslationDelay(0.1f);
        context.DisableSpamChecks();

        if (string.IsNullOrEmpty(_config.ApiKey) && _config.ApiKeyRequired)
            throw new Exception("The endpoint requires an API key which has not been provided.");
    }

    public override void OnCreateRequest(IHttpRequestCreationContext context)
    {
        var requestData = BaseEndpointBehavior.GetRequestData(_config, context.UntranslatedText);

        var request = new XUnityWebRequest("POST", _config.Url, requestData);
        request.Headers[HttpRequestHeader.ContentType] = "application/json";

        if (_config.ApiKeyRequired)
            request.Headers[HttpRequestHeader.Authorization] = $"Bearer {_config.ApiKey}";

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
