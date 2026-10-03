# Project Architecture & Patterns

This file defines the structural rules and core architectural patterns of the application.

## Folder Roles & Responsibilities

### `src/app/core/` — Global Infrastructure
- **Purpose:** Singleton services and infrastructure (interceptors, global services, low-level utils).
- **Rule:** Never import feature-specific code into `core`.

### `src/app/features/` — Domain Modules
- **Purpose:** Independent, lazy-loaded vertical slices (`components/`, `models/`, `services/`, `<feature>.routes.ts`).
- **Rule:** Avoid direct cross-feature imports. Shared logic belongs in `shared`.

### `src/app/shared/` — Reusable Primitives
- **Purpose:** Base classes, generic UI widgets, directives, pipes, common business services, and global models.
- **Rule:** Keep shared components pure and highly configurable through inputs/outputs.

## Recipe: Adding a New CRUD Setup
Follow these steps for any new entity (e.g., "Invoices"):
1. **Define DTO:** `features/invoices/models/invoice-dto.ts` extending `BaseDtoWithCommonFields`.
2. **Register URL:** Add to `BaseService.ApiUrls`.
3. **Create Service:** `features/invoices/services/invoice.service.ts` extending `BaseService<InvoiceDto>`.
4. **Implement Components:**
    - **List:** Extend `BaseListComponent` and wire up `SourceInfo`.
    - **Detail:** Extend `BaseDetailComponent` or `BaseDetailPopupComponent`.
5. **Configure Routing:** Create `features/invoices/invoices.routes.ts` and register in `app.routes.ts`.
6. **Update Navigation:** Add to `shared/models/menu-helper.ts`.
7. **Add Permissions:** Add to `EnumPermissionFor` and use in route `data`.

## List Screens — `SourceInfo` Pattern
All paginated list screens must:
1. Declare a `SourceInfo` instance.
2. Subscribe to `sourceInfo.filterChanged` to trigger the API call.
3. Use `ListHelper` static methods to mutate state (automatically emits `filterChanged`).
4. Display `sourceInfo.gridDataStatus` status.

## Routing Rules
- All app shell routes must be children of `HomeComponent`.
- Non-public routes must have `canActivate: [authGuard]`.
- Carry permission data: `data: { permission: EnumPermissions.X, permissionFor: EnumPermissionFor.Y }`.

## HTTP & API Rules
- All calls through `BaseService` methods.
- Centered handling in `errorInterceptor` and `loadingInterceptor`.
