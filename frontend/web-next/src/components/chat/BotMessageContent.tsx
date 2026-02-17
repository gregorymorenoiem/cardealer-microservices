// =============================================================================
// BotMessageContent — Lightweight markdown renderer for chat messages
// =============================================================================
// Renders a subset of Markdown commonly produced by the LLM:
//   **bold**  →  <strong>
//   *italic*  →  <em>
//   \n        →  line break / paragraph split
//   1. item   →  ordered list
//   - item    →  unordered list
//   • item    →  unordered list
//
// This avoids pulling in a full markdown parser (react-markdown + remark).

'use client';

import React, { useMemo } from 'react';

interface BotMessageContentProps {
  content: string;
}

// ─────────────────────────────────────────────────────────────────────────────
// Inline formatting — bold & italic
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Parse inline markdown tokens (**bold**, *italic*) and return React nodes.
 */
function renderInline(text: string): React.ReactNode[] {
  const nodes: React.ReactNode[] = [];
  // Regex: **bold** or *italic* (non-greedy, no nesting)
  const regex = /(\*\*(.+?)\*\*|\*(.+?)\*)/g;

  let lastIndex = 0;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(text)) !== null) {
    // Text before this match
    if (match.index > lastIndex) {
      nodes.push(text.slice(lastIndex, match.index));
    }

    if (match[2]) {
      // **bold**
      nodes.push(
        <strong key={`b-${match.index}`} className="font-semibold">
          {match[2]}
        </strong>
      );
    } else if (match[3]) {
      // *italic*
      nodes.push(<em key={`i-${match.index}`}>{match[3]}</em>);
    }

    lastIndex = match.index + match[0].length;
  }

  // Remaining text after last match
  if (lastIndex < text.length) {
    nodes.push(text.slice(lastIndex));
  }

  return nodes.length > 0 ? nodes : [text];
}

// ─────────────────────────────────────────────────────────────────────────────
// Block-level parsing
// ─────────────────────────────────────────────────────────────────────────────

interface Block {
  kind: 'paragraph' | 'ordered-list' | 'unordered-list';
  items: string[]; // for lists, each item; for paragraph, single entry
}

/**
 * Group lines into logical blocks (paragraphs, ordered lists, unordered lists).
 */
function parseBlocks(text: string): Block[] {
  const lines = text.split('\n');
  const blocks: Block[] = [];
  let current: Block | null = null;

  const flushCurrent = () => {
    if (current) {
      blocks.push(current);
      current = null;
    }
  };

  for (const rawLine of lines) {
    const line = rawLine;

    // Empty line → flush
    if (line.trim() === '') {
      flushCurrent();
      continue;
    }

    // Ordered list: "1. ", "2. ", etc.
    const olMatch = line.match(/^\s*(\d+)\.\s+(.*)/);
    if (olMatch) {
      if (current?.kind !== 'ordered-list') {
        flushCurrent();
        current = { kind: 'ordered-list', items: [] };
      }
      current!.items.push(olMatch[2]);
      continue;
    }

    // Unordered list: "- ", "• ", "* " (only at start, with space after)
    const ulMatch = line.match(/^\s*[-•*]\s+(.*)/);
    if (ulMatch && !line.match(/^\*\*.*\*\*$/)) {
      // Avoid matching a bold-only line like "**text**"
      if (current?.kind !== 'unordered-list') {
        flushCurrent();
        current = { kind: 'unordered-list', items: [] };
      }
      current!.items.push(ulMatch[1]);
      continue;
    }

    // Regular text → paragraph
    if (current?.kind === 'paragraph') {
      // Append to existing paragraph
      current.items[0] += '\n' + line;
    } else {
      flushCurrent();
      current = { kind: 'paragraph', items: [line] };
    }
  }

  flushCurrent();
  return blocks;
}

// ─────────────────────────────────────────────────────────────────────────────
// Component
// ─────────────────────────────────────────────────────────────────────────────

export function BotMessageContent({ content }: BotMessageContentProps) {
  const rendered = useMemo(() => {
    const blocks = parseBlocks(content);

    return blocks.map((block, idx) => {
      switch (block.kind) {
        case 'ordered-list':
          return (
            <ol key={`ol-${idx}`} className="my-1.5 list-decimal space-y-0.5 pl-5 text-sm">
              {block.items.map((item, i) => (
                <li key={i}>{renderInline(item)}</li>
              ))}
            </ol>
          );

        case 'unordered-list':
          return (
            <ul key={`ul-${idx}`} className="my-1.5 list-disc space-y-0.5 pl-5 text-sm">
              {block.items.map((item, i) => (
                <li key={i}>{renderInline(item)}</li>
              ))}
            </ul>
          );

        case 'paragraph':
        default: {
          const text = block.items[0];
          // Split inner newlines into <br>
          const parts = text.split('\n');
          return (
            <p key={`p-${idx}`} className="my-1 first:mt-0 last:mb-0">
              {parts.map((part, i) => (
                <React.Fragment key={i}>
                  {i > 0 && <br />}
                  {renderInline(part)}
                </React.Fragment>
              ))}
            </p>
          );
        }
      }
    });
  }, [content]);

  return <div className="text-sm leading-relaxed break-words">{rendered}</div>;
}
