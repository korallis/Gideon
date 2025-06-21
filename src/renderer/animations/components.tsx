/**
 * Animated Components
 * Pre-built animated components using Framer Motion
 */

import React from 'react';
import { motion, HTMLMotionProps } from 'framer-motion';
import { animations, presets, AnimationPreset } from './index';

// Animated page wrapper
interface AnimatedPageProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
}

export const AnimatedPage: React.FC<AnimatedPageProps> = ({ 
  children, 
  className = '',
  ...props 
}) => (
  <motion.div
    className={`w-full h-full ${className}`}
    {...animations.pageTransition}
    {...props}
  >
    {children}
  </motion.div>
);

// Animated modal
interface AnimatedModalProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  isOpen: boolean;
  onClose?: () => void;
}

export const AnimatedModal: React.FC<AnimatedModalProps> = ({
  children,
  isOpen,
  onClose,
  className = '',
  ...props
}) => {
  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <motion.div
        className="fixed inset-0 bg-black bg-opacity-50 z-50"
        {...animations.backdrop}
        onClick={onClose}
      />
      
      {/* Modal */}
      <motion.div
        className={`fixed inset-0 flex items-center justify-center z-50 p-4 ${className}`}
        {...animations.modal}
        {...props}
      >
        {children}
      </motion.div>
    </>
  );
};

// Animated button
interface AnimatedButtonProps extends HTMLMotionProps<'button'> {
  children: React.ReactNode;
  variant?: 'primary' | 'secondary' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  glow?: boolean;
}

export const AnimatedButton: React.FC<AnimatedButtonProps> = ({
  children,
  variant = 'primary',
  size = 'md',
  glow = false,
  className = '',
  ...props
}) => {
  const baseClasses = 'btn';
  const variantClasses = `btn-${variant}`;
  const sizeClasses = size !== 'md' ? `btn-${size}` : '';
  
  return (
    <motion.button
      className={`${baseClasses} ${variantClasses} ${sizeClasses} ${className}`}
      {...(glow ? animations.glow : animations.button)}
      {...props}
    >
      {children}
    </motion.button>
  );
};

// Animated card
interface AnimatedCardProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  hover?: boolean;
  glass?: boolean;
}

export const AnimatedCard: React.FC<AnimatedCardProps> = ({
  children,
  hover = true,
  glass = false,
  className = '',
  ...props
}) => {
  const cardClasses = glass ? 'card card-glass' : 'card';
  
  return (
    <motion.div
      className={`${cardClasses} ${className}`}
      {...(hover ? animations.card : {})}
      {...props}
    >
      {children}
    </motion.div>
  );
};

// Animated list container
interface AnimatedListProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  stagger?: boolean;
}

export const AnimatedList: React.FC<AnimatedListProps> = ({
  children,
  stagger = true,
  className = '',
  ...props
}) => (
  <motion.div
    className={className}
    {...(stagger ? animations.list.container : {})}
    {...props}
  >
    {children}
  </motion.div>
);

// Animated list item
interface AnimatedListItemProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
}

export const AnimatedListItem: React.FC<AnimatedListItemProps> = ({
  children,
  className = '',
  ...props
}) => (
  <motion.div
    className={className}
    {...animations.list.item}
    {...props}
  >
    {children}
  </motion.div>
);

// Animated notification
interface AnimatedNotificationProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  type?: 'info' | 'success' | 'warning' | 'error';
}

export const AnimatedNotification: React.FC<AnimatedNotificationProps> = ({
  children,
  type = 'info',
  className = '',
  ...props
}) => (
  <motion.div
    className={`alert alert-${type} ${className}`}
    {...animations.notification}
    {...props}
  >
    {children}
  </motion.div>
);

// Animated loading spinner
interface AnimatedSpinnerProps extends HTMLMotionProps<'div'> {
  size?: 'sm' | 'md' | 'lg';
  color?: string;
}

export const AnimatedSpinner: React.FC<AnimatedSpinnerProps> = ({
  size = 'md',
  color = 'border-accent-blue',
  className = '',
  ...props
}) => {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
  };

  return (
    <motion.div
      className={`
        border-2 border-gray-600 border-t-transparent rounded-full
        ${sizeClasses[size]} ${color} ${className}
      `}
      {...animations.spinner}
      {...props}
    />
  );
};

// Animated progress bar
interface AnimatedProgressProps extends HTMLMotionProps<'div'> {
  value: number;
  max?: number;
  color?: string;
}

export const AnimatedProgress: React.FC<AnimatedProgressProps> = ({
  value,
  max = 100,
  color = 'bg-accent-blue',
  className = '',
  ...props
}) => {
  const percentage = Math.min((value / max) * 100, 100);

  return (
    <div className={`progress ${className}`}>
      <motion.div
        className={`progress-bar ${color}`}
        initial={{ width: 0 }}
        animate={{ width: `${percentage}%` }}
        transition={{ duration: 0.5, ease: 'easeOut' }}
        {...props}
      />
    </div>
  );
};

// Animated tab content
interface AnimatedTabContentProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  activeKey: string;
  tabKey: string;
}

export const AnimatedTabContent: React.FC<AnimatedTabContentProps> = ({
  children,
  activeKey,
  tabKey,
  className = '',
  ...props
}) => {
  if (activeKey !== tabKey) return null;

  return (
    <motion.div
      className={`tab-content ${className}`}
      initial="hidden"
      animate="visible"
      exit="exit"
      variants={{
        hidden: { opacity: 0, x: 20 },
        visible: { opacity: 1, x: 0 },
        exit: { opacity: 0, x: -20 },
      }}
      transition={{ duration: 0.3 }}
      {...props}
    >
      {children}
    </motion.div>
  );
};

// Animated entrance wrapper
interface AnimatedEntranceProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  preset: AnimationPreset;
  delay?: number;
}

export const AnimatedEntrance: React.FC<AnimatedEntranceProps> = ({
  children,
  preset,
  delay = 0,
  className = '',
  ...props
}) => (
  <motion.div
    className={className}
    {...presets[preset]}
    transition={{
      ...presets[preset].transition,
      delay,
    }}
    {...props}
  >
    {children}
  </motion.div>
);

// Animated drag container (for ship fitting)
interface AnimatedDragProps extends HTMLMotionProps<'div'> {
  children: React.ReactNode;
  dragConstraints?: any;
  onDragEnd?: (event: any, info: any) => void;
}

export const AnimatedDrag: React.FC<AnimatedDragProps> = ({
  children,
  dragConstraints,
  onDragEnd,
  className = '',
  ...props
}) => (
  <motion.div
    className={`cursor-move ${className}`}
    drag
    dragConstraints={dragConstraints}
    dragElastic={0.1}
    onDragEnd={onDragEnd}
    {...animations.drag}
    {...props}
  >
    {children}
  </motion.div>
);

// Animated collapse/expand
interface AnimatedCollapseProps {
  children: React.ReactNode;
  isOpen: boolean;
  className?: string;
}

export const AnimatedCollapse: React.FC<AnimatedCollapseProps> = ({
  children,
  isOpen,
  className = '',
}) => (
  <motion.div
    className={className}
    initial={false}
    animate={{
      height: isOpen ? 'auto' : 0,
      opacity: isOpen ? 1 : 0,
    }}
    transition={{
      height: { duration: 0.3, ease: 'easeInOut' },
      opacity: { duration: 0.2, ease: 'easeInOut' },
    }}
    style={{ overflow: 'hidden' }}
  >
    {children}
  </motion.div>
);

// Animated number counter
interface AnimatedCounterProps {
  value: number;
  duration?: number;
  format?: (value: number) => string;
  className?: string;
}

export const AnimatedCounter: React.FC<AnimatedCounterProps> = ({
  value,
  duration = 1,
  format = (v) => Math.floor(v).toString(),
  className = '',
}) => {
  return (
    <motion.span
      className={className}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.3 }}
    >
      <motion.span
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration }}
        onUpdate={(latest) => {
          // This would need a ref to update the displayed value
          // Implementation depends on specific use case
        }}
      >
        {format(value)}
      </motion.span>
    </motion.span>
  );
};

// Export all animated components
export const Animated = {
  Page: AnimatedPage,
  Modal: AnimatedModal,
  Button: AnimatedButton,
  Card: AnimatedCard,
  List: AnimatedList,
  ListItem: AnimatedListItem,
  Notification: AnimatedNotification,
  Spinner: AnimatedSpinner,
  Progress: AnimatedProgress,
  TabContent: AnimatedTabContent,
  Entrance: AnimatedEntrance,
  Drag: AnimatedDrag,
  Collapse: AnimatedCollapse,
  Counter: AnimatedCounter,
};