import type { Review, ReviewStats } from '@/types/review';

// Mock reviews data
export const mockReviews: Review[] = [
  {
    id: 'r1',
    vehicleId: '1',
    userId: 'u1',
    userName: 'Michael Torres',
    userAvatar: 'https://i.pravatar.cc/150?u=michael',
    rating: 5,
    title: 'Amazing Electric Vehicle!',
    comment: 'Best car I have ever owned. The acceleration is incredible, autopilot works flawlessly, and the build quality is top-notch. Battery life exceeds expectations even in cold weather. Customer service was excellent throughout the purchase process.',
    photos: [
      'https://placehold.co/400x300/1e40af/ffffff?text=Tesla+Photo+1',
      'https://placehold.co/400x300/1e40af/ffffff?text=Interior+View',
    ],
    pros: ['Incredible acceleration', 'Autopilot features', 'Zero emissions', 'Low maintenance'],
    cons: ['Charging infrastructure could be better in rural areas'],
    verifiedPurchase: true,
    helpful: 45,
    date: '2024-11-15',
  },
  {
    id: 'r2',
    vehicleId: '1',
    userId: 'u2',
    userName: 'Sarah Johnson',
    userAvatar: 'https://i.pravatar.cc/150?u=sarah',
    rating: 4,
    title: 'Great car, minor issues',
    comment: 'Overall very satisfied with the purchase. The car is fun to drive and the tech is impressive. Had a few minor software glitches that were resolved with updates. Range is good for daily commuting.',
    pros: ['Smooth ride', 'Great technology', 'Spacious interior'],
    cons: ['Some software bugs', 'Premium price'],
    verifiedPurchase: true,
    helpful: 28,
    date: '2024-10-22',
  },
  {
    id: 'r3',
    vehicleId: '2',
    userId: 'u3',
    userName: 'David Martinez',
    userAvatar: 'https://i.pravatar.cc/150?u=david',
    rating: 5,
    title: 'Perfect Luxury Sedan',
    comment: 'The BMW 3 Series exceeded all my expectations. The handling is phenomenal, interior is luxurious, and the technology package is worth every penny. Fuel efficiency is better than advertised.',
    photos: [
      'https://placehold.co/400x300/2563eb/ffffff?text=BMW+Interior',
    ],
    pros: ['Excellent handling', 'Luxurious interior', 'Advanced safety features', 'Good fuel economy'],
    cons: [],
    verifiedPurchase: true,
    helpful: 52,
    date: '2024-11-28',
  },
  {
    id: 'r4',
    vehicleId: '2',
    userId: 'u4',
    userName: 'Jennifer Lee',
    userAvatar: 'https://i.pravatar.cc/150?u=jennifer',
    rating: 4,
    title: 'Solid Performance',
    comment: 'Very happy with this BMW. Drives like a dream and looks stunning. Only wish the infotainment system was more intuitive. Dealer service has been excellent.',
    pros: ['Powerful engine', 'Beautiful design', 'Comfortable seats'],
    cons: ['Complex infotainment', 'Expensive maintenance'],
    verifiedPurchase: true,
    helpful: 19,
    date: '2024-10-05',
  },
  {
    id: 'r5',
    vehicleId: '4',
    userId: 'u5',
    userName: 'Robert Chen',
    userAvatar: 'https://i.pravatar.cc/150?u=robert',
    rating: 5,
    title: 'Pure Driving Excitement!',
    comment: 'The Mustang GT is everything I dreamed of. The V8 sound is incredible, handling is tight, and the performance package makes it a true sports car. Gets lots of compliments everywhere I go.',
    photos: [
      'https://placehold.co/400x300/dc2626/ffffff?text=Mustang+Action',
      'https://placehold.co/400x300/dc2626/ffffff?text=Engine+Bay',
      'https://placehold.co/400x300/dc2626/ffffff?text=Interior+Dash',
    ],
    pros: ['Raw power', 'Iconic design', 'Engaging manual transmission', 'Great exhaust note'],
    cons: ['Fuel consumption is high', 'Limited rear visibility'],
    verifiedPurchase: true,
    helpful: 67,
    date: '2024-11-10',
  },
  {
    id: 'r6',
    vehicleId: '4',
    userId: 'u6',
    userName: 'Amanda Wilson',
    userAvatar: 'https://i.pravatar.cc/150?u=amanda',
    rating: 4,
    title: 'Fun but not practical',
    comment: 'Love driving this car on weekends. It is a head-turner and super fun on winding roads. However, fuel economy and back seat space are not ideal for daily family use.',
    pros: ['Thrilling to drive', 'Beautiful styling', 'Great sound system'],
    cons: ['Poor fuel economy', 'Small back seat', 'Stiff ride'],
    verifiedPurchase: false,
    helpful: 23,
    date: '2024-09-18',
  },
  {
    id: 'r7',
    vehicleId: '3',
    userId: 'u7',
    userName: 'James Thompson',
    userAvatar: 'https://i.pravatar.cc/150?u=james',
    rating: 5,
    title: 'Best Hybrid on the Market',
    comment: 'The Camry Hybrid is incredibly efficient and reliable. Getting 50+ MPG consistently. Cabin is quiet, ride is smooth, and maintenance costs are minimal. Perfect for long commutes.',
    pros: ['Exceptional fuel economy', 'Reliable', 'Spacious', 'Comfortable ride'],
    cons: [],
    verifiedPurchase: true,
    helpful: 41,
    date: '2024-11-20',
  },
  {
    id: 'r8',
    vehicleId: '6',
    userId: 'u8',
    userName: 'Lisa Anderson',
    userAvatar: 'https://i.pravatar.cc/150?u=lisa',
    rating: 5,
    title: 'Luxury and Performance Combined',
    comment: 'The Audi A4 is a perfect blend of luxury and sportiness. Quattro AWD is amazing in snow. Virtual cockpit is stunning and Bang & Olufsen sound system is incredible.',
    photos: [
      'https://placehold.co/400x300/1e293b/ffffff?text=Audi+Cockpit',
    ],
    pros: ['Quattro AWD', 'Premium interior', 'Advanced technology', 'Great handling'],
    cons: ['Pricey options', 'Small trunk'],
    verifiedPurchase: true,
    helpful: 35,
    date: '2024-10-30',
  },
];

// Calculate review stats for a vehicle
export const getReviewStats = (vehicleId: string): ReviewStats => {
  const vehicleReviews = mockReviews.filter(r => r.vehicleId === vehicleId);
  
  const distribution = {
    5: vehicleReviews.filter(r => r.rating === 5).length,
    4: vehicleReviews.filter(r => r.rating === 4).length,
    3: vehicleReviews.filter(r => r.rating === 3).length,
    2: vehicleReviews.filter(r => r.rating === 2).length,
    1: vehicleReviews.filter(r => r.rating === 1).length,
  };

  const totalReviews = vehicleReviews.length;
  const averageRating = totalReviews > 0
    ? vehicleReviews.reduce((sum, r) => sum + r.rating, 0) / totalReviews
    : 0;

  return {
    averageRating,
    totalReviews,
    distribution,
  };
};

// Get reviews for a specific vehicle
export const getVehicleReviews = (vehicleId: string): Review[] => {
  return mockReviews.filter(r => r.vehicleId === vehicleId);
};
