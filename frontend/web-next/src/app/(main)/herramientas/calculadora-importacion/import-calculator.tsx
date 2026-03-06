'use client';

import { useState, useMemo } from 'react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Ship, DollarSign, Fuel, Gauge, Car, MapPin, FileText, AlertTriangle } from 'lucide-react';

// Exchange rate (reference)
const EXCHANGE_RATE = 60.5; // RD$ per USD — referential

// Shipping costs by port
const PORTS = [
  { value: 'miami', label: 'Miami, FL', freight: 950 },
  { value: 'newjersey', label: 'New Jersey, NJ', freight: 1200 },
  { value: 'houston', label: 'Houston, TX', freight: 1100 },
  { value: 'savannah', label: 'Savannah, GA', freight: 1050 },
  { value: 'other', label: 'Otro puerto', freight: 1500 },
];

const FUEL_TYPES = [
  { value: 'gasoline', label: 'Gasolina' },
  { value: 'diesel', label: 'Diésel' },
  { value: 'electric', label: 'Eléctrico' },
  { value: 'hybrid', label: 'Híbrido' },
];

const ENGINE_SIZES = [
  { value: '1000', label: 'Hasta 1,000 cc' },
  { value: '1500', label: '1,001 - 1,500 cc' },
  { value: '2000', label: '1,501 - 2,000 cc' },
  { value: '2500', label: '2,001 - 2,500 cc' },
  { value: '3000', label: '2,501 - 3,000 cc' },
  { value: '3500', label: '3,001 - 3,500 cc' },
  { value: '4000', label: '3,501 - 4,000 cc' },
  { value: '5000', label: 'Más de 4,000 cc' },
];

const VEHICLE_TYPES = [
  { value: 'sedan', label: 'Sedán' },
  { value: 'suv', label: 'SUV / Crossover' },
  { value: 'pickup', label: 'Pickup / Camioneta' },
  { value: 'van', label: 'Van / Minivan' },
  { value: 'coupe', label: 'Coupé / Deportivo' },
  { value: 'hatchback', label: 'Hatchback' },
  { value: 'convertible', label: 'Convertible' },
];

function formatUSD(value: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
  }).format(value);
}

function formatRD(value: number): string {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function getSelectiveTaxRate(fuelType: string, engineSizeCC: number): number {
  if (fuelType === 'electric') return 0;

  let baseRate = 0;

  if (fuelType === 'gasoline') {
    if (engineSizeCC <= 2000) baseRate = 0;
    else if (engineSizeCC <= 3000) baseRate = 0.3;
    else baseRate = 0.51;
  } else if (fuelType === 'diesel') {
    if (engineSizeCC <= 2500) baseRate = 0;
    else baseRate = 0.51;
  }

  // Hybrids get 50% discount
  if (fuelType === 'hybrid') {
    // Use gasoline rates as base
    if (engineSizeCC <= 2000) baseRate = 0;
    else if (engineSizeCC <= 3000) baseRate = 0.3 * 0.5;
    else baseRate = 0.51 * 0.5;
  }

  return baseRate;
}

function getArancelRate(fuelType: string): number {
  if (fuelType === 'electric') return 0;
  return 0.2; // 20% for gasoline, diesel, hybrid
}

interface ImportResult {
  fobUSD: number;
  freightUSD: number;
  insuranceUSD: number;
  cifUSD: number;
  arancelUSD: number;
  arancelRate: number;
  selectiveUSD: number;
  selectiveRate: number;
  itbisUSD: number;
  firstPlateUSD: number;
  marbeteUSD: number;
  co2USD: number;
  totalUSD: number;
  totalRD: number;
  // RD$ breakdown
  cifRD: number;
  arancelRD: number;
  selectiveRD: number;
  itbisRD: number;
  freightRD: number;
  insuranceRD: number;
}

function calculateImport(
  fobUSD: number,
  freightUSD: number,
  fuelType: string,
  engineSizeCC: number
): ImportResult {
  const insuranceUSD = fobUSD * 0.015; // 1.5% of FOB
  const cifUSD = fobUSD + freightUSD + insuranceUSD;

  const arancelRate = getArancelRate(fuelType);
  const arancelUSD = cifUSD * arancelRate;

  const selectiveRate = getSelectiveTaxRate(fuelType, engineSizeCC);
  const selectiveBase = cifUSD + arancelUSD;
  const selectiveUSD = selectiveBase * selectiveRate;

  const itbisBase = cifUSD + arancelUSD + selectiveUSD;
  const itbisUSD = itbisBase * 0.18;

  // Fixed fees (estimated, in USD equivalent)
  const firstPlateUSD = 8000 / EXCHANGE_RATE;
  const marbeteUSD = 3500 / EXCHANGE_RATE;
  const co2USD = fuelType === 'electric' ? 0 : 2000 / EXCHANGE_RATE;

  const totalUSD =
    cifUSD + arancelUSD + selectiveUSD + itbisUSD + firstPlateUSD + marbeteUSD + co2USD;

  return {
    fobUSD,
    freightUSD,
    insuranceUSD,
    cifUSD,
    arancelUSD,
    arancelRate,
    selectiveUSD,
    selectiveRate,
    itbisUSD,
    firstPlateUSD,
    marbeteUSD,
    co2USD,
    totalUSD,
    totalRD: totalUSD * EXCHANGE_RATE,
    cifRD: cifUSD * EXCHANGE_RATE,
    arancelRD: arancelUSD * EXCHANGE_RATE,
    selectiveRD: selectiveUSD * EXCHANGE_RATE,
    itbisRD: itbisUSD * EXCHANGE_RATE,
    freightRD: freightUSD * EXCHANGE_RATE,
    insuranceRD: insuranceUSD * EXCHANGE_RATE,
  };
}

export function ImportCalculator() {
  const [fobValue, setFobValue] = useState<string>('15000');
  const [vehicleYear, setVehicleYear] = useState<string>('2023');
  const [fuelType, setFuelType] = useState<string>('gasoline');
  const [engineSize, setEngineSize] = useState<string>('2000');
  const [vehicleType, setVehicleType] = useState<string>('suv');
  const [port, setPort] = useState<string>('miami');

  const fob = parseFloat(fobValue.replace(/[^0-9.]/g, '')) || 0;
  const engineCC = parseInt(engineSize) || 2000;
  const selectedPort = PORTS.find(p => p.value === port);
  const freight = selectedPort?.freight || 1500;

  const result = useMemo(() => {
    if (fob <= 0) return null;
    return calculateImport(fob, freight, fuelType, engineCC);
  }, [fob, freight, fuelType, engineCC]);

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 15 }, (_, i) => currentYear - i);

  return (
    <div className="mx-auto max-w-5xl">
      <div className="grid gap-8 lg:grid-cols-5">
        {/* Input Form */}
        <Card className="lg:col-span-3">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Ship className="h-5 w-5 text-blue-600" />
              Datos del vehículo a importar
            </CardTitle>
            <CardDescription>
              Ingresa la información del vehículo que deseas traer desde EEUU
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Valor FOB */}
            <div className="space-y-2">
              <Label htmlFor="fob" className="flex items-center gap-2">
                <DollarSign className="h-4 w-4" />
                Valor FOB del vehículo (USD)
              </Label>
              <Input
                id="fob"
                type="text"
                inputMode="numeric"
                value={fobValue}
                onChange={e => {
                  const raw = e.target.value.replace(/[^0-9]/g, '');
                  setFobValue(raw);
                }}
                placeholder="15,000"
              />
              {fob > 0 && (
                <p className="text-muted-foreground text-sm">
                  {formatUSD(fob)} ≈ {formatRD(fob * EXCHANGE_RATE)}
                </p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              {/* Año del vehículo */}
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <Car className="h-4 w-4" />
                  Año del vehículo
                </Label>
                <Select value={vehicleYear} onValueChange={setVehicleYear}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecciona año" />
                  </SelectTrigger>
                  <SelectContent>
                    {years.map(y => (
                      <SelectItem key={y} value={y.toString()}>
                        {y}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Tipo de vehículo */}
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <Car className="h-4 w-4" />
                  Tipo de vehículo
                </Label>
                <Select value={vehicleType} onValueChange={setVehicleType}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecciona tipo" />
                  </SelectTrigger>
                  <SelectContent>
                    {VEHICLE_TYPES.map(t => (
                      <SelectItem key={t.value} value={t.value}>
                        {t.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              {/* Tipo de combustible */}
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <Fuel className="h-4 w-4" />
                  Tipo de combustible
                </Label>
                <Select value={fuelType} onValueChange={setFuelType}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecciona combustible" />
                  </SelectTrigger>
                  <SelectContent>
                    {FUEL_TYPES.map(f => (
                      <SelectItem key={f.value} value={f.value}>
                        {f.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {fuelType === 'electric' && (
                  <p className="text-xs text-emerald-600">
                    ✅ Exento de arancel e ISC (Ley 103-13)
                  </p>
                )}
                {fuelType === 'hybrid' && (
                  <p className="text-xs text-blue-600">✅ 50% descuento en ISC</p>
                )}
              </div>

              {/* Cilindrada */}
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <Gauge className="h-4 w-4" />
                  Cilindrada
                </Label>
                <Select
                  value={engineSize}
                  onValueChange={setEngineSize}
                  disabled={fuelType === 'electric'}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Selecciona cilindrada" />
                  </SelectTrigger>
                  <SelectContent>
                    {ENGINE_SIZES.map(e => (
                      <SelectItem key={e.value} value={e.value}>
                        {e.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {fuelType === 'electric' && (
                  <p className="text-muted-foreground text-xs">
                    No aplica para vehículos eléctricos
                  </p>
                )}
              </div>
            </div>

            {/* Puerto de origen */}
            <div className="space-y-2">
              <Label className="flex items-center gap-2">
                <MapPin className="h-4 w-4" />
                Puerto de origen (EEUU)
              </Label>
              <Select value={port} onValueChange={setPort}>
                <SelectTrigger>
                  <SelectValue placeholder="Selecciona puerto" />
                </SelectTrigger>
                <SelectContent>
                  {PORTS.map(p => (
                    <SelectItem key={p.value} value={p.value}>
                      {p.label} (flete est. {formatUSD(p.freight)})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Age warning */}
            {parseInt(vehicleYear) < currentYear - 5 && (
              <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 p-4">
                <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-amber-600" />
                <div className="text-sm text-amber-800">
                  <strong>Vehículo con más de 5 años.</strong> Puede tener un recargo adicional del
                  20% sobre el arancel y restricciones de importación adicionales.
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Results */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-blue-600" />
              Costo estimado
            </CardTitle>
            <CardDescription>Tasa de cambio ref.: RD${EXCHANGE_RATE}/USD</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {result ? (
              <>
                {/* Total */}
                <div className="rounded-xl bg-gradient-to-br from-[#1e40af] to-[#1e3a5f] p-6 text-center text-white">
                  <p className="mb-1 text-sm text-white/80">Costo total estimado</p>
                  <p className="text-3xl font-bold">{formatRD(result.totalRD)}</p>
                  <p className="mt-1 text-sm text-white/70">≈ {formatUSD(result.totalUSD)}</p>
                </div>

                <Separator />

                {/* Valor CIF */}
                <div className="space-y-2">
                  <p className="text-muted-foreground text-sm font-semibold tracking-wide uppercase">
                    Valor CIF
                  </p>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Valor FOB</span>
                    <span>{formatUSD(result.fobUSD)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Flete marítimo</span>
                    <span>{formatUSD(result.freightUSD)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Seguro marítimo (1.5%)</span>
                    <span>{formatUSD(result.insuranceUSD)}</span>
                  </div>
                  <div className="flex justify-between border-t pt-1 text-sm font-medium">
                    <span>Valor CIF</span>
                    <span>{formatUSD(result.cifUSD)}</span>
                  </div>
                </div>

                <Separator />

                {/* Impuestos */}
                <div className="space-y-2">
                  <p className="text-muted-foreground text-sm font-semibold tracking-wide uppercase">
                    Impuestos DGA
                  </p>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">
                      Arancel ({(result.arancelRate * 100).toFixed(0)}%)
                    </span>
                    <span
                      className={result.arancelUSD === 0 ? 'text-emerald-600' : 'text-amber-600'}
                    >
                      {result.arancelUSD === 0 ? 'Exento' : formatUSD(result.arancelUSD)}
                    </span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">
                      Selectivo al consumo ({(result.selectiveRate * 100).toFixed(0)}%)
                    </span>
                    <span
                      className={result.selectiveUSD === 0 ? 'text-emerald-600' : 'text-amber-600'}
                    >
                      {result.selectiveUSD === 0 ? 'Exento' : formatUSD(result.selectiveUSD)}
                    </span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">ITBIS (18%)</span>
                    <span className="text-amber-600">{formatUSD(result.itbisUSD)}</span>
                  </div>
                </div>

                <Separator />

                {/* Otros costos */}
                <div className="space-y-2">
                  <p className="text-muted-foreground text-sm font-semibold tracking-wide uppercase">
                    Otros costos
                  </p>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Primera placa</span>
                    <span>{formatRD(8000)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Marbete</span>
                    <span>{formatRD(3500)}</span>
                  </div>
                  {result.co2USD > 0 && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Emisiones CO₂</span>
                      <span>{formatRD(2000)}</span>
                    </div>
                  )}
                </div>

                <Separator />

                {/* Resumen en RD$ */}
                <div className="bg-muted/50 space-y-2 rounded-lg p-4">
                  <p className="text-sm font-semibold">Resumen en RD$</p>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Valor del vehículo (FOB)</span>
                    <span>{formatRD(fob * EXCHANGE_RATE)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Total impuestos y costos</span>
                    <span className="text-amber-600">
                      {formatRD(result.totalRD - fob * EXCHANGE_RATE)}
                    </span>
                  </div>
                  <div className="flex justify-between border-t pt-1 font-semibold">
                    <span>TOTAL ESTIMADO</span>
                    <span className="text-lg">{formatRD(result.totalRD)}</span>
                  </div>
                  <p className="text-muted-foreground text-xs">
                    Impuestos representan el {(((result.totalUSD - fob) / fob) * 100).toFixed(1)}%
                    del valor FOB
                  </p>
                </div>
              </>
            ) : (
              <div className="text-muted-foreground py-8 text-center">
                <Ship className="mx-auto mb-3 h-12 w-12 opacity-30" />
                <p>Ingresa el valor del vehículo para calcular los costos de importación</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
