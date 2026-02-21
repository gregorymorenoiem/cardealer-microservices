import { redirect } from 'next/navigation';

/**
 * Dealer Import Redirect Page
 *
 * Legacy route redirect: /dealer/importar → /dealer/inventario/importar
 * The canonical import page with full API integration is at the inventario route.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function DealerImportPage() {
  redirect('/dealer/inventario/importar');
}
import {
  Upload,
  FileSpreadsheet,
  Download,
  CheckCircle2,
  XCircle,
  Loader2,
  Info,
} from 'lucide-react';
import { useCurrentDealer } from '@/hooks/use-dealers';
import { toast } from 'sonner';

type ImportStatus = 'idle' | 'uploading' | 'processing' | 'complete' | 'error';

interface ImportResult {
  totalRows: number;
  imported: number;
  skipped: number;
  errors: Array<{ row: number; field: string; message: string }>;
}

export default function ImportarPage() {
  const { data: dealer } = useCurrentDealer();
  const [file, setFile] = React.useState<File | null>(null);
  const [status, setStatus] = React.useState<ImportStatus>('idle');
  const [progress, setProgress] = React.useState(0);
  const [result, setResult] = React.useState<ImportResult | null>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0];
    if (!selected) return;

    const validTypes = [
      'text/csv',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    ];
    if (!validTypes.includes(selected.type)) {
      toast.error('Formato no soportado. Usa CSV o Excel (.xlsx)');
      return;
    }

    if (selected.size > 10 * 1024 * 1024) {
      toast.error('El archivo no puede exceder 10 MB');
      return;
    }

    setFile(selected);
    setStatus('idle');
    setResult(null);
  };

  const handleUpload = async () => {
    if (!file || !dealer?.id) return;

    try {
      setStatus('uploading');
      setProgress(0);

      // Simulate upload progress (replace with real API call)
      const interval = setInterval(() => {
        setProgress(prev => {
          if (prev >= 90) {
            clearInterval(interval);
            return 90;
          }
          return prev + 10;
        });
      }, 300);

      // TODO: Replace with real API call
      // const formData = new FormData();
      // formData.append('file', file);
      // formData.append('dealerId', dealer.id);
      // const result = await dealerService.importVehicles(formData);

      // Simulated result
      await new Promise(resolve => setTimeout(resolve, 3000));
      clearInterval(interval);
      setProgress(100);
      setStatus('complete');
      setResult({
        totalRows: 25,
        imported: 22,
        skipped: 2,
        errors: [{ row: 5, field: 'price', message: 'Precio inválido' }],
      });
      toast.success('Importación completada');
    } catch (err) {
      console.error('Import error:', err);
      setStatus('error');
      toast.error('Error durante la importación');
    }
  };

  const handleDownloadTemplate = () => {
    // CSV template download
    const headers =
      'marca,modelo,año,precio,kilometraje,transmision,combustible,condicion,color_exterior,color_interior,vin,descripcion';
    const example =
      'Toyota,Corolla,2023,1250000,15000,automatica,gasolina,usado,Blanco,Negro,1HGBH41JXMN109186,Excelente condición';
    const csv = `${headers}\n${example}`;
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'plantilla_importacion_vehiculos.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-foreground text-2xl font-bold">Importar Vehículos</h1>
        <p className="text-muted-foreground">
          Carga tu inventario de forma masiva usando un archivo CSV o Excel
        </p>
      </div>

      {/* Template Download */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Info className="h-5 w-5 text-blue-500" />
            Antes de comenzar
          </CardTitle>
          <CardDescription>
            Descarga la plantilla con el formato requerido para la importación
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="text-muted-foreground text-sm">
              <p>La plantilla incluye las columnas necesarias y un ejemplo.</p>
              <p className="mt-1">
                Formatos aceptados: <strong>CSV</strong> o <strong>Excel (.xlsx)</strong> — Máximo
                10 MB
              </p>
            </div>
            <Button variant="outline" onClick={handleDownloadTemplate}>
              <Download className="mr-2 h-4 w-4" />
              Descargar Plantilla
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Upload Area */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Upload className="h-5 w-5" />
            Subir Archivo
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <label
            className={`flex min-h-[200px] cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 text-center transition-colors ${
              file ? 'border-primary bg-primary/5' : 'hover:border-primary/50 hover:bg-muted/50'
            }`}
          >
            {file ? (
              <>
                <FileSpreadsheet className="text-primary h-12 w-12" />
                <p className="mt-3 font-medium">{file.name}</p>
                <p className="text-muted-foreground text-sm">{(file.size / 1024).toFixed(1)} KB</p>
                <span className="text-primary mt-2 text-xs">Click para cambiar archivo</span>
              </>
            ) : (
              <>
                <Upload className="text-muted-foreground h-12 w-12" />
                <p className="mt-3 font-medium">
                  Arrastra tu archivo aquí o haz click para seleccionar
                </p>
                <p className="text-muted-foreground text-sm">CSV o Excel (.xlsx) — Máximo 10 MB</p>
              </>
            )}
            <input
              type="file"
              accept=".csv,.xlsx,.xls"
              className="hidden"
              onChange={handleFileSelect}
            />
          </label>

          {/* Progress */}
          {(status === 'uploading' || status === 'processing') && (
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="flex items-center gap-2">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  {status === 'uploading' ? 'Subiendo archivo...' : 'Procesando datos...'}
                </span>
                <span>{progress}%</span>
              </div>
              <Progress value={progress} />
            </div>
          )}

          {/* Upload Button */}
          {file && status === 'idle' && (
            <div className="flex justify-end">
              <Button onClick={handleUpload}>
                <Upload className="mr-2 h-4 w-4" />
                Iniciar Importación
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Results */}
      {result && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              {status === 'complete' ? (
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              ) : (
                <XCircle className="h-5 w-5 text-red-500" />
              )}
              Resultado de la Importación
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-green-600">{result.imported}</p>
                <p className="text-muted-foreground text-sm">Importados</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-amber-600">{result.skipped}</p>
                <p className="text-muted-foreground text-sm">Omitidos</p>
              </div>
              <div className="rounded-lg border p-4 text-center">
                <p className="text-2xl font-bold text-red-600">{result.errors.length}</p>
                <p className="text-muted-foreground text-sm">Errores</p>
              </div>
            </div>

            {result.errors.length > 0 && (
              <div className="space-y-2">
                <p className="text-sm font-medium">Detalle de errores:</p>
                <div className="max-h-48 overflow-y-auto rounded-lg border">
                  <table className="w-full text-sm">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-3 py-2 text-left">Fila</th>
                        <th className="px-3 py-2 text-left">Campo</th>
                        <th className="px-3 py-2 text-left">Error</th>
                      </tr>
                    </thead>
                    <tbody>
                      {result.errors.map((err, i) => (
                        <tr key={i} className="border-t">
                          <td className="px-3 py-2">{err.row}</td>
                          <td className="px-3 py-2">{err.field}</td>
                          <td className="px-3 py-2 text-red-600">{err.message}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
