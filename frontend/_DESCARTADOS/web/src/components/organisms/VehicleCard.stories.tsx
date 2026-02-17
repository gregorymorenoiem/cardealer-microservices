import type { Meta, StoryObj } from '@storybook/react';
import { MemoryRouter } from 'react-router-dom';
import VehicleCard, { type VehicleCardProps } from './VehicleCard';

/**
 * VehicleCard component displays a vehicle listing in a card format.
 * Shows image, details, badges (featured/new), and action buttons (favorite/compare).
 */
const meta = {
  title: 'Organisms/VehicleCard',
  component: VehicleCard,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'A card component for displaying vehicle listings with image, details, and action buttons.',
      },
    },
  },
  tags: ['autodocs'],
  argTypes: {
    id: { control: 'text' },
    make: { control: 'text' },
    model: { control: 'text' },
    year: { control: { type: 'number', min: 1990, max: 2025 } },
    price: { control: { type: 'number', min: 0 } },
    mileage: { control: { type: 'number', min: 0 } },
    location: { control: 'text' },
    imageUrl: { control: 'text' },
    isFeatured: { control: 'boolean' },
    isNew: { control: 'boolean' },
    transmission: { control: 'select', options: ['Automatic', 'Manual', 'CVT'] },
    fuelType: { control: 'select', options: ['Gasoline', 'Diesel', 'Electric', 'Hybrid'] },
  },
  decorators: [
    (Story) => (
      <MemoryRouter>
        <div className="w-[320px]">
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
} satisfies Meta<typeof VehicleCard>;

export default meta;
type Story = StoryObj<typeof meta>;

const baseVehicle: VehicleCardProps = {
  id: '1',
  make: 'Toyota',
  model: 'Camry',
  year: 2023,
  price: 28500,
  mileage: 15000,
  location: 'Santo Domingo',
  imageUrl: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400&h=300&fit=crop',
  transmission: 'Automatic',
  fuelType: 'Gasoline',
};

/**
 * Default vehicle card
 */
export const Default: Story = {
  args: baseVehicle,
};

/**
 * Featured vehicle with badge
 */
export const Featured: Story = {
  args: {
    ...baseVehicle,
    isFeatured: true,
  },
};

/**
 * New vehicle with badge
 */
export const New: Story = {
  args: {
    ...baseVehicle,
    isNew: true,
    mileage: 500,
  },
};

/**
 * Featured and new vehicle
 */
export const FeaturedAndNew: Story = {
  args: {
    ...baseVehicle,
    isFeatured: true,
    isNew: true,
    mileage: 0,
  },
};

/**
 * Luxury vehicle
 */
export const LuxuryVehicle: Story = {
  args: {
    id: '2',
    make: 'BMW',
    model: 'M5 Competition',
    year: 2024,
    price: 125000,
    mileage: 2500,
    location: 'Punta Cana',
    imageUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400&h=300&fit=crop',
    isFeatured: true,
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
};

/**
 * Electric vehicle
 */
export const ElectricVehicle: Story = {
  args: {
    id: '3',
    make: 'Tesla',
    model: 'Model 3',
    year: 2024,
    price: 45000,
    mileage: 8000,
    location: 'Santiago',
    imageUrl: 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=400&h=300&fit=crop',
    isNew: true,
    transmission: 'Automatic',
    fuelType: 'Electric',
  },
};

/**
 * Used vehicle with high mileage
 */
export const HighMileage: Story = {
  args: {
    id: '4',
    make: 'Honda',
    model: 'Accord',
    year: 2018,
    price: 15500,
    mileage: 145000,
    location: 'La Romana',
    imageUrl: 'https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=400&h=300&fit=crop',
    transmission: 'Automatic',
    fuelType: 'Gasoline',
  },
};

/**
 * Vehicle without image (shows placeholder)
 */
export const NoImage: Story = {
  args: {
    ...baseVehicle,
    imageUrl: undefined,
  },
};

/**
 * Affordable budget vehicle
 */
export const BudgetVehicle: Story = {
  args: {
    id: '5',
    make: 'Hyundai',
    model: 'Accent',
    year: 2020,
    price: 12000,
    mileage: 55000,
    location: 'Puerto Plata',
    imageUrl: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=400&h=300&fit=crop',
    transmission: 'Manual',
    fuelType: 'Gasoline',
  },
};

/**
 * Grid of vehicle cards
 */
export const CardGrid: Story = {
  args: baseVehicle,
  decorators: [
    () => (
      <MemoryRouter>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-4 max-w-5xl">
          <VehicleCard {...baseVehicle} />
          <VehicleCard 
            {...baseVehicle} 
            id="2" 
            make="Honda" 
            model="Civic" 
            isFeatured 
            imageUrl="https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400&h=300&fit=crop"
          />
          <VehicleCard 
            {...baseVehicle} 
            id="3" 
            make="Tesla" 
            model="Model Y" 
            isNew 
            price={52000}
            imageUrl="https://images.unsplash.com/photo-1619317304296-f9ac0fd37cf8?w=400&h=300&fit=crop"
          />
          <VehicleCard 
            {...baseVehicle} 
            id="4" 
            make="Mercedes" 
            model="C-Class" 
            isFeatured
            isNew
            price={48000}
            imageUrl="https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400&h=300&fit=crop"
          />
          <VehicleCard {...baseVehicle} id="5" make="Ford" model="Mustang" price={45000} />
          <VehicleCard {...baseVehicle} id="6" make="Chevrolet" model="Camaro" price={42000} />
        </div>
      </MemoryRouter>
    ),
  ],
};
