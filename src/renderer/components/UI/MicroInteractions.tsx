/**
 * Micro-Interactions Components
 * Subtle animations and interactions for enhanced UX
 */

import React from 'react';
import { motion, useMotionValue, useSpring, useTransform } from 'framer-motion';
import { cn } from '../../utils';

// Hover scale wrapper
interface HoverScaleProps {
  children: React.ReactNode;
  scale?: number;
  className?: string;
}

export const HoverScale: React.FC<HoverScaleProps> = ({
  children,
  scale = 1.02,
  className,
}) => {
  return (
    <motion.div
      whileHover={{ scale }}
      whileTap={{ scale: scale * 0.98 }}
      transition={{ type: 'spring', stiffness: 400, damping: 25 }}
      className={className}
    >
      {children}
    </motion.div>
  );
};

// Floating animation wrapper
interface FloatingProps {
  children: React.ReactNode;
  intensity?: 'subtle' | 'medium' | 'strong';
  className?: string;
}

export const Floating: React.FC<FloatingProps> = ({
  children,
  intensity = 'subtle',
  className,
}) => {
  const yOffset = {
    subtle: [-2, 2],
    medium: [-5, 5],
    strong: [-10, 10],
  }[intensity];

  return (
    <motion.div
      animate={{ y: yOffset }}
      transition={{
        duration: 3,
        repeat: Infinity,
        repeatType: 'reverse',
        ease: 'easeInOut',
      }}
      className={className}
    >
      {children}
    </motion.div>
  );
};

// Magnetic button effect
interface MagneticProps {
  children: React.ReactNode;
  strength?: number;
  className?: string;
}

export const Magnetic: React.FC<MagneticProps> = ({
  children,
  strength = 0.3,
  className,
}) => {
  const x = useMotionValue(0);
  const y = useMotionValue(0);

  const springX = useSpring(x, { stiffness: 150, damping: 15 });
  const springY = useSpring(y, { stiffness: 150, damping: 15 });

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const centerX = rect.left + rect.width / 2;
    const centerY = rect.top + rect.height / 2;
    
    const deltaX = (e.clientX - centerX) * strength;
    const deltaY = (e.clientY - centerY) * strength;

    x.set(deltaX);
    y.set(deltaY);
  };

  const handleMouseLeave = () => {
    x.set(0);
    y.set(0);
  };

  return (
    <motion.div
      style={{ x: springX, y: springY }}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
      className={className}
    >
      {children}
    </motion.div>
  );
};

// Pulse animation
interface PulseProps {
  children: React.ReactNode;
  color?: string;
  intensity?: 'subtle' | 'medium' | 'strong';
  className?: string;
}

export const Pulse: React.FC<PulseProps> = ({
  children,
  color = 'rgb(0, 212, 255)',
  intensity = 'medium',
  className,
}) => {
  const scale = {
    subtle: [1, 1.01],
    medium: [1, 1.05],
    strong: [1, 1.1],
  }[intensity];

  const opacity = {
    subtle: [0.8, 1],
    medium: [0.7, 1],
    strong: [0.6, 1],
  }[intensity];

  return (
    <motion.div
      animate={{
        scale,
        boxShadow: [
          `0 0 0 0 ${color}00`,
          `0 0 0 10px ${color}40`,
          `0 0 0 10px ${color}00`,
        ],
      }}
      transition={{
        duration: 2,
        repeat: Infinity,
        ease: 'easeInOut',
      }}
      className={className}
    >
      <motion.div
        animate={{ opacity }}
        transition={{
          duration: 2,
          repeat: Infinity,
          ease: 'easeInOut',
        }}
      >
        {children}
      </motion.div>
    </motion.div>
  );
};

// Glow on hover effect
interface GlowHoverProps {
  children: React.ReactNode;
  color?: string;
  intensity?: number;
  className?: string;
}

export const GlowHover: React.FC<GlowHoverProps> = ({
  children,
  color = '#00d4ff',
  intensity = 20,
  className,
}) => {
  return (
    <motion.div
      whileHover={{
        boxShadow: `0 0 ${intensity}px ${color}`,
        scale: 1.02,
      }}
      transition={{ duration: 0.2 }}
      className={className}
    >
      {children}
    </motion.div>
  );
};

// Tilt on hover
interface TiltProps {
  children: React.ReactNode;
  maxTilt?: number;
  perspective?: number;
  scale?: number;
  className?: string;
}

export const Tilt: React.FC<TiltProps> = ({
  children,
  maxTilt = 10,
  perspective = 1000,
  scale = 1.02,
  className,
}) => {
  const rotateX = useMotionValue(0);
  const rotateY = useMotionValue(0);

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const centerX = rect.left + rect.width / 2;
    const centerY = rect.top + rect.height / 2;
    
    const deltaX = (e.clientY - centerY) / (rect.height / 2);
    const deltaY = (e.clientX - centerX) / (rect.width / 2);

    rotateX.set(deltaX * maxTilt);
    rotateY.set(deltaY * maxTilt);
  };

  const handleMouseLeave = () => {
    rotateX.set(0);
    rotateY.set(0);
  };

  return (
    <motion.div
      style={{
        rotateX,
        rotateY,
        transformStyle: 'preserve-3d',
        perspective,
      }}
      whileHover={{ scale }}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
      transition={{ type: 'spring', stiffness: 300, damping: 30 }}
      className={className}
    >
      {children}
    </motion.div>
  );
};

// Ripple effect on click
interface RippleProps {
  children: React.ReactNode;
  color?: string;
  duration?: number;
  className?: string;
}

export const Ripple: React.FC<RippleProps> = ({
  children,
  color = 'rgba(0, 212, 255, 0.3)',
  duration = 0.6,
  className,
}) => {
  const [ripples, setRipples] = React.useState<Array<{ id: number; x: number; y: number }>>([]);

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    const newRipple = { id: Date.now(), x, y };
    setRipples(prev => [...prev, newRipple]);

    setTimeout(() => {
      setRipples(prev => prev.filter(ripple => ripple.id !== newRipple.id));
    }, duration * 1000);
  };

  return (
    <div
      className={cn('relative overflow-hidden', className)}
      onClick={handleClick}
    >
      {children}
      {ripples.map((ripple) => (
        <motion.div
          key={ripple.id}
          className="absolute pointer-events-none rounded-full"
          style={{
            left: ripple.x,
            top: ripple.y,
            backgroundColor: color,
          }}
          initial={{ width: 0, height: 0, opacity: 1 }}
          animate={{
            width: 400,
            height: 400,
            opacity: 0,
            x: -200,
            y: -200,
          }}
          transition={{ duration, ease: 'easeOut' }}
        />
      ))}
    </div>
  );
};

// Fade in animation wrapper
interface FadeInProps {
  children: React.ReactNode;
  delay?: number;
  direction?: 'up' | 'down' | 'left' | 'right';
  distance?: number;
  className?: string;
}

export const FadeIn: React.FC<FadeInProps> = ({
  children,
  delay = 0,
  direction = 'up',
  distance = 50,
  className,
}) => {
  const initialPosition = {
    up: { y: distance },
    down: { y: -distance },
    left: { x: distance },
    right: { x: -distance },
  }[direction];

  return (
    <motion.div
      initial={{ opacity: 0, ...initialPosition }}
      animate={{ opacity: 1, x: 0, y: 0 }}
      transition={{
        duration: 0.6,
        delay,
        ease: [0.25, 0.46, 0.45, 0.94],
      }}
      className={className}
    >
      {children}
    </motion.div>
  );
};