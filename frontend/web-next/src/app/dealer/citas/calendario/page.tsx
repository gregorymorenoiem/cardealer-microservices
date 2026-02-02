/**
 * Dealer Calendar View Page
 *
 * Calendar view for appointments and test drives
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  ArrowLeft,
  ChevronLeft,
  ChevronRight,
  Plus,
  Calendar,
  Clock,
  User,
  Car,
  Phone,
  MapPin,
} from 'lucide-react';
import Link from 'next/link';

const appointments = [
  {
    id: '1',
    date: '2024-02-15',
    time: '10:00',
    customer: 'Juan Martínez',
    phone: '809-555-0123',
    vehicle: 'Toyota RAV4 2023',
    type: 'test-drive',
    location: 'Sucursal Principal',
    status: 'confirmed',
  },
  {
    id: '2',
    date: '2024-02-15',
    time: '14:30',
    customer: 'María García',
    phone: '809-555-0456',
    vehicle: 'Honda CR-V 2022',
    type: 'viewing',
    location: 'Sucursal Principal',
    status: 'pending',
  },
  {
    id: '3',
    date: '2024-02-16',
    time: '11:00',
    customer: 'Carlos López',
    phone: '809-555-0789',
    vehicle: 'Hyundai Tucson 2023',
    type: 'test-drive',
    location: 'Sucursal Santiago',
    status: 'confirmed',
  },
  {
    id: '4',
    date: '2024-02-17',
    time: '09:00',
    customer: 'Ana Rodríguez',
    phone: '809-555-0111',
    vehicle: 'Mazda CX-5 2022',
    type: 'negotiation',
    location: 'Sucursal Principal',
    status: 'confirmed',
  },
];

const weekDays = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
const currentMonth = 'Febrero 2024';

// Generate calendar days (simplified)
const calendarDays = Array.from({ length: 35 }, (_, i) => {
  const day = i - 3; // Start from previous month
  return {
    day: day > 0 && day <= 29 ? day : null,
    isToday: day === 15,
    hasAppointments: [15, 16, 17, 20, 22].includes(day),
  };
});

export default function CalendarPage() {
  const [selectedDate, setSelectedDate] = useState('2024-02-15');

  const filteredAppointments = appointments.filter(apt => apt.date === selectedDate);

  const getTypeBadge = (type: string) => {
    switch (type) {
      case 'test-drive':
        return <Badge className="bg-blue-100 text-blue-700">Test Drive</Badge>;
      case 'viewing':
        return <Badge className="bg-purple-100 text-purple-700">Visita</Badge>;
      case 'negotiation':
        return <Badge className="bg-orange-100 text-orange-700">Negociación</Badge>;
      default:
        return <Badge variant="outline">{type}</Badge>;
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'confirmed':
        return <Badge className="bg-emerald-100 text-emerald-700">Confirmada</Badge>;
      case 'pending':
        return <Badge className="bg-yellow-100 text-yellow-700">Pendiente</Badge>;
      case 'cancelled':
        return <Badge className="bg-red-100 text-red-700">Cancelada</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/dealer/citas">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Calendario</h1>
            <p className="text-gray-600">Vista de calendario para citas</p>
          </div>
        </div>
        <Button className="bg-emerald-600 hover:bg-emerald-700">
          <Plus className="mr-2 h-4 w-4" />
          Nueva Cita
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Calendar */}
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              {currentMonth}
            </CardTitle>
            <div className="flex items-center gap-2">
              <Button variant="outline" size="icon">
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button variant="outline" size="sm">
                Hoy
              </Button>
              <Button variant="outline" size="icon">
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {/* Week Days Header */}
            <div className="mb-2 grid grid-cols-7 gap-1">
              {weekDays.map(day => (
                <div key={day} className="py-2 text-center text-sm font-medium text-gray-500">
                  {day}
                </div>
              ))}
            </div>

            {/* Calendar Grid */}
            <div className="grid grid-cols-7 gap-1">
              {calendarDays.map((item, index) => (
                <button
                  key={index}
                  className={`relative aspect-square rounded-lg p-2 text-sm transition-colors ${item.day ? 'hover:bg-gray-100' : 'text-gray-300'} ${item.isToday ? 'bg-emerald-100 font-bold' : ''} ${
                    item.day && selectedDate === `2024-02-${String(item.day).padStart(2, '0')}`
                      ? 'ring-2 ring-emerald-500'
                      : ''
                  } `}
                  onClick={() =>
                    item.day && setSelectedDate(`2024-02-${String(item.day).padStart(2, '0')}`)
                  }
                  disabled={!item.day}
                >
                  {item.day}
                  {item.hasAppointments && item.day && (
                    <span className="absolute bottom-1 left-1/2 h-1.5 w-1.5 -translate-x-1/2 rounded-full bg-emerald-500" />
                  )}
                </button>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Day Details */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">
              {new Date(selectedDate).toLocaleDateString('es-DO', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
              })}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {filteredAppointments.length > 0 ? (
              <div className="space-y-4">
                {filteredAppointments.map(apt => (
                  <div key={apt.id} className="rounded-lg border border-gray-100 bg-gray-50 p-4">
                    <div className="mb-3 flex items-start justify-between">
                      <div className="flex items-center gap-2">
                        <Clock className="h-4 w-4 text-gray-400" />
                        <span className="font-medium">{apt.time}</span>
                      </div>
                      {getStatusBadge(apt.status)}
                    </div>

                    <div className="space-y-2 text-sm">
                      <div className="flex items-center gap-2">
                        <User className="h-4 w-4 text-gray-400" />
                        <span>{apt.customer}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <Phone className="h-4 w-4 text-gray-400" />
                        <span>{apt.phone}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <Car className="h-4 w-4 text-gray-400" />
                        <span>{apt.vehicle}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <MapPin className="h-4 w-4 text-gray-400" />
                        <span>{apt.location}</span>
                      </div>
                    </div>

                    <div className="mt-3 flex items-center justify-between border-t pt-3">
                      {getTypeBadge(apt.type)}
                      <Button variant="outline" size="sm">
                        Ver detalles
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="py-8 text-center text-gray-400">
                <Calendar className="mx-auto mb-4 h-12 w-12 opacity-50" />
                <p className="font-medium">Sin citas programadas</p>
                <p className="text-sm">Selecciona otro día o crea una nueva cita</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Upcoming Appointments */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Próximas Citas</CardTitle>
            <Select defaultValue="week">
              <SelectTrigger className="w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="today">Hoy</SelectItem>
                <SelectItem value="week">Esta semana</SelectItem>
                <SelectItem value="month">Este mes</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {appointments.map(apt => (
              <div
                key={apt.id}
                className="flex items-center justify-between rounded-lg bg-gray-50 p-4"
              >
                <div className="flex items-center gap-4">
                  <div className="text-center">
                    <p className="text-2xl font-bold">{new Date(apt.date).getDate()}</p>
                    <p className="text-xs text-gray-500">
                      {new Date(apt.date).toLocaleDateString('es-DO', { month: 'short' })}
                    </p>
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <p className="font-medium">{apt.customer}</p>
                      {getTypeBadge(apt.type)}
                    </div>
                    <p className="text-sm text-gray-500">{apt.vehicle}</p>
                    <p className="text-xs text-gray-400">
                      {apt.time} • {apt.location}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">{getStatusBadge(apt.status)}</div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
