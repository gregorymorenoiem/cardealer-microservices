# 🧩 Componentes Base UI

> **Tiempo estimado:** 45 minutos
> **Prerrequisitos:** Design tokens configurados, shadcn/ui instalado

---

## 📋 OBJETIVO

Crear componentes base reutilizables con:

- Accesibilidad completa (WCAG 2.1 AA)
- Variants y sizes configurables
- Animaciones sutiles
- TypeScript estricto

---

## 🔧 PASO 1: Instalar shadcn/ui

### Comando

```bash
# Inicializar shadcn/ui
pnpm dlx shadcn@latest init

# Cuando pregunte, seleccionar:
# - Style: Default
# - Base color: Slate
# - CSS variables: Yes
```

### Instalar componentes necesarios

```bash
# Instalar componentes base
pnpm dlx shadcn@latest add button
pnpm dlx shadcn@latest add input
pnpm dlx shadcn@latest add label
pnpm dlx shadcn@latest add card
pnpm dlx shadcn@latest add badge
pnpm dlx shadcn@latest add skeleton
pnpm dlx shadcn@latest add dialog
pnpm dlx shadcn@latest add dropdown-menu
pnpm dlx shadcn@latest add select
pnpm dlx shadcn@latest add toast
pnpm dlx shadcn@latest add tooltip
pnpm dlx shadcn@latest add avatar
pnpm dlx shadcn@latest add separator
pnpm dlx shadcn@latest add sheet
pnpm dlx shadcn@latest add tabs
pnpm dlx shadcn@latest add accordion
```

---

## 🔧 PASO 2: Extender Button con variants OKLA

### Código a crear

```tsx
// filepath: src/components/ui/button.tsx
import * as React from "react";
import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-lg text-sm font-medium transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg]:size-4 [&_svg]:shrink-0",
  {
    variants: {
      variant: {
        default:
          "bg-primary text-primary-foreground shadow-sm hover:bg-primary-600 hover:shadow-button-primary active:scale-[0.98]",
        destructive:
          "bg-danger text-white shadow-sm hover:bg-danger-600 hover:shadow-button-danger active:scale-[0.98]",
        outline:
          "border border-input bg-background shadow-sm hover:bg-accent hover:text-accent-foreground active:scale-[0.98]",
        secondary:
          "bg-secondary text-secondary-foreground shadow-sm hover:bg-secondary-200 active:scale-[0.98]",
        ghost: "hover:bg-accent hover:text-accent-foreground",
        link: "text-primary underline-offset-4 hover:underline",
        success:
          "bg-success text-white shadow-sm hover:bg-success-600 hover:shadow-button-success active:scale-[0.98]",
        warning:
          "bg-warning text-black shadow-sm hover:bg-warning-600 active:scale-[0.98]",
      },
      size: {
        default: "h-10 px-4 py-2",
        sm: "h-8 rounded-md px-3 text-xs",
        lg: "h-12 rounded-lg px-6 text-base",
        xl: "h-14 rounded-xl px-8 text-lg",
        icon: "h-10 w-10",
        "icon-sm": "h-8 w-8",
        "icon-lg": "h-12 w-12",
      },
      fullWidth: {
        true: "w-full",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  },
);

export interface ButtonProps
  extends
    React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
  isLoading?: boolean;
  loadingText?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant,
      size,
      fullWidth,
      asChild = false,
      isLoading = false,
      loadingText,
      leftIcon,
      rightIcon,
      children,
      disabled,
      ...props
    },
    ref,
  ) => {
    const Comp = asChild ? Slot : "button";

    return (
      <Comp
        className={cn(buttonVariants({ variant, size, fullWidth, className }))}
        ref={ref}
        disabled={disabled || isLoading}
        {...props}
      >
        {isLoading ? (
          <>
            <Loader2 className="animate-spin" />
            {loadingText || children}
          </>
        ) : (
          <>
            {leftIcon}
            {children}
            {rightIcon}
          </>
        )}
      </Comp>
    );
  },
);
Button.displayName = "Button";

export { Button, buttonVariants };
```

---

## 🔧 PASO 3: Crear Input mejorado

### Código a crear

```tsx
// filepath: src/components/ui/input.tsx
import * as React from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

const inputVariants = cva(
  "flex w-full rounded-lg border bg-background text-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
  {
    variants: {
      size: {
        sm: "h-8 px-3 text-xs",
        default: "h-10 px-3 py-2",
        lg: "h-12 px-4 py-3 text-base",
      },
      state: {
        default: "border-input",
        error: "border-danger focus-visible:ring-danger",
        success: "border-success focus-visible:ring-success",
      },
    },
    defaultVariants: {
      size: "default",
      state: "default",
    },
  },
);

export interface InputProps
  extends
    Omit<React.InputHTMLAttributes<HTMLInputElement>, "size">,
    VariantProps<typeof inputVariants> {
  leftElement?: React.ReactNode;
  rightElement?: React.ReactNode;
  error?: string;
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      type,
      size,
      state,
      leftElement,
      rightElement,
      error,
      ...props
    },
    ref,
  ) => {
    const inputState = error ? "error" : state;

    if (leftElement || rightElement) {
      return (
        <div className="relative">
          {leftElement && (
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
              {leftElement}
            </div>
          )}
          <input
            type={type}
            className={cn(
              inputVariants({ size, state: inputState }),
              leftElement && "pl-10",
              rightElement && "pr-10",
              className,
            )}
            ref={ref}
            {...props}
          />
          {rightElement && (
            <div className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground">
              {rightElement}
            </div>
          )}
        </div>
      );
    }

    return (
      <input
        type={type}
        className={cn(inputVariants({ size, state: inputState, className }))}
        ref={ref}
        {...props}
      />
    );
  },
);
Input.displayName = "Input";

export { Input, inputVariants };
```

---

## 🔧 PASO 4: Crear FormField con Label y Error

### Código a crear

```tsx
// filepath: src/components/ui/form-field.tsx
import * as React from "react";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

interface FormFieldProps {
  id: string;
  label: string;
  error?: string;
  hint?: string;
  required?: boolean;
  children: React.ReactNode;
  className?: string;
}

export function FormField({
  id,
  label,
  error,
  hint,
  required,
  children,
  className,
}: FormFieldProps) {
  return (
    <div className={cn("space-y-2", className)}>
      <Label
        htmlFor={id}
        className={cn("text-sm font-medium", error && "text-danger")}
      >
        {label}
        {required && <span className="ml-1 text-danger">*</span>}
      </Label>
      {children}
      {error ? (
        <p className="text-xs text-danger" id={`${id}-error`} role="alert">
          {error}
        </p>
      ) : hint ? (
        <p className="text-xs text-muted-foreground" id={`${id}-hint`}>
          {hint}
        </p>
      ) : null}
    </div>
  );
}
```

---

## 🔧 PASO 5: Crear VehicleCard

### Código a crear

```tsx
// filepath: src/components/vehicles/VehicleCard.tsx
"use client";

import * as React from "react";
import Link from "next/link";
import Image from "next/image";
import { Heart, MapPin, Gauge, Calendar, Fuel } from "lucide-react";
import { cn, formatCurrency, formatNumber } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { VehicleSummary } from "@/types";

interface VehicleCardProps {
  vehicle: VehicleSummary;
  onFavoriteClick?: (vehicleId: string) => void;
  isFavorited?: boolean;
  className?: string;
  priority?: boolean;
}

export function VehicleCard({
  vehicle,
  onFavoriteClick,
  isFavorited = false,
  className,
  priority = false,
}: VehicleCardProps) {
  const {
    id,
    slug,
    title,
    price,
    year,
    make,
    model,
    mileage,
    city,
    condition,
    primaryImage,
    sellerType,
    isVerified,
  } = vehicle;

  return (
    <article
      className={cn(
        "group relative overflow-hidden rounded-xl border bg-card shadow-card transition-all duration-300",
        "hover:-translate-y-1 hover:shadow-card-lg",
        className,
      )}
    >
      {/* Image Container */}
      <Link
        href={`/vehiculos/${slug}`}
        className="relative block aspect-[4/3] overflow-hidden"
      >
        <Image
          src={primaryImage || "/images/placeholder-vehicle.jpg"}
          alt={title}
          fill
          sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
          className="object-cover transition-transform duration-300 group-hover:scale-105"
          priority={priority}
        />

        {/* Condition Badge */}
        <div className="absolute left-3 top-3 flex gap-2">
          {condition === "new" && (
            <Badge className="bg-primary text-white">Nuevo</Badge>
          )}
          {condition === "certified" && (
            <Badge className="bg-success text-white">Certificado</Badge>
          )}
          {sellerType === "dealer" && isVerified && (
            <Badge variant="secondary" className="bg-white/90">
              <span className="text-success">✓</span> Verificado
            </Badge>
          )}
        </div>

        {/* Gradient overlay */}
        <div className="absolute inset-x-0 bottom-0 h-20 bg-gradient-to-t from-black/50 to-transparent" />
      </Link>

      {/* Favorite Button */}
      <Button
        variant="ghost"
        size="icon"
        className={cn(
          "absolute right-3 top-3 z-10 bg-white/80 backdrop-blur-sm",
          "hover:bg-white hover:text-danger",
          isFavorited && "text-danger",
        )}
        onClick={(e) => {
          e.preventDefault();
          onFavoriteClick?.(id);
        }}
        aria-label={isFavorited ? "Quitar de favoritos" : "Agregar a favoritos"}
      >
        <Heart className={cn("h-5 w-5", isFavorited && "fill-current")} />
      </Button>

      {/* Content */}
      <div className="p-4">
        {/* Title */}
        <Link href={`/vehiculos/${slug}`}>
          <h3 className="line-clamp-1 font-semibold text-foreground group-hover:text-primary">
            {year} {make} {model}
          </h3>
        </Link>

        <p className="mt-1 line-clamp-1 text-sm text-muted-foreground">
          {title}
        </p>

        {/* Price */}
        <div className="mt-3">
          <span className="price-display text-price-md text-primary">
            <span className="price-currency">RD$</span>
            {formatNumber(price)}
          </span>
        </div>

        {/* Specs Grid */}
        <div className="mt-4 grid grid-cols-2 gap-2 text-sm text-muted-foreground">
          <div className="flex items-center gap-1.5">
            <Calendar className="h-4 w-4" />
            <span>{year}</span>
          </div>
          <div className="flex items-center gap-1.5">
            <Gauge className="h-4 w-4" />
            <span>{formatNumber(mileage)} km</span>
          </div>
          <div className="col-span-2 flex items-center gap-1.5">
            <MapPin className="h-4 w-4" />
            <span className="truncate">{city}</span>
          </div>
        </div>
      </div>
    </article>
  );
}

// Skeleton version for loading
export function VehicleCardSkeleton() {
  return (
    <div className="overflow-hidden rounded-xl border bg-card shadow-card">
      <div className="aspect-[4/3] animate-pulse bg-muted" />
      <div className="p-4 space-y-3">
        <div className="h-5 w-3/4 animate-pulse rounded bg-muted" />
        <div className="h-4 w-1/2 animate-pulse rounded bg-muted" />
        <div className="h-7 w-2/3 animate-pulse rounded bg-muted" />
        <div className="grid grid-cols-2 gap-2">
          <div className="h-4 animate-pulse rounded bg-muted" />
          <div className="h-4 animate-pulse rounded bg-muted" />
        </div>
      </div>
    </div>
  );
}
```

---

## 🔧 PASO 6: Crear PriceDisplay

### Código a crear

```tsx
// filepath: src/components/ui/price-display.tsx
import * as React from "react";
import { cn, formatNumber } from "@/lib/utils";

interface PriceDisplayProps {
  price: number;
  originalPrice?: number;
  size?: "sm" | "md" | "lg" | "xl";
  showDiscount?: boolean;
  className?: string;
}

const sizeClasses = {
  sm: "text-base",
  md: "text-xl",
  lg: "text-2xl",
  xl: "text-3xl",
};

export function PriceDisplay({
  price,
  originalPrice,
  size = "md",
  showDiscount = true,
  className,
}: PriceDisplayProps) {
  const hasDiscount = originalPrice && originalPrice > price;
  const discountPercent = hasDiscount
    ? Math.round(((originalPrice - price) / originalPrice) * 100)
    : 0;

  return (
    <div className={cn("flex flex-wrap items-baseline gap-2", className)}>
      {/* Main Price */}
      <span
        className={cn(
          "price-display font-bold text-primary",
          sizeClasses[size],
        )}
      >
        <span className="text-[0.6em] font-semibold">RD$</span>
        {formatNumber(price)}
      </span>

      {/* Original Price */}
      {hasDiscount && (
        <span className="text-sm text-muted-foreground line-through">
          RD${formatNumber(originalPrice)}
        </span>
      )}

      {/* Discount Badge */}
      {hasDiscount && showDiscount && (
        <span className="rounded-full bg-danger/10 px-2 py-0.5 text-xs font-medium text-danger">
          -{discountPercent}%
        </span>
      )}
    </div>
  );
}

// Compact version for lists
export function PriceDisplayCompact({
  price,
  className,
}: {
  price: number;
  className?: string;
}) {
  return (
    <span
      className={cn(
        "price-display tabular-nums font-semibold text-primary",
        className,
      )}
    >
      RD${formatNumber(price)}
    </span>
  );
}
```

---

## 🔧 PASO 7: Crear EmptyState

### Código a crear

```tsx
// filepath: src/components/ui/empty-state.tsx
import * as React from "react";
import { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

interface EmptyStateProps {
  icon: LucideIcon;
  title: string;
  description: string;
  action?: {
    label: string;
    onClick: () => void;
  };
  secondaryAction?: {
    label: string;
    onClick: () => void;
  };
  className?: string;
}

export function EmptyState({
  icon: Icon,
  title,
  description,
  action,
  secondaryAction,
  className,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center px-4 py-12 text-center",
        className,
      )}
    >
      <div className="mb-4 rounded-full bg-muted p-4">
        <Icon className="h-8 w-8 text-muted-foreground" />
      </div>

      <h3 className="mb-2 text-lg font-semibold text-foreground">{title}</h3>

      <p className="mb-6 max-w-md text-sm text-muted-foreground">
        {description}
      </p>

      {(action || secondaryAction) && (
        <div className="flex flex-wrap justify-center gap-3">
          {action && <Button onClick={action.onClick}>{action.label}</Button>}
          {secondaryAction && (
            <Button variant="outline" onClick={secondaryAction.onClick}>
              {secondaryAction.label}
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
```

---

## 🔧 PASO 8: Crear LoadingSpinner

### Código a crear

```tsx
// filepath: src/components/ui/loading.tsx
import * as React from "react";
import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface LoadingSpinnerProps {
  size?: "sm" | "md" | "lg";
  className?: string;
}

const sizeClasses = {
  sm: "h-4 w-4",
  md: "h-6 w-6",
  lg: "h-8 w-8",
};

export function LoadingSpinner({
  size = "md",
  className,
}: LoadingSpinnerProps) {
  return (
    <Loader2
      className={cn("animate-spin text-primary", sizeClasses[size], className)}
      aria-label="Cargando..."
    />
  );
}

// Full page loader
export function PageLoader() {
  return (
    <div className="flex min-h-[50vh] items-center justify-center">
      <div className="flex flex-col items-center gap-4">
        <LoadingSpinner size="lg" />
        <p className="text-sm text-muted-foreground">Cargando...</p>
      </div>
    </div>
  );
}

// Inline loader
export function InlineLoader({ text = "Cargando..." }: { text?: string }) {
  return (
    <div className="flex items-center gap-2 text-sm text-muted-foreground">
      <LoadingSpinner size="sm" />
      <span>{text}</span>
    </div>
  );
}

// Button loading state
export function ButtonLoader() {
  return <Loader2 className="h-4 w-4 animate-spin" />;
}
```

---

## 🔧 PASO 9: Test para componentes

### Código a crear

```tsx
// filepath: __tests__/components/Button.test.tsx
import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";

describe("Button", () => {
  it("renders children correctly", () => {
    render(<Button>Click me</Button>);
    expect(
      screen.getByRole("button", { name: /click me/i }),
    ).toBeInTheDocument();
  });

  it("handles click events", async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();

    render(<Button onClick={handleClick}>Click me</Button>);
    await user.click(screen.getByRole("button"));

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it("shows loading state", () => {
    render(<Button isLoading>Submit</Button>);

    expect(screen.getByRole("button")).toBeDisabled();
    // Loader icon should be visible
  });

  it("renders with left icon", () => {
    render(<Button leftIcon={<Plus data-testid="plus-icon" />}>Add</Button>);

    expect(screen.getByTestId("plus-icon")).toBeInTheDocument();
  });

  it("applies variant classes", () => {
    const { rerender } = render(<Button variant="destructive">Delete</Button>);
    expect(screen.getByRole("button")).toHaveClass("bg-danger");

    rerender(<Button variant="success">Confirm</Button>);
    expect(screen.getByRole("button")).toHaveClass("bg-success");
  });

  it("renders as full width when specified", () => {
    render(<Button fullWidth>Full Width</Button>);
    expect(screen.getByRole("button")).toHaveClass("w-full");
  });

  it("is disabled when disabled prop is true", () => {
    render(<Button disabled>Disabled</Button>);
    expect(screen.getByRole("button")).toBeDisabled();
  });
});
```

```tsx
// filepath: __tests__/components/VehicleCard.test.tsx
import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  VehicleCard,
  VehicleCardSkeleton,
} from "@/components/vehicles/VehicleCard";
import type { VehicleSummary } from "@/types";

const mockVehicle: VehicleSummary = {
  id: "1",
  slug: "toyota-camry-2024",
  title: "Toyota Camry SE 2024",
  price: 1850000,
  year: 2024,
  make: "Toyota",
  model: "Camry",
  mileage: 15000,
  city: "Santo Domingo",
  condition: "used",
  primaryImage: "/images/camry.jpg",
  sellerType: "dealer",
  isVerified: true,
};

describe("VehicleCard", () => {
  it("renders vehicle information correctly", () => {
    render(<VehicleCard vehicle={mockVehicle} />);

    expect(screen.getByText(/2024 Toyota Camry/i)).toBeInTheDocument();
    expect(screen.getByText(/1,850,000/)).toBeInTheDocument();
    expect(screen.getByText(/15,000 km/)).toBeInTheDocument();
    expect(screen.getByText(/Santo Domingo/)).toBeInTheDocument();
  });

  it("shows verified badge for verified dealers", () => {
    render(<VehicleCard vehicle={mockVehicle} />);

    expect(screen.getByText(/Verificado/)).toBeInTheDocument();
  });

  it("shows new badge for new vehicles", () => {
    render(<VehicleCard vehicle={{ ...mockVehicle, condition: "new" }} />);

    expect(screen.getByText("Nuevo")).toBeInTheDocument();
  });

  it("handles favorite click", async () => {
    const user = userEvent.setup();
    const handleFavorite = vi.fn();

    render(
      <VehicleCard vehicle={mockVehicle} onFavoriteClick={handleFavorite} />,
    );

    await user.click(
      screen.getByRole("button", { name: /agregar a favoritos/i }),
    );
    expect(handleFavorite).toHaveBeenCalledWith("1");
  });

  it("shows filled heart when favorited", () => {
    render(<VehicleCard vehicle={mockVehicle} isFavorited />);

    expect(
      screen.getByRole("button", { name: /quitar de favoritos/i }),
    ).toBeInTheDocument();
  });

  it("links to vehicle detail page", () => {
    render(<VehicleCard vehicle={mockVehicle} />);

    const links = screen.getAllByRole("link");
    expect(links[0]).toHaveAttribute("href", "/vehiculos/toyota-camry-2024");
  });
});

describe("VehicleCardSkeleton", () => {
  it("renders loading skeleton", () => {
    render(<VehicleCardSkeleton />);

    // Should have animated skeleton elements
    const skeletons = document.querySelectorAll(".animate-pulse");
    expect(skeletons.length).toBeGreaterThan(0);
  });
});
```

---

## ✅ VALIDACIÓN

### Ejecutar tests

```bash
# Ejecutar todos los tests de componentes
pnpm test __tests__/components/

# Output esperado:
# ✓ __tests__/components/Button.test.tsx (7)
# ✓ __tests__/components/VehicleCard.test.tsx (7)
#
# Test Files  2 passed (2)
# Tests  14 passed (14)
```

### Verificar build

```bash
pnpm build
# No debería haber errores de TypeScript
```

---

## 📊 RESUMEN

| Componente       | Ubicación                  | Propósito                           |
| ---------------- | -------------------------- | ----------------------------------- |
| `Button`         | `ui/button.tsx`            | Botón con variants, loading, iconos |
| `Input`          | `ui/input.tsx`             | Input con estados, iconos           |
| `FormField`      | `ui/form-field.tsx`        | Wrapper con label/error             |
| `VehicleCard`    | `vehicles/VehicleCard.tsx` | Card de vehículo para grids         |
| `PriceDisplay`   | `ui/price-display.tsx`     | Formateo de precios RD$             |
| `EmptyState`     | `ui/empty-state.tsx`       | Estado vacío con acción             |
| `LoadingSpinner` | `ui/loading.tsx`           | Indicadores de carga                |

---

## ➡️ SIGUIENTE PASO

Continuar con: `docs/frontend-rebuild/02-UX-DESIGN-SYSTEM/04-patrones-ux.md`
