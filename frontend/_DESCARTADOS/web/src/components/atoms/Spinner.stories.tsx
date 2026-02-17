import type { Meta, StoryObj } from '@storybook/react';
import Spinner from './Spinner';

/**
 * Spinner component for loading states.
 * Supports multiple sizes and colors.
 */
const meta: Meta<typeof Spinner> = {
  title: 'Atoms/Spinner',
  component: Spinner,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'A simple spinner component for indicating loading states.',
      },
    },
  },
  tags: ['autodocs'],
  argTypes: {
    size: {
      control: 'select',
      options: ['sm', 'md', 'lg', 'xl'],
      description: 'Size of the spinner',
    },
    color: {
      control: 'select',
      options: ['primary', 'secondary', 'white', 'gray'],
      description: 'Color of the spinner',
    },
  },
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Default medium primary spinner
 */
export const Default: Story = {
  args: {
    size: 'md',
    color: 'primary',
  },
};

/**
 * Small spinner
 */
export const Small: Story = {
  args: {
    size: 'sm',
    color: 'primary',
  },
};

/**
 * Large spinner
 */
export const Large: Story = {
  args: {
    size: 'lg',
    color: 'primary',
  },
};

/**
 * Extra large spinner
 */
export const ExtraLarge: Story = {
  args: {
    size: 'xl',
    color: 'primary',
  },
};

/**
 * Secondary color spinner
 */
export const Secondary: Story = {
  args: {
    size: 'md',
    color: 'secondary',
  },
};

/**
 * Gray spinner
 */
export const Gray: Story = {
  args: {
    size: 'md',
    color: 'gray',
  },
};

/**
 * White spinner (on dark background)
 */
export const White: Story = {
  args: {
    size: 'md',
    color: 'white',
  },
  decorators: [
    (Story) => (
      <div className="bg-gray-800 p-4 rounded-lg">
        <Story />
      </div>
    ),
  ],
};

/**
 * All sizes showcase
 */
export const AllSizes: Story = {
  render: () => (
    <div className="flex items-center gap-8">
      <div className="flex flex-col items-center gap-2">
        <Spinner size="sm" />
        <span className="text-sm text-gray-500">Small</span>
      </div>
      <div className="flex flex-col items-center gap-2">
        <Spinner size="md" />
        <span className="text-sm text-gray-500">Medium</span>
      </div>
      <div className="flex flex-col items-center gap-2">
        <Spinner size="lg" />
        <span className="text-sm text-gray-500">Large</span>
      </div>
      <div className="flex flex-col items-center gap-2">
        <Spinner size="xl" />
        <span className="text-sm text-gray-500">Extra Large</span>
      </div>
    </div>
  ),
};

/**
 * All colors showcase
 */
export const AllColors: Story = {
  render: () => (
    <div className="flex items-center gap-8">
      <div className="flex flex-col items-center gap-2">
        <Spinner color="primary" />
        <span className="text-sm text-gray-500">Primary</span>
      </div>
      <div className="flex flex-col items-center gap-2">
        <Spinner color="secondary" />
        <span className="text-sm text-gray-500">Secondary</span>
      </div>
      <div className="flex flex-col items-center gap-2">
        <Spinner color="gray" />
        <span className="text-sm text-gray-500">Gray</span>
      </div>
      <div className="flex flex-col items-center gap-2 bg-gray-800 p-4 rounded-lg">
        <Spinner color="white" />
        <span className="text-sm text-gray-300">White</span>
      </div>
    </div>
  ),
};

/**
 * Spinner in loading context
 */
export const InContext: Story = {
  render: () => (
    <div className="flex flex-col gap-4">
      <div className="flex items-center gap-2">
        <Spinner size="sm" />
        <span>Loading vehicles...</span>
      </div>
      <div className="flex items-center justify-center bg-gray-100 rounded-lg p-8">
        <div className="flex flex-col items-center gap-2">
          <Spinner size="lg" />
          <span className="text-gray-600">Please wait</span>
        </div>
      </div>
    </div>
  ),
};
