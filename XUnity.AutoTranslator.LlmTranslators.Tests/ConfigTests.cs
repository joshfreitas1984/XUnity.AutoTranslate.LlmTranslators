using XUnity.AutoTranslator.LlmTranslators.Config;

using System.Reflection;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

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

    [Fact]
    public void TestLmStudioConfig()
    {
        var config = Configuration.GetConfiguration($"{sampleDirectory}/LmStudio.yaml");

        Assert.Equal("http://localhost:1234/v1/chat/completions", config.Url);
        Assert.Equal("model-identifier", config.Model);
        Assert.False(config.ApiKeyRequired);
        Assert.True(config.SystemPrompt!.Split("\n").Length > 1);
    }

    [Fact]
    public void CalculateConfigFolderUsesBepInExConfigDirectory()
    {
        var originalCurrentDirectory = Directory.GetCurrentDirectory();
        var gameDirectory = Path.Combine(Path.GetTempPath(), $"한글 게임 폴더-{Guid.NewGuid():N}");
        var translatorDirectory = Path.Combine(
            gameDirectory,
            "BepInEx",
            "plugins",
            "XUnity.AutoTranslator",
            "Translators");

        try
        {
            Directory.CreateDirectory(translatorDirectory);

            var folder = Configuration.CalculateConfigFolder(translatorDirectory);

            Assert.Equal(Path.Combine(gameDirectory, "BepInEx", "config"), folder);
            Assert.Equal(originalCurrentDirectory, Directory.GetCurrentDirectory());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            if (Directory.Exists(gameDirectory))
                Directory.Delete(gameDirectory, recursive: true);
        }
    }

    [Fact]
    public void CalculateConfigFolderPreservesReiPatcherLayout()
    {
        var gameDirectory = Path.Combine(Path.GetTempPath(), "ReiPatcher Game");
        var translatorDirectory = Path.Combine(
            gameDirectory,
            "ReiPatcher Game_Data",
            "Managed",
            "Translators");

        var folder = Configuration.CalculateConfigFolder(translatorDirectory);

        Assert.Equal(Path.Combine(gameDirectory, "AutoTranslator"), folder);
    }

    [Fact]
    public void LmStudioEndpointLoadsBepInExYamlAndAdjacentOverrides()
    {
        var originalCurrentDirectory = Directory.GetCurrentDirectory();
        var gameDirectory = Path.Combine(Path.GetTempPath(), $"LlmTranslatorTests-{Guid.NewGuid():N}");
        var translatorDirectory = Path.Combine(
            gameDirectory,
            "BepInEx",
            "plugins",
            "XUnity.AutoTranslator",
            "Translators");
        var configDirectory = Path.Combine(gameDirectory, "BepInEx", "config");
        var testWorkingDirectory = Path.Combine(gameDirectory, "WorkingDirectory");
        var conflictingGlossaryFile = Path.Combine(testWorkingDirectory, "LmStudio-Glossary.yaml");

        try
        {
            Directory.CreateDirectory(translatorDirectory);
            Directory.CreateDirectory(configDirectory);
            Directory.CreateDirectory(testWorkingDirectory);
            Directory.SetCurrentDirectory(testWorkingDirectory);
            File.WriteAllText(
                Path.Combine(configDirectory, "LmStudio.yaml"),
                "apiKeyRequired: false\nurl: http://localhost:1234/v1/chat/completions\nmodel: path-test-model\n");
            File.WriteAllText(
                Path.Combine(configDirectory, "LmStudio-SystemPrompt.txt"),
                "Adjacent system prompt");
            File.WriteAllText(
                Path.Combine(configDirectory, "LmStudio-Glossary.yaml"),
                "- raw: source\n  result: target\n");
            File.WriteAllText(
                conflictingGlossaryFile,
                "- raw: source\n  result: wrong-relative-path\n");

            var endpoint = new LmStudioTranslatorEndpoint();
            endpoint.Initialize(new TestInitializationContext(translatorDirectory));

            var configField = typeof(LmStudioTranslatorEndpoint).GetField(
                "_config",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(configField);
            var config = Assert.IsType<LlmConfig>(configField.GetValue(endpoint));

            Assert.Equal("path-test-model", config.Model);
            Assert.Equal("Adjacent system prompt", config.SystemPrompt);
            Assert.Single(config.GlossaryLines);
            Assert.Equal("target", config.GlossaryLines[0].Result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            if (Directory.Exists(gameDirectory))
                Directory.Delete(gameDirectory, recursive: true);
        }
    }

    [Fact]
    public void TestGlossaryPromptOverride()
    {
        var config = new LlmConfig { SystemPrompt = "System prompt", GlossaryPrompt = "Glossary prompt" };
        var file = Path.GetTempFileName();

        try
        {
            File.WriteAllText(file, "Override glossary prompt");

            Configuration.LoadGlossaryPrompt(config, file);

            Assert.Equal("System prompt", config.SystemPrompt);
            Assert.Equal("Override glossary prompt", config.GlossaryPrompt);
        }
        finally
        {
            File.Delete(file);
        }
    }

    private sealed class TestInitializationContext : IInitializationContext
    {
        public TestInitializationContext(string translatorDirectory)
        {
            TranslatorDirectory = translatorDirectory;
        }

        public string TranslatorDirectory { get; }
        public string SourceLanguage => "ja";
        public string DestinationLanguage => "ko";

        public T GetOrCreateSetting<T>(string section, string key, T defaultValue) => defaultValue;
        public T GetOrCreateSetting<T>(string section, string key) => default!;
        public void SetSetting<T>(string section, string key, T value) { }
        public void DisableCertificateChecksFor(params string[] hosts) { }
        public void DisableSpamChecks() { }
        public void SetTranslationDelay(float delayInSeconds) { }
    }
}
