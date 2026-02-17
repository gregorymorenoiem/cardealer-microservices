import { useState, useCallback, useRef } from 'react';
import {
  FiUpload,
  FiFile,
  FiX,
  FiDownload,
  FiAlertCircle,
  FiCheck,
  FiLoader,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import inventoryManagementService from '../../services/inventoryManagementService';

interface ImportJob {
  id: string;
  fileName: string;
  status: string;
  totalRows: number;
  processedRows: number;
  successfulRows: number;
  failedRows: number;
  skippedRows: number;
  progressPercentage: number;
  errors: { rowNumber: number; field: string; errorMessage: string }[];
  createdAt: string;
  completedAt?: string;
}

interface CSVImportPageProps {
  dealerId: string;
  userId: string;
}

export default function CSVImportPage({ dealerId, userId }: CSVImportPageProps) {
  const [file, setFile] = useState<File | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [currentJob, setCurrentJob] = useState<ImportJob | null>(null);
  const [importHistory, setImportHistory] = useState<ImportJob[]>([]);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);

    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile && isValidFileType(droppedFile)) {
      setFile(droppedFile);
      setError(null);
    } else {
      setError('Tipo de archivo no válido. Use CSV, Excel (.xlsx, .xls) o JSON.');
    }
  }, []);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile && isValidFileType(selectedFile)) {
      setFile(selectedFile);
      setError(null);
    } else {
      setError('Tipo de archivo no válido. Use CSV, Excel (.xlsx, .xls) o JSON.');
    }
  };

  const isValidFileType = (file: File): boolean => {
    const validTypes = [
      'text/csv',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/json',
    ];
    const validExtensions = ['.csv', '.xlsx', '.xls', '.json'];
    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    return validTypes.includes(file.type) || validExtensions.includes(extension);
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleUpload = async () => {
    if (!file) return;

    try {
      setUploading(true);
      setError(null);

      const result = await inventoryManagementService.uploadCSVImport(dealerId, userId, file);
      setCurrentJob(result);
      setFile(null);

      // Poll for job status
      pollJobStatus(result.id);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al subir el archivo');
    } finally {
      setUploading(false);
    }
  };

  const pollJobStatus = async (jobId: string) => {
    const poll = async () => {
      try {
        const job = await inventoryManagementService.getBulkImportJob(jobId);
        setCurrentJob(job);

        if (job.status === 'Pending' || job.status === 'Processing') {
          setTimeout(poll, 2000); // Poll every 2 seconds
        } else {
          // Job completed, refresh history
          loadImportHistory();
        }
      } catch (err) {
        console.error('Error polling job status:', err);
      }
    };

    poll();
  };

  const loadImportHistory = async () => {
    try {
      const history = await inventoryManagementService.getBulkImportJobs(dealerId);
      setImportHistory(history);
    } catch (err) {
      console.error('Error loading import history:', err);
    }
  };

  const downloadTemplate = () => {
    window.open('/api/inventory/bulkimport/template', '_blank');
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Completed':
        return (
          <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
            <FiCheck /> Completado
          </span>
        );
      case 'Processing':
        return (
          <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
            <FiLoader className="animate-spin" /> Procesando
          </span>
        );
      case 'Failed':
        return (
          <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
            <FiAlertCircle /> Fallido
          </span>
        );
      case 'Pending':
        return (
          <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
            Pendiente
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
            {status}
          </span>
        );
    }
  };

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Importar Inventario</h1>
          <p className="text-gray-600 mt-2">
            Importa vehículos en masa usando archivos CSV, Excel o JSON
          </p>
        </div>

        {/* Download Template */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="font-semibold text-blue-900">¿Primera vez importando?</h3>
              <p className="text-blue-700 text-sm">
                Descarga nuestra plantilla CSV con el formato correcto
              </p>
            </div>
            <button
              onClick={downloadTemplate}
              className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition"
            >
              <FiDownload />
              Descargar Plantilla
            </button>
          </div>
        </div>

        {/* Upload Zone */}
        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Subir Archivo</h2>

          <div
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            className={`border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition
              ${isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400'}
              ${file ? 'bg-green-50 border-green-300' : ''}
            `}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".csv,.xlsx,.xls,.json"
              onChange={handleFileSelect}
              className="hidden"
            />

            {file ? (
              <div className="flex items-center justify-center gap-4">
                <FiFile size={40} className="text-green-600" />
                <div className="text-left">
                  <p className="font-medium text-gray-900">{file.name}</p>
                  <p className="text-sm text-gray-500">{formatFileSize(file.size)}</p>
                </div>
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    setFile(null);
                  }}
                  className="p-2 text-gray-400 hover:text-red-600"
                >
                  <FiX size={24} />
                </button>
              </div>
            ) : (
              <>
                <FiUpload size={48} className="mx-auto mb-4 text-gray-400" />
                <p className="text-lg font-medium text-gray-700">
                  Arrastra y suelta tu archivo aquí
                </p>
                <p className="text-gray-500 mt-1">o haz clic para seleccionar</p>
                <p className="text-sm text-gray-400 mt-2">
                  Soporta: CSV, Excel (.xlsx, .xls), JSON
                </p>
              </>
            )}
          </div>

          {error && (
            <div className="mt-4 flex items-center gap-2 text-red-600">
              <FiAlertCircle />
              <span>{error}</span>
            </div>
          )}

          {file && (
            <div className="mt-4 flex justify-end">
              <button
                onClick={handleUpload}
                disabled={uploading}
                className="flex items-center gap-2 bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 transition disabled:opacity-50"
              >
                {uploading ? (
                  <>
                    <FiLoader className="animate-spin" />
                    Subiendo...
                  </>
                ) : (
                  <>
                    <FiUpload />
                    Iniciar Importación
                  </>
                )}
              </button>
            </div>
          )}
        </div>

        {/* Current Job Progress */}
        {currentJob && (currentJob.status === 'Pending' || currentJob.status === 'Processing') && (
          <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Importación en Progreso</h2>

            <div className="mb-4">
              <div className="flex justify-between text-sm mb-1">
                <span className="text-gray-600">{currentJob.fileName}</span>
                <span className="font-medium">{currentJob.progressPercentage.toFixed(0)}%</span>
              </div>
              <div className="h-3 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-blue-600 transition-all duration-300"
                  style={{ width: `${currentJob.progressPercentage}%` }}
                />
              </div>
            </div>

            <div className="grid grid-cols-4 gap-4 text-center">
              <div>
                <p className="text-2xl font-bold text-gray-900">{currentJob.totalRows}</p>
                <p className="text-sm text-gray-500">Total</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-green-600">{currentJob.successfulRows}</p>
                <p className="text-sm text-gray-500">Exitosos</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-red-600">{currentJob.failedRows}</p>
                <p className="text-sm text-gray-500">Fallidos</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-yellow-600">{currentJob.skippedRows}</p>
                <p className="text-sm text-gray-500">Omitidos</p>
              </div>
            </div>
          </div>
        )}

        {/* Job Result */}
        {currentJob && currentJob.status === 'Completed' && (
          <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-12 h-12 rounded-full bg-green-100 flex items-center justify-center">
                <FiCheck size={24} className="text-green-600" />
              </div>
              <div>
                <h2 className="text-xl font-semibold text-gray-900">Importación Completada</h2>
                <p className="text-gray-500">{currentJob.fileName}</p>
              </div>
            </div>

            <div className="grid grid-cols-4 gap-4 text-center mb-4">
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-2xl font-bold text-gray-900">{currentJob.totalRows}</p>
                <p className="text-sm text-gray-500">Total Filas</p>
              </div>
              <div className="bg-green-50 rounded-lg p-4">
                <p className="text-2xl font-bold text-green-600">{currentJob.successfulRows}</p>
                <p className="text-sm text-gray-500">Importados</p>
              </div>
              <div className="bg-red-50 rounded-lg p-4">
                <p className="text-2xl font-bold text-red-600">{currentJob.failedRows}</p>
                <p className="text-sm text-gray-500">Errores</p>
              </div>
              <div className="bg-yellow-50 rounded-lg p-4">
                <p className="text-2xl font-bold text-yellow-600">{currentJob.skippedRows}</p>
                <p className="text-sm text-gray-500">Omitidos</p>
              </div>
            </div>

            {currentJob.errors.length > 0 && (
              <div className="border border-red-200 rounded-lg overflow-hidden">
                <div className="bg-red-50 px-4 py-2 border-b border-red-200">
                  <h3 className="font-medium text-red-800">Errores ({currentJob.errors.length})</h3>
                </div>
                <div className="max-h-48 overflow-y-auto">
                  <table className="w-full">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                          Fila
                        </th>
                        <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                          Campo
                        </th>
                        <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">
                          Error
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                      {currentJob.errors.map((err, idx) => (
                        <tr key={idx}>
                          <td className="px-4 py-2 text-sm text-gray-900">{err.rowNumber}</td>
                          <td className="px-4 py-2 text-sm text-gray-600">{err.field}</td>
                          <td className="px-4 py-2 text-sm text-red-600">{err.errorMessage}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            <div className="mt-4 flex justify-end">
              <button
                onClick={() => setCurrentJob(null)}
                className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition"
              >
                Nueva Importación
              </button>
            </div>
          </div>
        )}

        {/* Import History */}
        <div className="bg-white rounded-lg shadow-sm overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">Historial de Importaciones</h2>
          </div>

          {importHistory.length === 0 ? (
            <div className="p-8 text-center text-gray-500">No hay importaciones previas</div>
          ) : (
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-500">Archivo</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-500">Estado</th>
                  <th className="px-4 py-3 text-center text-sm font-medium text-gray-500">Total</th>
                  <th className="px-4 py-3 text-center text-sm font-medium text-gray-500">
                    Exitosos
                  </th>
                  <th className="px-4 py-3 text-center text-sm font-medium text-gray-500">
                    Errores
                  </th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-500">Fecha</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {importHistory.map((job) => (
                  <tr key={job.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{job.fileName}</td>
                    <td className="px-4 py-3">{getStatusBadge(job.status)}</td>
                    <td className="px-4 py-3 text-center text-sm">{job.totalRows}</td>
                    <td className="px-4 py-3 text-center text-sm text-green-600">
                      {job.successfulRows}
                    </td>
                    <td className="px-4 py-3 text-center text-sm text-red-600">{job.failedRows}</td>
                    <td className="px-4 py-3 text-sm text-gray-500">
                      {new Date(job.createdAt).toLocaleDateString('es-DO', {
                        day: '2-digit',
                        month: 'short',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Help Section */}
        <div className="mt-8 bg-gray-50 rounded-lg p-6">
          <h3 className="font-semibold text-gray-900 mb-4">Guía de Importación</h3>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <h4 className="font-medium text-gray-800 mb-2">Campos Requeridos</h4>
              <ul className="text-sm text-gray-600 space-y-1">
                <li>
                  • <code className="bg-gray-200 px-1 rounded">VIN</code> - Número de identificación
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">ListPrice</code> - Precio de lista
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Make</code> - Marca
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Model</code> - Modelo
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Year</code> - Año
                </li>
              </ul>
            </div>

            <div>
              <h4 className="font-medium text-gray-800 mb-2">Campos Opcionales</h4>
              <ul className="text-sm text-gray-600 space-y-1">
                <li>
                  • <code className="bg-gray-200 px-1 rounded">CostPrice</code> - Precio de costo
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">StockNumber</code> - Número de stock
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Location</code> - Ubicación física
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">InternalNotes</code> - Notas internas
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">AcquisitionSource</code> - Origen
                </li>
              </ul>
            </div>

            <div>
              <h4 className="font-medium text-gray-800 mb-2">Valores de AcquisitionSource</h4>
              <ul className="text-sm text-gray-600 space-y-1">
                <li>
                  • <code className="bg-gray-200 px-1 rounded">TradeIn</code> - Intercambio
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Auction</code> - Subasta
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Wholesale</code> - Mayoreo
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">DirectPurchase</code> - Compra
                  directa
                </li>
                <li>
                  • <code className="bg-gray-200 px-1 rounded">Consignment</code> - Consignación
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
