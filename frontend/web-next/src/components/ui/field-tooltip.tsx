/**
 * FieldTooltip — Reusable contextual help icon with tooltip
 *
 * Shows a small (?) icon next to form field labels.
 * Uses non-technical Spanish language for dealer-friendly onboarding.
 */

'use client';

import { HelpCircle } from 'lucide-react';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';

interface FieldTooltipProps {
  text: string;
}

export function FieldTooltip({ text }: FieldTooltipProps) {
  return (
    <TooltipProvider delayDuration={200}>
      <Tooltip>
        <TooltipTrigger asChild>
          <button
            type="button"
            className="text-muted-foreground hover:text-foreground ml-1 inline-flex items-center"
            aria-label="Más información"
          >
            <HelpCircle className="h-3.5 w-3.5" />
          </button>
        </TooltipTrigger>
        <TooltipContent side="top" className="max-w-[250px] text-sm">
          <p>{text}</p>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}
