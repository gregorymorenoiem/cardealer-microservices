/**
 * Breadcrumbs Component
 * Navigation breadcrumb trail
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { ChevronRight, Home } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

interface BreadcrumbsProps {
  items: BreadcrumbItem[];
  className?: string;
}

export function Breadcrumbs({ items, className }: BreadcrumbsProps) {
  return (
    <nav aria-label="Breadcrumb" className={cn('flex items-center text-sm', className)}>
      <ol className="flex flex-wrap items-center gap-1">
        {/* Home link */}
        <li className="flex items-center">
          <Link
            href="/"
            className="text-muted-foreground hover:text-foreground transition-colors"
            aria-label="Inicio"
          >
            <Home className="h-4 w-4" />
          </Link>
        </li>

        {/* Separator after home */}
        <li className="text-muted-foreground flex items-center" aria-hidden="true">
          <ChevronRight className="h-4 w-4" />
        </li>

        {/* Breadcrumb items */}
        {items.map((item, index) => {
          const isLast = index === items.length - 1;

          return (
            <React.Fragment key={index}>
              <li className="flex items-center">
                {item.href && !isLast ? (
                  <Link
                    href={item.href}
                    className="text-muted-foreground hover:text-foreground transition-colors"
                  >
                    {item.label}
                  </Link>
                ) : (
                  <span
                    className={cn(isLast ? 'text-foreground font-medium' : 'text-muted-foreground')}
                    aria-current={isLast ? 'page' : undefined}
                  >
                    {item.label}
                  </span>
                )}
              </li>

              {/* Separator (except for last item) */}
              {!isLast && (
                <li className="text-muted-foreground flex items-center" aria-hidden="true">
                  <ChevronRight className="h-4 w-4" />
                </li>
              )}
            </React.Fragment>
          );
        })}
      </ol>
    </nav>
  );
}

export default Breadcrumbs;
