using XUnity.AutoTranslator.LlmTranslators.Behavior;
using XUnity.AutoTranslator.LlmTranslators.Config;

namespace XUnity.AutoTranslator.LlmTranslators.Tests;

public class BehaviorTests
{
    const string workingDirectory = "../../../";
    const string sampleDirectory = $"{workingDirectory}/../XUnity.AutoTranslator.LlmTranslators/SampleConfig/";

    [Fact]
    public void TestCleanup()
    {
        var config = Configuration.GetConfiguration($"{sampleDirectory}/OpenAi.yaml");
        
        var raw = "我曉得。\r\n雖說妳是門中年紀最小的師妹，但妳天資聰穎，練功又勤，\r\n掌門獨創的「天地無聲勢」也唯獨傳妳一人，說不定妳的功夫已比我還深了。";
        var input = "\"I understand.  \\nAlthough you are the youngest junior sister in the sect, you are talented and diligent in your cultivation.  \\nThe sect leader's unique technique, \\\"Silent Force of Heaven and Earth,\\\" has only been passed down to you, so your skills might even surpass mine.\"";

        var output = BaseEndpointBehavior.ValidateAndCleanupTranslation(raw, input, config);
        File.WriteAllText($"{workingDirectory}/TestOutput/TestCleanup.txt", output);
    }

    [Fact]
    public void TestCleanup2()
    {
        var config = Configuration.GetConfiguration($"{sampleDirectory}/OpenAi.yaml");

        var raw = "你就著殘燭火光，讀了講經閣借來的《唐詩選輯》";
        var input = "You read the \"Selected Tang Poems\" borrowed from the Sutra Pavilion by the light of a flickering candle.";

        var output = BaseEndpointBehavior.ValidateAndCleanupTranslation(raw, input, config);
        File.WriteAllText($"{workingDirectory}/TestOutput/TestCleanup2.txt", output);
    }
}
