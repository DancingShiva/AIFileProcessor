# Web API Reference

`AIFileProcessor.Web` — ASP.NET Core. All endpoints require an **Entra ID JWT bearer token**
(authorization fallback policy), and all are relative to the app root. AI-provider API keys live on
the **server**; clients only send non-secret preferences.

| Method | Route | Auth | Purpose |
|---|---|---|---|
| `POST` | `/api/processing/process` | Bearer token | Run Chinese text through the AI pipeline. |
| `GET` | `/api/settings` | Bearer token | Get current provider defaults (keys stripped). |
| `POST` | `/api/settings` | Bearer token | No-op — returns guidance (persistence is client-side). |

Send the token in the standard header: `Authorization: Bearer <token>`.

---

## POST /api/processing/process

Processes a block of Chinese text. Request/response are JSON.

### Request body — `ProcessRequest`

| Field | Type | Required | Notes |
|---|---|---|---|
| `modelId` | string | ✔ | e.g. `gemini-3.5-flash`, `gpt-5.6-terra`, `deepseek-v4-flash`. Must match a registered provider's model. |
| `prompt` | string | ✔ | System instruction/rules sent to the model. |
| `data` | string | ✔ | Chinese sentences to process (one per line). |
| `idPrefix` | string | — | Prefix for generated sentence IDs. Default `""`. |
| `idStart` | int | — | First ID number. Default `1`. |
| `idPad` | int | — | Zero-padding for the ID number. Default `3`. |
| `lengthPad` | int | — | Padding for the length indicator. Default `3`. |
| `reasoningLevel` | enum? | — | `None, Minimal, Low, Medium, High, XHigh, Max` (integers). Overrides the provider default for this call. |
| `geminiPreferences` | object? | — | Non-secret Gemini settings override (`GeminiSettings`; `apiKey` ignored). |
| `openAiPreferences` | object? | — | Non-secret OpenAI settings override. |
| `deepSeekPreferences` | object? | — | Non-secret DeepSeek settings override. |

> `MaxTokens` may be supplied inside a provider's `*Preferences`; the first non-null provider preference
> (Gemini → OpenAI → DeepSeek) sets the per-model `MaxTokens`, which is then clamped server-side to the
> provider's hard max. Validation errors return `400`.

### Example

```http
POST /api/processing/process HTTP/1.1
Authorization: Bearer eyJ0eXA...
Content-Type: application/json

{
  "modelId": "gemini-3.5-flash",
  "prompt": "You are a Mandarin segmenter... (rules)",
  "data": "你好\n我想去上海。",
  "idPrefix": "S",
  "idStart": 1,
  "reasoningLevel": 4
}
```

### Response — `200 OK` → `ProcessResponse`

```json
{
  "rawResponse": "S001\t你好\tnǐ hǎo\t...",
  "scratchpad": "...",
  "corrections": ["..."],
  "processedSentenceRows": [
	{
	  "id": "S001",
	  "chinese": "你好",
	  "pinyin": "nǐ hǎo",
	  "breakdown": [ { "chinese": "你", "pinyin": "nǐ", "gloss": "you",
					   "partOfSpeech": "pron", "hskLevel": "1", "note": "" } ],
	  "audioReordering": [ { "visualString": "你好", "startIndex": 0, "endIndex": 1 } ],
	  "english": "hello",
	  "length": "002"
	}
  ]
}
```

| Field | Type | Notes |
|---|---|---|
| `rawResponse` | string | Tab-delimited rows (Id, Chinese, Pinyin, breakdown, audio reordering, English, length). |
| `scratchpad` | string | Provider reasoning / audit trail (debugging). |
| `corrections` | string[] | Corrections the model applied (e.g., OCR fixes). |
| `processedSentenceRows` | object[] | Structured per-sentence rows (see example). |

### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | Missing `modelId`/`prompt`/`data`; unsupported model; or AI API key not configured (`InvalidOperationException`). |
| `401 Unauthorized` | No/invalid bearer token (any endpoint). |
| `499 Client Closed Request` | Processing was cancelled (client disconnected / `ct` triggered). |
| `500 Internal Server Error` | Any other unhandled failure; message is generic. |

---

## GET /api/settings

Returns current **default** settings per provider, with `ApiKey` excluded via `[JsonIgnore]`.

### Response — `200 OK` → `ProviderSettingsResponse`

```json
{
  "geminiSettings":  { "endpoint": "...", "modelName": "...", "timeoutSeconds": 600, "...": "..." },
  "openAiSettings":  { "...": "..." },
  "deepSeekSettings": { "...": "..." }
}
```

Use this to pre-populate a settings UI; then persist the user's choices client-side
(see below) and echo them back in `*Preferences` on `/api/processing/process`.

---

## POST /api/settings

**No-op in web mode.** Returns `200 OK` with a guidance message; nothing is persisted server-side.

```json
{ "message": "In web mode, store preferences client-side in localStorage. API keys are configured via server-side environment variables." }
```

The intended pattern: the browser stores non-secret settings in `localStorage` and sends them in
`geminiPreferences`/`openAiPreferences`/`deepSeekPreferences` on each processing request.

---

## Client integration checklist

1. Acquire an Entra ID access token for this API's audience and send it as `Authorization: Bearer`.
2. `GET /api/settings` once to seed defaults; let the user edit; persist locally.
3. `POST /api/processing/process` with `modelId`/`prompt`/`data` + echoed `*Preferences`.
4. Handle `400` (validation/key), `401` (token), `499` (cancelled), `500` (server error).
