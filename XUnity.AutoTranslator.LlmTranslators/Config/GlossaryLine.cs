namespace XUnity.AutoTranslator.LlmTranslators.Config;

public class GlossaryLine
{
    public string Raw { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;        
    public string Transliteration { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public bool CheckForHallucination { get; set; } = true;
    public bool CheckForMistranslation { get; set; } = true;

    public GlossaryLine()
    {
    }

    public GlossaryLine(string raw, string result)
    {
        Raw = raw;
        Result = result;
    }
}
