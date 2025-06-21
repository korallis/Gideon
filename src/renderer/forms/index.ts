/**
 * Form System
 * React Hook Form configuration with validation schemas
 */

import { useForm, UseFormProps, FieldValues, Path, RegisterOptions } from 'react-hook-form';

// Common validation rules
export const validationRules = {
  required: 'This field is required',
  email: {
    pattern: {
      value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
      message: 'Invalid email address',
    },
  },
  minLength: (length: number) => ({
    minLength: {
      value: length,
      message: `Must be at least ${length} characters`,
    },
  }),
  maxLength: (length: number) => ({
    maxLength: {
      value: length,
      message: `Must be no more than ${length} characters`,
    },
  }),
  pattern: (regex: RegExp, message: string) => ({
    pattern: {
      value: regex,
      message,
    },
  }),
  min: (value: number) => ({
    min: {
      value,
      message: `Must be at least ${value}`,
    },
  }),
  max: (value: number) => ({
    max: {
      value,
      message: `Must be no more than ${value}`,
    },
  }),
} as const;

// EVE Online specific validation patterns
export const eveValidation = {
  characterName: {
    pattern: {
      value: /^[a-zA-Z0-9\s'-]{1,37}$/,
      message: 'Invalid character name format',
    },
    minLength: {
      value: 3,
      message: 'Character name must be at least 3 characters',
    },
    maxLength: {
      value: 37,
      message: 'Character name must be no more than 37 characters',
    },
  },
  
  corporationName: {
    pattern: {
      value: /^[a-zA-Z0-9\s\.\-\[\]]{1,50}$/,
      message: 'Invalid corporation name format',
    },
    maxLength: {
      value: 50,
      message: 'Corporation name must be no more than 50 characters',
    },
  },
  
  fittingName: {
    minLength: {
      value: 1,
      message: 'Fitting name is required',
    },
    maxLength: {
      value: 100,
      message: 'Fitting name must be no more than 100 characters',
    },
  },
  
  iskAmount: {
    pattern: {
      value: /^\d+(\.\d{1,2})?$/,
      message: 'Invalid ISK amount format',
    },
    min: {
      value: 0,
      message: 'Amount cannot be negative',
    },
  },
} as const;

// Form field configuration interface
export interface FormFieldConfig<T extends FieldValues = FieldValues> {
  name: Path<T>;
  label: string;
  type?: 'text' | 'email' | 'password' | 'number' | 'select' | 'textarea' | 'checkbox';
  placeholder?: string;
  validation?: RegisterOptions<T>;
  options?: Array<{ value: string | number; label: string }>;
  description?: string;
  disabled?: boolean;
}

// Enhanced useForm hook with common defaults
export const useAppForm = <T extends FieldValues = FieldValues>(
  options?: UseFormProps<T>
) => {
  const form = useForm<T>({
    mode: 'onChange',
    reValidateMode: 'onChange',
    ...options,
  });

  // Helper to get field error message
  const getFieldError = (fieldName: Path<T>) => {
    const error = form.formState.errors[fieldName];
    return error?.message as string | undefined;
  };

  // Helper to check if field has error
  const hasFieldError = (fieldName: Path<T>) => {
    return !!form.formState.errors[fieldName];
  };

  // Helper to get field state
  const getFieldState = (fieldName: Path<T>) => {
    const isDirty = form.formState.dirtyFields[fieldName];
    const isInvalid = hasFieldError(fieldName);
    const isValid = isDirty && !isInvalid;

    return {
      isDirty,
      isInvalid,
      isValid,
      error: getFieldError(fieldName),
    };
  };

  return {
    ...form,
    getFieldError,
    hasFieldError,
    getFieldState,
  };
};

// Common form schemas
export const formSchemas = {
  // Character authentication
  characterAuth: {
    clientId: {
      name: 'clientId' as const,
      label: 'Client ID',
      type: 'text' as const,
      placeholder: 'EVE Application Client ID',
      validation: {
        required: validationRules.required,
        ...validationRules.minLength(32),
      },
    },
    secretKey: {
      name: 'secretKey' as const,
      label: 'Secret Key',
      type: 'password' as const,
      placeholder: 'EVE Application Secret Key',
      validation: {
        required: validationRules.required,
        ...validationRules.minLength(32),
      },
    },
    scopes: {
      name: 'scopes' as const,
      label: 'Requested Scopes',
      type: 'select' as const,
      validation: {
        required: validationRules.required,
      },
      options: [
        { value: 'esi-characters.read_character_data.v1', label: 'Character Data' },
        { value: 'esi-skills.read_skills.v1', label: 'Skills' },
        { value: 'esi-assets.read_assets.v1', label: 'Assets' },
        { value: 'esi-markets.read_character_orders.v1', label: 'Market Orders' },
      ],
    },
  },

  // Ship fitting
  shipFitting: {
    name: {
      name: 'name' as const,
      label: 'Fitting Name',
      type: 'text' as const,
      placeholder: 'Enter fitting name',
      validation: {
        required: validationRules.required,
        ...eveValidation.fittingName,
      },
    },
    shipTypeId: {
      name: 'shipTypeId' as const,
      label: 'Ship Type',
      type: 'select' as const,
      validation: {
        required: validationRules.required,
      },
    },
    description: {
      name: 'description' as const,
      label: 'Description',
      type: 'textarea' as const,
      placeholder: 'Optional fitting description',
      validation: {
        ...validationRules.maxLength(500),
      },
    },
    tags: {
      name: 'tags' as const,
      label: 'Tags',
      type: 'text' as const,
      placeholder: 'PvP, Mining, Ratting (comma separated)',
    },
  },

  // Market price alerts
  priceAlert: {
    typeId: {
      name: 'typeId' as const,
      label: 'Item',
      type: 'select' as const,
      validation: {
        required: validationRules.required,
      },
    },
    condition: {
      name: 'condition' as const,
      label: 'Condition',
      type: 'select' as const,
      validation: {
        required: validationRules.required,
      },
      options: [
        { value: 'above', label: 'Price goes above' },
        { value: 'below', label: 'Price goes below' },
        { value: 'change', label: 'Price changes by %' },
      ],
    },
    threshold: {
      name: 'threshold' as const,
      label: 'Threshold',
      type: 'number' as const,
      placeholder: '0.00',
      validation: {
        required: validationRules.required,
        ...eveValidation.iskAmount,
      },
    },
    regionId: {
      name: 'regionId' as const,
      label: 'Region',
      type: 'select' as const,
      validation: {
        required: validationRules.required,
      },
    },
  },

  // Application settings
  settings: {
    theme: {
      name: 'theme' as const,
      label: 'Theme',
      type: 'select' as const,
      options: [
        { value: 'dark', label: 'Dark' },
        { value: 'light', label: 'Light' },
        { value: 'auto', label: 'Auto' },
      ],
    },
    autoRefresh: {
      name: 'autoRefresh' as const,
      label: 'Auto Refresh Data',
      type: 'checkbox' as const,
    },
    refreshInterval: {
      name: 'refreshInterval' as const,
      label: 'Refresh Interval (minutes)',
      type: 'number' as const,
      validation: {
        ...validationRules.min(1),
        ...validationRules.max(60),
      },
    },
    notifications: {
      name: 'notifications' as const,
      label: 'Enable Notifications',
      type: 'checkbox' as const,
    },
  },

  // Skill planning
  skillPlan: {
    name: {
      name: 'name' as const,
      label: 'Plan Name',
      type: 'text' as const,
      placeholder: 'My Skill Plan',
      validation: {
        required: validationRules.required,
        ...validationRules.maxLength(100),
      },
    },
    description: {
      name: 'description' as const,
      label: 'Description',
      type: 'textarea' as const,
      placeholder: 'Optional plan description',
      validation: {
        ...validationRules.maxLength(500),
      },
    },
    priority: {
      name: 'priority' as const,
      label: 'Priority',
      type: 'select' as const,
      options: [
        { value: 'low', label: 'Low' },
        { value: 'medium', label: 'Medium' },
        { value: 'high', label: 'High' },
      ],
    },
  },
} as const;

// Form validation helpers
export const validateForm = {
  // EVE Online character name validation
  characterName: (value: string) => {
    if (!value) return 'Character name is required';
    if (value.length < 3) return 'Character name must be at least 3 characters';
    if (value.length > 37) return 'Character name must be no more than 37 characters';
    if (!/^[a-zA-Z0-9\s'-]+$/.test(value)) return 'Character name contains invalid characters';
    return true;
  },

  // ISK amount validation
  iskAmount: (value: string | number) => {
    const numValue = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(numValue)) return 'Invalid number format';
    if (numValue < 0) return 'Amount cannot be negative';
    return true;
  },

  // Skill level validation (1-5)
  skillLevel: (value: number) => {
    if (value < 1 || value > 5) return 'Skill level must be between 1 and 5';
    return true;
  },

  // Type ID validation
  typeId: (value: number) => {
    if (!value || value <= 0) return 'Please select a valid item';
    return true;
  },
};

export type FormSchema = keyof typeof formSchemas;
export type FormField<T extends FormSchema> = typeof formSchemas[T][keyof typeof formSchemas[T]];

export default {
  useAppForm,
  validationRules,
  eveValidation,
  formSchemas,
  validateForm,
};