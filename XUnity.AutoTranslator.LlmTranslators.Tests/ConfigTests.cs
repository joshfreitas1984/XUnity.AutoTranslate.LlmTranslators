using XUnity.AutoTranslator.LlmTranslators.Config;

namespace XUnity.AutoTranslator.LlmTranslators.Tests;

public class ConfigTests
{
    const string workingDirectory = "../../../";
    const string sampleDirectory = $"{workingDirectory}/../XUnity.AutoTranslator.LlmTranslators/SampleConfig/";

    [Fact]
    public void TestDefaultConfig()
    {
        var config = Configuration.GetConfiguration($"{sampleDirectory}/OpenAi.yaml");

        Assert.True(config.SystemPrompt!.Split("\n").Length > 1);
    }
}
