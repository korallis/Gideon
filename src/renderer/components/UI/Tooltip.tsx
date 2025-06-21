/**
 * Tooltip Component
 * Radix UI Tooltip wrapper with EVE Online styling
 */

import React from 'react';
import * as TooltipPrimitive from '@radix-ui/react-tooltip';

// Tooltip Root
export const TooltipProvider = TooltipPrimitive.Provider;
export const Tooltip = TooltipPrimitive.Root;
export const TooltipTrigger = TooltipPrimitive.Trigger;

// Tooltip Content
export const TooltipContent = React.forwardRef<
  React.ElementRef<typeof TooltipPrimitive.Content>,
  React.ComponentProps<typeof TooltipPrimitive.Content>
>(({ className = '', sideOffset = 4, ...props }, ref) => (
  <TooltipPrimitive.Content
    ref={ref}
    sideOffset={sideOffset}
    className={`
      z-50 overflow-hidden rounded-md bg-black/90 px-3 py-1.5
      text-xs text-white animate-in fade-in-0 zoom-in-95
      data-[state=closed]:animate-out data-[state=closed]:fade-out-0
      data-[state=closed]:zoom-out-95 data-[side=bottom]:slide-in-from-top-2
      data-[side=left]:slide-in-from-right-2 data-[side=right]:slide-in-from-left-2
      data-[side=top]:slide-in-from-bottom-2
      ${className}
    `}
    {...props}
  />
));
TooltipContent.displayName = TooltipPrimitive.Content.displayName;

// Tooltip Arrow
export const TooltipArrow = React.forwardRef<
  React.ElementRef<typeof TooltipPrimitive.Arrow>,
  React.ComponentProps<typeof TooltipPrimitive.Arrow>
>(({ className = '', ...props }, ref) => (
  <TooltipPrimitive.Arrow
    ref={ref}
    className={`fill-black/90 ${className}`}
    {...props}
  />
));
TooltipArrow.displayName = TooltipPrimitive.Arrow.displayName;

// Convenience wrapper for common tooltip usage
interface SimpleTooltipProps {
  content: React.ReactNode;
  children: React.ReactNode;
  side?: 'top' | 'right' | 'bottom' | 'left';
  align?: 'start' | 'center' | 'end';
  delayDuration?: number;
}

export const SimpleTooltip: React.FC<SimpleTooltipProps> = ({
  content,
  children,
  side = 'top',
  align = 'center',
  delayDuration = 300,
}) => (
  <Tooltip delayDuration={delayDuration}>
    <TooltipTrigger asChild>
      {children}
    </TooltipTrigger>
    <TooltipContent side={side} align={align}>
      {content}
      <TooltipArrow />
    </TooltipContent>
  </Tooltip>
);