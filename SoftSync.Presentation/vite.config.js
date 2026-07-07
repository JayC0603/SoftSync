import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

// Vite compiles the Tailwind + liquid-glass source into wwwroot/dist,
// which the Blazor app serves as static assets. No dev server / HMR here —
// Blazor owns the page, Vite just bundles CSS/JS. Use `npm run dev` for a
// rebuild-on-save watcher alongside `dotnet watch`.
export default defineConfig({
  plugins: [tailwindcss()],
  build: {
    outDir: 'wwwroot/dist',
    emptyOutDir: true,
    manifest: false,
    rollupOptions: {
      // Single JS entry that imports the stylesheet; Vite emits app.js + app.css.
      input: { app: 'Scripts/main.js' },
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: '[name].js',
        assetFileNames: '[name][extname]',
      },
    },
  },
});
