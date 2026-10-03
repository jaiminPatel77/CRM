# Angular 20 Standards

This file defines the mandatory Angular 20 syntax and patterns for the project.

## Mandatory Modern Syntax

Agents **MUST** use the following modern syntax. Legacy directives or constructor-based injection are forbidden.

### Control Flow (Built-in)
Disable `CommonModule` directives in favor of the new control flow:
- **Use `@if` / `@else`** instead of `*ngIf`.
- **Use `@for`** instead of `*ngFor` (always include a `track` expression).
- **Use `@switch` / `@case`** instead of `[ngSwitch]`.

### Dependency Injection
- **Use `inject()`** instead of constructor injection for all services, `ActivatedRoute`, `Router`, etc.
  ```typescript
  private invoiceService = inject(InvoiceService);
  private route = inject(ActivatedRoute);
  ```

### State & Reactivity
- **Use Signals** (`signal()`, `computed()`, `effect()`) for local component/UI-only state.
- **Input/Output Signals:** Use `input()`, `output()`, and `model()` where applicable.
- **Computed:** Use `computed()` for derived state.
- **Effects:** Use `effect()` for side effects with caution.
- **Transition:** Do not use `BehaviorSubject` for state that could be a `signal`.

## Standalone Architecture
- This project is fully standalone. All components must declare their own `imports: []`.
- **Never import NgModules.**

## State Management (NgRx)
- Use **NgRx** (`@ngrx/store` + `@ngrx/effects`) only for shared, server-synchronised state.
- Keep NgRx artifacts (actions, reducer, selectors, effects) co-located in `features/<name>/store/`.
