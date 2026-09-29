// Validate the separately served stylesheet, which is not parsed by Vite's bundle build.
// lightningcss is already provided by the existing Tailwind toolchain; no new dependency.
import { readFile } from 'node:fs/promises';
import { transform } from 'lightningcss';

const path = new URL('../wwwroot/css/learning-platform.css', import.meta.url);
const code = await readFile(path);
const result = transform({ filename: path.pathname, code, errorRecovery: false });
if (result.warnings.length) {
    for (const warning of result.warnings) console.warn(warning.message);
    throw new Error('UI stylesheet validation reported warnings.');
}
console.log('Learning platform CSS syntax: PASS (not a browser or performance check).');
