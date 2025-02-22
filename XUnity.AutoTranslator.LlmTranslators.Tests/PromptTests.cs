using SimpleJSON;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using XUnity.AutoTranslator.LlmTranslators.Behavior;
using XUnity.AutoTranslator.LlmTranslators.Config;

namespace XUnity.AutoTranslator.LlmTranslators.Tests;

public class PromptTests
{
    const string workingDirectory = "../../../";
    const string sampleDirectory = $"{workingDirectory}/../XUnity.AutoTranslator.LlmTranslators/SampleConfig/";

    [Fact]
    public async Task OpenAiPayloadTest()
    {
        var config = Configuration.GetConfiguration($"{sampleDirectory}/OpenAi.yaml");
        var payload = File.ReadAllText($"{workingDirectory}/TestOutput/OpenAiPayload1.json");

        var client = new HttpClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", System.Environment.GetEnvironmentVariable("AutoTranslator_API_Key"));
        var response = await client.PostAsync(config.Url, content);

        var responseContent = await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task OpenAiPayloadGenTest()
    {
        //var raw = "你就著殘燭火光，讀了講經閣借來的《唐詩選輯》";
        var raw = "我曉得。\r\n雖說妳是門中年紀最小的師妹，但妳天資聰穎，練功又勤，\r\n掌門獨創的「天地無聲勢」也唯獨傳妳一人，說不定妳的功夫已比我還深了。";
        var config = Configuration.GetConfiguration($"{sampleDirectory}/OpenAi.yaml");
        var payload = BaseEndpointBehavior.GetRequestData(config, raw);

        var client = new HttpClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", System.Environment.GetEnvironmentVariable("AutoTranslator_API_Key"));
        var response = await client.PostAsync(config.Url, content);

        var responseContent = await response.Content.ReadAsStringAsync();
        File.WriteAllText($"{workingDirectory}/TestOutput/OpenAiPayloadGenTest.json", responseContent);

        // Check Serialisation parsing
        var jsonResponse = JSON.Parse(responseContent);
        var result = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
        File.WriteAllText($"{workingDirectory}/TestOutput/OpenAiPayloadGenTest.txt", result);
    }

    [Fact]
    public async Task OllamaPromptsTester()
    {
        var config = Configuration.GetConfiguration($"{sampleDirectory}/Ollama.yaml");
        const string outputFile = "../../../TestOutput/Outputs.txt";
        const string inputFile = "../../../Lines.txt";

        var lines = File.ReadAllLines(inputFile);
        var raws = new List<string>();
        var gpt4oTrans = new List<string>();

        //Split raw and expected
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith('='))
                gpt4oTrans.Add(lines[i]);
            else
                raws.Add(lines[i]);
        }

        //Setup
        var client = new HttpClient();
        var outputs = new string[raws.Count];

        if (config.ApiKeyRequired)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

        //Translate
        await Parallel.ForAsync(0, raws.Count, async (i, _) =>
        {
            var request = BaseEndpointBehavior.GetRequestData(config, raws[i]);
            var content = new StringContent(request, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(config.Url, content, _);

            var responseContent = await response.Content.ReadAsStringAsync(_);

            if (response.IsSuccessStatusCode)
            {
                using var jsonDoc = JsonDocument.Parse(responseContent);

                string translation = jsonDoc!
                    .RootElement!
                    .GetProperty("message")!
                    .GetProperty("content")!
                    .GetString()!;

                outputs[i] = translation;
            }
            else
                outputs[i] = responseContent;
        });

        if (File.Exists(outputFile))
            File.Delete(outputFile);

        for (var i = 0; i < raws.Count; i++)
            File.AppendAllText(outputFile, $"Raw:\n{raws[i]}\nTranslated:\n{outputs[i]}\nExpected:\n{gpt4oTrans[i]}\n\n");
    }
}
