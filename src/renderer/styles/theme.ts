import { THEME_COLORS, ANIMATIONS } from '@/constants';

export const theme = {
  colors: {
    primary: {
      bg: THEME_COLORS.PRIMARY_BG,
      surface: THEME_COLORS.SECONDARY_BG,
    },
    accent: {
      blue: THEME_COLORS.ACCENT_BLUE,
      orange: THEME_COLORS.ACCENT_ORANGE,
      green: THEME_COLORS.SUCCESS_GREEN,
      yellow: THEME_COLORS.WARNING_YELLOW,
      red: THEME_COLORS.ERROR_RED,
    },
    text: {
      primary: THEME_COLORS.TEXT_PRIMARY,
      secondary: THEME_COLORS.TEXT_SECONDARY,
    },
    border: {
      primary: THEME_COLORS.BORDER_PRIMARY,
      accent: THEME_COLORS.BORDER_ACCENT,
    },
  },
  animations: {
    fast: ANIMATIONS.FAST,
    normal: ANIMATIONS.NORMAL,
    slow: ANIMATIONS.SLOW,
    verySlow: ANIMATIONS.VERY_SLOW,
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

export type Theme = typeof theme;