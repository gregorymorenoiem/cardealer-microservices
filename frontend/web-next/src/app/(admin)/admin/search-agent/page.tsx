'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Slider } from '@/components/ui/slider';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Brain,
  Save,
  RefreshCw,
  Settings2,
  Sparkles,
  Shield,
  Gauge,
  FlaskConical,
  Loader2,
  CheckCircle2,
  AlertCircle,
  Activity,
} from 'lucide-react';
import {
  getSearchAgentConfig,
  getSearchAgentStatus,
  updateSearchAgentConfig,
} from '@/services/search-agent';
import type { SearchAgentConfig } from '@/services/search-agent';

export default function SearchAgentConfigPage() {
  const queryClient = useQueryClient();
  const [saveSuccess, setSaveSuccess] = useState(false);

  // Fetch current config
  const {
    data: config,
    isLoading: configLoading,
    error: configError,
    refetch: refetchConfig,
  } = useQuery({
    queryKey: ['search-agent', 'config'],
    queryFn: getSearchAgentConfig,
  });

  // Fetch status
  const { data: status, refetch: refetchStatus } = useQuery({
    queryKey: ['search-agent', 'status'],
    queryFn: getSearchAgentStatus,
    refetchInterval: 30000,
  });

  // Local form state (initialized from fetched config)
  const [formState, setFormState] = useState<Partial<SearchAgentConfig>>({});

  // Merge fetched config with local changes
  const mergedConfig = { ...config, ...formState } as SearchAgentConfig;

  // Update mutation
  const mutation = useMutation({
    mutationFn: (data: Partial<SearchAgentConfig>) => updateSearchAgentConfig(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['search-agent', 'config'] });
      setFormState({});
      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    },
  });

  const updateField = <K extends keyof SearchAgentConfig>(key: K, value: SearchAgentConfig[K]) => {
    setFormState(prev => ({ ...prev, [key]: value }));
  };

  const handleSave = () => {
    mutation.mutate(formState);
  };

  const hasChanges = Object.keys(formState).length > 0;

  if (configLoading) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <Loader2 className="text-primary h-8 w-8 animate-spin" />
        <span className="ml-3 text-lg">Cargando configuración del SearchAgent...</span>
      </div>
    );
  }

  if (configError) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
        <AlertCircle className="text-destructive h-12 w-12" />
        <p className="text-destructive text-lg">Error al cargar la configuración</p>
        <Button onClick={() => refetchConfig()}>
          <RefreshCw className="mr-2 h-4 w-4" /> Reintentar
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold">
            <Brain className="text-primary h-7 w-7" />
            SearchAgent — Configuración IA
          </h1>
          <p className="text-muted-foreground mt-1">
            Configura el comportamiento del agente de búsqueda inteligente con Claude Haiku 4.5
          </p>
        </div>
        <div className="flex items-center gap-2">
          {mergedConfig && (
            <Badge variant={mergedConfig.isEnabled ? 'default' : 'secondary'}>
              <Activity className="mr-1 h-3 w-3" />
              {mergedConfig.isEnabled ? 'Activo' : 'Inactivo'}
            </Badge>
          )}
          <Button
            variant="outline"
            size="sm"
            onClick={() => {
              refetchConfig();
              refetchStatus();
            }}
          >
            <RefreshCw className="mr-2 h-4 w-4" /> Refrescar
          </Button>
          <Button onClick={handleSave} disabled={!hasChanges || mutation.isPending}>
            {mutation.isPending ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : saveSuccess ? (
              <CheckCircle2 className="mr-2 h-4 w-4 text-green-500" />
            ) : (
              <Save className="mr-2 h-4 w-4" />
            )}
            {saveSuccess ? 'Guardado' : 'Guardar cambios'}
          </Button>
        </div>
      </div>

      {mutation.isError && (
        <div className="bg-destructive/10 border-destructive/20 text-destructive rounded-lg border p-3 text-sm">
          Error al guardar: {(mutation.error as Error)?.message || 'Error desconocido'}
        </div>
      )}

      <Tabs defaultValue="general" className="space-y-4">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="general">
            <Settings2 className="mr-2 h-4 w-4" /> General
          </TabsTrigger>
          <TabsTrigger value="model">
            <Sparkles className="mr-2 h-4 w-4" /> Modelo IA
          </TabsTrigger>
          <TabsTrigger value="rules">
            <Shield className="mr-2 h-4 w-4" /> Reglas de Negocio
          </TabsTrigger>
          <TabsTrigger value="advanced">
            <FlaskConical className="mr-2 h-4 w-4" /> Avanzado
          </TabsTrigger>
        </TabsList>

        {/* ============ GENERAL TAB ============ */}
        <TabsContent value="general" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Estado del Servicio</CardTitle>
              <CardDescription>
                Activa o desactiva la búsqueda con inteligencia artificial
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center justify-between">
                <div>
                  <Label className="text-base">Búsqueda IA habilitada</Label>
                  <p className="text-muted-foreground text-sm">
                    Si se desactiva, las búsquedas usarán solo filtros tradicionales
                  </p>
                </div>
                <Switch
                  checked={mergedConfig.isEnabled ?? true}
                  onCheckedChange={v => updateField('isEnabled', v)}
                />
              </div>

              <Separator />

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Resultados mínimos</Label>
                  <Input
                    type="number"
                    min={1}
                    max={50}
                    value={mergedConfig.minResultsPerPage ?? 8}
                    onChange={e => updateField('minResultsPerPage', parseInt(e.target.value))}
                  />
                  <p className="text-muted-foreground text-xs">
                    Siempre devolver al menos esta cantidad (Regla #1)
                  </p>
                </div>
                <div className="space-y-2">
                  <Label>Resultados máximos</Label>
                  <Input
                    type="number"
                    min={1}
                    max={100}
                    value={mergedConfig.maxResultsPerPage ?? 50}
                    onChange={e => updateField('maxResultsPerPage', parseInt(e.target.value))}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Status Metrics */}
          {status && (
            <div className="grid grid-cols-3 gap-4">
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-2">
                    <Gauge className="h-5 w-5 text-blue-500" />
                    <div>
                      <p className="text-muted-foreground text-sm">Estado</p>
                      <p className="text-2xl font-bold capitalize">{status.status}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-2">
                    <Activity className="h-5 w-5 text-green-500" />
                    <div>
                      <p className="text-muted-foreground text-sm">Versión</p>
                      <p className="text-2xl font-bold">{status.version}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-2">
                    <Gauge className="h-5 w-5 text-amber-500" />
                    <div>
                      <p className="text-muted-foreground text-sm">Última comprobación</p>
                      <p className="text-sm font-medium">{new Date(status.timestamp).toLocaleTimeString()}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          )}
        </TabsContent>

        {/* ============ MODEL TAB ============ */}
        <TabsContent value="model" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Configuración del Modelo</CardTitle>
              <CardDescription>
                Parámetros de Claude Haiku 4.5 para la interpretación de búsquedas
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="space-y-2">
                <Label>Modelo</Label>
                <Input
                  value={mergedConfig.model ?? 'claude-haiku-4-5-20251001'}
                  onChange={e => updateField('model', e.target.value)}
                />
                <p className="text-muted-foreground text-xs">
                  Identificador del modelo Anthropic (claude-haiku-4-5-20251001)
                </p>
              </div>

              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <Label>Temperatura: {(mergedConfig.temperature ?? 0.2).toFixed(2)}</Label>
                  <Badge variant="outline">{mergedConfig.temperature ?? 0.2}</Badge>
                </div>
                <Slider
                  min={0}
                  max={1}
                  step={0.05}
                  value={[mergedConfig.temperature ?? 0.2]}
                  onValueChange={([v]) => updateField('temperature', v)}
                />
                <p className="text-muted-foreground text-xs">
                  Menor = más determinista. Recomendado: 0.1-0.3 para búsquedas
                </p>
              </div>

              <div className="space-y-2">
                <Label>Tokens máximos de respuesta</Label>
                <Input
                  type="number"
                  min={256}
                  max={4096}
                  value={mergedConfig.maxTokens ?? 1024}
                  onChange={e => updateField('maxTokens', parseInt(e.target.value))}
                />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>System Prompt Override</CardTitle>
              <CardDescription>
                Reemplaza completamente el system prompt. Dejar vacío para usar el prompt por
                defecto.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Textarea
                rows={12}
                placeholder="Dejar vacío para usar el system prompt por defecto construido desde la configuración..."
                value={mergedConfig.systemPromptOverride ?? ''}
                onChange={e => updateField('systemPromptOverride', e.target.value || null)}
                className="font-mono text-sm"
              />
              <p className="text-muted-foreground mt-2 text-xs">
                ⚠️ Solo modifica esto si sabes lo que haces. El prompt por defecto se construye
                automáticamente con los parámetros configurados.
              </p>
            </CardContent>
          </Card>
        </TabsContent>

        {/* ============ BUSINESS RULES TAB ============ */}
        <TabsContent value="rules" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Regla #1 — Mínimo de Resultados</CardTitle>
              <CardDescription>
                Siempre devolver al menos N resultados, relajando filtros progresivamente
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label className="text-base">Auto-relajación activada</Label>
                  <p className="text-muted-foreground text-sm">
                    Relajar filtros automáticamente si no hay suficientes resultados
                  </p>
                </div>
                <Switch
                  checked={(mergedConfig.maxRelaxationLevel ?? 5) > 0}
                  onCheckedChange={v => updateField('maxRelaxationLevel', v ? 5 : 0)}
                />
              </div>

              <Separator />

              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label>Precio ±% (Relajación)</Label>
                  <Input
                    type="number"
                    min={5}
                    max={100}
                    value={mergedConfig.priceRelaxPercent ?? 25}
                    onChange={e =>
                      updateField('priceRelaxPercent', parseInt(e.target.value))
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label>Año ± (Relajación)</Label>
                  <Input
                    type="number"
                    min={1}
                    max={10}
                    value={mergedConfig.yearRelaxRange ?? 2}
                    onChange={e => updateField('yearRelaxRange', parseInt(e.target.value))}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Regla #2 — Resultados Patrocinados</CardTitle>
              <CardDescription>
                Inyectar resultados patrocinados con afinidad mínima
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label className="text-base">Patrocinados habilitados</Label>
                  <p className="text-muted-foreground text-sm">
                    Inyectar vehículos de dealers premium en posiciones estratégicas
                  </p>
                </div>
                <Switch
                  checked={(mergedConfig.maxSponsoredPercentage ?? 0.25) > 0}
                  onCheckedChange={v => updateField('maxSponsoredPercentage', v ? 0.25 : 0)}
                />
              </div>

              <Separator />

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <Label>
                      Afinidad mínima: {(mergedConfig.sponsoredAffinityThreshold ?? 0.45).toFixed(2)}
                    </Label>
                    <Badge variant="outline">{mergedConfig.sponsoredAffinityThreshold ?? 0.45}</Badge>
                  </div>
                  <Slider
                    min={0}
                    max={1}
                    step={0.05}
                    value={[mergedConfig.sponsoredAffinityThreshold ?? 0.45]}
                    onValueChange={([v]) => updateField('sponsoredAffinityThreshold', v)}
                  />
                  <p className="text-muted-foreground text-xs">
                    Solo insertar patrocinados con afinidad ≥ este valor
                  </p>
                </div>
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <Label>
                      Máximo % patrocinados: {(mergedConfig.maxSponsoredPercentage ?? 0.25) * 100}%
                    </Label>
                    <Badge variant="outline">
                      {((mergedConfig.maxSponsoredPercentage ?? 0.25) * 100).toFixed(0)}%
                    </Badge>
                  </div>
                  <Slider
                    min={0.05}
                    max={0.5}
                    step={0.05}
                    value={[mergedConfig.maxSponsoredPercentage ?? 0.25]}
                    onValueChange={([v]) => updateField('maxSponsoredPercentage', v)}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label>Posiciones de inserción</Label>
                <Input
                  value={mergedConfig.sponsoredPositions ?? '1,5,10'}
                  onChange={e => updateField('sponsoredPositions', e.target.value)}
                  placeholder="1,5,10"
                />
                <p className="text-muted-foreground text-xs">
                  Posiciones (separadas por coma) donde insertar patrocinados
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* ============ ADVANCED TAB ============ */}
        <TabsContent value="advanced" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Cache</CardTitle>
              <CardDescription>
                Configuración de caché para reducir llamadas a la API de Claude
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label className="text-base">Cache habilitado</Label>
                  <p className="text-muted-foreground text-sm">
                    Cachear respuestas de búsqueda idénticas en Redis
                  </p>
                </div>
                <Switch
                  checked={mergedConfig.enableCache ?? true}
                  onCheckedChange={v => updateField('enableCache', v)}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>TTL caché (minutos)</Label>
                  <Input
                    type="number"
                    min={1}
                    max={1440}
                    value={Math.round((mergedConfig.cacheTtlSeconds ?? 3600) / 60)}
                    onChange={e => updateField('cacheTtlSeconds', parseInt(e.target.value) * 60)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>A/B Testing</CardTitle>
              <CardDescription>
                Porcentaje de tráfico que usa búsqueda IA vs tradicional
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <Label className="text-base">Tráfico IA</Label>
                    <p className="text-muted-foreground text-sm">
                      Porcentaje del tráfico que usa búsqueda IA
                    </p>
                  </div>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <Label>Tráfico IA: {mergedConfig.aiSearchTrafficPercent ?? 100}%</Label>
                    <Badge variant="outline">{mergedConfig.aiSearchTrafficPercent ?? 100}%</Badge>
                  </div>
                  <Slider
                    min={1}
                    max={100}
                    step={1}
                    value={[mergedConfig.aiSearchTrafficPercent ?? 100]}
                    onValueChange={([v]) => updateField('aiSearchTrafficPercent', v)}
                  />
                  <p className="text-muted-foreground text-xs">
                    El resto usará búsqueda tradicional para comparación de métricas
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Rate Limiting</CardTitle>
              <CardDescription>Protección contra abuso de la API de Claude</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Máx. requests por minuto (por IP)</Label>
                  <Input
                    type="number"
                    min={1}
                    max={100}
                    value={mergedConfig.maxQueriesPerMinutePerIp ?? 60}
                    onChange={e => updateField('maxQueriesPerMinutePerIp', parseInt(e.target.value))}
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
