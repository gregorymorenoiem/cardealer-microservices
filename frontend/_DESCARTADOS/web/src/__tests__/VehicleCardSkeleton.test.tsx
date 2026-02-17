import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import VehicleCardSkeleton from '../components/organisms/VehicleCardSkeleton';

describe('VehicleCardSkeleton', () => {
  it('should render without crashing', () => {
    const { container } = render(<VehicleCardSkeleton />);
    expect(container.firstChild).toBeInTheDocument();
  });

  it('should have animate-pulse class', () => {
    const { container } = render(<VehicleCardSkeleton />);
    const skeleton = container.firstChild as HTMLElement;
    expect(skeleton).toHaveClass('animate-pulse');
  });

  it('should render all skeleton elements', () => {
    const { container } = render(<VehicleCardSkeleton />);
    
    // Check for image skeleton
    const imageSkeleton = container.querySelector('.h-48.bg-gray-300');
    expect(imageSkeleton).toBeInTheDocument();
    
    // Check for content skeletons
    const contentSkeletons = container.querySelectorAll('.bg-gray-300');
    expect(contentSkeletons.length).toBeGreaterThan(1);
  });
});
