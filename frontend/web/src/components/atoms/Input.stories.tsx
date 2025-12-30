import type { Meta, StoryObj } from '@storybook/react';
import { fn } from '@storybook/test';
import { FiSearch, FiMail, FiLock, FiEye, FiDollarSign } from 'react-icons/fi';
import Input from './Input';

/**
 * Input component for text entry.
 * Supports labels, icons, validation states, and helper text.
 */
const meta: Meta<typeof Input> = {
  title: 'Atoms/Input',
  component: Input,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'A flexible input component with support for labels, icons, and validation states.',
      },
    },
  },
  tags: ['autodocs'],
  argTypes: {
    label: {
      control: 'text',
      description: 'Label text displayed above the input',
    },
    error: {
      control: 'text',
      description: 'Error message to display',
    },
    helperText: {
      control: 'text',
      description: 'Helper text displayed below the input',
    },
    fullWidth: {
      control: 'boolean',
      description: 'Makes input take full width of container',
    },
    disabled: {
      control: 'boolean',
      description: 'Disables the input',
    },
    required: {
      control: 'boolean',
      description: 'Marks the field as required',
    },
  },
  args: {
    onChange: fn(),
    onBlur: fn(),
    onFocus: fn(),
  },
  decorators: [
    (Story) => (
      <div style={{ width: '300px' }}>
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Default input without label
 */
export const Default: Story = {
  args: {
    placeholder: 'Enter text...',
  },
};

/**
 * Input with label
 */
export const WithLabel: Story = {
  args: {
    label: 'Email Address',
    placeholder: 'john@example.com',
    type: 'email',
  },
};

/**
 * Required input with label
 */
export const Required: Story = {
  args: {
    label: 'Full Name',
    placeholder: 'John Doe',
    required: true,
  },
};

/**
 * Input with error state
 */
export const WithError: Story = {
  args: {
    label: 'Email',
    placeholder: 'Enter email',
    error: 'Please enter a valid email address',
    defaultValue: 'invalid-email',
  },
};

/**
 * Input with helper text
 */
export const WithHelperText: Story = {
  args: {
    label: 'Password',
    type: 'password',
    placeholder: 'Enter password',
    helperText: 'Must be at least 8 characters',
  },
};

/**
 * Input with left icon
 */
export const WithLeftIcon: Story = {
  args: {
    label: 'Search',
    placeholder: 'Search vehicles...',
    leftIcon: <FiSearch size={18} />,
  },
};

/**
 * Input with right icon
 */
export const WithRightIcon: Story = {
  args: {
    label: 'Password',
    type: 'password',
    placeholder: 'Enter password',
    rightIcon: <FiEye size={18} />,
  },
};

/**
 * Input with both icons
 */
export const WithBothIcons: Story = {
  args: {
    label: 'Price',
    placeholder: '0.00',
    type: 'number',
    leftIcon: <FiDollarSign size={18} />,
    rightIcon: <span className="text-gray-400 text-sm">USD</span>,
  },
};

/**
 * Disabled input
 */
export const Disabled: Story = {
  args: {
    label: 'Email',
    placeholder: 'Cannot edit',
    disabled: true,
    defaultValue: 'disabled@example.com',
  },
};

/**
 * Email input type
 */
export const EmailInput: Story = {
  args: {
    label: 'Email',
    type: 'email',
    placeholder: 'you@example.com',
    leftIcon: <FiMail size={18} />,
  },
};

/**
 * Password input type
 */
export const PasswordInput: Story = {
  args: {
    label: 'Password',
    type: 'password',
    placeholder: '••••••••',
    leftIcon: <FiLock size={18} />,
  },
};

/**
 * Full width input
 */
export const FullWidth: Story = {
  args: {
    label: 'Description',
    placeholder: 'Enter a detailed description...',
    fullWidth: true,
  },
  decorators: [
    (Story) => (
      <div style={{ width: '500px' }}>
        <Story />
      </div>
    ),
  ],
};

/**
 * Input states showcase
 */
export const AllStates: Story = {
  render: () => (
    <div className="flex flex-col gap-4">
      <Input label="Default" placeholder="Normal input" />
      <Input label="With Value" defaultValue="Some text" />
      <Input label="Disabled" disabled defaultValue="Cannot edit" />
      <Input label="With Error" error="This field is required" />
      <Input label="With Helper" helperText="Additional information" />
    </div>
  ),
  decorators: [
    (Story) => (
      <div style={{ width: '300px' }}>
        <Story />
      </div>
    ),
  ],
};
