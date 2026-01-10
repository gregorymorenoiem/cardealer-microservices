/**
 * DealerSettingsPage - Configuración del Dealer
 */

import { useState } from 'react';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiUser,
  FiMail,
  FiPhone,
  FiMapPin,
  FiGlobe,
  FiClock,
  FiCamera,
  FiSave,
  FiShield,
  FiBell,
  FiLock,
  FiUsers,
  FiCreditCard,
} from 'react-icons/fi';
import { Building2 } from 'lucide-react';

export default function DealerSettingsPage() {
  const [activeTab, setActiveTab] = useState('profile');

  const tabs = [
    { id: 'profile', label: 'Perfil', icon: Building2 },
    { id: 'users', label: 'Usuarios', icon: FiUsers },
    { id: 'notifications', label: 'Notificaciones', icon: FiBell },
    { id: 'security', label: 'Seguridad', icon: FiShield },
    { id: 'billing', label: 'Facturación', icon: FiCreditCard },
  ];

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Configuración</h1>
          <p className="text-gray-500 mt-1">Administra la configuración de tu cuenta de dealer</p>
        </div>

        <div className="flex flex-col lg:flex-row gap-8">
          {/* Tabs Sidebar */}
          <div className="lg:w-64 flex-shrink-0">
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-2">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-left transition-all ${
                    activeTab === tab.id
                      ? 'bg-blue-50 text-blue-700'
                      : 'text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  <tab.icon className="w-5 h-5" />
                  <span className="font-medium">{tab.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Content */}
          <div className="flex-1">
            {activeTab === 'profile' && (
              <div className="space-y-6">
                {/* Business Info */}
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                  <h2 className="text-lg font-bold text-gray-900 mb-6">Información del Negocio</h2>

                  {/* Logo Upload */}
                  <div className="flex items-center gap-6 mb-8 pb-8 border-b border-gray-100">
                    <div className="w-24 h-24 bg-gradient-to-br from-blue-600 to-emerald-500 rounded-2xl flex items-center justify-center text-white text-3xl font-bold">
                      D
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900 mb-1">Logo del Dealer</h3>
                      <p className="text-sm text-gray-500 mb-3">JPG, PNG o SVG. Máximo 2MB.</p>
                      <button className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50">
                        <FiCamera className="w-4 h-4" />
                        Cambiar Logo
                      </button>
                    </div>
                  </div>

                  <div className="grid md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Nombre del Negocio
                      </label>
                      <input
                        type="text"
                        defaultValue="Auto Elite RD"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">RNC</label>
                      <input
                        type="text"
                        defaultValue="131-12345-6"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                      <input
                        type="email"
                        defaultValue="contacto@autoelite.com.do"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Teléfono
                      </label>
                      <input
                        type="tel"
                        defaultValue="+1 809 555 0100"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Dirección
                      </label>
                      <input
                        type="text"
                        defaultValue="Av. Winston Churchill #45, Piantini, Santo Domingo"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Sitio Web
                      </label>
                      <input
                        type="url"
                        defaultValue="https://autoelite.com.do"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Horario
                      </label>
                      <input
                        type="text"
                        defaultValue="Lun-Sab 9:00 AM - 6:00 PM"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Descripción
                      </label>
                      <textarea
                        rows={4}
                        defaultValue="Somos un dealer premium especializado en vehículos de alta gama. Más de 15 años de experiencia en el mercado dominicano."
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                  </div>

                  <div className="flex justify-end mt-6">
                    <button className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-xl font-semibold hover:bg-blue-700 transition-colors">
                      <FiSave className="w-5 h-5" />
                      Guardar Cambios
                    </button>
                  </div>
                </div>
              </div>
            )}

            {activeTab === 'users' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-lg font-bold text-gray-900">Usuarios del Equipo</h2>
                  <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700">
                    <FiUsers className="w-4 h-4" />
                    Invitar Usuario
                  </button>
                </div>
                <div className="space-y-4">
                  {[
                    { name: 'Juan Pérez', email: 'juan@autoelite.com', role: 'Administrador' },
                    { name: 'María García', email: 'maria@autoelite.com', role: 'Vendedor' },
                    { name: 'Carlos López', email: 'carlos@autoelite.com', role: 'Vendedor' },
                  ].map((user, index) => (
                    <div
                      key={index}
                      className="flex items-center justify-between p-4 bg-gray-50 rounded-xl"
                    >
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center text-blue-600 font-bold">
                          {user.name.charAt(0)}
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900">{user.name}</p>
                          <p className="text-sm text-gray-500">{user.email}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <span className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium">
                          {user.role}
                        </span>
                        <button className="p-2 hover:bg-gray-200 rounded-lg">
                          <FiLock className="w-4 h-4 text-gray-500" />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {activeTab === 'notifications' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-6">
                  Preferencias de Notificaciones
                </h2>
                <div className="space-y-4">
                  {[
                    {
                      title: 'Nuevos leads',
                      description: 'Recibe notificaciones cuando llegue un nuevo lead',
                      enabled: true,
                    },
                    {
                      title: 'Consultas de vehículos',
                      description: 'Notificaciones de consultas sobre tus vehículos',
                      enabled: true,
                    },
                    {
                      title: 'Resumen diario',
                      description: 'Recibe un resumen de actividad cada día',
                      enabled: false,
                    },
                    {
                      title: 'Alertas de inventario',
                      description: 'Cuando un vehículo lleve mucho tiempo sin vistas',
                      enabled: true,
                    },
                    {
                      title: 'Actualizaciones del sistema',
                      description: 'Novedades y mejoras de la plataforma',
                      enabled: false,
                    },
                  ].map((item, index) => (
                    <div
                      key={index}
                      className="flex items-center justify-between p-4 border border-gray-100 rounded-xl"
                    >
                      <div>
                        <p className="font-semibold text-gray-900">{item.title}</p>
                        <p className="text-sm text-gray-500">{item.description}</p>
                      </div>
                      <label className="relative inline-flex items-center cursor-pointer">
                        <input
                          type="checkbox"
                          defaultChecked={item.enabled}
                          className="sr-only peer"
                        />
                        <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
                      </label>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {activeTab === 'security' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-6">Seguridad de la Cuenta</h2>
                <div className="space-y-6">
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Cambiar Contraseña</p>
                        <p className="text-sm text-gray-500">Última actualización hace 30 días</p>
                      </div>
                      <button className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50">
                        Cambiar
                      </button>
                    </div>
                  </div>
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Autenticación de Dos Factores</p>
                        <p className="text-sm text-gray-500">Añade una capa extra de seguridad</p>
                      </div>
                      <button className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700">
                        Activar
                      </button>
                    </div>
                  </div>
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Sesiones Activas</p>
                        <p className="text-sm text-gray-500">3 dispositivos conectados</p>
                      </div>
                      <button className="px-4 py-2 text-red-600 border border-red-200 rounded-xl text-sm font-medium hover:bg-red-50">
                        Cerrar Todas
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {activeTab === 'billing' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-6">Información de Facturación</h2>
                <div className="space-y-6">
                  <div className="p-4 bg-gradient-to-r from-blue-50 to-emerald-50 border border-blue-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Plan Actual: Pro</p>
                        <p className="text-sm text-gray-500">$129/mes • Renovación: 15 Feb 2026</p>
                      </div>
                      <button className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700">
                        Actualizar Plan
                      </button>
                    </div>
                  </div>
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div className="p-3 bg-gray-100 rounded-xl">
                          <FiCreditCard className="w-6 h-6 text-gray-600" />
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900">Visa •••• 4242</p>
                          <p className="text-sm text-gray-500">Expira 12/27</p>
                        </div>
                      </div>
                      <button className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50">
                        Editar
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </DealerPortalLayout>
  );
}
