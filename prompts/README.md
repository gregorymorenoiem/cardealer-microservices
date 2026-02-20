# Prompt Templates

This folder contains prompt templates for GitHub Copilot / Copilot Chat adapted to this repository.

Structure:

- `core/`: main workflows (audit, refactor, feature, execute)
- `extras/`: debug, tests, reviews, docs, performance, migrations
- `packages/`: combined premium packages

Usage:

1. In Copilot Chat, reference a file as context: `#file:prompts/core/audit_oneshot.md` or use `#codebase`.
2. Use `.vscode/settings.json` to ensure Copilot prioritizes `.github/copilot-instructions.md`.
