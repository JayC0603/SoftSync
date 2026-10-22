import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const component = await readFile(new URL('../Components/Shared/AnimatedSoftSyncBackground.razor', import.meta.url), 'utf8');
const styles = await readFile(new URL('../Styles/softsync-background.css', import.meta.url), 'utf8');
const globalStyles = await readFile(new URL('../Styles/main.css', import.meta.url), 'utf8');
const home = await readFile(new URL('../Components/Pages/Home.razor', import.meta.url), 'utf8');
const mainLayout = await readFile(new URL('../Components/Layout/MainLayout.razor', import.meta.url), 'utf8');
const accountLayout = await readFile(new URL('../Components/Account/Shared/AccountLayout.razor', import.meta.url), 'utf8');
const dashboard = await readFile(new URL('../Components/Pages/Dashboard.razor', import.meta.url), 'utf8');

assert.match(component, /aria-hidden="true"/, 'decorative layers must be hidden from assistive technology');
assert.match(styles, /\.ss-animated-bg\s*\{[^}]*pointer-events:\s*none/i, 'decorative layers must not block interaction');
assert.match(component, /Intensity|intensity/i, 'the component must accept page-specific intensity');
assert.match(component, /<svg[\s\S]*?aria-hidden="true"/, 'the Vietnamese network and ribbons should use decorative SVG');
assert.match(styles, /@media\s*\(prefers-reduced-motion:\s*reduce\)/, 'system reduced-motion preference must be honored');
assert.match(styles, /data-reduce-motion/, 'the saved SoftSync reduced-motion preference must be honored');
assert.match(styles, /@media\s*\(max-width:\s*720px\)/, 'mobile needs a reduced decorative composition');
assert.match(styles, /ss-bg-intensity-(hero|ambient|quiet)/i, 'all three page intensity variants must exist');
assert.match(styles, /data-theme="dark"/, 'dark theme needs explicit background colors');
assert.match(mainLayout, /AnimatedSoftSyncBackground Intensity="@BackgroundIntensity"/, 'main layout must mount one route-aware shared background');
assert.match(mainLayout, /"dashboard"[\s\S]*"Ambient"/, 'dashboard must use ambient intensity');
assert.match(dashboard, /\.ss-main-content:has\(> \.dash-home\)\s*\{[^}]*background:\s*transparent/i, 'dashboard shell must stay transparent so the shared background remains visible');
assert.match(dashboard, /html\[data-theme="dark"\] \.ss-main-content:has\(> \.dash-home\)\s*\{[^}]*background:\s*transparent/i, 'dark dashboard shell must stay transparent too');
assert.match(mainLayout, /\? "Hero"/, 'landing must use hero intensity');
assert.match(accountLayout, /AnimatedSoftSyncBackground[^\n]*Hero/i, 'account pages must mount the shared hero background');
assert.doesNotMatch(mainLayout, /<div class="ss-aurora"/, 'main layout must not retain the old duplicate background');
assert.doesNotMatch(accountLayout, /<div class="ss-aurora"/, 'account layout must not retain the old duplicate background');
assert.doesNotMatch(globalStyles, /\.ss-aurora|background-softsync\.png/, 'global styles must use the layered background instead of the old image');
assert.doesNotMatch(home, /background-softsync\.png/, 'landing page must not retain a full-page copy of the old image');
assert.match(home, /ss-home-hero-enter/, 'landing hero must use a short entrance animation');
assert.match(home, /prefers-reduced-motion:\s*reduce/, 'landing entrance must honor system reduced motion');
assert.match(home, /data-reduce-motion="1"/, 'landing entrance must honor saved reduced motion');

console.log('Animated SoftSync background checks: PASS');
