using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace XUnity.AutoTranslator.LlmTranslators.Config
{
    public class LlmConfig
    {
        public string? ApiKey { get; set; }
        public bool ApiKeyRequired { get; set; }
        public string? Url { get; set; }
        public string? Model { get; set; }
        public string? SystemPrompt { get; set; }
    }

    public static class Configuration
    {
        public static string CalculateConfigFolder()
        {
            //AutoTranslator Configuration details are public so we have to do this work around
            //Check for ReiPatcher or BepinEx and handle foreign chars       
            Directory.SetCurrentDirectory($"{Assembly.GetExecutingAssembly().Location}/../../../../");
            string ReiPatcherFolder = Path.GetFullPath(Path.Combine(".", "AutoTranslator"));
            string BepinExFolder = Path.GetFullPath(Path.Combine(".", "BepInEx"));

            string folder;
            if (Directory.Exists(ReiPatcherFolder))
                folder = ReiPatcherFolder;
            else
                folder = BepinExFolder;

            return folder;
        }

        public static LlmConfig GetConfiguration(string file)
        {
            if (!File.Exists(file))
            {
                var defaultConfig = new LlmConfig()
                {
                    Url = "http://localhost:11434/api/chat",
                    Model = "llama3.1",
                    SystemPrompt = "Please Adjust my Prompt\n\nI can be multi line",
                    ApiKeyRequired = false,
                    ApiKey = "Replace Me if needed"
                };

                var serializer = new SerializerBuilder()
                    .EmitDefaults()
                    .Build();

                string yaml = serializer.Serialize(defaultConfig);
                File.WriteAllText(file, yaml);
            }

            var yamlDeserializer = new DeserializerBuilder().Build();
            return yamlDeserializer.Deserialize<LlmConfig>(File.ReadAllText(file, Encoding.UTF8));
        }
    }
}
