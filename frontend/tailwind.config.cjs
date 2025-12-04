/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#00539F',
          50: '#E6F2FF',
          100: '#CCE5FF',
          200: '#99CBFF',
          300: '#66B0FF',
          400: '#3396FF',
          500: '#00539F',
          600: '#004380',
          700: '#003260',
          800: '#002240',
          900: '#001120',
        },
        secondary: {
          DEFAULT: '#0089FF',
          50: '#E6F5FF',
          100: '#CCEBFF',
          200: '#99D7FF',
          300: '#66C3FF',
          400: '#33AFFF',
          500: '#0089FF',
          600: '#006ECC',
          700: '#005299',
          800: '#003766',
          900: '#001B33',
        },
        accent: {
          DEFAULT: '#FF6B35',
          50: '#FFF2ED',
          100: '#FFE5DB',
          200: '#FFCBB7',
          300: '#FFB193',
          400: '#FF976F',
          500: '#FF6B35',
          600: '#FF4500',
          700: '#CC3700',
          800: '#992900',
          900: '#661C00',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        heading: ['Poppins', 'sans-serif'],
      },
      spacing: {
        '128': '32rem',
        '144': '36rem',
      },
      borderRadius: {
        '4xl': '2rem',
      },
      boxShadow: {
        'card': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        'card-hover': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
      },
    },
  },
  plugins: [],
}
