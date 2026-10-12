/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#2563EB",
          hover: "#1D4ED8",
          light: "#EFF6FF",
          dark: "#1E40AF",
        },
        slateBg: "#F8FAFC",
        darkNavy: "#0F172A",
        borderGray: "#E2E8F0",
      },
      borderRadius: {
        'card': '16px',
      }
    },
  },
  plugins: [],
  corePlugins: {
    preflight: false, // Prevent Tailwind reset from overriding Ant Design styles
  }
}
