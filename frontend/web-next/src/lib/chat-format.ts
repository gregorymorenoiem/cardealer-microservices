// =============================================================================
// Chat Format Utilities — Clean and parse chatbot responses
// =============================================================================
// Handles:
//  1. JSON-wrapper extraction (LLM sometimes wraps in {"response": "..."})
//  2. Escaped newline normalization (literal \n → real newlines)

/**
 * Clean a raw bot response string.
 *
 * - If the string is a JSON object with a `response` (or `text`) key,
 *   extract its value.
 * - Normalise escaped newlines (`\\n`) that survived JSON-parse into
 *   real newline characters.
 */
export function cleanBotResponse(raw: string): string {
  if (!raw || typeof raw !== 'string') return raw ?? '';

  let cleaned = raw.trim();

  // ── 1. Try full JSON parse ─────────────────────────────────────────────
  // LLM may output: {"response": "..."} or {"text": "..."}
  if (cleaned.startsWith('{')) {
    // 1a. Complete JSON — try JSON.parse
    if (cleaned.endsWith('}')) {
      try {
        const parsed = JSON.parse(cleaned) as Record<string, unknown>;
        const inner =
          typeof parsed.response === 'string'
            ? parsed.response
            : typeof parsed.text === 'string'
              ? parsed.text
              : typeof parsed.message === 'string'
                ? parsed.message
                : typeof parsed.content === 'string'
                  ? parsed.content
                  : null;

        if (inner !== null) {
          cleaned = inner;
        }
      } catch {
        // Invalid JSON — fall through to regex
      }
    }

    // 1b. Truncated JSON — LLM hit max_tokens before closing "}"
    // Strip the {"response": " prefix with regex
    if (cleaned.startsWith('{')) {
      const prefixMatch = cleaned.match(/^\{\s*"(?:response|text|message|content)"\s*:\s*"/);
      if (prefixMatch) {
        cleaned = cleaned.slice(prefixMatch[0].length);
        // Remove trailing incomplete JSON: ", "intent":... or "}
        // Find last complete sentence/thought
        cleaned = cleaned
          .replace(
            /",\s*"(?:intent|confidence|isFallback|parameters|leadSignals|suggestedAction|quickReplies).*$/s,
            ''
          )
          .replace(/"\s*\}\s*$/, '')
          .replace(/"\s*$/, '');
      }
    }
  }

  // ── 2. Normalise escaped newlines ──────────────────────────────────────
  // Sometimes the text still contains literal two-char sequences `\n`
  cleaned = cleaned.replace(/\\n/g, '\n');

  // ── 3. Unescape JSON-escaped quotes ────────────────────────────────────
  cleaned = cleaned.replace(/\\"/g, '"');

  return cleaned.trim();
}
