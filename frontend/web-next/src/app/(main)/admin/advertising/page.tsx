import { redirect } from 'next/navigation';

/**
 * Advertising Admin Redirect Page
 *
 * Legacy route redirect: (main)/admin/advertising → (admin)/admin/promociones
 * Admin pages should be under the (admin) layout group (with sidebar, auth guards, etc.)
 * not under (main) layout group.
 *
 * Keeping this for backward compatibility with bookmarks and email links.
 */
export default function AdvertisingPage() {
  redirect('/admin/promociones');
}
import {
  usePlatformReport,
  useRotationConfig,
  useUpdateRotationConfig,
  useRefreshRotation,
  useCategories,
  useUpdateCategory,
  useBrands,
  useUpdateBrand,
} from '@/hooks/use-advertising';
import type {
  CategoryImageConfig,
  BrandConfig,
  AdPlacementType,
  RotationAlgorithmType,
} from '@/types/advertising';

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(amount);
}

// =============================================================================
// PLATFORM OVERVIEW
// =============================================================================

function PlatformOverview() {
  const { data: report, isLoading } = usePlatformReport();

  if (isLoading) {
    return (
      <div className="grid grid-cols-4 gap-4">
        {[1, 2, 3, 4].map(i => (
          <Card key={i} className="animate-pulse">
            <CardContent className="h-24" />
          </Card>
        ))}
      </div>
    );
  }

  if (!report) return null;

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground text-sm">Campañas Activas</p>
            <p className="text-3xl font-bold">{report.totalActiveCampaigns}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground text-sm">Impresiones Totales</p>
            <p className="text-3xl font-bold">{report.totalImpressions.toLocaleString()}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground text-sm">Clics Totales</p>
            <p className="text-3xl font-bold">{report.totalClicks.toLocaleString()}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-muted-foreground text-sm">Revenue Total</p>
            <p className="text-primary text-3xl font-bold">{formatCurrency(report.totalRevenue)}</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>CTR Promedio</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-4xl font-bold">{report.averageCtr.toFixed(2)}%</p>
        </CardContent>
      </Card>
    </div>
  );
}

// =============================================================================
// ROTATION CONFIG PANEL (with editing — criterion #9)
// =============================================================================

function RotationConfigEditor({ section }: { section: AdPlacementType }) {
  const { data: config, isLoading } = useRotationConfig(section);
  const updateMutation = useUpdateRotationConfig();
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({
    algorithm: '' as string,
    maxSlots: 0,
    rotationIntervalMinutes: 0,
    minQualityScore: 0,
    isActive: true,
  });

  if (isLoading) return <div className="bg-muted h-40 animate-pulse rounded" />;
  if (!config) return null;

  const startEditing = () => {
    setForm({
      algorithm: config.algorithm,
      maxSlots: config.maxSlots,
      rotationIntervalMinutes: config.rotationIntervalMinutes,
      minQualityScore: config.minQualityScore,
      isActive: config.isActive,
    });
    setEditing(true);
  };

  const handleSave = () => {
    updateMutation.mutate(
      {
        section,
        algorithm: form.algorithm as RotationAlgorithmType,
        maxSlots: form.maxSlots,
        rotationIntervalMinutes: form.rotationIntervalMinutes,
        minQualityScore: form.minQualityScore,
        isActive: form.isActive,
      },
      { onSuccess: () => setEditing(false) }
    );
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between text-base">
          <span>{section === 'FeaturedSpot' ? '⭐ Featured' : '💎 Premium'}</span>
          <div className="flex items-center gap-2">
            <Badge variant={config.isActive ? 'default' : 'secondary'}>
              {config.isActive ? 'Activo' : 'Inactivo'}
            </Badge>
            {!editing && (
              <Button variant="outline" size="sm" onClick={startEditing}>
                ✏️ Editar
              </Button>
            )}
          </div>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {editing ? (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Algoritmo</Label>
                <select
                  className="border-input bg-background flex h-9 w-full rounded-md border px-3 py-1 text-sm"
                  value={form.algorithm}
                  onChange={e => setForm(f => ({ ...f, algorithm: e.target.value }))}
                >
                  <option value="WeightedRandom">WeightedRandom</option>
                  <option value="RoundRobin">RoundRobin</option>
                  <option value="CTROptimized">CTROptimized</option>
                  <option value="BudgetPriority">BudgetPriority</option>
                </select>
              </div>
              <div>
                <Label>Slots Máximos</Label>
                <Input
                  type="number"
                  min={1}
                  max={20}
                  value={form.maxSlots}
                  onChange={e => setForm(f => ({ ...f, maxSlots: Number(e.target.value) }))}
                />
              </div>
              <div>
                <Label>Intervalo (minutos)</Label>
                <Input
                  type="number"
                  min={1}
                  max={1440}
                  value={form.rotationIntervalMinutes}
                  onChange={e =>
                    setForm(f => ({ ...f, rotationIntervalMinutes: Number(e.target.value) }))
                  }
                />
              </div>
              <div>
                <Label>Quality Score Mínimo</Label>
                <Input
                  type="number"
                  min={0}
                  max={100}
                  step={0.1}
                  value={form.minQualityScore}
                  onChange={e => setForm(f => ({ ...f, minQualityScore: Number(e.target.value) }))}
                />
              </div>
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id={`active-${section}`}
                checked={form.isActive}
                onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))}
              />
              <Label htmlFor={`active-${section}`}>Activo</Label>
            </div>
            <div className="flex gap-2">
              <Button size="sm" onClick={handleSave} disabled={updateMutation.isPending}>
                {updateMutation.isPending ? 'Guardando...' : '💾 Guardar'}
              </Button>
              <Button size="sm" variant="outline" onClick={() => setEditing(false)}>
                Cancelar
              </Button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <p className="text-muted-foreground">Algoritmo</p>
              <p className="font-mono">{config.algorithm}</p>
            </div>
            <div>
              <p className="text-muted-foreground">Slots Máx</p>
              <p>{config.maxSlots}</p>
            </div>
            <div>
              <p className="text-muted-foreground">Intervalo</p>
              <p>{config.rotationIntervalMinutes} min</p>
            </div>
            <div>
              <p className="text-muted-foreground">Quality Score Mín</p>
              <p>{config.minQualityScore}</p>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function RotationConfigPanel() {
  const refreshMutation = useRefreshRotation();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">Configuración de Rotación</h3>
        <Button
          variant="outline"
          onClick={() => refreshMutation.mutate(undefined)}
          disabled={refreshMutation.isPending}
        >
          {refreshMutation.isPending ? 'Refrescando...' : '🔄 Refrescar Rotación'}
        </Button>
      </div>
      <div className="grid gap-4">
        <RotationConfigEditor section="FeaturedSpot" />
        <RotationConfigEditor section="PremiumSpot" />
      </div>
    </div>
  );
}

// =============================================================================
// CATEGORY MANAGER (with CRUD — criterion #11)
// =============================================================================

function CategoryManager() {
  const { data: categories, isLoading } = useCategories(true);
  const updateMutation = useUpdateCategory();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({
    displayName: '',
    imageUrl: '',
    description: '',
    href: '',
    accentColor: '',
    displayOrder: 0,
    isActive: true,
    isTrending: false,
  });

  if (isLoading) return <div className="bg-muted h-40 animate-pulse rounded" />;

  const startEditing = (cat: CategoryImageConfig) => {
    setEditForm({
      displayName: cat.displayName,
      imageUrl: cat.imageUrl,
      description: cat.description,
      href: cat.href,
      accentColor: cat.accentColor,
      displayOrder: cat.displayOrder,
      isActive: cat.isActive,
      isTrending: cat.isTrending,
    });
    setEditingId(cat.id);
  };

  const handleSave = (categoryKey: string) => {
    updateMutation.mutate({ categoryKey, ...editForm }, { onSuccess: () => setEditingId(null) });
  };

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">Categorías del Homepage</h3>
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {categories?.map((cat: CategoryImageConfig) => (
          <Card key={cat.id}>
            <CardContent className="pt-6">
              {editingId === cat.id ? (
                <div className="space-y-3">
                  <div>
                    <Label>Nombre</Label>
                    <Input
                      value={editForm.displayName}
                      onChange={e => setEditForm(f => ({ ...f, displayName: e.target.value }))}
                    />
                  </div>
                  <div>
                    <Label>URL Imagen</Label>
                    <Input
                      value={editForm.imageUrl}
                      onChange={e => setEditForm(f => ({ ...f, imageUrl: e.target.value }))}
                    />
                  </div>
                  <div>
                    <Label>Descripción</Label>
                    <Input
                      value={editForm.description}
                      onChange={e => setEditForm(f => ({ ...f, description: e.target.value }))}
                    />
                  </div>
                  <div>
                    <Label>Enlace (href)</Label>
                    <Input
                      value={editForm.href}
                      onChange={e => setEditForm(f => ({ ...f, href: e.target.value }))}
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <Label>Color</Label>
                      <Input
                        value={editForm.accentColor}
                        onChange={e => setEditForm(f => ({ ...f, accentColor: e.target.value }))}
                      />
                    </div>
                    <div>
                      <Label>Orden</Label>
                      <Input
                        type="number"
                        value={editForm.displayOrder}
                        onChange={e =>
                          setEditForm(f => ({ ...f, displayOrder: Number(e.target.value) }))
                        }
                      />
                    </div>
                  </div>
                  <div className="flex gap-4">
                    <label className="flex items-center gap-1 text-sm">
                      <input
                        type="checkbox"
                        checked={editForm.isActive}
                        onChange={e => setEditForm(f => ({ ...f, isActive: e.target.checked }))}
                      />
                      Activa
                    </label>
                    <label className="flex items-center gap-1 text-sm">
                      <input
                        type="checkbox"
                        checked={editForm.isTrending}
                        onChange={e => setEditForm(f => ({ ...f, isTrending: e.target.checked }))}
                      />
                      Trending
                    </label>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      onClick={() => handleSave(cat.categoryKey)}
                      disabled={updateMutation.isPending}
                    >
                      {updateMutation.isPending ? 'Guardando...' : '💾 Guardar'}
                    </Button>
                    <Button size="sm" variant="outline" onClick={() => setEditingId(null)}>
                      Cancelar
                    </Button>
                  </div>
                </div>
              ) : (
                <>
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-semibold">{cat.categoryKey}</p>
                      <p className="text-muted-foreground text-sm">{cat.displayName}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant={cat.isActive ? 'default' : 'secondary'}>
                        {cat.isActive ? 'Activa' : 'Inactiva'}
                      </Badge>
                      <Button size="sm" variant="outline" onClick={() => startEditing(cat)}>
                        ✏️
                      </Button>
                    </div>
                  </div>
                  <p className="text-muted-foreground mt-2 text-xs">
                    Orden: {cat.displayOrder}
                    {cat.isTrending && ' • 🔥 Trending'}
                  </p>
                </>
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// BRAND MANAGER (with CRUD — criterion #12)
// =============================================================================

function BrandManager() {
  const { data: brands, isLoading } = useBrands(true);
  const updateMutation = useUpdateBrand();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({
    displayName: '',
    logoUrl: '',
    vehicleCount: 0,
    displayOrder: 0,
    isActive: true,
  });

  if (isLoading) return <div className="bg-muted h-40 animate-pulse rounded" />;

  const startEditing = (brand: BrandConfig) => {
    setEditForm({
      displayName: brand.displayName,
      logoUrl: brand.logoUrl,
      vehicleCount: brand.vehicleCount,
      displayOrder: brand.displayOrder,
      isActive: brand.isActive,
    });
    setEditingId(brand.id);
  };

  const handleSave = (brandKey: string) => {
    updateMutation.mutate({ brandKey, ...editForm }, { onSuccess: () => setEditingId(null) });
  };

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">Marcas del Homepage</h3>
      <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
        {brands?.map((brand: BrandConfig) => (
          <Card key={brand.id}>
            <CardContent className="pt-4">
              {editingId === brand.id ? (
                <div className="space-y-3">
                  <div>
                    <Label>Nombre</Label>
                    <Input
                      value={editForm.displayName}
                      onChange={e => setEditForm(f => ({ ...f, displayName: e.target.value }))}
                    />
                  </div>
                  <div>
                    <Label>Logo URL</Label>
                    <Input
                      value={editForm.logoUrl}
                      onChange={e => setEditForm(f => ({ ...f, logoUrl: e.target.value }))}
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <Label>Vehículos</Label>
                      <Input
                        type="number"
                        value={editForm.vehicleCount}
                        onChange={e =>
                          setEditForm(f => ({ ...f, vehicleCount: Number(e.target.value) }))
                        }
                      />
                    </div>
                    <div>
                      <Label>Orden</Label>
                      <Input
                        type="number"
                        value={editForm.displayOrder}
                        onChange={e =>
                          setEditForm(f => ({ ...f, displayOrder: Number(e.target.value) }))
                        }
                      />
                    </div>
                  </div>
                  <label className="flex items-center gap-1 text-sm">
                    <input
                      type="checkbox"
                      checked={editForm.isActive}
                      onChange={e => setEditForm(f => ({ ...f, isActive: e.target.checked }))}
                    />
                    Activa
                  </label>
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      onClick={() => handleSave(brand.brandKey)}
                      disabled={updateMutation.isPending}
                    >
                      {updateMutation.isPending ? 'Guardando...' : '💾'}
                    </Button>
                    <Button size="sm" variant="outline" onClick={() => setEditingId(null)}>
                      ✕
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="text-center">
                  <p className="font-semibold">{brand.displayName}</p>
                  <p className="text-muted-foreground text-sm">{brand.vehicleCount} vehículos</p>
                  <div className="mt-2 flex items-center justify-center gap-2">
                    <Badge variant={brand.isActive ? 'default' : 'secondary'}>
                      {brand.isActive ? 'Activa' : 'Inactiva'}
                    </Badge>
                    <Button size="sm" variant="outline" onClick={() => startEditing(brand)}>
                      ✏️
                    </Button>
                  </div>
                  <p className="text-muted-foreground mt-1 text-xs">Orden: {brand.displayOrder}</p>
                </div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// MAIN PAGE
// =============================================================================

export default function AdminAdvertisingPage() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Publicidad — Panel Admin</h1>
        <p className="text-muted-foreground">
          Gestión de campañas, rotación y contenido del homepage.
        </p>
      </div>

      <Tabs defaultValue="overview">
        <TabsList>
          <TabsTrigger value="overview">📊 Resumen</TabsTrigger>
          <TabsTrigger value="rotation">🔄 Rotación</TabsTrigger>
          <TabsTrigger value="categories">📁 Categorías</TabsTrigger>
          <TabsTrigger value="brands">🏷️ Marcas</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6">
          <PlatformOverview />
        </TabsContent>

        <TabsContent value="rotation" className="mt-6">
          <RotationConfigPanel />
        </TabsContent>

        <TabsContent value="categories" className="mt-6">
          <CategoryManager />
        </TabsContent>

        <TabsContent value="brands" className="mt-6">
          <BrandManager />
        </TabsContent>
      </Tabs>
    </div>
  );
}
