// Regression check for the roadmap's dark-theme surfaces. This validates the
// contrast of the page's actual CSS declarations, not a screenshot substitute.
import { readFile } from 'node:fs/promises';
import assert from 'node:assert/strict';

const razor = await readFile(new URL('../Components/Pages/RoadmapOverview.razor', import.meta.url), 'utf8');
const css = razor.match(/<style>([\s\S]*?)<\/style>/)?.[1];
assert.ok(css, 'Roadmap stylesheet is missing');

function rule(selector) {
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const match = css.match(new RegExp(`${escaped}\\s*\\{([^{}]*)\\}`, 'g'))?.at(-1);
    assert.ok(match, `Missing rule for ${selector}`);
    return match.slice(match.indexOf('{') + 1, -1);
}

function property(selector, name) {
    const value = rule(selector).match(new RegExp(`(?:^|;)\\s*${name}\\s*:\\s*([^;]+)`))?.[1]?.trim();
    assert.ok(value, `${selector} needs ${name}`);
    return value;
}

function rgb(hex) {
    assert.match(hex, /^#[0-9a-f]{6}$/i);
    return [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16) / 255);
}
function luminance(hex) {
    return rgb(hex).map(x => x <= .04045 ? x / 12.92 : ((x + .055) / 1.055) ** 2.4)
        .reduce((sum, x, i) => sum + x * [.2126, .7152, .0722][i], 0);
}
function contrast(a, b) {
    const [high, low] = [luminance(a), luminance(b)].sort((x, y) => y - x);
    return (high + .05) / (low + .05);
}

const root = 'html[data-theme="dark"] .coursera-roadmap';
const foreground = property(root, 'color');
for (const selector of ['.roadmap-sidebar', '.overall-card', '.xp-card', '.streak', '.week-stat']) {
    const background = property(`html[data-theme="dark"] ${selector}`, 'background');
    assert.ok(contrast(foreground, background) >= 4.5,
        `${selector} has insufficient dark-theme text contrast`);
}
console.log('Roadmap dark surfaces: PASS');
