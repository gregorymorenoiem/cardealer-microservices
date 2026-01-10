/**
 * DealerLeadsPage - Vista de Leads con el nuevo layout
 *
 * Wrapper que integra LeadsDashboard con DealerPortalLayout
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiTarget,
  FiSearch,
  FiFilter,
  FiPhone,
  FiMail,
  FiMessageSquare,
  FiArrowRight,
  FiMoreVertical,
  FiClock,
  FiTrendingUp,
} from 'react-icons/fi';
import { FaWhatsapp } from 'react-icons/fa';

// Mock data
const mockLeads = [
  {
    id: '1',
    name: 'Juan Pérez',
    email: 'juan.perez@email.com',
    phone: '+1 809 555 0101',
    temperature: 'hot',
    score: 92,
    vehicle: 'Toyota Corolla 2022',
    lastContact: 'Hace 5 minutos',
    source: 'WhatsApp',
    actions: 12,
  },
  {
    id: '2',
    name: 'María García',
    email: 'maria.garcia@email.com',
    phone: '+1 809 555 0102',
    temperature: 'warm',
    score: 68,
    vehicle: 'Honda Civic 2021',
    lastContact: 'Hace 2 horas',
    source: 'Web',
    actions: 8,
  },
  {
    id: '3',
    name: 'Carlos Méndez',
    email: 'carlos.mendez@email.com',
    phone: '+1 809 555 0103',
    temperature: 'cold',
    score: 35,
    vehicle: 'BMW X5 2023',
    lastContact: 'Hace 3 días',
    source: 'Email',
    actions: 3,
  },
];

export default function DealerLeadsPage() {
  const [leads] = useState(mockLeads);
  const [filterTemp, setFilterTemp] = useState<'all' | 'hot' | 'warm' | 'cold'>('all');
  const [searchTerm, setSearchTerm] = useState('');

  const filteredLeads = leads.filter((l) => {
    const matchesSearch =
      l.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      l.vehicle.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesTemp = filterTemp === 'all' || l.temperature === filterTemp;
    return matchesSearch && matchesTemp;
  });

  const getTemperatureBadge = (temp: string, score: number) => {
    const badges: Record<string, { bg: string; text: string; icon: string }> = {
      hot: { bg: 'bg-gradient-to-r from-red-500 to-orange-500', text: 'text-white', icon: '🔥' },
      warm: {
        bg: 'bg-gradient-to-r from-amber-400 to-yellow-400',
        text: 'text-amber-900',
        icon: '☀️',
      },
      cold: { bg: 'bg-gradient-to-r from-blue-400 to-cyan-400', text: 'text-white', icon: '❄️' },
    };
    const badge = badges[temp] || badges.cold;
    return (
      <div
        className={`${badge.bg} ${badge.text} px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1`}
      >
        <span>{badge.icon}</span>
        <span>{score}</span>
      </div>
    );
  };

  const stats = {
    total: leads.length,
    hot: leads.filter((l) => l.temperature === 'hot').length,
    warm: leads.filter((l) => l.temperature === 'warm').length,
    cold: leads.filter((l) => l.temperature === 'cold').length,
  };

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 space-y-6">
        {/* Header */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Leads</h1>
            <p className="text-gray-500 mt-1">Gestiona y da seguimiento a tus leads</p>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-white rounded-2xl p-5 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-2">
              <span className="text-gray-500 text-sm">Total Leads</span>
              <FiTarget className="w-5 h-5 text-blue-600" />
            </div>
            <p className="text-3xl font-bold text-gray-900">{stats.total}</p>
          </div>
          <div className="bg-gradient-to-br from-red-500 to-orange-500 rounded-2xl p-5 text-white">
            <div className="flex items-center justify-between mb-2">
              <span className="text-white/80 text-sm">Calientes 🔥</span>
            </div>
            <p className="text-3xl font-bold">{stats.hot}</p>
          </div>
          <div className="bg-gradient-to-br from-amber-400 to-yellow-400 rounded-2xl p-5 text-amber-900">
            <div className="flex items-center justify-between mb-2">
              <span className="text-amber-800/80 text-sm">Tibios ☀️</span>
            </div>
            <p className="text-3xl font-bold">{stats.warm}</p>
          </div>
          <div className="bg-gradient-to-br from-blue-400 to-cyan-400 rounded-2xl p-5 text-white">
            <div className="flex items-center justify-between mb-2">
              <span className="text-white/80 text-sm">Fríos ❄️</span>
            </div>
            <p className="text-3xl font-bold">{stats.cold}</p>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100">
          <div className="flex flex-col lg:flex-row lg:items-center gap-4">
            <div className="relative flex-1">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar leads..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>
            <div className="flex items-center gap-2">
              {(['all', 'hot', 'warm', 'cold'] as const).map((temp) => (
                <button
                  key={temp}
                  onClick={() => setFilterTemp(temp)}
                  className={`px-4 py-2 rounded-xl text-sm font-medium transition-all ${
                    filterTemp === temp
                      ? 'bg-blue-600 text-white shadow-lg'
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
                >
                  {temp === 'all'
                    ? 'Todos'
                    : temp === 'hot'
                      ? '🔥 Calientes'
                      : temp === 'warm'
                        ? '☀️ Tibios'
                        : '❄️ Fríos'}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Leads List */}
        <div className="space-y-4">
          {filteredLeads.map((lead) => (
            <div
              key={lead.id}
              className="bg-white rounded-2xl p-5 shadow-sm border border-gray-100 hover:shadow-md transition-all"
            >
              <div className="flex flex-col lg:flex-row lg:items-center gap-4">
                {/* Lead Info */}
                <div className="flex items-center gap-4 flex-1">
                  <div className="w-14 h-14 bg-gradient-to-br from-blue-600 to-emerald-500 rounded-full flex items-center justify-center text-white text-xl font-bold">
                    {lead.name.charAt(0)}
                  </div>
                  <div>
                    <div className="flex items-center gap-2 mb-1">
                      <h3 className="font-bold text-gray-900">{lead.name}</h3>
                      {getTemperatureBadge(lead.temperature, lead.score)}
                    </div>
                    <p className="text-sm text-gray-500 mb-1">
                      Interesado en: <span className="font-medium">{lead.vehicle}</span>
                    </p>
                    <div className="flex items-center gap-3 text-xs text-gray-400">
                      <span className="flex items-center gap-1">
                        <FiClock className="w-3 h-3" /> {lead.lastContact}
                      </span>
                      <span className="flex items-center gap-1">
                        <FiTrendingUp className="w-3 h-3" /> {lead.actions} acciones
                      </span>
                    </div>
                  </div>
                </div>

                {/* Contact Info */}
                <div className="flex items-center gap-2 text-sm text-gray-500">
                  <span className="flex items-center gap-1">
                    <FiMail className="w-4 h-4" /> {lead.email}
                  </span>
                  <span className="hidden lg:flex items-center gap-1">
                    <FiPhone className="w-4 h-4" /> {lead.phone}
                  </span>
                </div>

                {/* Actions */}
                <div className="flex items-center gap-2">
                  <button
                    className="p-2.5 bg-green-100 hover:bg-green-200 rounded-xl transition-colors"
                    title="WhatsApp"
                  >
                    <FaWhatsapp className="w-5 h-5 text-green-600" />
                  </button>
                  <button
                    className="p-2.5 bg-blue-100 hover:bg-blue-200 rounded-xl transition-colors"
                    title="Llamar"
                  >
                    <FiPhone className="w-5 h-5 text-blue-600" />
                  </button>
                  <button
                    className="p-2.5 bg-purple-100 hover:bg-purple-200 rounded-xl transition-colors"
                    title="Email"
                  >
                    <FiMail className="w-5 h-5 text-purple-600" />
                  </button>
                  <Link
                    to={`/dealer/leads/${lead.id}`}
                    className="flex items-center gap-2 px-4 py-2.5 bg-gray-100 hover:bg-gray-200 rounded-xl text-sm font-medium transition-colors"
                  >
                    Ver Detalle
                    <FiArrowRight className="w-4 h-4" />
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>

        {filteredLeads.length === 0 && (
          <div className="text-center py-16 bg-white rounded-2xl">
            <FiTarget className="w-16 h-16 mx-auto text-gray-300 mb-4" />
            <h3 className="text-xl font-bold text-gray-900 mb-2">No hay leads</h3>
            <p className="text-gray-500">Los leads aparecerán aquí cuando recibas consultas</p>
          </div>
        )}
      </div>
    </DealerPortalLayout>
  );
}
