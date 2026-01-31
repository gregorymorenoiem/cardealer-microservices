/**
 * DealerVehicleEditPage - Edición de Vehículo
 *
 * Permite a los dealers editar la información de sus vehículos
 * Conectado al backend VehiclesSaleService
 */

import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiSave,
  FiArrowLeft,
  FiImage,
  FiDollarSign,
  FiMapPin,
  FiInfo,
  FiSettings,
  FiTrash2,
  FiPlus,
  FiX,
  FiLoader,
  FiAlertCircle,
  FiCheckCircle,
  FiEye,
  FiVideo,
  FiRotateCw,
  FiUpload,
  FiRefreshCw,
  FiExternalLink,
} from 'react-icons/fi';
import { getVehicleById, updateVehicle, type Vehicle } from '@/services/vehicleService';
import { generateListingUrl } from '@/utils/seoSlug';
import {
  startProcessing,
  getJobStatus,
  getVehicleViewer,
  retryJob,
  type Vehicle360JobStatus,
  type JobStatusResponse,
  type Vehicle360ViewerData,
} from '@/services/vehicle360Service';

type TabType = 'basic' | 'details' | 'images' | 'media360' | 'pricing';

export default function DealerVehicleEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const videoInputRef = useRef<HTMLInputElement>(null);

  const [vehicle, setVehicle] = useState<Vehicle | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('basic');

  // Media 360 state
  const [media360Data, setMedia360Data] = useState<Vehicle360ViewerData | null>(null);
  const [media360JobStatus, setMedia360JobStatus] = useState<JobStatusResponse | null>(null);
  const [isUploading360, setIsUploading360] = useState(false);
  const [upload360Progress, setUpload360Progress] = useState(0);
  const [selectedVideoFile, setSelectedVideoFile] = useState<File | null>(null);

  // Form state
  const [formData, setFormData] = useState({
    title: '',
    make: '',
    model: '',
    year: 2024,
    price: 0,
    mileage: 0,
    fuelType: 'Gasoline',
    transmission: 'Automatic',
    bodyType: 'Sedan',
    color: '',
    interiorColor: '',
    description: '',
    location: '',
    features: [] as string[],
    vin: '',
    condition: 'Used',
  });

  const [newFeature, setNewFeature] = useState('');

  // Fetch vehicle data
  useEffect(() => {
    const fetchVehicle = async () => {
      if (!id) {
        setError('ID de vehículo no proporcionado');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError(null);
        const data = await getVehicleById(id);
        setVehicle(data);

        // Populate form
        setFormData({
          title: data.title || `${data.year} ${data.make} ${data.model}`,
          make: data.make || '',
          model: data.model || '',
          year: data.year || 2024,
          price: data.price || 0,
          mileage: data.mileage || 0,
          fuelType: data.fuelType || 'Gasoline',
          transmission: data.transmission || 'Automatic',
          bodyType: data.bodyType || 'Sedan',
          color: data.color || '',
          interiorColor: data.interiorColor || '',
          description: data.description || '',
          location: data.location || '',
          features: data.features || [],
          vin: data.vin || '',
          condition: data.condition || 'Used',
        });
      } catch (err) {
        console.error('Error fetching vehicle:', err);
        setError('No se pudo cargar el vehículo');
      } finally {
        setIsLoading(false);
      }
    };

    fetchVehicle();
  }, [id]);

  // Fetch Media 360 data
  useEffect(() => {
    const fetchMedia360 = async () => {
      if (!id) return;
      try {
        const data = await getVehicleViewer(id);
        setMedia360Data(data);
      } catch {
        // No hay datos 360 para este vehículo, es normal
        setMedia360Data(null);
      }
    };
    fetchMedia360();
  }, [id]);

  // Handle 360 video upload
  const handleVideoSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validar formato y tamaño
      const validFormats = ['video/mp4', 'video/quicktime', 'video/webm'];
      if (!validFormats.includes(file.type)) {
        setError('Formato de video no válido. Use MP4, MOV o WebM.');
        return;
      }
      if (file.size > 500 * 1024 * 1024) {
        setError('El video no puede superar los 500MB');
        return;
      }
      setSelectedVideoFile(file);
      setError(null);
    }
  };

  const handleUpload360Video = async () => {
    if (!selectedVideoFile || !id) return;

    try {
      setIsUploading360(true);
      setUpload360Progress(10);

      // Iniciar procesamiento
      const response = await startProcessing({
        vehicleId: id,
        video: selectedVideoFile,
        frameCount: 6,
        options: {
          outputFormat: 'png',
          backgroundColor: 'transparent',
          smartFrameSelection: true,
          autoCorrectExposure: true,
          generateThumbnails: true,
        },
      });

      setUpload360Progress(30);

      // Iniciar polling del status
      const pollStatus = async () => {
        try {
          const status = await getJobStatus(response.jobId);
          setMedia360JobStatus(status);
          setUpload360Progress(30 + status.progress.percentage * 0.6);

          if (status.isComplete) {
            setUpload360Progress(100);
            // Recargar datos del visor
            const viewerData = await getVehicleViewer(id);
            setMedia360Data(viewerData);
            setSuccess('Video 360° procesado correctamente');
            setIsUploading360(false);
            setSelectedVideoFile(null);
            setTimeout(() => setSuccess(null), 3000);
          } else if (status.isFailed) {
            setError(status.errorMessage || 'Error al procesar el video');
            setIsUploading360(false);
          } else {
            // Continuar polling
            setTimeout(pollStatus, 3000);
          }
        } catch (pollError) {
          console.error('Error polling status:', pollError);
          setIsUploading360(false);
        }
      };

      pollStatus();
    } catch (err) {
      console.error('Error uploading 360 video:', err);
      setError('Error al subir el video');
      setIsUploading360(false);
    }
  };

  const handleRetry360 = async () => {
    if (!media360JobStatus?.jobId) return;
    try {
      await retryJob(media360JobStatus.jobId);
      setSuccess('Reintentando procesamiento...');
    } catch {
      setError('Error al reintentar');
    }
  };

  const getStatusColor = (status?: Vehicle360JobStatus | string) => {
    switch (status) {
      case 'Completed':
        return 'text-green-600 bg-green-100';
      case 'Processing':
      case 'ExtractingFrames':
      case 'RemovingBackground':
        return 'text-blue-600 bg-blue-100';
      case 'Failed':
      case 'ValidationFailed':
      case 'ExtractionFailed':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  };

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'price' || name === 'mileage' || name === 'year' ? Number(value) : value,
    }));
  };

  const handleAddFeature = () => {
    if (newFeature.trim() && !formData.features.includes(newFeature.trim())) {
      setFormData((prev) => ({
        ...prev,
        features: [...prev.features, newFeature.trim()],
      }));
      setNewFeature('');
    }
  };

  const handleRemoveFeature = (feature: string) => {
    setFormData((prev) => ({
      ...prev,
      features: prev.features.filter((f) => f !== feature),
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    try {
      setIsSaving(true);
      setError(null);
      setSuccess(null);

      await updateVehicle(id, formData);

      setSuccess('Vehículo actualizado correctamente');

      // Refresh vehicle data
      const updated = await getVehicleById(id);
      setVehicle(updated);

      // Clear success message after 3 seconds
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      console.error('Error updating vehicle:', err);
      setError('No se pudo actualizar el vehículo');
    } finally {
      setIsSaving(false);
    }
  };

  const tabs = [
    { id: 'basic' as TabType, label: 'Información Básica', icon: FiInfo },
    { id: 'details' as TabType, label: 'Detalles', icon: FiSettings },
    { id: 'images' as TabType, label: 'Imágenes', icon: FiImage },
    { id: 'media360' as TabType, label: 'Media 360°', icon: FiRotateCw },
    { id: 'pricing' as TabType, label: 'Precio', icon: FiDollarSign },
  ];

  // Loading state
  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="p-6 lg:p-8">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <FiLoader className="w-8 h-8 mx-auto text-blue-500 animate-spin mb-4" />
              <p className="text-gray-500">Cargando vehículo...</p>
            </div>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  // Error state
  if (error && !vehicle) {
    return (
      <DealerPortalLayout>
        <div className="p-6 lg:p-8">
          <div className="flex flex-col items-center justify-center min-h-[400px]">
            <FiAlertCircle className="w-12 h-12 text-red-500 mb-4" />
            <p className="text-red-600 mb-4">{error}</p>
            <button
              onClick={() => navigate('/dealer/inventory')}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Volver al Inventario
            </button>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate('/dealer/inventory')}
              className="p-2 hover:bg-gray-100 rounded-lg"
            >
              <FiArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Editar Vehículo</h1>
              <p className="text-gray-500">{formData.title}</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            {vehicle && (
              <Link
                to={generateListingUrl(vehicle.id, vehicle.title)}
                target="_blank"
                className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-lg hover:bg-gray-50"
              >
                <FiEye className="w-4 h-4" />
                Ver Publicación
              </Link>
            )}
            <button
              onClick={handleSubmit}
              disabled={isSaving}
              className="flex items-center gap-2 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {isSaving ? (
                <FiLoader className="w-4 h-4 animate-spin" />
              ) : (
                <FiSave className="w-4 h-4" />
              )}
              {isSaving ? 'Guardando...' : 'Guardar Cambios'}
            </button>
          </div>
        </div>

        {/* Alerts */}
        {success && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-3">
            <FiCheckCircle className="w-5 h-5 text-green-600" />
            <span className="text-green-700">{success}</span>
          </div>
        )}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3">
            <FiAlertCircle className="w-5 h-5 text-red-600" />
            <span className="text-red-700">{error}</span>
          </div>
        )}

        {/* Tabs */}
        <div className="mb-6 border-b border-gray-200">
          <nav className="flex gap-6">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-2 pb-3 border-b-2 transition-colors ${
                  activeTab === tab.id
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                <tab.icon className="w-4 h-4" />
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit}>
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            {/* Basic Info Tab */}
            {activeTab === 'basic' && (
              <div className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Título del anuncio
                    </label>
                    <input
                      type="text"
                      name="title"
                      value={formData.title}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">VIN</label>
                    <input
                      type="text"
                      name="vin"
                      value={formData.vin}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Marca</label>
                    <input
                      type="text"
                      name="make"
                      value={formData.make}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Modelo</label>
                    <input
                      type="text"
                      name="model"
                      value={formData.model}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Año</label>
                    <input
                      type="number"
                      name="year"
                      value={formData.year}
                      onChange={handleInputChange}
                      min="1900"
                      max="2030"
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Descripción
                  </label>
                  <textarea
                    name="description"
                    value={formData.description}
                    onChange={handleInputChange}
                    rows={5}
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    <FiMapPin className="inline w-4 h-4 mr-1" />
                    Ubicación
                  </label>
                  <input
                    type="text"
                    name="location"
                    value={formData.location}
                    onChange={handleInputChange}
                    placeholder="Ciudad, Estado"
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>
            )}

            {/* Details Tab */}
            {activeTab === 'details' && (
              <div className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Millaje</label>
                    <input
                      type="number"
                      name="mileage"
                      value={formData.mileage}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Combustible
                    </label>
                    <select
                      name="fuelType"
                      value={formData.fuelType}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="Gasoline">Gasolina</option>
                      <option value="Diesel">Diésel</option>
                      <option value="Electric">Eléctrico</option>
                      <option value="Hybrid">Híbrido</option>
                      <option value="PluginHybrid">Híbrido Enchufable</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Transmisión
                    </label>
                    <select
                      name="transmission"
                      value={formData.transmission}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="Automatic">Automático</option>
                      <option value="Manual">Manual</option>
                      <option value="CVT">CVT</option>
                      <option value="DualClutch">Doble Embrague</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Tipo de carrocería
                    </label>
                    <select
                      name="bodyType"
                      value={formData.bodyType}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="Sedan">Sedán</option>
                      <option value="SUV">SUV</option>
                      <option value="Coupe">Coupé</option>
                      <option value="Truck">Camioneta</option>
                      <option value="Van">Van</option>
                      <option value="Wagon">Wagon</option>
                      <option value="Convertible">Convertible</option>
                      <option value="Hatchback">Hatchback</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Condición
                    </label>
                    <select
                      name="condition"
                      value={formData.condition}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="New">Nuevo</option>
                      <option value="Used">Usado</option>
                      <option value="Certified Pre-Owned">Certificado Pre-Owned</option>
                    </select>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Color Exterior
                    </label>
                    <input
                      type="text"
                      name="color"
                      value={formData.color}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Color Interior
                    </label>
                    <input
                      type="text"
                      name="interiorColor"
                      value={formData.interiorColor}
                      onChange={handleInputChange}
                      className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                </div>

                {/* Features */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Características
                  </label>
                  <div className="flex gap-2 mb-3">
                    <input
                      type="text"
                      value={newFeature}
                      onChange={(e) => setNewFeature(e.target.value)}
                      onKeyPress={(e) =>
                        e.key === 'Enter' && (e.preventDefault(), handleAddFeature())
                      }
                      placeholder="Agregar característica..."
                      className="flex-1 px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                    <button
                      type="button"
                      onClick={handleAddFeature}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                      <FiPlus className="w-5 h-5" />
                    </button>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {formData.features.map((feature, index) => (
                      <span
                        key={index}
                        className="inline-flex items-center gap-1 px-3 py-1 bg-gray-100 rounded-full text-sm"
                      >
                        {feature}
                        <button
                          type="button"
                          onClick={() => handleRemoveFeature(feature)}
                          className="hover:text-red-500"
                        >
                          <FiX className="w-4 h-4" />
                        </button>
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {/* Images Tab */}
            {activeTab === 'images' && (
              <div className="space-y-6">
                <div className="text-center py-12 border-2 border-dashed border-gray-200 rounded-xl">
                  <FiImage className="w-12 h-12 mx-auto text-gray-400 mb-4" />
                  <p className="text-gray-500 mb-2">Arrastra imágenes aquí o</p>
                  <button
                    type="button"
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    Seleccionar archivos
                  </button>
                  <p className="text-sm text-gray-400 mt-2">PNG, JPG hasta 10MB</p>
                </div>

                {/* Current Images */}
                {vehicle?.images && vehicle.images.length > 0 && (
                  <div>
                    <h3 className="text-sm font-medium text-gray-700 mb-3">Imágenes actuales</h3>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                      {vehicle.images.map((img, index) => (
                        <div key={index} className="relative group">
                          <img
                            src={img}
                            alt={`Imagen ${index + 1}`}
                            className="w-full h-32 object-cover rounded-lg"
                          />
                          <button
                            type="button"
                            className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                          >
                            <FiTrash2 className="w-4 h-4" />
                          </button>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Media 360° Tab */}
            {activeTab === 'media360' && (
              <div className="space-y-6">
                {/* Status actual */}
                {media360Data && (
                  <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <div className="p-2 bg-green-100 rounded-lg">
                          <FiRotateCw className="w-5 h-5 text-green-600" />
                        </div>
                        <div>
                          <h4 className="font-medium text-green-900">Vista 360° Activa</h4>
                          <p className="text-sm text-green-700">
                            {media360Data.spinData?.frameUrls?.length || 0} frames procesados
                          </p>
                        </div>
                      </div>
                      <a
                        href={`/vehicles/${id}/360`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                      >
                        <FiExternalLink className="w-4 h-4" />
                        Ver 360°
                      </a>
                    </div>
                  </div>
                )}

                {/* Job status */}
                {media360JobStatus && !media360Data && (
                  <div
                    className={`p-4 rounded-lg border ${getStatusColor(media360JobStatus.currentStep)}`}
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Estado: {media360JobStatus.currentStep}</h4>
                        <p className="text-sm mt-1">
                          Progreso: {media360JobStatus.progress.percentage}%
                        </p>
                        {media360JobStatus.isFailed && (
                          <p className="text-sm text-red-600 mt-2">
                            {media360JobStatus.errorMessage}
                          </p>
                        )}
                      </div>
                      {media360JobStatus.isFailed && (
                        <button
                          type="button"
                          onClick={handleRetry360}
                          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                        >
                          <FiRefreshCw className="w-4 h-4" />
                          Reintentar
                        </button>
                      )}
                    </div>
                    {!media360JobStatus.isComplete && !media360JobStatus.isFailed && (
                      <div className="mt-3">
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div
                            className="bg-blue-600 h-2 rounded-full transition-all duration-500"
                            style={{ width: `${media360JobStatus.progress.percentage}%` }}
                          />
                        </div>
                      </div>
                    )}
                  </div>
                )}

                {/* Upload section */}
                <div className="border-2 border-dashed border-gray-200 rounded-xl p-6">
                  <div className="text-center">
                    <FiVideo className="w-12 h-12 mx-auto text-gray-400 mb-4" />
                    <h3 className="text-lg font-medium text-gray-900 mb-2">
                      {media360Data ? 'Reemplazar Video 360°' : 'Subir Video 360°'}
                    </h3>
                    <p className="text-gray-500 mb-4">
                      Sube un video en 360° del vehículo para generar una vista interactiva
                    </p>

                    {selectedVideoFile ? (
                      <div className="p-4 bg-gray-50 rounded-lg mb-4">
                        <p className="font-medium text-gray-900">{selectedVideoFile.name}</p>
                        <p className="text-sm text-gray-500">
                          {(selectedVideoFile.size / 1024 / 1024).toFixed(2)} MB
                        </p>
                      </div>
                    ) : null}

                    <input
                      ref={videoInputRef}
                      type="file"
                      accept="video/mp4,video/quicktime,video/webm"
                      onChange={handleVideoSelect}
                      className="hidden"
                    />

                    <div className="flex flex-col sm:flex-row gap-3 justify-center">
                      <button
                        type="button"
                        onClick={() => videoInputRef.current?.click()}
                        disabled={isUploading360}
                        className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                      >
                        <FiUpload className="inline w-4 h-4 mr-2" />
                        Seleccionar Video
                      </button>

                      {selectedVideoFile && (
                        <button
                          type="button"
                          onClick={handleUpload360Video}
                          disabled={isUploading360}
                          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                        >
                          {isUploading360 ? (
                            <>
                              <FiLoader className="inline w-4 h-4 mr-2 animate-spin" />
                              Procesando... {Math.round(upload360Progress)}%
                            </>
                          ) : (
                            <>
                              <FiRotateCw className="inline w-4 h-4 mr-2" />
                              Procesar Video
                            </>
                          )}
                        </button>
                      )}
                    </div>

                    <p className="text-sm text-gray-400 mt-4">MP4, MOV o WebM hasta 500MB</p>
                  </div>
                </div>

                {/* Info */}
                <div className="p-4 bg-blue-50 rounded-lg">
                  <h4 className="font-medium text-blue-900 mb-2">💡 Sobre Media 360°</h4>
                  <ul className="text-sm text-blue-800 space-y-1">
                    <li>• El video debe mostrar una vuelta completa al vehículo</li>
                    <li>• Se extraerán los mejores frames automáticamente</li>
                    <li>• El fondo se removerá para una presentación profesional</li>
                    <li>• Los compradores podrán rotar el vehículo interactivamente</li>
                  </ul>
                </div>
              </div>
            )}

            {/* Pricing Tab */}
            {activeTab === 'pricing' && (
              <div className="space-y-6">
                <div className="max-w-md">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    <FiDollarSign className="inline w-4 h-4 mr-1" />
                    Precio de venta
                  </label>
                  <div className="relative">
                    <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500">
                      $
                    </span>
                    <input
                      type="number"
                      name="price"
                      value={formData.price}
                      onChange={handleInputChange}
                      className="w-full pl-8 pr-4 py-3 text-2xl font-bold border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <p className="text-sm text-gray-500 mt-2">Precio mostrado a los compradores</p>
                </div>

                <div className="p-4 bg-blue-50 rounded-lg">
                  <h4 className="font-medium text-blue-900 mb-2">💡 Consejos de precio</h4>
                  <ul className="text-sm text-blue-800 space-y-1">
                    <li>• Investiga precios similares en el mercado</li>
                    <li>• Considera el millaje y condición del vehículo</li>
                    <li>• Los precios competitivos generan más consultas</li>
                  </ul>
                </div>
              </div>
            )}
          </div>

          {/* Footer Actions */}
          <div className="mt-6 flex items-center justify-between">
            <button
              type="button"
              onClick={() => navigate('/dealer/inventory')}
              className="px-4 py-2 text-gray-600 hover:text-gray-800"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSaving}
              className="flex items-center gap-2 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {isSaving ? (
                <FiLoader className="w-4 h-4 animate-spin" />
              ) : (
                <FiSave className="w-4 h-4" />
              )}
              {isSaving ? 'Guardando...' : 'Guardar Cambios'}
            </button>
          </div>
        </form>
      </div>
    </DealerPortalLayout>
  );
}
