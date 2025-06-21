/**
 * Animation System
 * Comprehensive Framer Motion configuration for Gideon
 */

import { Variants, Transition } from 'framer-motion';

// Standard easing curves
export const easings = {
  // Default easing for most animations
  default: [0.4, 0, 0.2, 1],
  
  // Sharp entrance/exit
  sharp: [0.4, 0, 1, 1],
  
  // Smooth organic movement
  smooth: [0.25, 0.46, 0.45, 0.94],
  
  // Bouncy entrance
  bounce: [0.68, -0.55, 0.265, 1.55],
  
  // Spring-like
  spring: [0.175, 0.885, 0.32, 1.275],
} as const;

// Standard durations
export const durations = {
  fast: 0.15,
  normal: 0.3,
  slow: 0.5,
  verySlow: 1.0,
} as const;

// Common transitions
export const transitions: Record<string, Transition> = {
  default: {
    duration: durations.normal,
    ease: easings.default,
  },
  
  fast: {
    duration: durations.fast,
    ease: easings.default,
  },
  
  slow: {
    duration: durations.slow,
    ease: easings.smooth,
  },
  
  spring: {
    type: 'spring',
    stiffness: 400,
    damping: 30,
  },
  
  bouncy: {
    type: 'spring',
    stiffness: 600,
    damping: 15,
  },
  
  smooth: {
    duration: durations.normal,
    ease: easings.smooth,
  },
} as const;

// Page transition variants
export const pageVariants: Variants = {
  initial: {
    opacity: 0,
    y: 20,
  },
  enter: {
    opacity: 1,
    y: 0,
    transition: transitions.default,
  },
  exit: {
    opacity: 0,
    y: -20,
    transition: transitions.fast,
  },
};

// Modal/overlay variants
export const modalVariants: Variants = {
  hidden: {
    opacity: 0,
    scale: 0.8,
    y: 20,
  },
  visible: {
    opacity: 1,
    scale: 1,
    y: 0,
    transition: transitions.spring,
  },
  exit: {
    opacity: 0,
    scale: 0.8,
    y: 20,
    transition: transitions.fast,
  },
};

// Backdrop variants
export const backdropVariants: Variants = {
  hidden: {
    opacity: 0,
  },
  visible: {
    opacity: 1,
    transition: transitions.fast,
  },
  exit: {
    opacity: 0,
    transition: transitions.fast,
  },
};

// Sidebar/drawer variants
export const sidebarVariants: Variants = {
  closed: {
    x: '-100%',
    transition: transitions.default,
  },
  open: {
    x: 0,
    transition: transitions.default,
  },
};

// Dropdown variants
export const dropdownVariants: Variants = {
  hidden: {
    opacity: 0,
    scale: 0.95,
    y: -10,
  },
  visible: {
    opacity: 1,
    scale: 1,
    y: 0,
    transition: transitions.fast,
  },
  exit: {
    opacity: 0,
    scale: 0.95,
    y: -10,
    transition: transitions.fast,
  },
};

// Button hover/tap variants
export const buttonVariants: Variants = {
  hover: {
    scale: 1.05,
    transition: transitions.fast,
  },
  tap: {
    scale: 0.95,
    transition: transitions.fast,
  },
};

// Card hover variants
export const cardVariants: Variants = {
  rest: {
    scale: 1,
    y: 0,
    boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.4)',
  },
  hover: {
    scale: 1.02,
    y: -2,
    boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.5)',
    transition: transitions.fast,
  },
};

// List item variants (for stagger animations)
export const listItemVariants: Variants = {
  hidden: {
    opacity: 0,
    y: 20,
  },
  visible: {
    opacity: 1,
    y: 0,
  },
};

// Container variants for stagger animations
export const listContainerVariants: Variants = {
  hidden: {
    opacity: 0,
  },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
      delayChildren: 0.2,
    },
  },
};

// Loading spinner variants
export const spinnerVariants: Variants = {
  animate: {
    rotate: 360,
    transition: {
      duration: 1,
      repeat: Infinity,
      ease: 'linear',
    },
  },
};

// Pulse animation for loading states
export const pulseVariants: Variants = {
  animate: {
    opacity: [1, 0.5, 1],
    transition: {
      duration: 2,
      repeat: Infinity,
      ease: 'easeInOut',
    },
  },
};

// Notification variants
export const notificationVariants: Variants = {
  hidden: {
    opacity: 0,
    x: 300,
    scale: 0.8,
  },
  visible: {
    opacity: 1,
    x: 0,
    scale: 1,
    transition: transitions.spring,
  },
  exit: {
    opacity: 0,
    x: 300,
    scale: 0.8,
    transition: transitions.fast,
  },
};

// Tab switching variants
export const tabVariants: Variants = {
  hidden: {
    opacity: 0,
    x: 20,
  },
  visible: {
    opacity: 1,
    x: 0,
    transition: transitions.default,
  },
  exit: {
    opacity: 0,
    x: -20,
    transition: transitions.fast,
  },
};

// Progress bar variants
export const progressVariants: Variants = {
  initial: {
    scaleX: 0,
    transformOrigin: "left",
  },
  animate: {
    scaleX: 1,
    transition: {
      duration: durations.slow,
      ease: easings.smooth,
    },
  },
};

// Glow effect variants (for EVE Online aesthetic)
export const glowVariants: Variants = {
  rest: {
    boxShadow: '0 0 0px rgba(0, 212, 255, 0)',
  },
  hover: {
    boxShadow: '0 0 20px rgba(0, 212, 255, 0.3)',
    transition: transitions.fast,
  },
  active: {
    boxShadow: '0 0 30px rgba(0, 212, 255, 0.5)',
    transition: transitions.fast,
  },
};

// Ship fitting drag variants
export const dragVariants: Variants = {
  drag: {
    scale: 1.1,
    rotate: 5,
    zIndex: 1000,
    transition: transitions.fast,
  },
  dragEnd: {
    scale: 1,
    rotate: 0,
    zIndex: 'auto',
    transition: transitions.spring,
  },
};

// Export commonly used animation configurations
export const animations = {
  // Page transitions
  pageTransition: {
    variants: pageVariants,
    initial: 'initial',
    animate: 'enter',
    exit: 'exit',
  },
  
  // Modal animations
  modal: {
    variants: modalVariants,
    initial: 'hidden',
    animate: 'visible',
    exit: 'exit',
  },
  
  // Backdrop animations
  backdrop: {
    variants: backdropVariants,
    initial: 'hidden',
    animate: 'visible',
    exit: 'exit',
  },
  
  // Button interactions
  button: {
    variants: buttonVariants,
    whileHover: 'hover',
    whileTap: 'tap',
  },
  
  // Card interactions
  card: {
    variants: cardVariants,
    initial: 'rest',
    whileHover: 'hover',
  },
  
  // List animations
  list: {
    container: {
      variants: listContainerVariants,
      initial: 'hidden',
      animate: 'visible',
    },
    item: {
      variants: listItemVariants,
    },
  },
  
  // Loading states
  spinner: {
    variants: spinnerVariants,
    animate: 'animate',
  },
  
  pulse: {
    variants: pulseVariants,
    animate: 'animate',
  },
  
  // Notifications
  notification: {
    variants: notificationVariants,
    initial: 'hidden',
    animate: 'visible',
    exit: 'exit',
  },
  
  // EVE Online specific
  glow: {
    variants: glowVariants,
    initial: 'rest',
    whileHover: 'hover',
    whileTap: 'active',
  },
  
  drag: {
    variants: dragVariants,
    whileDrag: 'drag',
    onDragEnd: 'dragEnd',
  },
} as const;

// Animation presets for common use cases
export const presets = {
  // Entrance animations
  fadeIn: {
    initial: { opacity: 0 },
    animate: { opacity: 1 },
    transition: transitions.default,
  },
  
  slideInUp: {
    initial: { opacity: 0, y: 50 },
    animate: { opacity: 1, y: 0 },
    transition: transitions.default,
  },
  
  slideInDown: {
    initial: { opacity: 0, y: -50 },
    animate: { opacity: 1, y: 0 },
    transition: transitions.default,
  },
  
  slideInLeft: {
    initial: { opacity: 0, x: -50 },
    animate: { opacity: 1, x: 0 },
    transition: transitions.default,
  },
  
  slideInRight: {
    initial: { opacity: 0, x: 50 },
    animate: { opacity: 1, x: 0 },
    transition: transitions.default,
  },
  
  scaleIn: {
    initial: { opacity: 0, scale: 0.8 },
    animate: { opacity: 1, scale: 1 },
    transition: transitions.spring,
  },
  
  // Interaction animations
  gentleHover: {
    whileHover: { scale: 1.02, transition: transitions.fast },
  },
  
  strongHover: {
    whileHover: { scale: 1.05, transition: transitions.fast },
  },
  
  tap: {
    whileTap: { scale: 0.95, transition: transitions.fast },
  },
  
  // Loading animations
  rotate: {
    animate: { 
      rotate: 360,
      transition: { duration: 1, repeat: Infinity, ease: 'linear' }
    },
  },
  
  bounce: {
    animate: {
      y: [0, -10, 0] as any,
      transition: { duration: 0.6, repeat: Infinity, ease: 'easeInOut' }
    },
  },
};

export type AnimationPreset = keyof typeof presets;
export type AnimationConfig = keyof typeof animations;