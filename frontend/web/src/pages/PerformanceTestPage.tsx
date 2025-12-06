/**
 * Performance Test Page - Verificar que los componentes de rendimiento funcionen correctamente
 * Esta página solo es para pruebas en desarrollo
 * 
 * URL: /test/performance
 * 
 * ⚠️ IMPORTANTE: Esta ruta debe ser removida o protegida antes de producción
 * 
 * Componentes probados:
 * - OptimizedImage (lazy loading con blur placeholder)
 * - ImageGallery (galería con lazy loading)
 * - SkeletonLoaders (CardSkeleton, ListItemSkeleton, CardGridSkeleton)
 * - BottomNavigation (navegación móvil)
 * - SwipeableCarousel (carrusel con gestos)
 * - PullToRefresh (pull to refresh)
 * - SEO, VehicleSEO, OrganizationSchema (meta tags y JSON-LD)
 * - Accessibility components (SkipLinks, VisuallyHidden, etc.)
 * - useNetworkStatus, useReducedMotion hooks
 */

import React, { useState } from 'react';
import { OptimizedImage, ImageGallery, CardSkeleton, CardGridSkeleton, ListItemSkeleton } from '@/components/performance';
import { BottomNavigation, SwipeableCarousel, PullToRefresh } from '@/components/mobile';
import { SEO, VehicleSEO, OrganizationSchema, SearchActionSchema } from '@/components/seo';
import { SkipLinks, VisuallyHidden, LoadingAnnouncer, AccessibleImage } from '@/components/a11y';
import { useNetworkStatus, useReducedMotion } from '@/hooks/usePerformance';

const PerformanceTestPage: React.FC = () => {
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [showSkeletons, setShowSkeletons] = useState(true);
  const { effectiveType, saveData, isOnline } = useNetworkStatus();
  const prefersReducedMotion = useReducedMotion();

  // Test images
  const testImages = [
    { url: 'https://picsum.photos/800/600?random=1', alt: 'Test Image 1' },
    { url: 'https://picsum.photos/800/600?random=2', alt: 'Test Image 2' },
    { url: 'https://picsum.photos/800/600?random=3', alt: 'Test Image 3' },
  ];

  // Mock vehicle for VehicleSEO
  const mockVehicle = {
    id: 'test-123',
    title: 'Toyota Corolla 2023',
    description: 'Un vehículo en excelentes condiciones',
    price: 850000,
    currency: 'DOP',
    images: [{ 
      id: '1',
      url: 'https://picsum.photos/800/600', 
      thumbnailUrl: 'https://picsum.photos/200/150',
      sortOrder: 1,
      isPrimary: true,
    }],
    primaryImageUrl: 'https://picsum.photos/800/600',
    location: { city: 'Santo Domingo', state: 'DN', country: 'DO', address: '', zipCode: '' },
    vertical: 'vehicles' as const,
    sellerId: 'seller-1',
    dealerId: 'dealer-1',
    status: 'active' as const,
    listingType: 'sale' as const,
    isFeatured: false,
    viewCount: 100,
    favoriteCount: 10,
    inquiryCount: 5,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    seller: {
      id: 'seller-1',
      name: 'Test Seller',
      isVerified: true,
      isDealership: false,
    },
    // Vehicle specific
    vehicleType: 'car' as const,
    make: 'Toyota',
    model: 'Corolla',
    year: 2023,
    mileage: 15000,
    mileageUnit: 'km' as const,
    transmission: 'automatic' as const,
    fuelType: 'gasoline' as const,
    bodyType: 'sedan' as const,
    exteriorColor: 'Blanco',
    condition: 'used' as const,
    drivetrain: 'fwd' as const,
    features: ['Aire acondicionado', 'Bluetooth', 'Cámara de reversa'],
  };

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await new Promise(resolve => setTimeout(resolve, 2000));
    setIsRefreshing(false);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* SEO Components */}
      <SEO 
        title="Prueba de Rendimiento"
        description="Página de prueba para componentes de rendimiento"
      />
      <OrganizationSchema />
      <SearchActionSchema />

      {/* Accessibility Components */}
      <SkipLinks 
        links={[
          { id: 'main-content', label: 'Saltar al contenido principal' },
          { id: 'network-status', label: 'Ver estado de conexión' },
        ]}
      />

      <main id="main-content" className="container mx-auto px-4 py-8 pb-24">
        <h1 className="text-3xl font-bold mb-8">🧪 Prueba de Componentes de Rendimiento</h1>

        {/* Network Status */}
        <section id="network-status" className="mb-8 p-4 bg-white rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">📡 Estado de Conexión</h2>
          <ul className="space-y-2">
            <li>🌐 En línea: <strong>{isOnline ? '✅ Sí' : '❌ No'}</strong></li>
            <li>📶 Tipo de conexión: <strong>{effectiveType}</strong></li>
            <li>💾 Ahorro de datos: <strong>{saveData ? '✅ Activo' : '❌ Inactivo'}</strong></li>
            <li>🎭 Movimiento reducido: <strong>{prefersReducedMotion ? '✅ Sí' : '❌ No'}</strong></li>
          </ul>
        </section>

        {/* OptimizedImage Test */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">🖼️ OptimizedImage (Lazy Loading)</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {testImages.map((img, idx) => (
              <OptimizedImage
                key={idx}
                src={img.url}
                alt={img.alt}
                className="w-full h-48 rounded-lg"
                priority={idx === 0}
                placeholder="blur"
              />
            ))}
          </div>
        </section>

        {/* ImageGallery Test */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">📷 ImageGallery</h2>
          <ImageGallery 
            images={testImages}
            className="max-w-2xl"
          />
        </section>

        {/* Skeleton Loaders */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">💀 Skeleton Loaders</h2>
          <button 
            onClick={() => setShowSkeletons(!showSkeletons)}
            className="mb-4 px-4 py-2 bg-blue-500 text-white rounded-lg"
          >
            {showSkeletons ? 'Ocultar' : 'Mostrar'} Skeletons
          </button>
          {showSkeletons && (
            <div className="space-y-8">
              <div>
                <h3 className="text-lg font-medium mb-2">CardSkeleton</h3>
                <CardSkeleton />
              </div>
              <div>
                <h3 className="text-lg font-medium mb-2">ListItemSkeleton (x3)</h3>
                <div className="space-y-2">
                  <ListItemSkeleton />
                  <ListItemSkeleton />
                  <ListItemSkeleton />
                </div>
              </div>
              <div>
                <h3 className="text-lg font-medium mb-2">CardGridSkeleton (4 columnas)</h3>
                <CardGridSkeleton count={4} columns={2} />
              </div>
            </div>
          )}
        </section>

        {/* SwipeableCarousel Test */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">📱 SwipeableCarousel</h2>
          <SwipeableCarousel
            items={testImages}
            renderItem={(_, idx, isActive) => (
              <div className={`w-full h-64 flex items-center justify-center bg-gradient-to-br from-blue-400 to-purple-500 text-white text-2xl font-bold rounded-lg transition-opacity ${isActive ? 'opacity-100' : 'opacity-70'}`}>
                Slide {idx + 1}
              </div>
            )}
            autoPlay={false}
            showDots
          />
        </section>

        {/* Pull to Refresh Test */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">🔄 Pull to Refresh</h2>
          <PullToRefresh 
            onRefresh={handleRefresh}
          >
            <div className="p-4 bg-white rounded-lg text-center">
              <p>⬇️ Arrastra hacia abajo para refrescar</p>
              <p className="text-sm text-gray-500 mt-2">
                {isRefreshing ? '⏳ Refrescando...' : '✅ Listo'}
              </p>
            </div>
          </PullToRefresh>
        </section>

        {/* VehicleSEO Test */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">🔍 VehicleSEO (JSON-LD)</h2>
          <VehicleSEO 
            vehicle={mockVehicle}
            dealerName="CarDealer RD"
            dealerPhone="+1-809-555-0123"
          />
          <p className="text-sm text-gray-500">
            (Revisa el código fuente de la página para ver el JSON-LD inyectado)
          </p>
        </section>

        {/* Accessibility Components */}
        <section className="mb-8">
          <h2 className="text-xl font-semibold mb-4">♿ Componentes de Accesibilidad</h2>
          <div className="space-y-4">
            <div>
              <h3 className="font-medium">VisuallyHidden</h3>
              <VisuallyHidden>Este texto solo es visible para screen readers</VisuallyHidden>
              <p className="text-sm text-gray-500">(Hay texto oculto arriba, solo visible para screen readers)</p>
            </div>
            <div>
              <h3 className="font-medium">AccessibleImage</h3>
              <AccessibleImage
                src="https://picsum.photos/200/100"
                alt="Imagen accesible de prueba con descripción completa"
                className="w-48 h-24 rounded-lg"
              />
            </div>
            <div>
              <h3 className="font-medium">LoadingAnnouncer</h3>
              <LoadingAnnouncer isLoading={isRefreshing} loadingMessage="Cargando contenido..." />
            </div>
          </div>
        </section>

        <div className="h-32" /> {/* Space for bottom nav */}
      </main>

      {/* Bottom Navigation */}
      <BottomNavigation />
    </div>
  );
};

export default PerformanceTestPage;
