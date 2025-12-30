/**
 * Lint-Staged Configuration
 * 
 * Runs linting and formatting on staged files before commit.
 */

export default {
  // TypeScript and JavaScript files
  '*.{ts,tsx,js,jsx}': [
    'eslint --fix --max-warnings=0',
    'prettier --write',
  ],
  
  // JSON files
  '*.json': [
    'prettier --write',
  ],
  
  // Markdown files
  '*.md': [
    'prettier --write',
  ],
  
  // CSS files
  '*.{css,scss}': [
    'prettier --write',
  ],
  
  // HTML files
  '*.html': [
    'prettier --write',
  ],
};
