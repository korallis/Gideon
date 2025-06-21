/**
 * Loading Components
 * Advanced loading states and micro-interactions for EVE Online theme
 */

import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '../../utils';

// Spinner variants with EVE Online styling
const spinnerVariants = cva('rounded-full border-2', {
  variants: {
    size: {
      xs: 'w-3 h-3 border-[1px]',
      sm: 'w-4 h-4',
      md: 'w-8 h-8',
      lg: 'w-12 h-12 border-3',
      xl: 'w-16 h-16 border-4',
    },
    variant: {
      primary: 'border-border-primary border-t-accent-blue',
      secondary: 'border-border-primary border-t-text-secondary',
      success: 'border-border-primary border-t-status-success',
      warning: 'border-border-primary border-t-status-warning',
      error: 'border-border-primary border-t-status-error',
      glow: 'border-border-primary border-t-accent-blue shadow-glow',
    },
  },
  defaultVariants: {
    size: 'md',
    variant: 'primary',
  },
});

interface LoadingSpinnerProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof spinnerVariants> {
  speed?: 'slow' | 'normal' | 'fast';
}

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size,
  variant,
  speed = 'normal',
  className,
  ...props
}) => {
  const duration = {
    slow: 2,
    normal: 1,
    fast: 0.5,
  }[speed];

  return (
    <motion.div
      animate={{ rotate: 360 }}
      transition={{ duration, repeat: Infinity, ease: 'linear' }}
      className={cn(spinnerVariants({ size, variant }), className)}
      {...props}
    />
  );
};

// Pulsing dots loader
interface LoadingDotsProps {
  size?: 'sm' | 'md' | 'lg';
  color?: string;
  className?: string;
}

export const LoadingDots: React.FC<LoadingDotsProps> = ({
  size = 'md',
  color = 'bg-accent-blue',
  className,
}) => {
  const sizeClasses = {
    sm: 'w-1 h-1',
    md: 'w-2 h-2',
    lg: 'w-3 h-3',
  };

  const containerClasses = {
    sm: 'gap-1',
    md: 'gap-2',
    lg: 'gap-3',
  };

  return (
    <div className={cn('flex items-center', containerClasses[size], className)}>
      {[0, 1, 2].map((index) => (
        <motion.div
          key={index}
          className={cn('rounded-full', sizeClasses[size], color)}
          animate={{
            scale: [1, 1.2, 1],
            opacity: [0.7, 1, 0.7],
          }}
          transition={{
            duration: 1.5,
            repeat: Infinity,
            delay: index * 0.2,
            ease: 'easeInOut',
          }}
        />
      ))}
    </div>
  );
};

// Skeleton loader for content
interface SkeletonProps {
  width?: string | number;
  height?: string | number;
  className?: string;
  rounded?: boolean;
}

export const Skeleton: React.FC<SkeletonProps> = ({
  width = '100%',
  height = '1rem',
  className,
  rounded = false,
}) => {
  return (
    <motion.div
      className={cn(
        'bg-primary-elevated',
        rounded ? 'rounded-full' : 'rounded-md',
        className
      )}
      style={{ width, height }}
      animate={{
        opacity: [0.6, 1, 0.6],
      }}
      transition={{
        duration: 1.5,
        repeat: Infinity,
        ease: 'easeInOut',
      }}
    />
  );
};

// Progress bar with EVE styling
interface ProgressBarProps {
  progress: number;
  size?: 'sm' | 'md' | 'lg';
  variant?: 'primary' | 'success' | 'warning' | 'error';
  showText?: boolean;
  className?: string;
}

export const ProgressBar: React.FC<ProgressBarProps> = ({
  progress,
  size = 'md',
  variant = 'primary',
  showText = false,
  className,
}) => {
  const heightClasses = {
    sm: 'h-1',
    md: 'h-2',
    lg: 'h-3',
  };

  const colorClasses = {
    primary: 'bg-accent-blue',
    success: 'bg-status-success',
    warning: 'bg-status-warning',
    error: 'bg-status-error',
  };

  const clampedProgress = Math.max(0, Math.min(100, progress));

  return (
    <div className={cn('relative w-full', className)}>
      <div
        className={cn(
          'w-full bg-primary-elevated rounded-full overflow-hidden',
          heightClasses[size]
        )}
      >
        <motion.div
          className={cn('h-full rounded-full', colorClasses[variant])}
          initial={{ width: 0 }}
          animate={{ width: `${clampedProgress}%` }}
          transition={{ duration: 0.5, ease: 'easeOut' }}
        />
      </div>
      {showText && (
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="text-xs font-medium text-text-primary">
            {Math.round(clampedProgress)}%
          </span>
        </div>
      )}
    </div>
  );
};

// Loading overlay for full-screen loading
interface LoadingOverlayProps {
  isVisible: boolean;
  message?: string;
  variant?: 'spinner' | 'dots';
  children?: React.ReactNode;
}

export const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  isVisible,
  message = 'Loading...',
  variant = 'spinner',
  children,
}) => {
  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/80 backdrop-blur-sm"
        >
          <motion.div
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.8, opacity: 0 }}
            className="flex flex-col items-center gap-4 p-8 bg-primary-surface rounded-lg border border-border-accent shadow-xl"
          >
            {variant === 'spinner' ? (
              <LoadingSpinner size="lg" variant="glow" />
            ) : (
              <LoadingDots size="lg" />
            )}
            {message && (
              <p className="text-text-primary text-sm font-medium">{message}</p>
            )}
            {children}
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};