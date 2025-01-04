using System.Reflection;
using XUnity.AutoTranslator.LlmTranslators.Config;

namespace XUnity.AutoTranslator.LlmTranslators.Tests
{
    public class ConfigTests
    {
        [Fact]
        public void TestDefaultConfig()
        {
            var config = Configuration.GetConfiguration($"{Assembly.GetExecutingAssembly().Location}/../../../../TestOutput/Ollama2.yaml");

            Assert.True(config.SystemPrompt!.Split("\n").Length > 1);
        }
    }
}
