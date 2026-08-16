# XUnity.AutoTranslator.LlmTranslators

LLM-backed translator endpoints for [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator).

This plugin adds OpenAI-compatible, Ollama, and LM Studio chat-completion translators for games that use XUnity.AutoTranslator. It is designed for prompt-driven game translation, glossary-assisted terminology control, and higher request concurrency than the built-in `Custom` endpoint.

한국어 빌드 및 YAML 작성 예시는 [docs/BUILD_AND_YAML_EXAMPLES.ko.md](docs/BUILD_AND_YAML_EXAMPLES.ko.md)를 참고하세요.

## Supported Endpoints

| Endpoint | Service ID | Use case |
| --- | --- | --- |
| OpenAI | `OpenAiTranslate` | Hosted OpenAI chat-completion models such as `gpt-4o-mini`. |
| Ollama | `OllamaTranslate` | Local Ollama models served from `http://localhost:11434/api/chat` or another compatible URL. |
| LM Studio | `LmStudioTranslate` | Local OpenAI-compatible chat-completion servers from LM Studio, usually `http://localhost:1234/v1/chat/completions`. |

## Why Use This Instead Of `Custom`

- Runs multiple translations in parallel.
- Removes the default spam restriction used by `Custom`.
- Keeps LLM prompts, model parameters, API keys, and glossary data in YAML/text files that can be reused across games.
- Supports per-game override files for prompts, glossary prompts, and API keys.
- Supports hosted OpenAI, local Ollama, and local LM Studio workflows.

## Repository Layout

```text
XUnity.AutoTranslator.LlmTranslators/
  OpenAiTranslatorEndpoint.cs       OpenAI endpoint implementation
  OllamaTranslatorEndpoint.cs       Ollama endpoint implementation
  LmStudioTranslatorEndpoint.cs     LM Studio endpoint implementation
  Behavior/                         Shared request and response handling
  Config/                           YAML configuration and glossary models
  SampleConfig/                     Example OpenAI/Ollama/LM Studio config files
XUnity.AutoTranslator.LlmTranslators.Tests/
  BehaviorTests.cs                  Cleanup and request behavior tests
  ConfigTests.cs                    Configuration loading tests
  PromptTests.cs                    Prompt evaluation helpers
libs/                               XUnity.AutoTranslator reference assemblies
```

## Installation

1. Download a release build or build the project locally.
2. Install XUnity.AutoTranslator into your game with ReiPatcher or BepInEx.
3. Copy `XUnity.AutoTranslator.LlmTranslators.dll` into the game's `Translators` folder:
   - ReiPatcher: `<GameDir>/<GameName>_ManagedData/Translators`
   - BepInEx: `<GameDir>/BepInEx/plugins/XUnity.AutoTranslator/Translators`
4. Copy the sample config files from `XUnity.AutoTranslator.LlmTranslators/SampleConfig` into the AutoTranslator config folder:
   - ReiPatcher: `<GameDir>/AutoTranslator`
   - BepInEx: `<GameDir>/BepInEx/config`
5. Edit the AutoTranslator INI file and select the endpoint:

```ini
[Service]
Endpoint=OpenAiTranslate
FallbackEndpoint=
```

Use `OllamaTranslate` for Ollama or `LmStudioTranslate` for LM Studio.

## Configuration

Each endpoint uses a YAML file in the AutoTranslator config folder:

- `OpenAi.yaml`
- `Ollama.yaml`
- `LmStudio.yaml`

Important fields:

| Field | Description |
| --- | --- |
| `apiKey` | API key used for `Authorization: Bearer ...` when `apiKeyRequired` is `true`. |
| `apiKeyRequired` | Set `false` for local services that do not require an API key. |
| `url` | Chat-completion endpoint URL. |
| `model` | Model name sent in the request payload. |
| `modelParams` | Additional model parameters such as `temperature`, `top_p`, or local-model-specific values. |
| `systemPrompt` | Main translation instruction sent as the system message. |
| `glossaryPrompt` | Instruction prepended before matching glossary terms. |

Example OpenAI configuration:

```yaml
apiKey: "Change me"
apiKeyRequired: true
url: "https://api.openai.com/v1/chat/completions"
model: "gpt-4o-mini"
modelParams:
  temperature: 0.2
  top_p: 0.9
systemPrompt: |
  Translate Simplified Chinese into English. Output only the translation.
glossaryPrompt: |
  # Glossary for Consistent Translations
  Use the translation for exact matches.
  ## Terms
```

## LM Studio

To use LM Studio, start the local server in LM Studio and load the model you want to use for translation. The default LM Studio OpenAI-compatible chat completion URL is:

```yaml
url: "http://localhost:1234/v1/chat/completions"
```

Copy `LmStudio.yaml` into your AutoTranslator config folder, then update the `model` value to the model identifier shown by LM Studio:

```yaml
apiKey: "None"
apiKeyRequired: false
url: "http://localhost:1234/v1/chat/completions"
model: "model-identifier"
```

Finally, set the translator endpoint in `Config.ini`:

```ini
[Service]
Endpoint=LmStudioTranslate
FallbackEndpoint=
```

## API Key Options

You can set an API key in either location:

- In the endpoint YAML file through `apiKey`.
- In an override file named `OpenAi-ApiKey.txt`, `Ollama-ApiKey.txt`, or `LmStudio-ApiKey.txt`.

Keep API key override files out of source control.

## Override Files

Override files live next to the endpoint YAML file and take precedence over YAML values:

| File | Purpose |
| --- | --- |
| `OpenAi-SystemPrompt.txt` / `Ollama-SystemPrompt.txt` / `LmStudio-SystemPrompt.txt` | Replaces the configured system prompt. |
| `OpenAi-GlossaryPrompt.txt` / `Ollama-GlossaryPrompt.txt` / `LmStudio-GlossaryPrompt.txt` | Replaces the configured glossary prompt. |
| `OpenAi-ApiKey.txt` / `Ollama-ApiKey.txt` / `LmStudio-ApiKey.txt` | Replaces the configured API key. |

These files are useful when maintaining per-game prompts without editing the main YAML file.

## Glossary

Glossary YAML files are named `OpenAi-Glossary.yaml`, `Ollama-Glossary.yaml`, or `LmStudio-Glossary.yaml`. Matching `raw` terms are appended to the prompt for the current source string.

Minimum entry:

```yaml
- raw: 舅舅
  result: Uncle
```

Full entry format:

```yaml
- raw: 舅舅
  result: Uncle
  transliteration: Jiu Jiu
  context: Endearing way to address an uncle
  checkForHallucination: true
  checkForMistranslation: true
```

`transliteration` and `context` are documentation fields for translators. `checkForHallucination` and `checkForMistranslation` are reserved for future validation behavior.

## Prompt Tuning

Good results depend heavily on the prompt and model. For game translation, include:

- Source and target languages.
- Tone and genre context such as wuxia, eroge, fantasy, or historical drama.
- Name handling rules, including whether to keep names, use romanization, or translate titles.
- Formatting rules, especially for escaped characters, tags, and line breaks.
- A short "output only the translation" instruction.

The test project includes `PromptTests` that can be used to compare prompt outputs against example translations.

## Build And Test

Prerequisites:

- .NET SDK that can build the solution.
- XUnity.AutoTranslator reference assemblies in `libs/`.

Build:

```powershell
dotnet build XUnity.AutoTranslate.LlmTranslators.sln -c Release
```

Run non-live tests:

```powershell
dotnet test XUnity.AutoTranslate.LlmTranslators.sln -c Release --no-build --filter "FullyQualifiedName!~PromptTests"
```

`PromptTests` are environment-dependent because they can call live OpenAI, Ollama, or LM Studio endpoints.

Release builds merge `YamlDotNet.dll` into the translator assembly through ILRepack and copy the translator DLL plus sample configs into the repository `Release` folder.

## Current Project Notes

- The plugin target framework is `net45` for compatibility with Unity/XUnity.AutoTranslator environments.
- Local game-directory deployment is opt-in through `DeployToGameDirs=true`.
- The included `libs/` assemblies are XUnity.AutoTranslator development references used for compilation.
- This repository includes development references and sample configs, but users should keep private API key override files out of source control.

## License

This project is licensed under the MIT License. See [LICENSE](./LICENSE).
