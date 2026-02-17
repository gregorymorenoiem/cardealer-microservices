import type { Meta, StoryObj } from '@storybook/react';
import { fn } from '@storybook/test';
import SearchBar, { type SearchFilters } from './SearchBar';

/**
 * SearchBar component for vehicle search with multiple filter options.
 * Allows users to search by make, model, year range, and price range.
 */
const meta = {
  title: 'Molecules/SearchBar',
  component: SearchBar,
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        component: 'A comprehensive search bar for filtering vehicles by make, model, year, and price.',
      },
    },
  },
  tags: ['autodocs'],
  argTypes: {
    onSearch: { action: 'searched' },
    className: { control: 'text' },
  },
  decorators: [
    (Story) => (
      <div className="bg-gray-100 p-8 min-h-[300px]">
        <Story />
      </div>
    ),
  ],
} satisfies Meta<typeof SearchBar>;

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Default search bar with all filter options
 */
export const Default: Story = {
  args: {
    onSearch: fn((filters: SearchFilters) => console.log('Search:', filters)),
  },
};

/**
 * Search bar with custom styling
 */
export const WithCustomClass: Story = {
  args: {
    onSearch: fn((filters: SearchFilters) => console.log('Search:', filters)),
    className: 'border-2 border-primary-500',
  },
};

/**
 * Search bar in a dark container (to show contrast)
 */
export const OnDarkBackground: Story = {
  args: {
    onSearch: fn((filters: SearchFilters) => console.log('Search:', filters)),
  },
  decorators: [
    (Story) => (
      <div className="bg-gray-800 p-8 min-h-[300px]">
        <Story />
      </div>
    ),
  ],
};

/**
 * Compact search bar in narrow container
 */
export const CompactWidth: Story = {
  args: {
    onSearch: fn((filters: SearchFilters) => console.log('Search:', filters)),
  },
  decorators: [
    (Story) => (
      <div className="bg-gray-100 p-8 max-w-md">
        <Story />
      </div>
    ),
  ],
};

/**
 * Full width search bar
 */
export const FullWidth: Story = {
  args: {
    onSearch: fn((filters: SearchFilters) => console.log('Search:', filters)),
  },
  decorators: [
    (Story) => (
      <div className="bg-gray-100 p-8 w-full">
        <Story />
      </div>
    ),
  ],
};
