/**
 * Enhanced Theme Configuration for Gideon EVE Online Application
 * Re-export from comprehensive theme configuration
 */

export {
  theme,
  colors,
  spacing,
  radius,
  typography,
  duration,
  shadows,
  zIndex,
  breakpoints,
  components,
  themeVariants,
  generateCSSVariables,
  getColor,
  getSpacing,
  getRadius,
  getShadow,
} from './theme.config';

export type {
  Theme,
  ThemeColors,
  ThemeSpacing,
} from './theme.config';

// Legacy exports for backward compatibility
export const theme_legacy = {
  colors: {
    primary: {
      bg: '#0a0e1a',
      surface: '#1a1f2e',
    },
    accent: {
      blue: '#00d4ff',
      orange: '#ff8c00',
      green: '#00ff88',
      yellow: '#ffcc00',
      red: '#ff4444',
    },
    text: {
      primary: '#ffffff',
      secondary: '#a0a8b8',
    },
    border: {
      primary: '#2a3441',
      accent: '#3a4552',
    },
  },
  animations: {
    fast: '150ms',
    normal: '300ms',
    slow: '500ms',
    verySlow: '1000ms',
  },
  fonts: {
    primary: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    mono: "'JetBrains Mono', 'Consolas', 'Monaco', monospace",
  },
  spacing: {
    xs: '4px',
    sm: '8px',
    md: '16px',
    lg: '24px',
    xl: '32px',
    xxl: '48px',
  },
  borderRadius: {
    sm: '4px',
    md: '8px',
    lg: '12px',
    xl: '16px',
    full: '9999px',
  },
  shadows: {
    sm: '0 1px 2px 0 rgba(0, 0, 0, 0.3)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.4)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.5)',
    xl: '0 20px 25px -5px rgba(0, 0, 0, 0.6)',
    glow: '0 0 20px rgba(0, 212, 255, 0.3)',
    glowOrange: '0 0 20px rgba(255, 140, 0, 0.3)',
  },
  breakpoints: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    xxl: '1536px',
  },
  zIndex: {
    dropdown: 1000,
    modal: 2000,
    tooltip: 3000,
    notification: 4000,
    loading: 5000,
  },
};