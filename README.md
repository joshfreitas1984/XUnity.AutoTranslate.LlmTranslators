# XUnity.AutoTranslator.LlmTranslators

A series of LLM Translators that can be used with popular LLMs such as ChatGpt configured with prompts to translate games with XUnity.AutoTranslator. 

Current supported plugins include:
- [OpenAI](https://platform.openai.com/)
	- Probably the most popular LLM that has the highest quality but is not Free
- [Ollama Models](https://ollama.com/)
	- Ollama is a local hosting option for LLMs. You are able to run one or more llms on your local machine of varying size. This option is free but will require you to engineer your prompts dependant on the model and/or language.

# Why use this instead of the [Custom] endpoint?

- We run up to 15 translations in parallel (unlike the custom endpoint which is tied to 1)
- We have removed the spam restriction (which has 1 second by default on custom)

# Installation instructions

1. Download or Build the latest assembly from the Releases
2. Install [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator) into your game as normal with either ReiPatcher or BepinEx
3. Place assembly into Translators folder for your game, you should see the other translators in the folder (eg. CustomTranslate.dll)
	- If used ReiPatcher: `<GameDir>/<Game name>_ManagedData/Translators` 
	- If used BepinEx: `<GameDir>/BepinEx/plugins/XUnity.AutoTranslator/Translators` 

# Configuration

We use an additional yaml configuration file to make it easier to copy around with multiple games. We also support configuration override files which take precedence over the main file. This is so things like copying and pasting prompts and glossary items becomes easier while in game. It also means we do not mess with the standard Autotranslator INI file.

To configure your LLM you will need to follow the following steps:

1. Either run the game to create the default config or copy the [Sample Configs](./XUnity.AutoTranslator.LlmTranslators/SampleConfig) into the AutoTranslator folder
	- If used ReiPatcher: `<GameDir>/AutoTranslator`
	- If used BepinEx: `<GameDir>/BepinEx/config`
2. Open the config for the LLMTranslator you wish to use
	- If OpenaI: `OpenAi.Yml`
	- If a local Olama LLM: `Ollama.Yml`
3. Update your config with any API keys, custom urls, glossaries and system prompts.
4. Finally update your AutoTranslator INI file with your translate service
	- ```
	  [Service]
	  Endpoint=OpenAiTranslate
	  FallbackEndpoint=
	  ```
	- If OpenAi: `OpenAiTranslate`
	- If a local Olama LLM: `OllamaTranslate`

## Global API Key

We also use global environment variables so you can just set your API Key once and never have to think about it again.

1. Google how to set environment variables on your operating system
2. Set the following environment variable: `AutoTranslator_API_Key` to the value of your API Key.

## Configuration Override Files

We have seperate files that can be override any config you have loaded in your config file. This makes it easier to publish game specific prompts, glossaries or just make it easier to use multi line prompts without having to worry about YAML formatting.

These files are:
  - `OpenAi-SystemPrompt.txt` or `Ollama-SystemPrompt.txt`
	- Use this file to update your system prompt
  - `OpenAi-GlossaryPrompt.txt` or `Ollama-GlossaryPrompt.txt`
	- Use this file to update your glossary prompt
  - `OpenAi-ApiKey.txt` or `Ollama-ApiKey.txt`
	- Use this file to update your API Key

# Glossary

The glossary feature scans for text that matches entries in the glossary and allows you to instruct how the LLM will translate that word/term/sentence. This reduces hallucinations and mistranslations significantly. The format for a glossary is as follows:

```yaml
- raw: 舅舅
  result: Uncle
```

This is the minimum required for an entry in a glossary. You can also specifically give a seperate glossary prompt to guide your LLM better.

The glossary format supports more options that are mostly there to help translation teams produce more consistent Autotranslator glossaries. The full list is as follows:

```yaml
- raw: 舅舅
  result: Uncle
  transliteration: Jiu Jiu
  context: Endearing way to address an uncle
  checkForHallucination: true
  checkForMistranslation: true
```

Please note `transliteration`, `context` do nothing in the plugin.
Currently `checkForHallucination` and `checkForMistranslation` have not been implemented - stay tuned.

# Fine tuning your prompt

Please note the prompt is what actually tells ChatGPT what to translate. Some things that will help:
- Update the languages eg. Simplified Chinese to English, Japanese to English
- Ensure you add context to the prompt for the game such as 'Wuxia', 'Sengoku Jidai', 'Xanxia', 'Eroge'. 
- Make sure you tell it how to translate names whether you want literal translation or keep the original names

A test project is included with the project. The [PromptTests](./XUnity.AutoTranslator.LlmTranslators.Tests/PromptTests.cs) will let you easily change your prompt based on your model and compare outputs to some ChatGPT4o pretranslated values. These are a good baseline to compare your prompts or other models to, most cases will show you where the model will lose the plot and hallucinate.

# Packages

The assemblies included are the Dev versions of XUnity.AutoTranslator. Feel free to star/fork this repo however you like.