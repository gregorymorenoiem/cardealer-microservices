import type { Preview } from '@storybook/react';
import { withThemeByClassName } from '@storybook/addon-themes';
import '../src/index.css';

// Viewport definitions for responsive testing
const customViewports = {
  mobile: {
    name: 'Mobile',
    styles: {
      width: '375px',
      height: '667px',
    },
  },
  mobileLarge: {
    name: 'Mobile Large',
    styles: {
      width: '414px',
      height: '896px',
    },
  },
  tablet: {
    name: 'Tablet',
    styles: {
      width: '768px',
      height: '1024px',
    },
  },
  laptop: {
    name: 'Laptop',
    styles: {
      width: '1366px',
      height: '768px',
    },
  },
  desktop: {
    name: 'Desktop',
    styles: {
      width: '1920px',
      height: '1080px',
    },
  },
};

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    layout: 'centered',
    viewport: {
      viewports: customViewports,
    },
    backgrounds: {
      default: 'light',
      values: [
        {
          name: 'light',
          value: '#ffffff',
        },
        {
          name: 'gray',
          value: '#f5f5f5',
        },
        {
          name: 'dark',
          value: '#1a1a1a',
        },
      ],
    },
    a11y: {
      // Accessibility testing configuration
      element: '#storybook-root',
      config: {},
      options: {},
      manual: false,
    },
    docs: {
      toc: true,
    },
  },
  decorators: [
    // Theme decorator for light/dark mode
    withThemeByClassName({
      themes: {
        light: '',
        dark: 'dark',
      },
      defaultTheme: 'light',
    }),
    // Wrapper decorator for consistent styling
    (Story) => (
      <div className="font-sans antialiased">
        <Story />
      </div>
    ),
  ],
  tags: ['autodocs'],
};

export default preview;
