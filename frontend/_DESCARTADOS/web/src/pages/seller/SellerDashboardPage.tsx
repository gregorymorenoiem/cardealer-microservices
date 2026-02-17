/**
 * Seller Dashboard Page
 * Dashboard for individual sellers to manage their listings
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import { useAuth } from '@/hooks/useAuth';
import {
  FiPlus,
  FiEye,
  FiMessageSquare,
  FiDollarSign,
  FiTrendingUp,
  FiClock,
  FiCheckCircle,
  FiAlertCircle,
  FiEdit,
  FiTrash2,
  FiMoreVertical,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';

interface VehicleListing {
  id: string;
  title: string;
  price: number;
  status: 'active' | 'pending' | 'sold' | 'expired';
  views: number;
  inquiries: number;
  createdAt: string;
  image: string;
}

// Mock data - en producción vendría del API
const mockListings: VehicleListing[] = [
  {
    id: '1',
    title: '2022 Toyota Camry SE',
    price: 1250000,
    status: 'active',
    views: 245,
    inquiries: 12,
    createdAt: '2026-01-05',
    image: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400',
  },
  {
    id: '2',
    title: '2020 Honda Accord Sport',
    price: 980000,
    status: 'pending',
    views: 0,
    inquiries: 0,
    createdAt: '2026-01-08',
    image: 'https://images.unsplash.com/photo-1606611013016-96f8c4a7d3b0?w=400',
  },
];

const statusConfig = {
  active: { label: 'Activo', color: 'bg-green-100 text-green-700', icon: FiCheckCircle },
  pending: { label: 'Pendiente', color: 'bg-yellow-100 text-yellow-700', icon: FiClock },
  sold: { label: 'Vendido', color: 'bg-blue-100 text-blue-700', icon: FiDollarSign },
  expired: { label: 'Expirado', color: 'bg-red-100 text-red-700', icon: FiAlertCircle },
};

export default function SellerDashboardPage() {
  const { user } = useAuth();
  const [listings] = useState<VehicleListing[]>(mockListings);

  const stats = [
    {
      label: 'Publicaciones Activas',
      value: listings.filter((l) => l.status === 'active').length,
      icon: FaCar,
      color: 'text-blue-600 bg-blue-100',
    },
    {
      label: 'Vistas Totales',
      value: listings.reduce((acc, l) => acc + l.views, 0),
      icon: FiEye,
      color: 'text-purple-600 bg-purple-100',
    },
    {
      label: 'Consultas Recibidas',
      value: listings.reduce((acc, l) => acc + l.inquiries, 0),
      icon: FiMessageSquare,
      color: 'text-green-600 bg-green-100',
    },
    {
      label: 'Valor Total',
      value: `RD$${(listings.reduce((acc, l) => acc + l.price, 0) / 1000000).toFixed(1)}M`,
      icon: FiDollarSign,
      color: 'text-amber-600 bg-amber-100',
    },
  ];

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Header */}
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">
                Mis Vehículos en Venta
              </h1>
              <p className="text-gray-600 mt-1">
                Hola, {user?.name || 'Vendedor'}. Gestiona tus publicaciones.
              </p>
            </div>
            <Link
              to="/sell"
              className="inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl hover:scale-105 transition-all"
            >
              <FiPlus className="w-5 h-5" />
              Publicar Vehículo
            </Link>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
            {stats.map((stat, index) => (
              <div
                key={index}
                className="bg-white rounded-xl p-4 sm:p-6 shadow-sm border border-gray-100"
              >
                <div className="flex items-center gap-3">
                  <div className={`p-3 rounded-xl ${stat.color}`}>
                    <stat.icon className="w-5 h-5 sm:w-6 sm:h-6" />
                  </div>
                  <div>
                    <p className="text-xl sm:text-2xl font-bold text-gray-900">{stat.value}</p>
                    <p className="text-xs sm:text-sm text-gray-500">{stat.label}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Tips Banner */}
          <div className="bg-gradient-to-r from-purple-600 to-indigo-600 rounded-2xl p-6 mb-8 text-white">
            <div className="flex items-start gap-4">
              <div className="p-3 bg-white/20 rounded-xl">
                <FiTrendingUp className="w-6 h-6" />
              </div>
              <div>
                <h3 className="font-semibold text-lg">Tips para vender más rápido</h3>
                <ul className="mt-2 space-y-1 text-purple-100 text-sm">
                  <li>• Agrega fotos de alta calidad (mínimo 5 fotos)</li>
                  <li>• Completa todos los detalles del vehículo</li>
                  <li>• Responde rápido a las consultas</li>
                  <li>• Mantén tu precio competitivo con el mercado</li>
                </ul>
              </div>
            </div>
          </div>

          {/* Listings */}
          <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100">
              <h2 className="text-lg font-semibold text-gray-900">Mis Publicaciones</h2>
            </div>

            {listings.length === 0 ? (
              <div className="p-12 text-center">
                <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FaCar className="w-8 h-8 text-gray-400" />
                </div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                  No tienes publicaciones aún
                </h3>
                <p className="text-gray-500 mb-6">
                  Comienza a vender tu vehículo publicándolo en OKLA
                </p>
                <Link
                  to="/sell"
                  className="inline-flex items-center gap-2 px-6 py-3 bg-purple-600 text-white rounded-xl font-semibold hover:bg-purple-700 transition-colors"
                >
                  <FiPlus className="w-5 h-5" />
                  Publicar mi primer vehículo
                </Link>
              </div>
            ) : (
              <div className="divide-y divide-gray-100">
                {listings.map((listing) => {
                  const StatusIcon = statusConfig[listing.status].icon;
                  return (
                    <div key={listing.id} className="p-4 sm:p-6 hover:bg-gray-50 transition-colors">
                      <div className="flex flex-col sm:flex-row gap-4">
                        {/* Image */}
                        <div className="w-full sm:w-40 h-32 sm:h-28 rounded-xl overflow-hidden flex-shrink-0">
                          <img
                            src={listing.image}
                            alt={listing.title}
                            className="w-full h-full object-cover"
                          />
                        </div>

                        {/* Info */}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-start justify-between gap-4">
                            <div>
                              <h3 className="font-semibold text-gray-900 truncate">
                                {listing.title}
                              </h3>
                              <p className="text-lg font-bold text-purple-600 mt-1">
                                RD${listing.price.toLocaleString()}
                              </p>
                            </div>
                            <span
                              className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-medium ${statusConfig[listing.status].color}`}
                            >
                              <StatusIcon className="w-3.5 h-3.5" />
                              {statusConfig[listing.status].label}
                            </span>
                          </div>

                          <div className="flex items-center gap-4 mt-3 text-sm text-gray-500">
                            <span className="flex items-center gap-1">
                              <FiEye className="w-4 h-4" />
                              {listing.views} vistas
                            </span>
                            <span className="flex items-center gap-1">
                              <FiMessageSquare className="w-4 h-4" />
                              {listing.inquiries} consultas
                            </span>
                            <span className="flex items-center gap-1">
                              <FiClock className="w-4 h-4" />
                              {new Date(listing.createdAt).toLocaleDateString('es-DO')}
                            </span>
                          </div>

                          {/* Actions */}
                          <div className="flex items-center gap-2 mt-4">
                            <Link
                              to={`/vehicles/${listing.id}/edit`}
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                            >
                              <FiEdit className="w-4 h-4" />
                              Editar
                            </Link>
                            <button className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-red-600 bg-red-50 rounded-lg hover:bg-red-100 transition-colors">
                              <FiTrash2 className="w-4 h-4" />
                              Eliminar
                            </button>
                            <button className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">
                              <FiMoreVertical className="w-5 h-5" />
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* Help Section */}
          <div className="mt-8 bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <h3 className="font-semibold text-gray-900 mb-4">¿Necesitas ayuda?</h3>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <Link
                to="/help/selling"
                className="p-4 rounded-xl border border-gray-200 hover:border-purple-300 hover:bg-purple-50 transition-colors"
              >
                <h4 className="font-medium text-gray-900">Guía de venta</h4>
                <p className="text-sm text-gray-500 mt-1">Aprende a vender tu vehículo rápido</p>
              </Link>
              <Link
                to="/help/pricing"
                className="p-4 rounded-xl border border-gray-200 hover:border-purple-300 hover:bg-purple-50 transition-colors"
              >
                <h4 className="font-medium text-gray-900">Pricing inteligente</h4>
                <p className="text-sm text-gray-500 mt-1">Encuentra el precio ideal</p>
              </Link>
              <Link
                to="/contact"
                className="p-4 rounded-xl border border-gray-200 hover:border-purple-300 hover:bg-purple-50 transition-colors"
              >
                <h4 className="font-medium text-gray-900">Contactar soporte</h4>
                <p className="text-sm text-gray-500 mt-1">Estamos para ayudarte</p>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
