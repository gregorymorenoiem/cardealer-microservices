/**
 * Admin Content Page
 *
 * Manage static content, banners, and CMS
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import {
  FileText,
  Image,
  Video,
  Plus,
  Edit,
  Trash2,
  Eye,
  Calendar,
  Globe,
  LayoutGrid,
  Megaphone,
  Settings,
  Search,
} from 'lucide-react';

const mockContent = {
  banners: [
    {
      id: '1',
      title: 'Promo Verano 2024',
      placement: 'homepage_hero',
      status: 'active',
      views: 45000,
    },
    {
      id: '2',
      title: 'Financiamiento 0%',
      placement: 'search_sidebar',
      status: 'active',
      views: 12000,
    },
    { id: '3', title: 'Black Friday', placement: 'homepage_hero', status: 'scheduled', views: 0 },
  ],
  pages: [
    {
      id: '1',
      title: 'Términos y Condiciones',
      slug: '/terminos',
      lastUpdate: '2024-01-15',
      status: 'published',
    },
    {
      id: '2',
      title: 'Política de Privacidad',
      slug: '/privacidad',
      lastUpdate: '2024-01-15',
      status: 'published',
    },
    {
      id: '3',
      title: 'Preguntas Frecuentes',
      slug: '/faq',
      lastUpdate: '2024-02-10',
      status: 'published',
    },
    {
      id: '4',
      title: 'Cómo Funciona',
      slug: '/como-funciona',
      lastUpdate: '2024-02-05',
      status: 'published',
    },
    {
      id: '5',
      title: 'Guía para Dealers',
      slug: '/guia-dealers',
      lastUpdate: '2024-01-20',
      status: 'draft',
    },
  ],
  blogPosts: [
    {
      id: '1',
      title: '10 Tips para Comprar tu Primer Vehículo',
      author: 'Admin',
      date: '2024-02-10',
      status: 'published',
    },
    {
      id: '2',
      title: 'Mejores SUVs del 2024',
      author: 'Admin',
      date: '2024-02-08',
      status: 'published',
    },
    {
      id: '3',
      title: 'Guía de Financiamiento',
      author: 'Admin',
      date: '2024-02-01',
      status: 'draft',
    },
  ],
};

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'active':
    case 'published':
      return <Badge className="bg-emerald-100 text-emerald-700">Activo</Badge>;
    case 'scheduled':
      return <Badge className="bg-blue-100 text-blue-700">Programado</Badge>;
    case 'draft':
      return <Badge className="bg-gray-100 text-gray-700">Borrador</Badge>;
    case 'inactive':
      return <Badge variant="outline">Inactivo</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

export default function AdminContentPage() {
  const [activeTab, setActiveTab] = useState<'banners' | 'pages' | 'blog'>('banners');

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Contenido</h1>
          <p className="text-gray-400">Gestión de banners, páginas y blog</p>
        </div>
        <Button className="bg-emerald-600 hover:bg-emerald-700">
          <Plus className="mr-2 h-4 w-4" />
          Nuevo Contenido
        </Button>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 border-b pb-2">
        <Button
          variant={activeTab === 'banners' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('banners')}
          className={activeTab === 'banners' ? 'bg-emerald-600' : ''}
        >
          <Megaphone className="mr-2 h-4 w-4" />
          Banners
        </Button>
        <Button
          variant={activeTab === 'pages' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('pages')}
          className={activeTab === 'pages' ? 'bg-emerald-600' : ''}
        >
          <FileText className="mr-2 h-4 w-4" />
          Páginas
        </Button>
        <Button
          variant={activeTab === 'blog' ? 'default' : 'ghost'}
          onClick={() => setActiveTab('blog')}
          className={activeTab === 'blog' ? 'bg-emerald-600' : ''}
        >
          <Globe className="mr-2 h-4 w-4" />
          Blog
        </Button>
      </div>

      {/* Banners Tab */}
      {activeTab === 'banners' && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle>Banners Publicitarios</CardTitle>
              <CardDescription>Gestiona los banners en el sitio</CardDescription>
            </div>
            <Button className="bg-emerald-600 hover:bg-emerald-700">
              <Plus className="mr-2 h-4 w-4" />
              Nuevo Banner
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {mockContent.banners.map(banner => (
                <div
                  key={banner.id}
                  className="flex items-center justify-between rounded-lg border p-4"
                >
                  <div className="flex items-center gap-4">
                    <div className="flex h-16 w-32 items-center justify-center rounded bg-gradient-to-r from-gray-200 to-gray-300">
                      <Image className="h-6 w-6 text-gray-400" />
                    </div>
                    <div>
                      <h4 className="font-medium">{banner.title}</h4>
                      <p className="text-sm text-gray-500">
                        Ubicación: {banner.placement.replace('_', ' ')}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <p className="font-medium">{banner.views.toLocaleString()} vistas</p>
                      {getStatusBadge(banner.status)}
                    </div>
                    <div className="flex gap-1">
                      <Button variant="ghost" size="icon">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" className="text-red-500">
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Pages Tab */}
      {activeTab === 'pages' && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle>Páginas Estáticas</CardTitle>
              <CardDescription>Contenido legal y páginas informativas</CardDescription>
            </div>
            <Button className="bg-emerald-600 hover:bg-emerald-700">
              <Plus className="mr-2 h-4 w-4" />
              Nueva Página
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {mockContent.pages.map(page => (
                <div
                  key={page.id}
                  className="flex items-center justify-between rounded-lg border p-4 hover:bg-gray-50"
                >
                  <div className="flex items-center gap-4">
                    <div className="rounded-lg bg-gray-100 p-2">
                      <FileText className="h-5 w-5 text-gray-600" />
                    </div>
                    <div>
                      <h4 className="font-medium">{page.title}</h4>
                      <p className="text-sm text-gray-500">{page.slug}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <p className="text-sm text-gray-500">
                        Actualizado: {new Date(page.lastUpdate).toLocaleDateString('es-DO')}
                      </p>
                      {getStatusBadge(page.status)}
                    </div>
                    <div className="flex gap-1">
                      <Button variant="ghost" size="icon">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <Edit className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Blog Tab */}
      {activeTab === 'blog' && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle>Artículos del Blog</CardTitle>
              <CardDescription>Contenido editorial y guías</CardDescription>
            </div>
            <Button className="bg-emerald-600 hover:bg-emerald-700">
              <Plus className="mr-2 h-4 w-4" />
              Nuevo Artículo
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {mockContent.blogPosts.map(post => (
                <div
                  key={post.id}
                  className="flex items-center justify-between rounded-lg border p-4 hover:bg-gray-50"
                >
                  <div className="flex items-center gap-4">
                    <div className="flex h-14 w-20 items-center justify-center rounded bg-gray-100">
                      <Image className="h-5 w-5 text-gray-400" />
                    </div>
                    <div>
                      <h4 className="font-medium">{post.title}</h4>
                      <p className="text-sm text-gray-500">
                        Por {post.author} • {new Date(post.date).toLocaleDateString('es-DO')}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    {getStatusBadge(post.status)}
                    <div className="flex gap-1">
                      <Button variant="ghost" size="icon">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon">
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" className="text-red-500">
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Content Stats */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <Megaphone className="h-8 w-8 text-purple-500" />
              <div>
                <p className="text-2xl font-bold">{mockContent.banners.length}</p>
                <p className="text-sm text-gray-500">Banners</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <FileText className="h-8 w-8 text-blue-500" />
              <div>
                <p className="text-2xl font-bold">{mockContent.pages.length}</p>
                <p className="text-sm text-gray-500">Páginas</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <Globe className="h-8 w-8 text-emerald-500" />
              <div>
                <p className="text-2xl font-bold">{mockContent.blogPosts.length}</p>
                <p className="text-sm text-gray-500">Artículos</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <Eye className="h-8 w-8 text-amber-500" />
              <div>
                <p className="text-2xl font-bold">57K</p>
                <p className="text-sm text-gray-500">Vistas Totales</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
