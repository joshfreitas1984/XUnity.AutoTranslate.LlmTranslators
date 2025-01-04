using System.Reflection;
using System.Text;
using System.Text.Json;
using XUnity.AutoTranslator.LlmTranslators.Config;
using XUnity.Common.Extensions;

namespace Tests
{
    public class PromptTests
    {
        [Fact]
        public async Task PromptTest()
        {
            var model = "vanilj/Phi-4";
            const string outputFile = "../../../Outputs.txt";
            const string inputFile = "../../../Lines.txt";
            string promptFile = $"../../../{model.Replace("/", "")}.txt";

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

            //Read Prompt
            var prompt = File.ReadAllText(promptFile);

            //Setup
            var client = new HttpClient();
            var outputs = new string[raws.Count];

            //Translate
            await Parallel.ForAsync(0, raws.Count, async (i, _) =>
            {
                var request = OllamaTranslatorEndpoint.GetRequestData(prompt, model, [raws[i]]);
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:11434/api/chat", content, _);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(_);
                    //outputs[i] = responseContent;

                    using var jsonDoc = JsonDocument.Parse(responseContent);

                    string translation = jsonDoc!
                        .RootElement!
                        .GetProperty("message")!
                        .GetProperty("content")!
                        .GetString()!;

                    outputs[i] = translation;
                }
            });

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            for (var i = 0; i < raws.Count; i++)
                File.AppendAllText(outputFile, $"Raw:\n{raws[i]}\nTranslated:\n{outputs[i]}\nExpected:\n{gpt4oTrans[i]}\n\n");
        }

        [Fact]
        public void TestDefaultConfig()
        {           
            var config = Configuration.GetConfiguration($"{Assembly.GetExecutingAssembly().Location}/../../../../Ollama2.yaml");

            Assert.True(config.SystemPrompt!.Split("\n").Length > 1);
        }
    }
}
