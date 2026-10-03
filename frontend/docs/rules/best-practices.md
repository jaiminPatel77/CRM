# Best Practices & Quality

This file defines general coding standards, forms, i18n, and quality assurance rules.

## Forms
- **Reactive Forms exclusively.** Template-driven forms are forbidden.
- Apply `appFocusFirstInvalidField` on `<form>`.
- Use `ConfirmDialogPopupComponent` for deletions; no `window.confirm`.

## Internationalisation (i18n)
- No hardcoded strings. Use `public/i18n/*.json`.
- Key format: `SCREAMING_SNAKE_CASE`.
- Template: `{{ 'KEY' | translate }}`. TS: `translateService.instant('KEY')`.

## Authentication & Permissions
- Token: `authService.getCurrentToken().isValid()`.
- Check: `permissionService.getEvaluatedPermission(rolePermDto)`.
- **Bit-flags:** Use bitwise `&` for comparisons.

## General Best Practices
- **Accessibility:** ARIA labels, focusable roles, 44px tap targets (`--min-tap-target`).
- **Semantic HTML:** Use `<nav>`, `<main>`, `<section>`, etc.
- **No magic numbers:** Extract to tokens or `ConstString`.
- **No inline styles:** Use classes or variables.
- **Single Responsibility:** Logic in services, not components.

## Code Quality
- **Read before editing.**
- **One concern per PR.**
- **No dead code.**
- **Spec files required** for any new service/directive.
- **No `@ts-ignore`** without comment and TODO.
- **Prefer `const` over `let`.** No `var`.
