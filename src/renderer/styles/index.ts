/**
 * Styles Module Barrel Export
 * Centralized access to all styling utilities and configurations
 */

// Theme configuration
export * from './theme';
export { default as themeConfig } from './theme.config';

// CSS utility classes are automatically imported via global.css
// This file provides TypeScript access to theme tokens

// Utility functions for working with CSS-in-JS
export const createUtilityClasses = (styles: Record<string, string>) => {
  return Object.entries(styles).reduce((acc, [key, value]) => {
    acc[key] = value;
    return acc;
  }, {} as Record<string, string>);
};

// Helper to generate responsive styles
export const responsive = {
  sm: (styles: string) => `@media (min-width: 640px) { ${styles} }`,
  md: (styles: string) => `@media (min-width: 768px) { ${styles} }`,
  lg: (styles: string) => `@media (min-width: 1024px) { ${styles} }`,
  xl: (styles: string) => `@media (min-width: 1280px) { ${styles} }`,
  '2xl': (styles: string) => `@media (min-width: 1536px) { ${styles} }`,
};

// Helper to generate hover/focus states
export const states = {
  hover: (styles: string) => `&:hover { ${styles} }`,
  focus: (styles: string) => `&:focus { ${styles} }`,
  active: (styles: string) => `&:active { ${styles} }`,
  disabled: (styles: string) => `&:disabled { ${styles} }`,
};

// CSS-in-JS theme object for emotion/styled-components
export const cssTheme = {
  // Convert CSS custom properties to theme object
  colors: {
    primaryBg: 'var(--color-primary-bg)',
    secondaryBg: 'var(--color-secondary-bg)',
    accentBlue: 'var(--color-accent-blue)',
    accentOrange: 'var(--color-accent-orange)',
    successGreen: 'var(--color-success-green)',
    warningYellow: 'var(--color-warning-yellow)',
    errorRed: 'var(--color-error-red)',
    textPrimary: 'var(--color-text-primary)',
    textSecondary: 'var(--color-text-secondary)',
    borderPrimary: 'var(--color-border-primary)',
    borderAccent: 'var(--color-border-accent)',
  },
  spacing: {
    xs: 'var(--spacing-xs)',
    sm: 'var(--spacing-sm)',
    md: 'var(--spacing-md)',
    lg: 'var(--spacing-lg)',
    xl: 'var(--spacing-xl)',
    xxl: 'var(--spacing-xxl)',
  },
  radius: {
    sm: 'var(--radius-sm)',
    md: 'var(--radius-md)',
    lg: 'var(--radius-lg)',
    xl: 'var(--radius-xl)',
    full: '9999px',
  },
  shadows: {
    sm: 'var(--shadow-sm)',
    md: 'var(--shadow-md)',
    lg: 'var(--shadow-lg)',
    xl: 'var(--shadow-xl)',
    glow: 'var(--shadow-glow)',
    glowOrange: 'var(--shadow-glow-orange)',
  },
  duration: {
    fast: 'var(--duration-fast)',
    normal: 'var(--duration-normal)',
    slow: 'var(--duration-slow)',
    verySlow: 'var(--duration-very-slow)',
  },
};

// Commonly used CSS snippets
export const commonStyles = {
  flexCenter: 'display: flex; align-items: center; justify-content: center;',
  flexBetween: 'display: flex; align-items: center; justify-content: space-between;',
  absoluteCenter: 'position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);',
  visuallyHidden: 'position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;',
  truncate: 'overflow: hidden; text-overflow: ellipsis; white-space: nowrap;',
  
  // Glass effect
  glass: `
    background: rgba(26, 31, 46, 0.8);
    backdrop-filter: blur(10px);
    -webkit-backdrop-filter: blur(10px);
    border: 1px solid var(--color-border-accent);
  `,
  
  // Glow effects
  glowBlue: `
    border: 1px solid var(--color-accent-blue);
    box-shadow: var(--shadow-glow);
  `,
  
  glowOrange: `
    border: 1px solid var(--color-accent-orange);
    box-shadow: var(--shadow-glow-orange);
  `,
  
  // Smooth transitions
  transition: `
    transition: all var(--duration-fast) ease;
  `,
  
  // Custom scrollbar
  customScrollbar: `
    scrollbar-width: thin;
    scrollbar-color: var(--color-border-accent) transparent;
    
    &::-webkit-scrollbar {
      width: 6px;
      height: 6px;
    }
    
    &::-webkit-scrollbar-track {
      background: transparent;
    }
    
    &::-webkit-scrollbar-thumb {
      background: var(--color-border-accent);
      border-radius: 3px;
      transition: background var(--duration-fast) ease;
    }
    
    &::-webkit-scrollbar-thumb:hover {
      background: var(--color-accent-blue);
    }
  `,
};

// Export default theme for easy access
export { theme as default } from './theme.config';