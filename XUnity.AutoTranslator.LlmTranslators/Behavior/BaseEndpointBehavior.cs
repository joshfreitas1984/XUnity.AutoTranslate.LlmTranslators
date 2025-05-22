using SimpleJSON;
using System.Text;
using System.Text.RegularExpressions;
using XUnity.AutoTranslator.LlmTranslators.Config;

namespace XUnity.AutoTranslator.LlmTranslators.Behavior;

public static class BaseEndpointBehavior
{
    public static string GetRequestData(LlmConfig config, string raw)
    {
        var systemPrompt = new StringBuilder(config.SystemPrompt);
        systemPrompt.AppendLine(ConstructGlossaryPrompt(raw, config));

        // Create messages using SimpleJSON (since some games don't have access to Newtonsoft)
        var messages = new JSONArray();
        var systemMessage = new JSONObject();
        systemMessage["role"] = "system";
        systemMessage["content"] = systemPrompt.ToString();
        messages.Add(systemMessage);

        var userMessage = new JSONObject();
        userMessage["role"] = "user";
        userMessage["content"] = raw;
        messages.Add(userMessage);

        // Create the requestBody object using SimpleJSON
        var requestBody = new JSONObject
        {
            ["model"] = config.Model,
            ["stream"] = false,
            ["messages"] = messages
        };

        // Add model parameters if available
        if (config.ModelParams != null)
        {
            foreach (var param in config.ModelParams)
            {
                if (decimal.TryParse(param.Value.ToString(), out decimal isDecimal))
                    requestBody[param.Key] = (double)isDecimal;
                else if (int.TryParse(param.Value.ToString(), out int isInt))
                    requestBody[param.Key] = isInt;
                else
                    requestBody[param.Key] = param.Value.ToString();
            }
        }
        else
        {
            requestBody["temperature"] = 0.2;
            requestBody["top_p"] = 0.9;
            requestBody["frequency_penalty"] = 0;
            requestBody["presence_penalty"] = 0;
        }

        return requestBody.ToString();
    }

    private static string AppendPromptsFor(string raw, List<GlossaryLine> glossaryLines)
    {
        var prompt = new StringBuilder();

        foreach (var line in glossaryLines)
        {
            if (raw.Contains(line.Raw))
                prompt.Append($"- {line.Raw}: {line.Result}\n");
        }

        return prompt.ToString();
    }

    public static string ConstructGlossaryPrompt(string raw, LlmConfig config)
    {
        var prompt = new StringBuilder(config.GlossaryPrompt);
        prompt.AppendLine(AppendPromptsFor(raw, config.GlossaryLines));
        return prompt.ToString();
    }

    public static string ValidateAndCleanupTranslation(string raw, string result, LlmConfig config)
    {
        // Check glossary mistranslation here
        // If we do any other clean up should be done here
        if ((result.StartsWith("\"") && result.EndsWith("\""))
            || (result.StartsWith("'") && result.EndsWith("'")))
            result = result.Substring(1, result.Length - 2);      

        //Take out wide quotes
        result = result
            .Replace("’", "'")
            .Replace("‘", "'");

        result = Regex.Unescape(result);
        
        //Make sure first character is upper case
        if (Char.IsLower(result[0]) && raw != result)
            result = Char.ToUpper(result[0]) + result.Substring(1, result.Length - 1);

        return result.Trim();
    }
}
