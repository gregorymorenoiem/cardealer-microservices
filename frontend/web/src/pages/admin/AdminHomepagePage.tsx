/**
 * AdminHomepagePage - Admin page for managing homepage sections
 *
 * Features:
 * - List all homepage sections
 * - Create, edit, delete sections
 * - Reorder sections via drag & drop
 * - Assign/remove vehicles from sections
 * - Preview section configuration
 *
 * @author OKLA Team
 * @date January 28, 2026
 */

import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import {
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiEye,
  FiEyeOff,
  FiGrid,
  FiList,
  FiImage,
  FiStar,
  FiArrowUp,
  FiArrowDown,
  FiSave,
  FiX,
  FiRefreshCw,
  FiSettings,
  FiLayout,
} from 'react-icons/fi';
import {
  getAllSectionConfigs,
  createSection,
  updateSection,
  deleteSection,
  type HomepageSectionDto,
  type CreateSectionRequest,
  type UpdateSectionRequest,
} from '@/services/homepageSectionsService';

// Layout type options
const layoutTypes = [
  { value: 'Carousel', label: 'Carrusel', icon: FiList },
  { value: 'Grid', label: 'Cuadrícula', icon: FiGrid },
  { value: 'Featured', label: 'Destacados', icon: FiStar },
  { value: 'Hero', label: 'Hero Banner', icon: FiImage },
] as const;

// Accent color options
const accentColors = [
  { value: 'blue', label: 'Azul', class: 'bg-blue-500' },
  { value: 'green', label: 'Verde', class: 'bg-green-500' },
  { value: 'red', label: 'Rojo', class: 'bg-red-500' },
  { value: 'amber', label: 'Ámbar', class: 'bg-amber-500' },
  { value: 'purple', label: 'Púrpura', class: 'bg-purple-500' },
  { value: 'pink', label: 'Rosa', class: 'bg-pink-500' },
  { value: 'gray', label: 'Gris', class: 'bg-gray-500' },
];

// Empty section form
const emptySectionForm: CreateSectionRequest = {
  name: '',
  slug: '',
  description: '',
  displayOrder: 0,
  maxItems: 10,
  isActive: true,
  accentColor: 'blue',
  layoutType: 'Carousel',
  subtitle: '',
  viewAllHref: '',
};

const AdminHomepagePage = () => {
  const { t } = useTranslation(['admin', 'common']);

  // State
  const [sections, setSections] = useState<HomepageSectionDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSection, setEditingSection] = useState<HomepageSectionDto | null>(null);
  const [formData, setFormData] = useState<CreateSectionRequest>(emptySectionForm);
  const [isSaving, setIsSaving] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);

  // Fetch sections on mount
  useEffect(() => {
    fetchSections();
  }, []);

  const fetchSections = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await getAllSectionConfigs();
      setSections(data.sort((a, b) => a.displayOrder - b.displayOrder));
    } catch (err) {
      setError('Error al cargar las secciones');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  // Generate slug from name
  const generateSlug = (name: string) => {
    return name
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, '');
  };

  // Open modal for create
  const handleCreate = () => {
    setEditingSection(null);
    setFormData({
      ...emptySectionForm,
      displayOrder: sections.length > 0 ? Math.max(...sections.map((s) => s.displayOrder)) + 1 : 1,
    });
    setIsModalOpen(true);
  };

  // Open modal for edit
  const handleEdit = (section: HomepageSectionDto) => {
    setEditingSection(section);
    setFormData({
      name: section.name,
      slug: section.slug,
      description: section.description || '',
      displayOrder: section.displayOrder,
      maxItems: section.maxItems,
      isActive: section.isActive,
      accentColor: section.accentColor || 'blue',
      layoutType: section.layoutType,
      subtitle: section.subtitle || '',
      viewAllHref: section.viewAllHref || '',
    });
    setIsModalOpen(true);
  };

  // Handle form input changes
  const handleInputChange = (
    field: keyof CreateSectionRequest,
    value: string | number | boolean
  ) => {
    setFormData((prev) => {
      const updated = { ...prev, [field]: value };
      // Auto-generate slug when name changes (only for new sections)
      if (field === 'name' && !editingSection) {
        updated.slug = generateSlug(value as string);
      }
      return updated;
    });
  };

  // Save section (create or update)
  const handleSave = async () => {
    if (!formData.name || !formData.slug) {
      setError('Nombre y slug son requeridos');
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      if (editingSection) {
        // Update existing
        const updateData: UpdateSectionRequest = {
          name: formData.name,
          description: formData.description,
          displayOrder: formData.displayOrder,
          maxItems: formData.maxItems,
          isActive: formData.isActive,
          accentColor: formData.accentColor,
          layoutType: formData.layoutType,
          subtitle: formData.subtitle,
          viewAllHref: formData.viewAllHref,
        };
        await updateSection(editingSection.slug, updateData);
      } else {
        // Create new
        await createSection(formData);
      }

      setIsModalOpen(false);
      await fetchSections();
    } catch (err) {
      setError(editingSection ? 'Error al actualizar la sección' : 'Error al crear la sección');
      console.error(err);
    } finally {
      setIsSaving(false);
    }
  };

  // Delete section
  const handleDelete = async (slug: string) => {
    try {
      await deleteSection(slug);
      setDeleteConfirm(null);
      await fetchSections();
    } catch (err) {
      setError('Error al eliminar la sección');
      console.error(err);
    }
  };

  // Toggle section active status
  const handleToggleActive = async (section: HomepageSectionDto) => {
    try {
      await updateSection(section.slug, { isActive: !section.isActive });
      await fetchSections();
    } catch (err) {
      setError('Error al cambiar el estado de la sección');
      console.error(err);
    }
  };

  // Move section up/down
  const handleMove = async (section: HomepageSectionDto, direction: 'up' | 'down') => {
    const currentIndex = sections.findIndex((s) => s.id === section.id);
    const targetIndex = direction === 'up' ? currentIndex - 1 : currentIndex + 1;

    if (targetIndex < 0 || targetIndex >= sections.length) return;

    const targetSection = sections[targetIndex];

    try {
      // Swap display orders
      await updateSection(section.slug, { displayOrder: targetSection.displayOrder });
      await updateSection(targetSection.slug, { displayOrder: section.displayOrder });
      await fetchSections();
    } catch (err) {
      setError('Error al reordenar las secciones');
      console.error(err);
    }
  };

  // Get layout icon
  const getLayoutIcon = (type: string) => {
    const layout = layoutTypes.find((l) => l.value === type);
    return layout ? layout.icon : FiGrid;
  };

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <FiLayout className="text-blue-600" />
            Gestión de Homepage
          </h1>
          <p className="text-gray-600 mt-1">
            Configura las secciones y vehículos que aparecen en la página principal
          </p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={fetchSections}
            className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 flex items-center gap-2"
          >
            <FiRefreshCw className={isLoading ? 'animate-spin' : ''} />
            Actualizar
          </button>
          <button
            onClick={handleCreate}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 flex items-center gap-2"
          >
            <FiPlus />
            Nueva Sección
          </button>
        </div>
      </div>

      {/* Error message */}
      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 flex justify-between items-center">
          <span>{error}</span>
          <button onClick={() => setError(null)} className="text-red-500 hover:text-red-700">
            <FiX />
          </button>
        </div>
      )}

      {/* Sections list */}
      {isLoading ? (
        <div className="flex justify-center items-center py-12">
          <FiRefreshCw className="animate-spin text-4xl text-blue-600" />
        </div>
      ) : sections.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <FiLayout className="mx-auto text-5xl text-gray-400 mb-4" />
          <h3 className="text-lg font-medium text-gray-900">No hay secciones configuradas</h3>
          <p className="text-gray-600 mt-1">Crea tu primera sección para empezar</p>
          <button
            onClick={handleCreate}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Crear Sección
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {sections.map((section, index) => {
            const LayoutIcon = getLayoutIcon(section.layoutType);
            return (
              <div
                key={section.id}
                className={`bg-white rounded-lg border ${
                  section.isActive ? 'border-gray-200' : 'border-gray-300 bg-gray-50'
                } shadow-sm hover:shadow-md transition-shadow`}
              >
                <div className="p-4 flex flex-col sm:flex-row justify-between gap-4">
                  {/* Section info */}
                  <div className="flex items-start gap-4 flex-1">
                    {/* Order & Layout icon */}
                    <div className="flex flex-col items-center gap-1">
                      <span className="text-xs text-gray-500">#{section.displayOrder}</span>
                      <div
                        className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                          section.isActive
                            ? 'bg-blue-100 text-blue-600'
                            : 'bg-gray-200 text-gray-500'
                        }`}
                      >
                        <LayoutIcon className="text-lg" />
                      </div>
                    </div>

                    {/* Details */}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <h3
                          className={`font-semibold ${section.isActive ? 'text-gray-900' : 'text-gray-500'}`}
                        >
                          {section.name}
                        </h3>
                        <span className="text-xs px-2 py-0.5 bg-gray-100 text-gray-600 rounded">
                          /{section.slug}
                        </span>
                        {section.accentColor && (
                          <span
                            className={`w-4 h-4 rounded-full ${
                              accentColors.find((c) => c.value === section.accentColor)?.class ||
                              'bg-blue-500'
                            }`}
                          />
                        )}
                        {!section.isActive && (
                          <span className="text-xs px-2 py-0.5 bg-gray-200 text-gray-600 rounded">
                            Inactivo
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-600 mt-1 truncate">
                        {section.description || section.subtitle || 'Sin descripción'}
                      </p>
                      <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                        <span>Layout: {section.layoutType}</span>
                        <span>Max: {section.maxItems} vehículos</span>
                        <span className="flex items-center gap-1">
                          <FiImage className="text-gray-400" />
                          {section.vehicles?.length || 0} asignados
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-2 flex-shrink-0">
                    {/* Move buttons */}
                    <button
                      onClick={() => handleMove(section, 'up')}
                      disabled={index === 0}
                      className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded disabled:opacity-30"
                      title="Mover arriba"
                    >
                      <FiArrowUp />
                    </button>
                    <button
                      onClick={() => handleMove(section, 'down')}
                      disabled={index === sections.length - 1}
                      className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded disabled:opacity-30"
                      title="Mover abajo"
                    >
                      <FiArrowDown />
                    </button>

                    {/* Toggle active */}
                    <button
                      onClick={() => handleToggleActive(section)}
                      className={`p-2 rounded ${
                        section.isActive
                          ? 'text-green-600 hover:bg-green-50'
                          : 'text-gray-400 hover:bg-gray-100'
                      }`}
                      title={section.isActive ? 'Desactivar' : 'Activar'}
                    >
                      {section.isActive ? <FiEye /> : <FiEyeOff />}
                    </button>

                    {/* Edit */}
                    <button
                      onClick={() => handleEdit(section)}
                      className="p-2 text-blue-600 hover:bg-blue-50 rounded"
                      title="Editar"
                    >
                      <FiEdit2 />
                    </button>

                    {/* Delete */}
                    <button
                      onClick={() => setDeleteConfirm(section.slug)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded"
                      title="Eliminar"
                    >
                      <FiTrash2 />
                    </button>
                  </div>
                </div>

                {/* Delete confirmation */}
                {deleteConfirm === section.slug && (
                  <div className="px-4 pb-4 pt-0">
                    <div className="bg-red-50 border border-red-200 rounded-lg p-3 flex items-center justify-between">
                      <span className="text-sm text-red-700">
                        ¿Eliminar la sección "{section.name}"? Esta acción no se puede deshacer.
                      </span>
                      <div className="flex gap-2 ml-4">
                        <button
                          onClick={() => setDeleteConfirm(null)}
                          className="px-3 py-1 text-sm bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
                        >
                          Cancelar
                        </button>
                        <button
                          onClick={() => handleDelete(section.slug)}
                          className="px-3 py-1 text-sm bg-red-600 text-white rounded hover:bg-red-700"
                        >
                          Eliminar
                        </button>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Create/Edit Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
            {/* Modal header */}
            <div className="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
              <h2 className="text-xl font-semibold text-gray-900">
                {editingSection ? 'Editar Sección' : 'Nueva Sección'}
              </h2>
              <button
                onClick={() => setIsModalOpen(false)}
                className="p-2 text-gray-400 hover:text-gray-600 rounded"
              >
                <FiX className="text-xl" />
              </button>
            </div>

            {/* Modal body */}
            <div className="p-6 space-y-4">
              {/* Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => handleInputChange('name', e.target.value)}
                  placeholder="Ej: Vehículos Destacados"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Slug */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Slug <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.slug}
                  onChange={(e) => handleInputChange('slug', e.target.value)}
                  placeholder="vehiculos-destacados"
                  disabled={!!editingSection}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
                />
                {editingSection && (
                  <p className="text-xs text-gray-500 mt-1">El slug no se puede modificar</p>
                )}
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  placeholder="Descripción de la sección..."
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Subtitle */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Subtítulo</label>
                <input
                  type="text"
                  value={formData.subtitle}
                  onChange={(e) => handleInputChange('subtitle', e.target.value)}
                  placeholder="Texto que aparece debajo del título"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Layout Type */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Tipo de Layout
                </label>
                <div className="grid grid-cols-2 gap-2">
                  {layoutTypes.map((layout) => (
                    <button
                      key={layout.value}
                      type="button"
                      onClick={() => handleInputChange('layoutType', layout.value)}
                      className={`flex items-center gap-2 p-3 border rounded-lg transition-colors ${
                        formData.layoutType === layout.value
                          ? 'border-blue-500 bg-blue-50 text-blue-700'
                          : 'border-gray-200 hover:border-gray-300'
                      }`}
                    >
                      <layout.icon />
                      <span>{layout.label}</span>
                    </button>
                  ))}
                </div>
              </div>

              {/* Display Order & Max Items */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Orden</label>
                  <input
                    type="number"
                    value={formData.displayOrder}
                    onChange={(e) =>
                      handleInputChange('displayOrder', parseInt(e.target.value) || 0)
                    }
                    min={1}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Máx. Vehículos
                  </label>
                  <input
                    type="number"
                    value={formData.maxItems}
                    onChange={(e) => handleInputChange('maxItems', parseInt(e.target.value) || 10)}
                    min={1}
                    max={50}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
              </div>

              {/* Accent Color */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Color de Acento
                </label>
                <div className="flex flex-wrap gap-2">
                  {accentColors.map((color) => (
                    <button
                      key={color.value}
                      type="button"
                      onClick={() => handleInputChange('accentColor', color.value)}
                      className={`w-8 h-8 rounded-full ${color.class} ${
                        formData.accentColor === color.value
                          ? 'ring-2 ring-offset-2 ring-gray-400'
                          : ''
                      }`}
                      title={color.label}
                    />
                  ))}
                </div>
              </div>

              {/* View All Href */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Link "Ver Todos"
                </label>
                <input
                  type="text"
                  value={formData.viewAllHref}
                  onChange={(e) => handleInputChange('viewAllHref', e.target.value)}
                  placeholder="/search?category=suv"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Is Active */}
              <div className="flex items-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => handleInputChange('isActive', !formData.isActive)}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    formData.isActive ? 'bg-blue-600' : 'bg-gray-300'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      formData.isActive ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
                <span className="text-sm text-gray-700">
                  {formData.isActive
                    ? 'Sección activa (visible en homepage)'
                    : 'Sección inactiva (oculta)'}
                </span>
              </div>
            </div>

            {/* Modal footer */}
            <div className="sticky bottom-0 bg-gray-50 border-t px-6 py-4 flex justify-end gap-3">
              <button
                onClick={() => setIsModalOpen(false)}
                className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Cancelar
              </button>
              <button
                onClick={handleSave}
                disabled={isSaving}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
              >
                {isSaving ? (
                  <>
                    <FiRefreshCw className="animate-spin" />
                    Guardando...
                  </>
                ) : (
                  <>
                    <FiSave />
                    {editingSection ? 'Guardar Cambios' : 'Crear Sección'}
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Help text */}
      <div className="mt-8 bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h4 className="font-medium text-blue-900 flex items-center gap-2">
          <FiSettings className="text-blue-600" />
          Cómo funciona
        </h4>
        <ul className="mt-2 text-sm text-blue-800 space-y-1">
          <li>• Las secciones aparecen en el homepage ordenadas por el campo "Orden"</li>
          <li>• Solo las secciones activas son visibles para los usuarios</li>
          <li>• El "Máx. Vehículos" limita cuántos vehículos se muestran en cada sección</li>
          <li>• Para asignar vehículos a una sección, usa el panel de edición de vehículos</li>
          <li>• El layout determina cómo se muestran los vehículos (carrusel, cuadrícula, etc.)</li>
        </ul>
      </div>
    </div>
  );
};

export default AdminHomepagePage;
