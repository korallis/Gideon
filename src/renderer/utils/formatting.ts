/**
 * Formatting Utilities
 * String, number, and date formatting functions
 */

/**
 * Format ISK currency values with appropriate units
 */
export function formatISK(value: number, decimals = 2): string {
  if (value === 0) return '0 ISK';
  
  const units = [
    { threshold: 1e12, suffix: 'T' },
    { threshold: 1e9, suffix: 'B' },
    { threshold: 1e6, suffix: 'M' },
    { threshold: 1e3, suffix: 'K' }
  ];

  for (const unit of units) {
    if (Math.abs(value) >= unit.threshold) {
      const formatted = (value / unit.threshold).toFixed(decimals);
      return `${formatted}${unit.suffix} ISK`;
    }
  }

  return `${value.toLocaleString()} ISK`;
}

/**
 * Format large numbers with appropriate units
 */
export function formatNumber(value: number, decimals = 1): string {
  if (value === 0) return '0';
  
  const units = [
    { threshold: 1e12, suffix: 'T' },
    { threshold: 1e9, suffix: 'B' },
    { threshold: 1e6, suffix: 'M' },
    { threshold: 1e3, suffix: 'K' }
  ];

  for (const unit of units) {
    if (Math.abs(value) >= unit.threshold) {
      const formatted = (value / unit.threshold).toFixed(decimals);
      return `${formatted}${unit.suffix}`;
    }
  }

  return value.toLocaleString();
}

/**
 * Format skill points with appropriate units
 */
export function formatSkillPoints(sp: number): string {
  return formatNumber(sp, 1) + ' SP';
}

/**
 * Format percentage with specified decimal places
 */
export function formatPercentage(value: number, decimals = 1): string {
  return `${(value * 100).toFixed(decimals)}%`;
}

/**
 * Format duration from milliseconds to human readable
 */
export function formatDuration(ms: number): string {
  const seconds = Math.floor(ms / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);

  if (days > 0) {
    return `${days}d ${hours % 24}h ${minutes % 60}m`;
  } else if (hours > 0) {
    return `${hours}h ${minutes % 60}m`;
  } else if (minutes > 0) {
    return `${minutes}m ${seconds % 60}s`;
  } else {
    return `${seconds}s`;
  }
}

/**
 * Format training time for skills
 */
export function formatTrainingTime(seconds: number): string {
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);

  if (days > 0) {
    const remainingHours = hours % 24;
    const remainingMinutes = minutes % 60;
    return `${days}d ${remainingHours}h ${remainingMinutes}m`;
  } else if (hours > 0) {
    const remainingMinutes = minutes % 60;
    return `${hours}h ${remainingMinutes}m`;
  } else {
    return `${minutes}m`;
  }
}

/**
 * Format ship attribute values with units
 */
export function formatAttribute(value: number, unit: string, decimals = 2): string {
  switch (unit) {
    case 'ISK':
      return formatISK(value, decimals);
    case '%':
      return formatPercentage(value / 100, decimals);
    case 'km':
      return `${formatNumber(value, decimals)} km`;
    case 'm':
      return `${formatNumber(value, decimals)} m`;
    case 'm³':
      return `${formatNumber(value, decimals)} m³`;
    case 'MW':
      return `${formatNumber(value, decimals)} MW`;
    case 'GJ':
      return `${formatNumber(value, decimals)} GJ`;
    case 'tf':
      return `${formatNumber(value, decimals)} tf`;
    case 'HP':
      return `${formatNumber(value, decimals)} HP`;
    case 'DPS':
      return `${formatNumber(value, decimals)} DPS`;
    case 's':
      return `${value.toFixed(decimals)}s`;
    default:
      return `${formatNumber(value, decimals)} ${unit}`;
  }
}

/**
 * Format relative time (e.g., "2 hours ago")
 */
export function formatRelativeTime(date: Date): string {
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffSeconds = Math.floor(diffMs / 1000);
  const diffMinutes = Math.floor(diffSeconds / 60);
  const diffHours = Math.floor(diffMinutes / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffDays > 0) {
    return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
  } else if (diffHours > 0) {
    return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
  } else if (diffMinutes > 0) {
    return `${diffMinutes} minute${diffMinutes > 1 ? 's' : ''} ago`;
  } else if (diffSeconds > 5) {
    return `${diffSeconds} second${diffSeconds > 1 ? 's' : ''} ago`;
  } else {
    return 'just now';
  }
}

/**
 * Truncate text with ellipsis
 */
export function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength - 3) + '...';
}

/**
 * Capitalize first letter of each word
 */
export function titleCase(text: string): string {
  return text.replace(/\w\S*/g, (txt) => 
    txt.charAt(0).toUpperCase() + txt.substring(1).toLowerCase()
  );
}