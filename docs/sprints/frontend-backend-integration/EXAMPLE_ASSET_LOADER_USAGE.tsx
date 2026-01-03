// ==============================================================================
// Ejemplo: Uso del assetLoader en HomePage
// Archivo: frontend/web/src/pages/HomePage.tsx (fragmento de ejemplo)
// ==============================================================================

import { getAssetUrl, preloadImages } from '@/utils/assetLoader';

const HomePage = () => {
  const featuredVehicles = [
    {
      id: 1,
      name: 'Mercedes-Benz Clase C AMG 2024',
      // ❌ ANTES: URL externa o local
      // image: 'https://images.unsplash.com/photo-1234567890/car.jpg',
      
      // ✅ DESPUÉS: Desde S3 con helper
      image: 'images/vehicles/mercedes-c-class.jpg',
      price: 85000
    },
    {
      id: 2,
      name: 'BMW Serie 3 2023',
      image: 'images/vehicles/bmw-3-series.jpg',
      price: 65000
    }
  ];
  
  useEffect(() => {
    // Precargar imágenes para mejor performance
    const imagePaths = featuredVehicles.map(v => v.image);
    preloadImages(imagePaths.map(p => getAssetUrl(p)))
      .catch(err => console.error('Error precargando imágenes:', err));
  }, []);
  
  return (
    <div className="container">
      <h1>Vehículos Destacados</h1>
      
      <div className="grid grid-cols-3 gap-6">
        {featuredVehicles.map(vehicle => (
          <div key={vehicle.id} className="vehicle-card">
            {/* Usar getAssetUrl() para obtener URL de S3 */}
            <img
              src={getAssetUrl(vehicle.image)}
              alt={vehicle.name}
              className="w-full h-48 object-cover rounded"
              loading="lazy"
            />
            
            <h3>{vehicle.name}</h3>
            <p className="text-2xl font-bold">
              ${vehicle.price.toLocaleString()}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
};

export default HomePage;

// ==============================================================================
// Resultado: Las imágenes se cargan desde:
// https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/images/vehicles/...
//
// Beneficios:
// - ✅ Carga rápida desde S3
// - ✅ Sin dependencia de servicios externos
// - ✅ Preload automático para mejor UX
// - ✅ Escalable con CDN
// ==============================================================================
