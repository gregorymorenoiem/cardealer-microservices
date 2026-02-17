import type { Meta, StoryObj } from '@storybook/react';
import { fn } from '@storybook/test';
import Pagination from './Pagination';

/**
 * Pagination component for navigating through large datasets.
 * Shows page numbers, navigation arrows, and current results info.
 */
const meta = {
  title: 'Molecules/Pagination',
  component: Pagination,
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        component: 'A pagination component with page numbers, ellipsis for large ranges, and result count display.',
      },
    },
  },
  tags: ['autodocs'],
  argTypes: {
    currentPage: { control: { type: 'number', min: 1 } },
    totalPages: { control: { type: 'number', min: 1 } },
    totalItems: { control: { type: 'number', min: 0 } },
    itemsPerPage: { control: { type: 'number', min: 1 } },
    onPageChange: { action: 'pageChanged' },
  },
} satisfies Meta<typeof Pagination>;

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Default pagination with multiple pages
 */
export const Default: Story = {
  args: {
    currentPage: 1,
    totalPages: 10,
    totalItems: 100,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};

/**
 * Pagination at the middle of a large dataset
 */
export const MiddlePage: Story = {
  args: {
    currentPage: 5,
    totalPages: 10,
    totalItems: 100,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};

/**
 * Last page
 */
export const LastPage: Story = {
  args: {
    currentPage: 10,
    totalPages: 10,
    totalItems: 98,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};

/**
 * Few pages (no ellipsis needed)
 */
export const FewPages: Story = {
  args: {
    currentPage: 2,
    totalPages: 5,
    totalItems: 50,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};

/**
 * Many pages with ellipsis
 */
export const ManyPages: Story = {
  args: {
    currentPage: 50,
    totalPages: 100,
    totalItems: 1000,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};

/**
 * Single page (should not render)
 */
export const SinglePage: Story = {
  args: {
    currentPage: 1,
    totalPages: 1,
    totalItems: 5,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
  parameters: {
    docs: {
      description: {
        story: 'When there is only one page, the pagination component does not render.',
      },
    },
  },
};

/**
 * First page with large dataset
 */
export const FirstPageLargeDataset: Story = {
  args: {
    currentPage: 1,
    totalPages: 50,
    totalItems: 500,
    itemsPerPage: 10,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};

/**
 * Custom items per page
 */
export const CustomItemsPerPage: Story = {
  args: {
    currentPage: 3,
    totalPages: 20,
    totalItems: 500,
    itemsPerPage: 25,
    onPageChange: fn((page: number) => console.log('Page:', page)),
  },
};
