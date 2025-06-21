/**
 * Form Components
 * Reusable form components with React Hook Form integration
 */

import React from 'react';
import { useController, Control, FieldValues, Path } from 'react-hook-form';
import { motion } from 'framer-motion';

// Base form field props
interface BaseFieldProps<T extends FieldValues = FieldValues> {
  name: Path<T>;
  control: Control<T>;
  label?: string;
  description?: string;
  disabled?: boolean;
  className?: string;
}

// Text input component
interface TextFieldProps<T extends FieldValues = FieldValues> extends BaseFieldProps<T> {
  type?: 'text' | 'email' | 'password' | 'url';
  placeholder?: string;
  autoComplete?: string;
}

export const TextField = <T extends FieldValues = FieldValues>({
  name,
  control,
  label,
  description,
  type = 'text',
  placeholder,
  autoComplete,
  disabled = false,
  className = '',
}: TextFieldProps<T>) => {
  const {
    field,
    fieldState: { error, isDirty, invalid },
  } = useController({
    name,
    control,
  });

  const inputClasses = `
    input w-full
    ${error ? 'border-error' : ''}
    ${isDirty && !invalid ? 'border-success' : ''}
    ${className}
  `;

  return (
    <div className="form-field">
      {label && (
        <label htmlFor={name} className="block text-sm font-medium text-text-primary mb-2">
          {label}
        </label>
      )}
      
      <input
        {...field}
        id={name}
        type={type}
        placeholder={placeholder}
        autoComplete={autoComplete}
        disabled={disabled}
        className={inputClasses}
      />
      
      {description && !error && (
        <p className="mt-1 text-xs text-text-secondary">{description}</p>
      )}
      
      {error && (
        <motion.p
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-1 text-xs text-error"
        >
          {error.message}
        </motion.p>
      )}
    </div>
  );
};

// Number input component
interface NumberFieldProps<T extends FieldValues = FieldValues> extends BaseFieldProps<T> {
  min?: number;
  max?: number;
  step?: number;
  placeholder?: string;
}

export const NumberField = <T extends FieldValues = FieldValues>({
  name,
  control,
  label,
  description,
  min,
  max,
  step,
  placeholder,
  disabled = false,
  className = '',
}: NumberFieldProps<T>) => {
  const {
    field: { onChange, onBlur, value, ref },
    fieldState: { error, isDirty, invalid },
  } = useController({
    name,
    control,
  });

  const inputClasses = `
    input w-full
    ${error ? 'border-error' : ''}
    ${isDirty && !invalid ? 'border-success' : ''}
    ${className}
  `;

  return (
    <div className="form-field">
      {label && (
        <label htmlFor={name} className="block text-sm font-medium text-text-primary mb-2">
          {label}
        </label>
      )}
      
      <input
        ref={ref}
        id={name}
        type="number"
        min={min}
        max={max}
        step={step}
        placeholder={placeholder}
        disabled={disabled}
        value={value || ''}
        onChange={(e) => {
          const val = e.target.value;
          onChange(val === '' ? undefined : Number(val));
        }}
        onBlur={onBlur}
        className={inputClasses}
      />
      
      {description && !error && (
        <p className="mt-1 text-xs text-text-secondary">{description}</p>
      )}
      
      {error && (
        <motion.p
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-1 text-xs text-error"
        >
          {error.message}
        </motion.p>
      )}
    </div>
  );
};

// Textarea component
interface TextAreaFieldProps<T extends FieldValues = FieldValues> extends BaseFieldProps<T> {
  rows?: number;
  placeholder?: string;
  resize?: boolean;
}

export const TextAreaField = <T extends FieldValues = FieldValues>({
  name,
  control,
  label,
  description,
  rows = 4,
  placeholder,
  resize = true,
  disabled = false,
  className = '',
}: TextAreaFieldProps<T>) => {
  const {
    field,
    fieldState: { error, isDirty, invalid },
  } = useController({
    name,
    control,
  });

  const textareaClasses = `
    input w-full
    ${error ? 'border-error' : ''}
    ${isDirty && !invalid ? 'border-success' : ''}
    ${!resize ? 'resize-none' : ''}
    ${className}
  `;

  return (
    <div className="form-field">
      {label && (
        <label htmlFor={name} className="block text-sm font-medium text-text-primary mb-2">
          {label}
        </label>
      )}
      
      <textarea
        {...field}
        id={name}
        rows={rows}
        placeholder={placeholder}
        disabled={disabled}
        className={textareaClasses}
      />
      
      {description && !error && (
        <p className="mt-1 text-xs text-text-secondary">{description}</p>
      )}
      
      {error && (
        <motion.p
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-1 text-xs text-error"
        >
          {error.message}
        </motion.p>
      )}
    </div>
  );
};

// Select component
interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

interface SelectFieldProps<T extends FieldValues = FieldValues> extends BaseFieldProps<T> {
  options: SelectOption[];
  placeholder?: string;
}

export const SelectField = <T extends FieldValues = FieldValues>({
  name,
  control,
  label,
  description,
  options,
  placeholder = 'Select an option',
  disabled = false,
  className = '',
}: SelectFieldProps<T>) => {
  const {
    field,
    fieldState: { error, isDirty, invalid },
  } = useController({
    name,
    control,
  });

  const selectClasses = `
    input w-full
    ${error ? 'border-error' : ''}
    ${isDirty && !invalid ? 'border-success' : ''}
    ${className}
  `;

  return (
    <div className="form-field">
      {label && (
        <label htmlFor={name} className="block text-sm font-medium text-text-primary mb-2">
          {label}
        </label>
      )}
      
      <select
        {...field}
        id={name}
        disabled={disabled}
        className={selectClasses}
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option
            key={option.value}
            value={option.value}
            disabled={option.disabled}
          >
            {option.label}
          </option>
        ))}
      </select>
      
      {description && !error && (
        <p className="mt-1 text-xs text-text-secondary">{description}</p>
      )}
      
      {error && (
        <motion.p
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-1 text-xs text-error"
        >
          {error.message}
        </motion.p>
      )}
    </div>
  );
};

// Checkbox component
interface CheckboxFieldProps<T extends FieldValues = FieldValues> extends BaseFieldProps<T> {
  checkboxLabel?: string;
}

export const CheckboxField = <T extends FieldValues = FieldValues>({
  name,
  control,
  label,
  description,
  checkboxLabel,
  disabled = false,
  className = '',
}: CheckboxFieldProps<T>) => {
  const {
    field: { onChange, onBlur, value, ref },
    fieldState: { error },
  } = useController({
    name,
    control,
  });

  return (
    <div className={`form-field ${className}`}>
      {label && (
        <div className="block text-sm font-medium text-text-primary mb-2">
          {label}
        </div>
      )}
      
      <label className="flex items-center space-x-3 cursor-pointer">
        <input
          ref={ref}
          type="checkbox"
          checked={value || false}
          onChange={(e) => onChange(e.target.checked)}
          onBlur={onBlur}
          disabled={disabled}
          className="w-4 h-4 text-accent-blue bg-secondary border-border-primary rounded focus:ring-accent-blue focus:ring-2"
        />
        <span className="text-sm text-text-primary">
          {checkboxLabel || 'Enable this option'}
        </span>
      </label>
      
      {description && !error && (
        <p className="mt-1 text-xs text-text-secondary ml-7">{description}</p>
      )}
      
      {error && (
        <motion.p
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-1 text-xs text-error ml-7"
        >
          {error.message}
        </motion.p>
      )}
    </div>
  );
};

// Radio group component
interface RadioOption {
  value: string | number;
  label: string;
  description?: string;
  disabled?: boolean;
}

interface RadioGroupFieldProps<T extends FieldValues = FieldValues> extends BaseFieldProps<T> {
  options: RadioOption[];
  direction?: 'horizontal' | 'vertical';
}

export const RadioGroupField = <T extends FieldValues = FieldValues>({
  name,
  control,
  label,
  description,
  options,
  direction = 'vertical',
  disabled = false,
  className = '',
}: RadioGroupFieldProps<T>) => {
  const {
    field: { onChange, onBlur, value, ref },
    fieldState: { error },
  } = useController({
    name,
    control,
  });

  const groupClasses = direction === 'horizontal' 
    ? 'flex flex-wrap gap-6' 
    : 'space-y-3';

  return (
    <div className={`form-field ${className}`}>
      {label && (
        <div className="block text-sm font-medium text-text-primary mb-3">
          {label}
        </div>
      )}
      
      <div className={groupClasses}>
        {options.map((option) => (
          <label
            key={option.value}
            className="flex items-start space-x-3 cursor-pointer"
          >
            <input
              ref={ref}
              type="radio"
              value={option.value}
              checked={value === option.value}
              onChange={(e) => onChange(e.target.value)}
              onBlur={onBlur}
              disabled={disabled || option.disabled}
              className="w-4 h-4 mt-0.5 text-accent-blue bg-secondary border-border-primary focus:ring-accent-blue focus:ring-2"
            />
            <div>
              <div className="text-sm text-text-primary">{option.label}</div>
              {option.description && (
                <div className="text-xs text-text-secondary mt-1">
                  {option.description}
                </div>
              )}
            </div>
          </label>
        ))}
      </div>
      
      {description && !error && (
        <p className="mt-2 text-xs text-text-secondary">{description}</p>
      )}
      
      {error && (
        <motion.p
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="mt-2 text-xs text-error"
        >
          {error.message}
        </motion.p>
      )}
    </div>
  );
};

// Form section component
interface FormSectionProps {
  title?: string;
  description?: string;
  children: React.ReactNode;
  className?: string;
}

export const FormSection: React.FC<FormSectionProps> = ({
  title,
  description,
  children,
  className = '',
}) => (
  <div className={`space-y-6 ${className}`}>
    {(title || description) && (
      <div className="border-b border-border-primary pb-4">
        {title && (
          <h3 className="text-lg font-semibold text-text-primary">{title}</h3>
        )}
        {description && (
          <p className="mt-1 text-sm text-text-secondary">{description}</p>
        )}
      </div>
    )}
    <div className="space-y-6">
      {children}
    </div>
  </div>
);

// Form actions component
interface FormActionsProps {
  children: React.ReactNode;
  align?: 'left' | 'center' | 'right';
  className?: string;
}

export const FormActions: React.FC<FormActionsProps> = ({
  children,
  align = 'right',
  className = '',
}) => {
  const alignClasses = {
    left: 'justify-start',
    center: 'justify-center',
    right: 'justify-end',
  };

  return (
    <div className={`flex gap-3 pt-6 border-t border-border-primary ${alignClasses[align]} ${className}`}>
      {children}
    </div>
  );
};

// Field error component
interface FieldErrorProps {
  error?: string;
}

export const FieldError: React.FC<FieldErrorProps> = ({ error }) => {
  if (!error) return null;

  return (
    <motion.p
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -10 }}
      className="mt-1 text-xs text-error"
    >
      {error}
    </motion.p>
  );
};

// Form wrapper component
interface FormWrapperProps {
  onSubmit: (e: React.FormEvent) => void;
  children: React.ReactNode;
  className?: string;
}

export const FormWrapper: React.FC<FormWrapperProps> = ({
  onSubmit,
  children,
  className = '',
}) => (
  <form onSubmit={onSubmit} className={`space-y-6 ${className}`} noValidate>
    {children}
  </form>
);

// Export all form components
export const Form = {
  Wrapper: FormWrapper,
  Section: FormSection,
  Actions: FormActions,
  TextField,
  NumberField,
  TextAreaField,
  SelectField,
  CheckboxField,
  RadioGroupField,
  FieldError,
};