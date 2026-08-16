# AGENTS.md

## Project Identity

This repository is a translator extension for **XUnity.AutoTranslator**, the Unity game auto-translation plugin.

It is not a standalone Unity project. Treat it as a .NET plugin that adds LLM-backed translator endpoints to XUnity.AutoTranslator, with **LM Studio support as a first-class added extension**.

The main runtime goal is:

- Load inside XUnity.AutoTranslator's `Translators` folder.
- Register translator endpoints such as `OpenAiTranslate`, `OllamaTranslate`, and `LmStudioTranslate`.
- Let games using XUnity.AutoTranslator translate text through local or hosted chat-completion APIs.

## Important Context

- `LmStudioTranslatorEndpoint.cs` implements the LM Studio endpoint.
- LM Studio uses an OpenAI-compatible chat-completion API, normally:
  `http://localhost:1234/v1/chat/completions`
- The service ID for LM Studio is `LmStudioTranslate`.
- Sample LM Studio configuration lives in:
  `XUnity.AutoTranslator.LlmTranslators/SampleConfig/LmStudio.yaml`
- XUnity.AutoTranslator configuration selects the endpoint through:

```ini
[Service]
Endpoint=LmStudioTranslate
FallbackEndpoint=
```

## Repository Layout

```text
XUnity.AutoTranslator.LlmTranslators/
  OpenAiTranslatorEndpoint.cs
  OllamaTranslatorEndpoint.cs
  LmStudioTranslatorEndpoint.cs
  Behavior/
  Config/
  SampleConfig/

XUnity.AutoTranslator.LlmTranslators.Tests/
  BehaviorTests.cs
  ConfigTests.cs
  PromptTests.cs

libs/
  XUnity.AutoTranslator reference assemblies used for compilation
```

## Development Rules

- Preserve compatibility with Unity/XUnity.AutoTranslator environments.
- Keep the plugin target framework at `net45` unless there is an explicit compatibility decision.
- Do not convert this into a Unity project layout.
- Do not remove the local-reference assemblies in `libs/`; they are build references for XUnity.AutoTranslator APIs.
- Keep endpoint IDs stable because users reference them from `Config.ini`.
- Keep LM Studio local-server defaults friendly to offline/local model workflows.
- Avoid committing real API keys or per-user override files.

## Configuration Model

Each endpoint is driven by YAML files in the AutoTranslator config folder:

- `OpenAi.yaml`
- `Ollama.yaml`
- `LmStudio.yaml`

Override files can replace YAML values:

- `*-SystemPrompt.txt`
- `*-GlossaryPrompt.txt`
- `*-ApiKey.txt`

Glossary files use endpoint-specific names, for example:

- `LmStudio-Glossary.yaml`

## Build

```bash
dotnet build XUnity.AutoTranslate.LlmTranslators.sln -c Release
```

Release builds copy the translator DLL and sample configs into the repository `Release` folder. The project also supports optional game-directory deployment through `DeployToGameDirs=true`; do not enable it by default.

## Tests

Run the non-live test suite with:

```bash
dotnet test XUnity.AutoTranslate.LlmTranslators.sln -c Release --no-build --filter "FullyQualifiedName!~PromptTests"
```

`PromptTests` may call live OpenAI, Ollama, or LM Studio endpoints, so do not assume they are safe for ordinary CI or offline runs.

## Common Change Guidance

When changing LM Studio behavior:

- Check parity with the shared behavior in `Behavior/BaseEndpointBehavior.cs`.
- Keep request payloads OpenAI-chat-completion compatible unless the change is explicitly LM Studio-specific.
- Keep `apiKeyRequired: false` as the expected local LM Studio default.
- Validate response extraction against the `choices[0].message.content` shape.

When changing prompt or glossary behavior:

- Prefer shared logic in `Config/` or `Behavior/`.
- Keep sample config files usable as copy-paste starting points for game users.
- Preserve formatting-sensitive game text such as tags, escaped characters, and line breaks.
