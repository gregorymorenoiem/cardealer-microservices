/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: ['class', '[data-theme="dark"]'],
  theme: {
    extend: {
      /* ==========================================
         COLORES OKLA
         ========================================== */
      colors: {
        // Colores primarios OKLA
        okla: {
          primary: 'var(--okla-primary)',
          'primary-hover': 'var(--okla-primary-hover)',
          'primary-active': 'var(--okla-primary-active)',
        },
        
        // Dorado Champagne - Acento principal
        gold: {
          DEFAULT: '#C9A962',
          light: '#D4B978',
          dark: '#B8944D',
          50: '#FAF7F0',
          100: '#F5EFE0',
          200: '#EBE0C2',
          300: '#E0D0A3',
          400: '#D4BF84',
          500: '#C9A962',
          600: '#B8944D',
          700: '#967838',
          800: '#745D2C',
          900: '#524220',
        },
        
        // Bronce Elegante
        bronze: {
          DEFAULT: '#8B7355',
          light: '#A08B6B',
          dark: '#6D5A43',
        },
        
        // Fondos y superficies
        surface: {
          DEFAULT: 'var(--okla-surface)',
          elevated: 'var(--okla-surface-elevated)',
          hover: 'var(--okla-surface-hover)',
        },
        
        // Colores primarios actualizados para OKLA
        primary: {
          DEFAULT: '#1A1A1A',
          50: '#F5F5F5',
          100: '#E0E0E0',
          200: '#C0C0C0',
          300: '#A0A0A0',
          400: '#808080',
          500: '#1A1A1A',
          600: '#141414',
          700: '#0F0F0F',
          800: '#0A0A0A',
          900: '#050505',
        },
        secondary: {
          DEFAULT: '#C9A962',
          50: '#FAF7F0',
          100: '#F5EFE0',
          200: '#EBE0C2',
          300: '#E0D0A3',
          400: '#D4BF84',
          500: '#C9A962',
          600: '#B8944D',
          700: '#967838',
          800: '#745D2C',
          900: '#524220',
        },
        accent: {
          DEFAULT: '#8B7355',
          50: '#F7F5F3',
          100: '#EDE9E5',
          200: '#DDD6CE',
          300: '#CCC2B6',
          400: '#BBAD9E',
          500: '#8B7355',
          600: '#6D5A43',
          700: '#554633',
          800: '#3D3225',
          900: '#251E17',
        },
      },
      
      /* ==========================================
         TIPOGRAFÍA
         ========================================== */
      fontFamily: {
        display: ['Playfair Display', 'Georgia', 'Times New Roman', 'serif'],
        sans: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
        body: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
        heading: ['Playfair Display', 'Georgia', 'serif'],
        mono: ['JetBrains Mono', 'Fira Code', 'Consolas', 'monospace'],
      },
      
      fontSize: {
        'display-1': ['4.5rem', { lineHeight: '1.1', letterSpacing: '-0.02em' }],
        'display-2': ['3.75rem', { lineHeight: '1.15', letterSpacing: '-0.02em' }],
        'display-3': ['3rem', { lineHeight: '1.2', letterSpacing: '-0.01em' }],
      },
      
      letterSpacing: {
        tighter: '-0.05em',
        tight: '-0.025em',
        normal: '0',
        wide: '0.025em',
        wider: '0.05em',
        widest: '0.1em',
      },
      
      /* ==========================================
         ESPACIADO EXTENDIDO
         ========================================== */
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem',
        '144': '36rem',
      },
      
      /* ==========================================
         BORDES Y RADIUS
         ========================================== */
      borderRadius: {
        '4xl': '2rem',
        '5xl': '2.5rem',
      },
      
      /* ==========================================
         SOMBRAS PREMIUM
         ========================================== */
      boxShadow: {
        'xs': '0 1px 2px 0 rgba(0, 0, 0, 0.03)',
        'card': '0 1px 3px 0 rgba(0, 0, 0, 0.04), 0 1px 2px -1px rgba(0, 0, 0, 0.02)',
        'card-hover': '0 10px 15px -3px rgba(0, 0, 0, 0.05), 0 4px 6px -4px rgba(0, 0, 0, 0.03)',
        'elegant': '0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.03)',
        'elegant-lg': '0 10px 15px -3px rgba(0, 0, 0, 0.05), 0 4px 6px -4px rgba(0, 0, 0, 0.03)',
        'elegant-xl': '0 20px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.03)',
        'gold': '0 4px 14px 0 rgba(201, 169, 98, 0.15)',
        'gold-lg': '0 10px 25px 0 rgba(201, 169, 98, 0.20)',
        'gold-glow': '0 0 20px 0 rgba(201, 169, 98, 0.30)',
        'inner-subtle': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.02)',
      },
      
      /* ==========================================
         ANIMACIONES Y TRANSICIONES
         ========================================== */
      transitionDuration: {
        'fast': '150ms',
        'base': '200ms',
        'slow': '300ms',
        'slower': '500ms',
      },
      
      transitionTimingFunction: {
        'ease-elegant': 'cubic-bezier(0.4, 0, 0.2, 1)',
        'ease-bounce': 'cubic-bezier(0.68, -0.55, 0.265, 1.55)',
        'ease-smooth': 'cubic-bezier(0.25, 0.1, 0.25, 1)',
      },
      
      animation: {
        'fade-in': 'fadeIn 0.3s ease-out',
        'fade-in-up': 'fadeInUp 0.4s ease-out',
        'fade-in-down': 'fadeInDown 0.4s ease-out',
        'scale-in': 'scaleIn 0.2s ease-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'slide-in-left': 'slideInLeft 0.3s ease-out',
        'shimmer': 'shimmer 2s infinite linear',
        'pulse-soft': 'pulseSoft 2s infinite',
        'float': 'float 6s ease-in-out infinite',
      },
      
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        fadeInUp: {
          '0%': { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeInDown: {
          '0%': { opacity: '0', transform: 'translateY(-20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        scaleIn: {
          '0%': { opacity: '0', transform: 'scale(0.95)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
        slideInRight: {
          '0%': { opacity: '0', transform: 'translateX(20px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        slideInLeft: {
          '0%': { opacity: '0', transform: 'translateX(-20px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-200% 0' },
          '100%': { backgroundPosition: '200% 0' },
        },
        pulseSoft: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.7' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
      },
      
      /* ==========================================
         BACKDROP
         ========================================== */
      backdropBlur: {
        'xs': '2px',
      },
      
      /* ==========================================
         Z-INDEX
         ========================================== */
      zIndex: {
        'dropdown': '1000',
        'sticky': '1020',
        'fixed': '1030',
        'modal-backdrop': '1040',
        'modal': '1050',
        'popover': '1060',
        'tooltip': '1070',
        'toast': '1080',
      },
      
      /* ==========================================
         ASPECTOS
         ========================================== */
      aspectRatio: {
        'card': '4 / 3',
        'product': '3 / 4',
        'hero': '16 / 9',
        'square': '1 / 1',
      },
      
      /* ==========================================
         CONTAINER
         ========================================== */
      container: {
        center: true,
        padding: {
          DEFAULT: '1rem',
          sm: '2rem',
          lg: '4rem',
          xl: '5rem',
          '2xl': '6rem',
        },
      },
      
      /* ==========================================
         GRADIENTES PERSONALIZADOS
         ========================================== */
      backgroundImage: {
        'gold-gradient': 'linear-gradient(135deg, #E0C55C 0%, #C9A962 50%, #B8944D 100%)',
        'gold-gradient-soft': 'linear-gradient(135deg, #FAF7F0 0%, #F5EFE0 50%, #EBE0C2 100%)',
        'dark-gradient': 'linear-gradient(180deg, #1A1A1A 0%, #0A0A0A 100%)',
        'shimmer': 'linear-gradient(90deg, transparent 0%, rgba(255,255,255,0.1) 50%, transparent 100%)',
        'hero-overlay': 'linear-gradient(180deg, rgba(0,0,0,0.3) 0%, rgba(0,0,0,0.6) 100%)',
      },
    },
  },
  plugins: [],
}
