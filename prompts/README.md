# Prompt Templates

This folder contains prompt templates for GitHub Copilot / Copilot Chat adapted to this repository.

Structure:

- `core/`: main workflows (audit, refactor, feature, execute)
- `extras/`: debug, tests, reviews, docs, performance, migrations
- `packages/`: combined premium packages

Usage:

1. In Copilot Chat, reference a file as context: `#file:prompts/core/audit_oneshot.md` or use `#codebase`.
2. Use `.vscode/settings.json` to ensure Copilot prioritizes `.github/copilot-instructions.md`.

Examples:

- Audit the codebase (global):

```
Revisa #codebase con foco en seguridad y CI/CD. Usa la plantilla: #file:prompts/core/audit_oneshot.md
```

- Quick fix a un archivo específico:

```
Aplica /fix a #file:src/services/auth.service.ts usando la plantilla #file:prompts/extras/debug_oneshot.md
```

Metadata policy:

- Each template includes minimal YAML metadata at the top: `version`, `lastUpdated`, `author`.
