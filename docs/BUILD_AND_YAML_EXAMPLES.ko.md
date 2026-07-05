# 빌드 및 YAML 작성 예시

이 프로젝트는 Unity 프로젝트가 아니라 **XUnity.AutoTranslator용 번역기 DLL 확장**입니다. 빌드 결과물은 게임의 XUnity.AutoTranslator `Translators` 폴더에 넣어 사용하는 `XUnity.AutoTranslator.LlmTranslators.dll`입니다.

## 1. 빌드 전제

필요한 항목:

- .NET SDK
- 저장소의 `libs/` 폴더에 포함된 XUnity.AutoTranslator 참조 DLL
- LM Studio를 사용할 경우 로컬 서버 실행

현재 프로젝트는 Unity/XUnity.AutoTranslator 호환성을 위해 `net45`를 대상으로 합니다.

`mise`를 사용하는 경우 저장소 루트의 `mise.toml`에 .NET SDK와 작업이 명시되어 있습니다.

```bash
mise install
mise run build
mise run test
```

## 2. Release 빌드

저장소 루트에서 실행합니다.

```bash
dotnet restore XUnity.AutoTranslate.LlmTranslators.sln
dotnet build XUnity.AutoTranslate.LlmTranslators.sln -c Release
```

Release 빌드 후 결과물은 다음 위치에 복사됩니다.

```text
Release/
  Translators/
    XUnity.AutoTranslator.LlmTranslators.dll
  SampleConfig/
    Config.ini
    LmStudio.yaml
    Ollama.yaml
    OpenAi.yaml
```

## 3. 테스트

일반 테스트만 실행하려면 `PromptTests`를 제외합니다. `PromptTests`는 OpenAI, Ollama, LM Studio 같은 실제 API 또는 로컬 서버를 호출할 수 있습니다.

```bash
dotnet test XUnity.AutoTranslate.LlmTranslators.sln -c Release --no-build --filter "FullyQualifiedName!~PromptTests"
```

## 4. 게임 폴더 배치 예시

BepInEx 기반 XUnity.AutoTranslator:

```text
<GameDir>/BepInEx/plugins/XUnity.AutoTranslator/Translators/
  XUnity.AutoTranslator.LlmTranslators.dll

<GameDir>/BepInEx/config/
  Config.ini
  LmStudio.yaml
  LmStudio-Glossary.yaml
```

ReiPatcher 기반 XUnity.AutoTranslator:

```text
<GameDir>/<GameName>_ManagedData/Translators/
  XUnity.AutoTranslator.LlmTranslators.dll

<GameDir>/AutoTranslator/
  Config.ini
  LmStudio.yaml
  LmStudio-Glossary.yaml
```

## 5. XUnity Config.ini 예시

LM Studio를 사용하려면 AutoTranslator 설정에서 endpoint를 `LmStudioTranslate`로 지정합니다.

```ini
[Service]
Endpoint=LmStudioTranslate
FallbackEndpoint=

[General]
Language=ko
FromLanguage=ja
```

중국어 원문을 한국어로 번역한다면 다음처럼 바꿉니다.

```ini
[General]
Language=ko
FromLanguage=zh
```

## 6. LM Studio 최소 YAML

파일명은 반드시 `LmStudio.yaml`이어야 합니다.

저장소에는 바로 복사해서 쓸 수 있는 예시도 포함되어 있습니다.

```text
XUnity.AutoTranslator.LlmTranslators/SampleConfig/LmStudio-Korean.example.yaml
XUnity.AutoTranslator.LlmTranslators/SampleConfig/LmStudio-Glossary.example.yaml
```

```yaml
apiKey: "None"
apiKeyRequired: false
url: "http://localhost:1234/v1/chat/completions"
model: "local-model-id"
modelParams:
  temperature: 0.2
  max_tokens: 1024
  top_p: 0.9
systemPrompt: |
  Translate the provided Japanese game text into natural Korean.
  Preserve names, tags, escaped characters, variables, and line breaks.
  Output only the translated text.
glossaryPrompt: |
  # Glossary for Consistent Translations
  Use the translation when an exact match appears in the source text.
  ## Terms
```

`model` 값은 LM Studio의 local server 화면에 표시되는 모델 ID로 바꿉니다.

## 7. LM Studio 권장 YAML

게임 텍스트 번역용으로 조금 더 엄격하게 작성한 예시입니다.

```yaml
apiKey: "None"
apiKeyRequired: false
url: "http://localhost:1234/v1/chat/completions"
model: "qwen3-8b"
modelParams:
  temperature: 0.15
  max_tokens: 2048
  top_p: 0.9
  frequency_penalty: 0
  presence_penalty: 0
systemPrompt: |
  You are translating game UI and dialogue into Korean.

  Rules:
  - Translate the source text into fluent Korean.
  - Keep the original meaning, tone, and speaker intent.
  - Preserve placeholders, variables, rich-text tags, escaped characters, and line breaks.
  - Preserve strings such as {0}, {name}, %s, \n, <color=...>, </color>, <b>, </b>.
  - Do not add explanations, notes, markdown, or quotation marks.
  - If the source is already Korean, return it unchanged.
  - Use the glossary exactly when a matching source term appears.
glossaryPrompt: |
  # Glossary for Consistent Translations
  The following terms have fixed translations.
  Use them exactly when the source text contains the matching term.
  ## Terms
```

## 8. OpenAI 호환 서버 YAML

LM Studio 외에도 OpenAI 호환 API 서버라면 같은 응답 형식(`choices[0].message.content`)을 쓰는 경우 `LmStudioTranslate`로 붙일 수 있습니다.

```yaml
apiKey: "your-api-key"
apiKeyRequired: true
url: "https://example.com/v1/chat/completions"
model: "model-name"
modelParams:
  temperature: 0.2
  max_tokens: 2048
  top_p: 0.9
systemPrompt: |
  Translate Simplified Chinese game text into Korean.
  Preserve variables, tags, escaped characters, and line breaks.
  Output only the translation.
glossaryPrompt: |
  # Glossary for Consistent Translations
  Use the translation when an exact match appears in the source text.
  ## Terms
```

로컬 LM Studio처럼 API 키가 필요 없는 서버는 다음처럼 둡니다.

```yaml
apiKey: "None"
apiKeyRequired: false
```

## 9. Ollama YAML 예시

Ollama endpoint를 사용할 경우 파일명은 `Ollama.yaml`, endpoint는 `OllamaTranslate`입니다.

```yaml
apiKey: "None"
apiKeyRequired: false
url: "http://localhost:11434/api/chat"
model: "qwen2.5:7b"
modelParams:
  temperature: 0.2
  num_ctx: 8192
  top_p: 0.9
systemPrompt: |
  Translate Japanese game text into Korean.
  Preserve placeholders, tags, escaped characters, and line breaks.
  Output only the translated text.
glossaryPrompt: |
  # Glossary for Consistent Translations
  Use the translation when an exact match appears in the source text.
  ## Terms
```

`Config.ini`에서는 다음처럼 지정합니다.

```ini
[Service]
Endpoint=OllamaTranslate
FallbackEndpoint=
```

## 10. Glossary YAML 예시

LM Studio용 glossary 파일명은 `LmStudio-Glossary.yaml`입니다.

```yaml
- raw: 先輩
  result: 선배
  transliteration: senpai
  context: Character relationship title
  checkForHallucination: true
  checkForMistranslation: true

- raw: 魔王
  result: 마왕
  context: Fantasy title

- raw: HP
  result: HP
  context: Game stat label; keep unchanged
```

코드는 원문 문자열에 `raw`가 포함되어 있을 때만 해당 glossary 항목을 프롬프트에 추가합니다.

## 11. 오버라이드 파일 예시

YAML을 직접 수정하지 않고 게임별 프롬프트나 키를 바꾸고 싶으면 같은 폴더에 파일을 추가합니다.

```text
LmStudio-SystemPrompt.txt
LmStudio-GlossaryPrompt.txt
LmStudio-ApiKey.txt
LmStudio-Glossary.yaml
```

예를 들어 `LmStudio-SystemPrompt.txt`:

```text
Translate Japanese visual novel dialogue into natural Korean.
Keep character names consistent.
Preserve tags, variables, escaped sequences, and line breaks.
Output only the Korean translation.
```

API 키가 필요한 호환 서버를 쓴다면 `LmStudio-ApiKey.txt`에 키만 넣습니다. 실제 API 키 파일은 소스 관리에 넣지 않습니다.

## 12. YAML 작성 규칙

- 필드명은 `apiKey`, `apiKeyRequired`, `url`, `model`, `modelParams`, `systemPrompt`, `glossaryPrompt`를 사용합니다.
- 여러 줄 프롬프트는 `|` 블록을 사용합니다.
- `modelParams` 아래 값은 요청 JSON의 최상위 필드로 전달됩니다.
- LM Studio 기본 URL은 `http://localhost:1234/v1/chat/completions`입니다.
- LM Studio 기본값은 `apiKeyRequired: false`가 적합합니다.
- 특수문자, 콜론, URL이 들어간 값은 따옴표로 감싸는 편이 안전합니다.
