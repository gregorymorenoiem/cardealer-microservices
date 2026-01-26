import React, { useState, useEffect } from 'react';
import {
  FiSearch,
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiEye,
  FiAlertTriangle,
  FiShield,
  FiGlobe,
  FiUser,
  FiFilter,
  FiDownload,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import { kycService } from '../../services/kycService';
import type { WatchlistEntry } from '../../services/kycService';
import { WatchlistType } from '../../services/kycService';

const WatchlistAdminPage: React.FC = () => {
  const [entries, setEntries] = useState<WatchlistEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState<WatchlistType | ''>('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<WatchlistEntry | null>(null);
  const [processing, setProcessing] = useState(false);

  const [newEntry, setNewEntry] = useState({
    fullName: '',
    listType: WatchlistType.PEP,
    position: '',
    jurisdiction: '',
    dateOfBirth: '',
    documentNumber: '',
    remarks: '',
  });

  const [stats, setStats] = useState({
    pep: 0,
    sanctions: 0,
    internal: 0,
    total: 0,
  });

  useEffect(() => {
    loadEntries();
  }, [selectedType]);

  const loadEntries = async () => {
    try {
      setLoading(true);
      const type = selectedType === '' ? undefined : selectedType;
      const result = await kycService.searchWatchlist(searchTerm || '*', type);
      setEntries(result || []);

      // Calculate stats
      const allEntries = await kycService.searchWatchlist('*');
      setStats({
        pep: allEntries.filter((e) => e.listType === WatchlistType.PEP).length,
        sanctions: allEntries.filter((e) => e.listType === WatchlistType.Sanctions).length,
        internal: allEntries.filter((e) => e.listType === WatchlistType.Internal).length,
        total: allEntries.length,
      });
    } catch (error) {
      console.error('Error loading watchlist:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    loadEntries();
  };

  const handleAddEntry = async () => {
    try {
      setProcessing(true);
      await kycService.addWatchlistEntry({
        fullName: newEntry.fullName,
        listType: newEntry.listType,
        position: newEntry.position || undefined,
        jurisdiction: newEntry.jurisdiction || undefined,
        dateOfBirth: newEntry.dateOfBirth || undefined,
        documentNumber: newEntry.documentNumber || undefined,
        remarks: newEntry.remarks || undefined,
      });
      setShowAddModal(false);
      setNewEntry({
        fullName: '',
        listType: WatchlistType.PEP,
        position: '',
        jurisdiction: '',
        dateOfBirth: '',
        documentNumber: '',
        remarks: '',
      });
      loadEntries();
    } catch (error) {
      console.error('Error adding entry:', error);
      alert('Error al agregar entrada');
    } finally {
      setProcessing(false);
    }
  };

  const getTypeBadge = (type: WatchlistType) => {
    const typeMap: Record<WatchlistType, { label: string; color: string; icon: React.ReactNode }> =
      {
        [WatchlistType.PEP]: {
          label: 'PEP',
          color: 'bg-purple-100 text-purple-800',
          icon: <FiUser size={12} />,
        },
        [WatchlistType.Sanctions]: {
          label: 'Sanciones',
          color: 'bg-red-100 text-red-800',
          icon: <FiAlertTriangle size={12} />,
        },
        [WatchlistType.Internal]: {
          label: 'Interna',
          color: 'bg-blue-100 text-blue-800',
          icon: <FiShield size={12} />,
        },
        [WatchlistType.OFAC]: {
          label: 'OFAC',
          color: 'bg-orange-100 text-orange-800',
          icon: <FiGlobe size={12} />,
        },
        [WatchlistType.UN]: {
          label: 'ONU',
          color: 'bg-blue-100 text-blue-800',
          icon: <FiGlobe size={12} />,
        },
        [WatchlistType.EU]: {
          label: 'UE',
          color: 'bg-indigo-100 text-indigo-800',
          icon: <FiGlobe size={12} />,
        },
      };
    const t = typeMap[type] || { label: 'Otro', color: 'bg-gray-100 text-gray-800', icon: null };
    return (
      <span
        className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${t.color}`}
      >
        {t.icon}
        {t.label}
      </span>
    );
  };

  const filteredEntries = entries.filter(
    (e) =>
      e.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      e.documentNumber?.includes(searchTerm)
  );

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="flex justify-between items-center mb-8">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Lista de Vigilancia</h1>
              <p className="text-gray-600 mt-1">
                Gestiona PEPs, Sanciones y listas internas de vigilancia
              </p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => {}}
                className="flex items-center gap-2 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100"
              >
                <FiDownload size={18} />
                Exportar
              </button>
              <button
                onClick={() => setShowAddModal(true)}
                className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                <FiPlus size={18} />
                Agregar Entrada
              </button>
            </div>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Total Entradas</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
                </div>
                <FiShield className="text-blue-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">PEPs</p>
                  <p className="text-2xl font-bold text-purple-600">{stats.pep}</p>
                </div>
                <FiUser className="text-purple-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Sanciones</p>
                  <p className="text-2xl font-bold text-red-600">{stats.sanctions}</p>
                </div>
                <FiAlertTriangle className="text-red-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Lista Interna</p>
                  <p className="text-2xl font-bold text-blue-600">{stats.internal}</p>
                </div>
                <FiShield className="text-blue-500" size={32} />
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="bg-white rounded-lg shadow p-4 mb-6">
            <div className="flex flex-col md:flex-row gap-4">
              <div className="flex-1 relative">
                <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="Buscar por nombre o documento..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
              <div className="flex items-center gap-2">
                <FiFilter className="text-gray-400" />
                <select
                  value={selectedType}
                  onChange={(e) =>
                    setSelectedType(
                      e.target.value === '' ? '' : (Number(e.target.value) as WatchlistType)
                    )
                  }
                  className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Todos los tipos</option>
                  <option value={WatchlistType.PEP}>PEP</option>
                  <option value={WatchlistType.Sanctions}>Sanciones</option>
                  <option value={WatchlistType.Internal}>Lista Interna</option>
                  <option value={WatchlistType.OFAC}>OFAC</option>
                  <option value={WatchlistType.UN}>ONU</option>
                  <option value={WatchlistType.EU}>Unión Europea</option>
                </select>
              </div>
              <button
                onClick={handleSearch}
                className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Buscar
              </button>
            </div>
          </div>

          {/* Entries Table */}
          <div className="bg-white rounded-lg shadow overflow-hidden">
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span className="ml-3 text-gray-600">Cargando entradas...</span>
              </div>
            ) : filteredEntries.length === 0 ? (
              <div className="text-center py-12">
                <FiShield className="mx-auto text-gray-400 mb-4" size={48} />
                <p className="text-gray-500">No hay entradas en la lista de vigilancia</p>
                <button
                  onClick={() => setShowAddModal(true)}
                  className="mt-4 text-blue-600 hover:text-blue-700"
                >
                  Agregar primera entrada
                </button>
              </div>
            ) : (
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Nombre
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tipo
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Posición/Cargo
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Jurisdicción
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Estado
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Agregado
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {filteredEntries.map((entry) => (
                    <tr key={entry.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div className="flex-shrink-0 h-10 w-10 bg-gray-200 rounded-full flex items-center justify-center">
                            <FiUser className="text-gray-500" />
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {entry.fullName}
                            </div>
                            {entry.documentNumber && (
                              <div className="text-sm text-gray-500">{entry.documentNumber}</div>
                            )}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getTypeBadge(entry.listType)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {entry.position || '-'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {entry.jurisdiction || '-'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          className={`px-2 py-1 rounded-full text-xs font-medium ${
                            entry.isActive
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}
                        >
                          {entry.isActive ? 'Activo' : 'Inactivo'}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {entry.createdAt
                          ? new Date(entry.createdAt).toLocaleDateString('es-DO')
                          : '-'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => setSelectedEntry(entry)}
                            className="text-blue-600 hover:text-blue-900 p-2"
                            title="Ver detalles"
                          >
                            <FiEye size={18} />
                          </button>
                          <button
                            onClick={() => {}}
                            className="text-gray-600 hover:text-gray-900 p-2"
                            title="Editar"
                          >
                            <FiEdit2 size={18} />
                          </button>
                          <button
                            onClick={() => {}}
                            className="text-red-600 hover:text-red-900 p-2"
                            title="Eliminar"
                          >
                            <FiTrash2 size={18} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Add Entry Modal */}
          {showAddModal && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-lg w-full mx-4">
                <div className="p-6 border-b">
                  <h2 className="text-xl font-bold text-gray-900">
                    Agregar Entrada a Lista de Vigilancia
                  </h2>
                </div>
                <div className="p-6 space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Nombre Completo *
                    </label>
                    <input
                      type="text"
                      value={newEntry.fullName}
                      onChange={(e) => setNewEntry({ ...newEntry, fullName: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="Juan Pérez García"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Tipo de Lista *
                    </label>
                    <select
                      value={newEntry.listType}
                      onChange={(e) =>
                        setNewEntry({
                          ...newEntry,
                          listType: Number(e.target.value) as WatchlistType,
                        })
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value={WatchlistType.PEP}>
                        PEP (Persona Expuesta Políticamente)
                      </option>
                      <option value={WatchlistType.Sanctions}>Sanciones</option>
                      <option value={WatchlistType.Internal}>Lista Interna</option>
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Posición/Cargo
                      </label>
                      <input
                        type="text"
                        value={newEntry.position}
                        onChange={(e) => setNewEntry({ ...newEntry, position: e.target.value })}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                        placeholder="Senador"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Jurisdicción
                      </label>
                      <input
                        type="text"
                        value={newEntry.jurisdiction}
                        onChange={(e) => setNewEntry({ ...newEntry, jurisdiction: e.target.value })}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                        placeholder="República Dominicana"
                      />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Fecha de Nacimiento
                      </label>
                      <input
                        type="date"
                        value={newEntry.dateOfBirth}
                        onChange={(e) => setNewEntry({ ...newEntry, dateOfBirth: e.target.value })}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Número de Documento
                      </label>
                      <input
                        type="text"
                        value={newEntry.documentNumber}
                        onChange={(e) =>
                          setNewEntry({ ...newEntry, documentNumber: e.target.value })
                        }
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                        placeholder="001-1234567-8"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Observaciones
                    </label>
                    <textarea
                      value={newEntry.remarks}
                      onChange={(e) => setNewEntry({ ...newEntry, remarks: e.target.value })}
                      rows={3}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="Notas adicionales..."
                    />
                  </div>
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => setShowAddModal(false)}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleAddEntry}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                    disabled={!newEntry.fullName || processing}
                  >
                    {processing ? 'Agregando...' : 'Agregar Entrada'}
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Entry Detail Modal */}
          {selectedEntry && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-lg w-full mx-4">
                <div className="p-6 border-b">
                  <div className="flex justify-between items-start">
                    <div>
                      <h2 className="text-xl font-bold text-gray-900">Detalles de Entrada</h2>
                      <p className="text-gray-500">{selectedEntry.fullName}</p>
                    </div>
                    <button
                      onClick={() => setSelectedEntry(null)}
                      className="text-gray-400 hover:text-gray-600"
                    >
                      ✕
                    </button>
                  </div>
                </div>
                <div className="p-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">Nombre</label>
                      <p className="text-gray-900">{selectedEntry.fullName}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Tipo</label>
                      <p>{getTypeBadge(selectedEntry.listType)}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Posición</label>
                      <p className="text-gray-900">{selectedEntry.position || '-'}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Jurisdicción</label>
                      <p className="text-gray-900">{selectedEntry.jurisdiction || '-'}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Documento</label>
                      <p className="text-gray-900">{selectedEntry.documentNumber || '-'}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Estado</label>
                      <p className="text-gray-900">
                        {selectedEntry.isActive ? 'Activo' : 'Inactivo'}
                      </p>
                    </div>
                    {selectedEntry.remarks && (
                      <div className="col-span-2">
                        <label className="text-sm font-medium text-gray-500">Observaciones</label>
                        <p className="text-gray-900">{selectedEntry.remarks}</p>
                      </div>
                    )}
                  </div>
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end">
                  <button
                    onClick={() => setSelectedEntry(null)}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cerrar
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
};

export default WatchlistAdminPage;
