/**
 * Seller Wizard - Step Progress Indicator
 *
 * Mobile-first horizontal stepper showing 3 steps.
 * Completed steps show a check, current step is highlighted.
 */

'use client';

import * as React from 'react';
import { Check } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface WizardStep {
  label: string;
  description: string;
}

interface StepIndicatorProps {
  steps: WizardStep[];
  currentStep: number;
}

export function StepIndicator({ steps, currentStep }: StepIndicatorProps) {
  return (
    <nav aria-label="Progreso del registro" className="w-full">
      {/* Mobile: Compact view */}
      <div className="flex items-center justify-between sm:hidden">
        <span className="text-sm font-medium text-[#00A870]">
          Paso {currentStep + 1} de {steps.length}
        </span>
        <span className="text-muted-foreground text-sm">{steps[currentStep]?.label}</span>
      </div>

      {/* Desktop: Full stepper */}
      <ol className="hidden sm:flex sm:items-center sm:justify-center sm:gap-0">
        {steps.map((step, index) => {
          const isCompleted = index < currentStep;
          const isCurrent = index === currentStep;

          return (
            <li key={index} className="flex items-center">
              {/* Step circle + label */}
              <div className="flex flex-col items-center gap-2">
                <div
                  className={cn(
                    'flex h-10 w-10 items-center justify-center rounded-full border-2 text-sm font-semibold transition-all duration-300',
                    isCompleted && 'border-[#00A870] bg-[#00A870] text-white',
                    isCurrent && 'border-[#00A870] bg-[#00A870]/10 text-[#00A870]',
                    !isCompleted && !isCurrent && 'border-muted-foreground/30 text-muted-foreground'
                  )}
                  aria-current={isCurrent ? 'step' : undefined}
                >
                  {isCompleted ? <Check className="h-5 w-5" /> : <span>{index + 1}</span>}
                </div>
                <div className="text-center">
                  <p
                    className={cn(
                      'text-xs font-medium',
                      isCompleted || isCurrent ? 'text-[#00A870]' : 'text-muted-foreground'
                    )}
                  >
                    {step.label}
                  </p>
                  <p className="text-muted-foreground hidden text-[10px] lg:block">
                    {step.description}
                  </p>
                </div>
              </div>

              {/* Connector line */}
              {index < steps.length - 1 && (
                <div
                  className={cn(
                    'mx-4 h-0.5 w-16 transition-colors duration-300 lg:w-24',
                    isCompleted ? 'bg-[#00A870]' : 'bg-muted-foreground/20'
                  )}
                />
              )}
            </li>
          );
        })}
      </ol>

      {/* Progress bar (mobile) */}
      <div className="mt-3 sm:hidden">
        <div className="bg-muted h-1.5 w-full overflow-hidden rounded-full">
          <div
            className="h-full rounded-full bg-[#00A870] transition-all duration-500"
            style={{ width: `${((currentStep + 1) / steps.length) * 100}%` }}
          />
        </div>
      </div>
    </nav>
  );
}
