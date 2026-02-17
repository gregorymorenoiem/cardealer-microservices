/**
 * CategoriesManagementPage - Admin page for managing categories and subcategories
 * Full CRUD operations for multi-vertical marketplace categories
 */

import React, { useState, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { mockCategories } from '../../data/mockAdmin';
import type { Category, Subcategory, CategoryFormData, SubcategoryFormData, VerticalType } from '../../types/admin';
import {
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiChevronDown,
  FiChevronRight,
  FiSearch,
  FiTruck,
  FiHome,
  FiGrid,
  FiToggleLeft,
  FiToggleRight,
  FiMove,
  FiX,
  FiCheck,
  FiAlertCircle,
  FiLayers,
  FiTag,
  FiEye,
  FiEyeOff,
} from 'react-icons/fi';

// Icon options for categories
const CATEGORY_ICONS = ['🚗', '🚙', '🛻', '⚡', '🏎️', '🚐', '🏠', '🏢', '🌳', '🏪', '🏛️', '📦', '✨', '🔑', '💎', '🏗️'];

// Color options for categories
const CATEGORY_COLORS = [
  '#3B82F6', '#10B981', '#F59E0B', '#8B5CF6', '#EF4444', '#EC4899',
  '#06B6D4', '#84CC16', '#F97316', '#6366F1', '#14B8A6', '#A855F7',
];

const CategoriesManagementPage: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>(mockCategories);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeVertical, setActiveVertical] = useState<VerticalType | 'all'>('all');
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set());
  
  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalType, setModalType] = useState<'category' | 'subcategory'>('category');
  const [editingItem, setEditingItem] = useState<Category | Subcategory | null>(null);
  const [parentCategoryId, setParentCategoryId] = useState<string | null>(null);
  
  // Delete confirmation
  const [deleteConfirm, setDeleteConfirm] = useState<{ type: 'category' | 'subcategory'; id: string; name: string } | null>(null);

  // Form state
  const [formData, setFormData] = useState<CategoryFormData | SubcategoryFormData>({
    name: '',
    slug: '',
    icon: '🚗',
    description: '',
    isActive: true,
    sortOrder: 1,
    vertical: 'vehicles',
    color: '#3B82F6',
  });

  // Filter categories
  const filteredCategories = useMemo(() => {
    return categories.filter((cat) => {
      const matchesVertical = activeVertical === 'all' || cat.vertical === activeVertical;
      const matchesSearch = !searchTerm || 
        cat.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cat.subcategories.some(sub => sub.name.toLowerCase().includes(searchTerm.toLowerCase()));
      return matchesVertical && matchesSearch;
    });
  }, [categories, activeVertical, searchTerm]);

  // Stats
  const stats = useMemo(() => {
    const vehicleCats = categories.filter(c => c.vertical === 'vehicles');
    const propertyCats = categories.filter(c => c.vertical === 'properties');
    return {
      totalCategories: categories.length,
      totalSubcategories: categories.reduce((acc, cat) => acc + cat.subcategories.length, 0),
      vehicleCategories: vehicleCats.length,
      propertyCategories: propertyCats.length,
      activeCategories: categories.filter(c => c.isActive).length,
    };
  }, [categories]);

  // Toggle category expansion
  const toggleExpand = (categoryId: string) => {
    setExpandedCategories(prev => {
      const next = new Set(prev);
      if (next.has(categoryId)) {
        next.delete(categoryId);
      } else {
        next.add(categoryId);
      }
      return next;
    });
  };

  // Open modal for creating/editing
  const openModal = (type: 'category' | 'subcategory', item?: Category | Subcategory, parentId?: string) => {
    setModalType(type);
    setEditingItem(item || null);
    setParentCategoryId(parentId || null);
    
    if (item) {
      if (type === 'category') {
        const cat = item as Category;
        setFormData({
          name: cat.name,
          slug: cat.slug,
          icon: cat.icon,
          description: cat.description || '',
          isActive: cat.isActive,
          sortOrder: cat.sortOrder,
          vertical: cat.vertical,
          color: cat.color,
        });
      } else {
        const sub = item as Subcategory;
        setFormData({
          name: sub.name,
          slug: sub.slug,
          icon: sub.icon || '',
          description: sub.description || '',
          isActive: sub.isActive,
          sortOrder: sub.sortOrder,
        });
      }
    } else {
      // Reset form for new item
      const maxOrder = type === 'category' 
        ? Math.max(...categories.map(c => c.sortOrder), 0)
        : parentId 
          ? Math.max(...(categories.find(c => c.id === parentId)?.subcategories.map(s => s.sortOrder) || [0]))
          : 0;
      
      setFormData({
        name: '',
        slug: '',
        icon: type === 'category' ? '🚗' : '',
        description: '',
        isActive: true,
        sortOrder: maxOrder + 1,
        vertical: activeVertical !== 'all' ? activeVertical : 'vehicles',
        color: CATEGORY_COLORS[Math.floor(Math.random() * CATEGORY_COLORS.length)],
      });
    }
    
    setIsModalOpen(true);
  };

  // Close modal
  const closeModal = () => {
    setIsModalOpen(false);
    setEditingItem(null);
    setParentCategoryId(null);
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

  // Handle form submit
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const now = new Date().toISOString();
    
    if (modalType === 'category') {
      const catData = formData as CategoryFormData;
      
      if (editingItem) {
        // Update existing category
        setCategories(prev => prev.map(cat => 
          cat.id === editingItem.id 
            ? { 
                ...cat, 
                ...catData, 
                updatedAt: now 
              }
            : cat
        ));
      } else {
        // Create new category
        const newCategory: Category = {
          id: `cat-${Date.now()}`,
          ...catData,
          listingsCount: 0,
          subcategories: [],
          createdAt: now,
          updatedAt: now,
        };
        setCategories(prev => [...prev, newCategory]);
      }
    } else {
      const subData = formData as SubcategoryFormData;
      
      if (editingItem) {
        // Update existing subcategory
        setCategories(prev => prev.map(cat => ({
          ...cat,
          subcategories: cat.subcategories.map(sub =>
            sub.id === editingItem.id
              ? { ...sub, ...subData, updatedAt: now }
              : sub
          ),
          updatedAt: cat.subcategories.some(s => s.id === editingItem.id) ? now : cat.updatedAt,
        })));
      } else if (parentCategoryId) {
        // Create new subcategory
        const newSubcategory: Subcategory = {
          id: `sub-${Date.now()}`,
          categoryId: parentCategoryId,
          ...subData,
          listingsCount: 0,
          createdAt: now,
          updatedAt: now,
        };
        setCategories(prev => prev.map(cat =>
          cat.id === parentCategoryId
            ? { 
                ...cat, 
                subcategories: [...cat.subcategories, newSubcategory],
                updatedAt: now,
              }
            : cat
        ));
        // Auto-expand the parent category
        setExpandedCategories(prev => new Set([...prev, parentCategoryId]));
      }
    }
    
    closeModal();
  };

  // Handle delete
  const handleDelete = () => {
    if (!deleteConfirm) return;
    
    if (deleteConfirm.type === 'category') {
      setCategories(prev => prev.filter(cat => cat.id !== deleteConfirm.id));
    } else {
      setCategories(prev => prev.map(cat => ({
        ...cat,
        subcategories: cat.subcategories.filter(sub => sub.id !== deleteConfirm.id),
      })));
    }
    
    setDeleteConfirm(null);
  };

  // Toggle active status
  const toggleActive = (type: 'category' | 'subcategory', id: string) => {
    const now = new Date().toISOString();
    
    if (type === 'category') {
      setCategories(prev => prev.map(cat =>
        cat.id === id ? { ...cat, isActive: !cat.isActive, updatedAt: now } : cat
      ));
    } else {
      setCategories(prev => prev.map(cat => ({
        ...cat,
        subcategories: cat.subcategories.map(sub =>
          sub.id === id ? { ...sub, isActive: !sub.isActive, updatedAt: now } : sub
        ),
      })));
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
                <FiLayers className="w-7 h-7 text-primary" />
                Gestión de Categorías
              </h1>
              <p className="mt-1 text-gray-600">
                Administra las categorías y subcategorías del marketplace multi-vertical
              </p>
            </div>
            <button
              onClick={() => openModal('category')}
              className="inline-flex items-center gap-2 px-4 py-2.5 bg-primary hover:bg-primary-600 text-white font-medium rounded-xl transition-colors shadow-lg shadow-primary/25"
            >
              <FiPlus className="w-5 h-5" />
              Nueva Categoría
            </button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
          <div className="bg-white rounded-xl p-4 border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                <FiGrid className="w-5 h-5 text-gray-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.totalCategories}</p>
                <p className="text-xs text-gray-500">Categorías</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                <FiTag className="w-5 h-5 text-gray-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.totalSubcategories}</p>
                <p className="text-xs text-gray-500">Subcategorías</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                <FiTruck className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.vehicleCategories}</p>
                <p className="text-xs text-gray-500">Vehículos</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-emerald-100 rounded-lg flex items-center justify-center">
                <FiHome className="w-5 h-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.propertyCategories}</p>
                <p className="text-xs text-gray-500">Inmuebles</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                <FiCheck className="w-5 h-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.activeCategories}</p>
                <p className="text-xs text-gray-500">Activas</p>
              </div>
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-xl border border-gray-200 p-4 mb-6">
          <div className="flex flex-col md:flex-row gap-4">
            {/* Search */}
            <div className="flex-1 relative">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar categorías o subcategorías..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2.5 bg-gray-100 border-0 rounded-lg text-gray-900 placeholder-gray-500 focus:ring-2 focus:ring-primary"
              />
            </div>

            {/* Vertical Filter */}
            <div className="flex gap-2">
              <button
                onClick={() => setActiveVertical('all')}
                className={`px-4 py-2.5 rounded-lg font-medium transition-colors ${
                  activeVertical === 'all'
                    ? 'bg-gray-900 text-white'
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
              >
                Todas
              </button>
              <button
                onClick={() => setActiveVertical('vehicles')}
                className={`inline-flex items-center gap-2 px-4 py-2.5 rounded-lg font-medium transition-colors ${
                  activeVertical === 'vehicles'
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 text-gray-600 hover:bg-blue-50 hover:text-blue-600'
                }`}
              >
                <FiTruck className="w-4 h-4" />
                Vehículos
              </button>
              <button
                onClick={() => setActiveVertical('properties')}
                className={`inline-flex items-center gap-2 px-4 py-2.5 rounded-lg font-medium transition-colors ${
                  activeVertical === 'properties'
                    ? 'bg-emerald-600 text-white'
                    : 'bg-gray-100 text-gray-600 hover:bg-emerald-50 hover:text-emerald-600'
                }`}
              >
                <FiHome className="w-4 h-4" />
                Inmuebles
              </button>
            </div>
          </div>
        </div>

        {/* Categories List */}
        <div className="space-y-4">
          {filteredCategories.length === 0 ? (
            <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
              <FiGrid className="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <p className="text-gray-500">No se encontraron categorías</p>
              <button
                onClick={() => openModal('category')}
                className="mt-4 inline-flex items-center gap-2 px-4 py-2 text-primary hover:bg-primary/10 rounded-lg transition-colors"
              >
                <FiPlus className="w-4 h-4" />
                Crear primera categoría
              </button>
            </div>
          ) : (
            filteredCategories.map((category) => (
              <motion.div
                key={category.id}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-white rounded-xl border border-gray-200 overflow-hidden"
              >
                {/* Category Header */}
                <div className="p-4 flex items-center gap-4">
                  <button
                    onClick={() => toggleExpand(category.id)}
                    className="p-1 hover:bg-gray-100 rounded transition-colors"
                  >
                    {expandedCategories.has(category.id) ? (
                      <FiChevronDown className="w-5 h-5 text-gray-400" />
                    ) : (
                      <FiChevronRight className="w-5 h-5 text-gray-400" />
                    )}
                  </button>

                  <div
                    className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl"
                    style={{ backgroundColor: `${category.color}20` }}
                  >
                    {category.icon}
                  </div>

                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <h3 className="text-lg font-semibold text-gray-900">
                        {category.name}
                      </h3>
                      <span
                        className={`px-2 py-0.5 text-xs font-medium rounded-full ${
                          category.vertical === 'vehicles'
                            ? 'bg-blue-100 text-blue-600'
                            : 'bg-emerald-100 text-emerald-600'
                        }`}
                      >
                        {category.vertical === 'vehicles' ? 'Vehículos' : 'Inmuebles'}
                      </span>
                      {!category.isActive && (
                        <span className="px-2 py-0.5 text-xs font-medium rounded-full bg-gray-100 text-gray-500">
                          Inactiva
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-4 text-sm text-gray-500">
                      <span>/{category.slug}</span>
                      <span>•</span>
                      <span>{category.subcategories.length} subcategorías</span>
                      <span>•</span>
                      <span>{category.listingsCount.toLocaleString()} listings</span>
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    {/* Toggle Active */}
                    <button
                      onClick={() => toggleActive('category', category.id)}
                      className={`p-2 rounded-lg transition-colors ${
                        category.isActive
                          ? 'text-green-600 hover:bg-green-50'
                          : 'text-gray-400 hover:bg-gray-100'
                      }`}
                      title={category.isActive ? 'Desactivar' : 'Activar'}
                    >
                      {category.isActive ? <FiEye className="w-5 h-5" /> : <FiEyeOff className="w-5 h-5" />}
                    </button>

                    {/* Add Subcategory */}
                    <button
                      onClick={() => openModal('subcategory', undefined, category.id)}
                      className="p-2 text-gray-400 hover:text-primary hover:bg-primary/10 rounded-lg transition-colors"
                      title="Agregar subcategoría"
                    >
                      <FiPlus className="w-5 h-5" />
                    </button>

                    {/* Edit */}
                    <button
                      onClick={() => openModal('category', category)}
                      className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                      title="Editar"
                    >
                      <FiEdit2 className="w-5 h-5" />
                    </button>

                    {/* Delete */}
                    <button
                      onClick={() => setDeleteConfirm({ type: 'category', id: category.id, name: category.name })}
                      className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                      title="Eliminar"
                    >
                      <FiTrash2 className="w-5 h-5" />
                    </button>
                  </div>
                </div>

                {/* Subcategories */}
                <AnimatePresence>
                  {expandedCategories.has(category.id) && category.subcategories.length > 0 && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: 'auto', opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      transition={{ duration: 0.2 }}
                      className="border-t border-gray-100"
                    >
                      <div className="bg-gray-50 p-4">
                        <div className="space-y-2">
                          {category.subcategories
                            .sort((a, b) => a.sortOrder - b.sortOrder)
                            .map((subcategory) => (
                              <div
                                key={subcategory.id}
                                className="flex items-center gap-4 p-3 bg-white rounded-lg border border-gray-200"
                              >
                                <FiMove className="w-4 h-4 text-gray-300 cursor-move" />
                                
                                <div className="w-8 h-8 bg-gray-100 rounded-lg flex items-center justify-center text-lg">
                                  {subcategory.icon || '📁'}
                                </div>

                                <div className="flex-1 min-w-0">
                                  <div className="flex items-center gap-2">
                                    <span className="font-medium text-gray-900">
                                      {subcategory.name}
                                    </span>
                                    {!subcategory.isActive && (
                                      <span className="px-1.5 py-0.5 text-[10px] font-medium rounded bg-gray-100 text-gray-500">
                                        INACTIVA
                                      </span>
                                    )}
                                  </div>
                                  <div className="text-sm text-gray-500">
                                    /{category.slug}/{subcategory.slug} • {subcategory.listingsCount} listings
                                  </div>
                                </div>

                                <div className="flex items-center gap-1">
                                  <button
                                    onClick={() => toggleActive('subcategory', subcategory.id)}
                                    className={`p-1.5 rounded transition-colors ${
                                      subcategory.isActive
                                        ? 'text-green-600 hover:bg-green-50'
                                        : 'text-gray-400 hover:bg-gray-100'
                                    }`}
                                  >
                                    {subcategory.isActive ? <FiToggleRight className="w-4 h-4" /> : <FiToggleLeft className="w-4 h-4" />}
                                  </button>
                                  <button
                                    onClick={() => openModal('subcategory', subcategory, category.id)}
                                    className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded transition-colors"
                                  >
                                    <FiEdit2 className="w-4 h-4" />
                                  </button>
                                  <button
                                    onClick={() => setDeleteConfirm({ type: 'subcategory', id: subcategory.id, name: subcategory.name })}
                                    className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
                                  >
                                    <FiTrash2 className="w-4 h-4" />
                                  </button>
                                </div>
                              </div>
                            ))}
                        </div>
                        
                        {/* Add Subcategory Button */}
                        <button
                          onClick={() => openModal('subcategory', undefined, category.id)}
                          className="mt-3 w-full flex items-center justify-center gap-2 p-2 text-gray-500 hover:text-primary border-2 border-dashed border-gray-200 hover:border-primary rounded-lg transition-colors"
                        >
                          <FiPlus className="w-4 h-4" />
                          Agregar subcategoría
                        </button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>

                {/* Empty subcategories message */}
                {expandedCategories.has(category.id) && category.subcategories.length === 0 && (
                  <div className="border-t border-gray-100 bg-gray-50 p-6 text-center">
                    <p className="text-gray-500 mb-3">Sin subcategorías</p>
                    <button
                      onClick={() => openModal('subcategory', undefined, category.id)}
                      className="inline-flex items-center gap-2 px-4 py-2 text-primary hover:bg-primary/10 rounded-lg transition-colors"
                    >
                      <FiPlus className="w-4 h-4" />
                      Crear primera subcategoría
                    </button>
                  </div>
                )}
              </motion.div>
            ))
          )}
        </div>

        {/* Modal for Create/Edit */}
        <AnimatePresence>
          {isModalOpen && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
              onClick={closeModal}
            >
              <motion.div
                initial={{ scale: 0.95, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                exit={{ scale: 0.95, opacity: 0 }}
                className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="p-6 border-b border-gray-200">
                  <div className="flex items-center justify-between">
                    <h2 className="text-xl font-bold text-gray-900">
                      {editingItem 
                        ? `Editar ${modalType === 'category' ? 'Categoría' : 'Subcategoría'}`
                        : `Nueva ${modalType === 'category' ? 'Categoría' : 'Subcategoría'}`
                      }
                    </h2>
                    <button
                      onClick={closeModal}
                      className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                    >
                      <FiX className="w-5 h-5 text-gray-500" />
                    </button>
                  </div>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-5">
                  {/* Name */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Nombre *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ 
                        ...formData, 
                        name: e.target.value,
                        slug: formData.slug || generateSlug(e.target.value),
                      })}
                      className="w-full px-4 py-2.5 bg-gray-100 border-0 rounded-lg text-gray-900 focus:ring-2 focus:ring-primary"
                      placeholder="Ej: Sedanes, Casas..."
                    />
                  </div>

                  {/* Slug */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Slug (URL) *
                    </label>
                    <div className="flex items-center gap-2">
                      <span className="text-gray-400">/</span>
                      <input
                        type="text"
                        required
                        value={formData.slug}
                        onChange={(e) => setFormData({ ...formData, slug: generateSlug(e.target.value) })}
                        className="flex-1 px-4 py-2.5 bg-gray-100 border-0 rounded-lg text-gray-900 focus:ring-2 focus:ring-primary"
                        placeholder="sedan"
                      />
                    </div>
                  </div>

                  {/* Vertical (only for categories) */}
                  {modalType === 'category' && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Vertical *
                      </label>
                      <div className="grid grid-cols-2 gap-3">
                        <button
                          type="button"
                          onClick={() => setFormData({ ...formData, vertical: 'vehicles' } as CategoryFormData)}
                          className={`flex items-center justify-center gap-2 p-3 rounded-lg border-2 transition-colors ${
                            (formData as CategoryFormData).vertical === 'vehicles'
                              ? 'border-blue-500 bg-blue-50 text-blue-600'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          <FiTruck className="w-5 h-5" />
                          Vehículos
                        </button>
                        <button
                          type="button"
                          onClick={() => setFormData({ ...formData, vertical: 'properties' } as CategoryFormData)}
                          className={`flex items-center justify-center gap-2 p-3 rounded-lg border-2 transition-colors ${
                            (formData as CategoryFormData).vertical === 'properties'
                              ? 'border-emerald-500 bg-emerald-50 text-emerald-600'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          <FiHome className="w-5 h-5" />
                          Inmuebles
                        </button>
                      </div>
                    </div>
                  )}

                  {/* Icon */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Ícono
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {CATEGORY_ICONS.map((icon) => (
                        <button
                          key={icon}
                          type="button"
                          onClick={() => setFormData({ ...formData, icon })}
                          className={`w-10 h-10 flex items-center justify-center text-xl rounded-lg border-2 transition-colors ${
                            formData.icon === icon
                              ? 'border-primary bg-primary/10'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          {icon}
                        </button>
                      ))}
                    </div>
                  </div>

                  {/* Color (only for categories) */}
                  {modalType === 'category' && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Color
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {CATEGORY_COLORS.map((color) => (
                          <button
                            key={color}
                            type="button"
                            onClick={() => setFormData({ ...formData, color } as CategoryFormData)}
                            className={`w-8 h-8 rounded-full transition-transform ${
                              (formData as CategoryFormData).color === color
                                ? 'ring-2 ring-offset-2 ring-gray-400 scale-110'
                                : 'hover:scale-110'
                            }`}
                            style={{ backgroundColor: color }}
                          />
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Description */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Descripción
                    </label>
                    <textarea
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                      rows={3}
                      className="w-full px-4 py-2.5 bg-gray-100 border-0 rounded-lg text-gray-900 focus:ring-2 focus:ring-primary resize-none"
                      placeholder="Descripción opcional..."
                    />
                  </div>

                  {/* Sort Order */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Orden de visualización
                    </label>
                    <input
                      type="number"
                      min="1"
                      value={formData.sortOrder}
                      onChange={(e) => setFormData({ ...formData, sortOrder: parseInt(e.target.value) || 1 })}
                      className="w-24 px-4 py-2.5 bg-gray-100 border-0 rounded-lg text-gray-900 focus:ring-2 focus:ring-primary"
                    />
                  </div>

                  {/* Active Toggle */}
                  <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                    <div>
                      <p className="font-medium text-gray-900">Estado activo</p>
                      <p className="text-sm text-gray-500">La categoría será visible en el marketplace</p>
                    </div>
                    <button
                      type="button"
                      onClick={() => setFormData({ ...formData, isActive: !formData.isActive })}
                      className={`relative w-12 h-6 rounded-full transition-colors ${
                        formData.isActive ? 'bg-green-500' : 'bg-gray-300'
                      }`}
                    >
                      <span
                        className={`absolute top-0.5 left-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${
                          formData.isActive ? 'translate-x-6' : ''
                        }`}
                      />
                    </button>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-3 pt-4">
                    <button
                      type="button"
                      onClick={closeModal}
                      className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 font-medium rounded-lg hover:bg-gray-200 transition-colors"
                    >
                      Cancelar
                    </button>
                    <button
                      type="submit"
                      className="flex-1 px-4 py-2.5 bg-primary hover:bg-primary-600 text-white font-medium rounded-lg transition-colors"
                    >
                      {editingItem ? 'Guardar cambios' : 'Crear'}
                    </button>
                  </div>
                </form>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Delete Confirmation Modal */}
        <AnimatePresence>
          {deleteConfirm && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
              onClick={() => setDeleteConfirm(null)}
            >
              <motion.div
                initial={{ scale: 0.95, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                exit={{ scale: 0.95, opacity: 0 }}
                className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-6"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="flex items-center gap-4 mb-4">
                  <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center">
                    <FiAlertCircle className="w-6 h-6 text-red-600" />
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-gray-900">
                      Eliminar {deleteConfirm.type === 'category' ? 'categoría' : 'subcategoría'}
                    </h3>
                    <p className="text-gray-500">Esta acción no se puede deshacer</p>
                  </div>
                </div>

                <p className="text-gray-600 mb-6">
                  ¿Estás seguro de que deseas eliminar <strong>"{deleteConfirm.name}"</strong>?
                  {deleteConfirm.type === 'category' && (
                    <span className="block mt-2 text-sm text-red-600">
                      ⚠️ Esto también eliminará todas las subcategorías asociadas.
                    </span>
                  )}
                </p>

                <div className="flex gap-3">
                  <button
                    onClick={() => setDeleteConfirm(null)}
                    className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 font-medium rounded-lg hover:bg-gray-200 transition-colors"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleDelete}
                    className="flex-1 px-4 py-2.5 bg-red-600 hover:bg-red-700 text-white font-medium rounded-lg transition-colors"
                  >
                    Eliminar
                  </button>
                </div>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
};

export default CategoriesManagementPage;
