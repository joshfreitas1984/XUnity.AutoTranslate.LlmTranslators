using System.Reflection;
using System.Text;
using System.Text.Json;
using XUnity.AutoTranslator.LlmTranslators.Config;

namespace Tests
{
    public class PromptTests
    {
        [Fact]
        public async Task OllamaPromptsTester()
        {
            var config = Configuration.GetConfiguration($"{Assembly.GetExecutingAssembly().Location}/../../../../TestConfig/Ollama.yaml");
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

            //Translate
            await Parallel.ForAsync(0, raws.Count, async (i, _) =>
            {
                var request = OllamaTranslatorEndpoint.GetRequestData(config.SystemPrompt, config.Model, [raws[i]]);
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
}
