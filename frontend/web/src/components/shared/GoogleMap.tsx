/**
 * Google Map Component
 * Interactive map with marker for location selection
 */

import { useCallback, useState } from 'react';
import { GoogleMap, Marker, useJsApiLoader } from '@react-google-maps/api';
import { MapPin, Loader2, AlertCircle } from 'lucide-react';

interface GoogleMapComponentProps {
  latitude: number;
  longitude: number;
  onLocationChange: (lat: number, lng: number) => void;
  address?: string;
}

const mapContainerStyle = {
  width: '100%',
  height: '400px',
  borderRadius: '0.5rem',
};

const defaultCenter = {
  lat: 18.4861, // Santo Domingo
  lng: -69.9312,
};

export const GoogleMapComponent: React.FC<GoogleMapComponentProps> = ({
  latitude,
  longitude,
  onLocationChange,
  address,
}) => {
  const [map, setMap] = useState<google.maps.Map | null>(null);
  const [markerPosition, setMarkerPosition] = useState({
    lat: latitude || defaultCenter.lat,
    lng: longitude || defaultCenter.lng,
  });

  // Get API key from environment with fallback
  const apiKey = import.meta.env.VITE_GOOGLE_MAPS_KEY || 'AIzaSyBHYPOPdUI45VrntYnM173q19BYVQ7k4nY';

  const { isLoaded, loadError } = useJsApiLoader({
    id: 'google-map-script',
    googleMapsApiKey: apiKey,
  });

  const onLoad = useCallback((map: google.maps.Map) => {
    setMap(map);
  }, []);

  const onUnmount = useCallback(() => {
    setMap(null);
  }, []);

  const onMapClick = useCallback(
    (e: google.maps.MapMouseEvent) => {
      if (e.latLng) {
        const lat = e.latLng.lat();
        const lng = e.latLng.lng();
        setMarkerPosition({ lat, lng });
        onLocationChange(lat, lng);
      }
    },
    [onLocationChange]
  );

  const onMarkerDragEnd = useCallback(
    (e: google.maps.MapMouseEvent) => {
      if (e.latLng) {
        const lat = e.latLng.lat();
        const lng = e.latLng.lng();
        setMarkerPosition({ lat, lng });
        onLocationChange(lat, lng);
      }
    },
    [onLocationChange]
  );

  if (!apiKey) {
    return (
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <div className="flex items-start gap-3">
          <AlertCircle className="h-5 w-5 text-yellow-600 flex-shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-semibold text-yellow-900 mb-1">
              API Key de Google Maps No Configurada
            </h4>
            <p className="text-sm text-yellow-800 mb-2">
              Para usar el mapa interactivo, configura tu API key de Google Maps:
            </p>
            <ol className="text-xs text-yellow-700 list-decimal list-inside space-y-1">
              <li>Crea un archivo <code className="bg-yellow-100 px-1 py-0.5 rounded">.env</code> en la raíz del proyecto</li>
              <li>Agrega: <code className="bg-yellow-100 px-1 py-0.5 rounded">VITE_GOOGLE_MAPS_KEY=tu-api-key</code></li>
              <li>Reinicia el servidor de desarrollo</li>
            </ol>
            <p className="text-xs text-yellow-700 mt-2">
              Mientras tanto, puedes ingresar las coordenadas manualmente abajo.
            </p>
          </div>
        </div>
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-start gap-3">
          <AlertCircle className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-semibold text-red-900 mb-1">
              Error al Cargar el Mapa
            </h4>
            <p className="text-sm text-red-800">
              No se pudo cargar Google Maps. Verifica tu conexión a internet y tu API key.
            </p>
          </div>
        </div>
      </div>
    );
  }

  if (!isLoaded) {
    return (
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-8 flex flex-col items-center justify-center">
        <Loader2 className="h-8 w-8 text-blue-600 animate-spin mb-2" />
        <p className="text-sm text-gray-600">Cargando mapa...</p>
      </div>
    );
  }

  return (
    <div>
      <GoogleMap
        mapContainerStyle={mapContainerStyle}
        center={markerPosition}
        zoom={15}
        onClick={onMapClick}
        onLoad={onLoad}
        onUnmount={onUnmount}
        options={{
          streetViewControl: false,
          mapTypeControl: true,
          fullscreenControl: true,
          zoomControl: true,
        }}
      >
        <Marker
          position={markerPosition}
          draggable={true}
          onDragEnd={onMarkerDragEnd}
          title={address || 'Ubicación del Dealer'}
        />
      </GoogleMap>

      <div className="mt-3 p-3 bg-blue-50 rounded-lg border border-blue-200">
        <p className="text-xs text-blue-800 flex items-start gap-2">
          <MapPin className="h-4 w-4 flex-shrink-0 mt-0.5" />
          <span>
            <strong>Consejo:</strong> Haz clic en el mapa o arrastra el marcador para establecer tu ubicación exacta.
            Las coordenadas se actualizarán automáticamente.
          </span>
        </p>
      </div>
    </div>
  );
};
