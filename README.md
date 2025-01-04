# XUnity.AutoTranslator.LlmTranslators

A series of LLM Translators that can be used with popular LLMs such as ChatGpt configured with prompts to translate games with XUnity.AutoTranslator. 

Current supported plugins include:
- [OpenAI\ChatGPT](https://platform.openai.com/)
	- Probably the most popular LLM that has the highest quality but is not Free
- [Ollama Models](https://ollama.com/)
	- Ollama is a local hosting option for LLMs. You are able to run one or more llms on your local machine of varying size. This option is free but will require you to engineer your prompts dependant on the model and/or language.

## Why use this instead of the [Custom] endpoint?

- We run up to 15 translations in parallel (unlike the custom endpoint which is tied to 1)
- We have removed the spam restriction (which has 1 second by default on custom)

## Installation instructions

1. Download or Build the latest assembly from the Releases
2. Install [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator) into your game as normal with either ReiPatcher or BepinEx
3. Place assembly into Translators folder for your game, you should see the other translators in the folder (eg. CustomTranslate.dll)
	- If used ReiPatcher: `<GameDir>/<Game name>_ManagedData/Translators` 
	- If used BepinEx: `<GameDir>/BepinEx/plugins/XUnity.AutoTranslator/Translators` 

# Configuration

We use yaml configuration to make multi-line prompt engineering much easier. It also means we do not mess with the standard Autotranslator INI file so that there is less to configure.

To configure your LLM you will need to follow the following steps:

1. Either run the game to create the default config or copy the [Sample Configs](./XUnity.AutoTranslator.LlmTranslators.Tests/SampleConfig) into the AutoTranslator folder
	- If used ReiPatcher: `<GameDir>/AutoTranslator`
	- If used BepinEx: `<GameDir>/BepinEx/config`
2. Open the config for the LLMTranslator you wish to use
	- If ChatGPT: `ChatGPT.Yml`
	- If a local Olama LLM: `Ollama.Yml`
3. Update your config with any API keys, custom urls and system prompts.
4. Finally update your AutoTranslator INI file with your translate service
	- ```
	  [Service]
	  Endpoint=ChatGPTTranslate
	  FallbackEndpoint=
	  ```
	- If ChatGPT: `ChatGPTTranslate`
	- If a local Olama LLM: `OllamaTranslate`

## Fine tuning your prompt

Please note the prompt is what actually tells ChatGPT what to translate. Some things that will help:
- Update the languages eg. Simplified Chinese to English, Japanese to English
- Ensure you add context to the prompt for the game such as 'Wuxia', 'Sengoku Jidai', 'Xanxia', 'Eroge'. 
- Make sure you tell it how to translate names whether you want literal translation or keep the original names

A test project is included with the project. The `PromptTests`((./XUnity.AutoTranslator.LlmTranslators.Tests/PromptTests.cs) will let you easily change your prompt based on your model and compare outputs to some ChatGPT4o pretranslated values. These are a good baseline to compare your prompts or other models to, most cases will show you where the model will lose the plot and hallucinate.

## Packages

The assemblies included are the Dev versions of XUnity.AutoTranslator. Feel free to star/fork this repo however you like.