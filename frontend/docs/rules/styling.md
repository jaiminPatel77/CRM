# Styling & Design System

This file defines the styling rules, CSS tokens, and branding guidelines.

## CSS Custom Properties
**Never hardcode colors, pixels, or font sizes.** Use these variables:

| Category | Variables |
|---|---|
| Backgrounds | `--bg`, `--surface`, `--surface-elevated` |
| Text | `--text`, `--text-secondary`, `--text-disabled` |
| Brand | `--primary`, `--primary-hover`, `--primary-light` |
| Status | `--success`, `--danger`, `--warning` |
| Sidebar | `--sidebar-bg`, `--sidebar-active-bg` |
| Inputs | `--input-bg`, `--input-border` |
| Layout | `--header-height`, `--sidebar-width`, `--page-gutter` |
| Transitions | `--transition-fast`, `--transition-base`, `--transition-slow` |
| Elevations | `--elevation-0` to `--elevation-5` |

## Spacing (8px Grid)
| Token | Value | Token | Value |
|---|---|---|---|
| `--space-1` | 4px | `--space-4` | 24px |
| `--space-2` | 8px | `--space-5` | 32px |
| `--space-3` | 16px | `--space-6` | 48px |

## Typography
- **Font Family:** Roboto (`--font-family-base`, `--font-family-heading`)
- **Sizes:** `--font-size-xs` (11px) to `--font-size-2xl` (28px).
- **Weights:** `--font-weight-light` (300) to `--font-weight-bold` (700).
- **Line Heights:** `--line-height-tight` (1.25) to `--line-height-relaxed` (1.75).

## Radius & Icons
- **Radius:** `--radius-sm` (4px) to `--radius-xl` (16px), `--radius-full`.
- **Icons:** Use `SvgIconDirective` (`appSvgIcon="name"`). Never embed raw SVG strings.

## Utility Classes (`_utilities.scss`)
**Always prefer utility classes over custom CSS for styling components:**
- **Spacing:** `p-1` to `p-5`, `m-1` to `m-5`, `gap-1` to `gap-6`.
- **Typography:** `text-sm`, `font-bold`, `leading-relaxed`, `text-primary`, `text-secondary`, etc.
- **Layout:** `page-padding`, `section`, `page-header`, `toolbar`, `action-btn`.
- **Appearance:** `rounded-lg`, `elevation-2`, `bg-surface`, `border-divider`.
- **Avoid Redundancy:** Do NOT append utility classes (like `rounded-sm`, `p-3`, `bg-surface`) to base components (like `.btn` or `.form-control`) that already inherit these properties from our design tokens and `_bootstrap-overrides.scss`.

## Interactive & Accessibility
- **Tap Targets:** Elements must respect `--min-tap-target` (44px) where applicable.
- **Inputs/Buttons:** Use `--input-height-*` and `--button-height-*` tokens.

## Theming
- **Themes:** `viraj` (default/brand), `light`, `dark`, `blue`, `glass`, `bold`, `soft`, `corporate`.
- **Service:** Use `ThemeService.setTheme(theme)`. Active theme is a `signal<Theme>`.
- **Rule:** Never read `localStorage` for theme; use `ThemeService`.

---

## AI Agent Instructions: Check Before Writing CSS
To prevent accidentally adding duplicate CSS code, **you MUST ALWAYS check the corresponding file in `src/styles`** before writing new CSS, depending on what you are trying to style:

### File Inventory & Roles
| File | Role | Action |
|---|---|---|
| `_tokens.scss` | Foundation: Spacing, colors, radius, typography, layout dimensions (e.g. `$header-height`). | **Check first** to see if a token exists for what you are styling. Never hardcode these values. |
| `_utilities.scss` | Utility Classes: Spacing padding/margins (`p-3`, `mt-4`), layout helpers (`page-header`, `toolbar`), typography styles. | **Check second.** Always prefer applying these utility classes in the HTML template directly over writing custom component CSS. |
| `themes.scss` | Theme Definitions: Colors and theme-specific component overrides (e.g. how `.card` looks in `theme-glass`). | Modify this if you need to alter a component's specific look across the whole app or a specific theme. |
| `_bootstrap-overrides.scss` | Bootstrap Integration: Maps our custom tokens (`--radius-md`) to Bootstrap CSS variables (`--bs-border-radius`). | Modify this if you are using a Bootstrap component that isn't matching the design system. |
| `fonts.scss` | Typography Loads: Local font face definitions (`@font-face`). | Modify only when adding a new font family weight or entirely new webfont. |

---

## Layout & Constraints
- **Bootstrap:** Customizations ONLY in `src/styles/_bootstrap-overrides.scss`.
- **Import Order:** Never change the load-bearing order in `src/styles.scss`.
- **Mobile-first:** Write base styles for mobile, then `min-width` queries.
- **NO Duplicate CSS:** Never repeat the same CSS blocks across multiple components. If a styling pattern is reused (e.g., auth hero layouts, specific card types), move them to a shared SCSS file (e.g., `_auth-shared.scss` within the feature folder) or global utilities.
