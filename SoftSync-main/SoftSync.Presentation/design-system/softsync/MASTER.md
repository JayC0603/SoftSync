# SoftSync Focus Journey — Design System

SoftSync is a communication-skills learning workspace for university students and young professionals. It is calm, approachable, focused and progressive; it is not a children's learning app.

## Product principles

1. One primary action per screen.
2. Always show what is complete, current and next.
3. Prefer learning content over decorative statistics.
4. Status never relies on color alone: use icon, label and border treatment.
5. Motion explains state change and never delays navigation.

## Foundation

- Font: Be Vietnam Pro; fallback Inter, Arial, sans-serif.
- Body: 16px minimum, line-height 1.5.
- Primary `#5146E5`; hover `#4338CA`; primary soft `#EEEDFF`.
- Background `#F7F7FA`; surface `#FFFFFF`; text `#17182F`; muted `#626A7F`; border `#E3E5ED`.
- Success `#16845B`; warning `#A96500`; danger `#C93636`.
- Space scale: 4, 8, 12, 16, 24, 32, 48, 64px.
- Control radius: 10px; panel radius: 16px; large modal radius: 20px.
- Touch target: at least 44 by 44px.
- Focus: visible 3px indigo ring with 3px offset.

## Motion

- Hover/focus: 150–180ms.
- Dialog and dropdown: 180–220ms.
- Progress: 250–300ms.
- Easing: `cubic-bezier(.2,.8,.2,1)`.
- Respect both `prefers-reduced-motion` and the app's `data-reduce-motion` preference.

## Responsive contract

- Verify 375, 768, 1024 and 1440px.
- Desktop uses the shared top shell; mobile uses at most five bottom-navigation destinations.
- No horizontal page overflow. Horizontal scrolling is allowed only for explicit step/tab controls.
- Important mobile actions stay reachable above the bottom navigation and safe area.

## Vertical-slice hierarchy

- Home: next lesson, weekly rhythm, coaching insight.
- Roadmap: learning sequence and state, with progress supporting it.
- Video: media first, lesson goal second, one next action.
- Quiz: one clear question group, stable actions, feedback adjacent to the answer.

## Forbidden patterns

- Baloo, Comic Neue, emoji as interface icons.
- Galaxy, neon, glassmorphism, giant mascot, decorative gradients.
- Equal-weight card walls and multiple competing primary CTAs.
- Invisible focus states, hover-only interaction and status conveyed only by color.
