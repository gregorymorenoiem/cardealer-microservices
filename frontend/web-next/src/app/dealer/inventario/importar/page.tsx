/**
 * Dealer CSV Import Page
 *
 * Bulk import vehicles from CSV/Excel files
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  ArrowLeft,
  Upload,
  FileSpreadsheet,
  Download,
  CheckCircle,
  AlertCircle,
  XCircle,
  FileText,
  RefreshCw,
} from 'lucide-react';
import Link from 'next/link';

const importHistory = [
  {
    id: '1',
    filename: 'inventario_febrero_2024.csv',
    date: '2024-02-15',
    totalRecords: 25,
    successful: 23,
    failed: 2,
    status: 'completed',
  },
  {
    id: '2',
    filename: 'nuevos_vehiculos.xlsx',
    date: '2024-02-10',
    totalRecords: 10,
    successful: 10,
    failed: 0,
    status: 'completed',
  },
];

export default function DealerImportPage() {
  const [dragActive, setDragActive] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [importing, setImporting] = useState(false);

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/dealer/inventario">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-5 w-5" />
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold">Importar Vehículos</h1>
          <p className="text-gray-600">Carga masiva desde CSV o Excel</p>
        </div>
      </div>

      {/* Instructions */}
      <Card className="border-emerald-200 bg-emerald-50">
        <CardContent className="p-6">
          <div className="flex items-start gap-4">
            <FileSpreadsheet className="h-8 w-8 flex-shrink-0 text-emerald-600" />
            <div>
              <h3 className="mb-2 font-semibold text-emerald-900">¿Primera vez importando?</h3>
              <p className="mb-3 text-sm text-emerald-700">
                Descarga nuestra plantilla para asegurarte de que tu archivo tenga el formato
                correcto. Incluye todas las columnas requeridas y ejemplos de datos.
              </p>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" className="border-emerald-300">
                  <Download className="mr-2 h-4 w-4" />
                  Descargar Plantilla CSV
                </Button>
                <Button variant="outline" size="sm" className="border-emerald-300">
                  <Download className="mr-2 h-4 w-4" />
                  Descargar Plantilla Excel
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Upload Area */}
      <Card>
        <CardHeader>
          <CardTitle>Subir Archivo</CardTitle>
          <CardDescription>
            Formatos soportados: CSV, XLSX. Máximo 1000 registros por archivo.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div
            className={`rounded-lg border-2 border-dashed p-8 text-center transition-colors ${
              dragActive
                ? 'border-emerald-400 bg-emerald-50'
                : 'border-gray-200 hover:border-gray-300'
            }`}
            onDragEnter={handleDrag}
            onDragLeave={handleDrag}
            onDragOver={handleDrag}
          >
            <Upload
              className={`mx-auto mb-4 h-12 w-12 ${dragActive ? 'text-emerald-500' : 'text-gray-400'}`}
            />

            {uploadedFile ? (
              <div className="space-y-4">
                <div className="inline-flex items-center gap-2 rounded-lg bg-gray-100 px-4 py-2">
                  <FileText className="h-5 w-5 text-gray-600" />
                  <span className="font-medium">{uploadedFile.name}</span>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 w-6 p-0"
                    onClick={() => setUploadedFile(null)}
                  >
                    <XCircle className="h-4 w-4" />
                  </Button>
                </div>
                <Button
                  className="bg-emerald-600 hover:bg-emerald-700"
                  disabled={importing}
                  onClick={() => setImporting(true)}
                >
                  {importing ? (
                    <>
                      <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                      Importando...
                    </>
                  ) : (
                    'Iniciar Importación'
                  )}
                </Button>
              </div>
            ) : (
              <>
                <p className="mb-2 text-gray-600">
                  Arrastra tu archivo aquí o haz clic para seleccionar
                </p>
                <p className="mb-4 text-sm text-gray-400">CSV o Excel (.xlsx)</p>
                <Button variant="outline">
                  <Upload className="mr-2 h-4 w-4" />
                  Seleccionar Archivo
                </Button>
              </>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Required Fields */}
      <Card>
        <CardHeader>
          <CardTitle>Campos Requeridos</CardTitle>
          <CardDescription>Tu archivo debe incluir estas columnas</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            {[
              'marca',
              'modelo',
              'año',
              'precio',
              'kilometraje',
              'transmision',
              'combustible',
              'color',
            ].map(field => (
              <div key={field} className="flex items-center gap-2 rounded-lg bg-gray-50 p-3">
                <CheckCircle className="h-4 w-4 text-emerald-500" />
                <span className="text-sm font-medium capitalize">{field}</span>
              </div>
            ))}
          </div>
          <div className="mt-4">
            <p className="text-sm text-gray-500">
              Campos opcionales: version, puertas, cilindros, traccion, descripcion, vin
            </p>
          </div>
        </CardContent>
      </Card>

      {/* Import History */}
      <Card>
        <CardHeader>
          <CardTitle>Historial de Importaciones</CardTitle>
        </CardHeader>
        <CardContent>
          {importHistory.length > 0 ? (
            <div className="space-y-4">
              {importHistory.map(item => (
                <div
                  key={item.id}
                  className="flex items-center justify-between rounded-lg bg-gray-50 p-4"
                >
                  <div className="flex items-center gap-4">
                    <div className="rounded-lg bg-gray-100 p-2">
                      <FileSpreadsheet className="h-5 w-5 text-gray-600" />
                    </div>
                    <div>
                      <p className="font-medium">{item.filename}</p>
                      <p className="text-sm text-gray-500">
                        {new Date(item.date).toLocaleDateString('es-DO')} • {item.totalRecords}{' '}
                        registros
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <div className="flex items-center gap-2">
                        <CheckCircle className="h-4 w-4 text-emerald-500" />
                        <span className="text-sm text-emerald-600">{item.successful} exitosos</span>
                      </div>
                      {item.failed > 0 && (
                        <div className="flex items-center gap-2">
                          <AlertCircle className="h-4 w-4 text-red-500" />
                          <span className="text-sm text-red-600">{item.failed} fallidos</span>
                        </div>
                      )}
                    </div>
                    <Badge className="bg-emerald-100 text-emerald-700">Completado</Badge>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="py-8 text-center text-gray-400">
              <FileSpreadsheet className="mx-auto mb-4 h-12 w-12 opacity-50" />
              <p>No hay importaciones anteriores</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
