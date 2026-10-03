# crm-app — Developer Guide

> **Angular 20 Enterprise Template** · Bootstrap 5 · NgRx · ngx-translate · SCSS Design System

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Prerequisites & Setup](#2-prerequisites--setup)
3. [Project Structure](#3-project-structure)
4. [Architecture](#4-architecture)
5. [Routing](#5-routing)
6. [HTTP Layer](#6-http-layer)
7. [State Management (NgRx)](#7-state-management-ngrx)
8. [Authentication & Authorization](#8-authentication--authorization)
9. [Design System & Styling](#9-design-system--styling)
10. [Theming](#10-theming)
11. [Shared Library](#11-shared-library)
12. [Core Services](#12-core-services)
13. [Feature Modules](#13-feature-modules)
14. [Adding a New Feature](#14-adding-a-new-feature)
15. [Internationalisation (i18n)](#15-internationalisation-i18n)
16. [Logging](#16-logging)
17. [Environment Configuration](#17-environment-configuration)
18. [Testing](#18-testing)
19. [Common Patterns & Conventions](#19-common-patterns--conventions)

---

## 1. Project Overview

`crm-app` is a production-ready Angular 20 starter template designed to accelerate enterprise application development. It ships with:

| Feature | Detail |
|---|---|
| **Framework** | Angular 20 (standalone components, signals) |
| **CSS framework** | Bootstrap 5.3 |
| **State** | @ngrx/store + @ngrx/effects |
| **i18n** | @ngx-translate/core |
| **UI extras** | ng-bootstrap, ng-select, ngx-captcha |
| **Testing** | Karma/Jasmine (unit), Cypress (e2e) |
| **Styling** | SCSS design system — 7 built-in themes |
| **Auth** | JWT Bearer token, idle-session management |

---

## 2. Prerequisites & Setup

### Requirements

| Tool | Minimum version |
|---|---|
| Node.js | 20 LTS |
| npm | 10+ |
| Angular CLI | 20 |

### First-time Setup

```bash
# Install dependencies
npm install

# Start dev server (http://localhost:4200)
npm start             # alias: ng serve

# Run unit tests
npm test

# Open Cypress e2e runner
npm run e2e

# Run Cypress headlessly (CI)
npm run e2e:run
```

> **User-rule note:** Do **not** run `npm run build` or `ng serve` without asking the developer to verify it manually first.

### Environment Variables

Before running, update `src/environments/environment.development.ts` with your local API URL:

```typescript
getSettings(): IEnvironmentSetting {
  return {
    rootURL: '/',
    apiServiceUrl: 'http://localhost:5105',   // ← your API base URL
    captchaKey: '<your-recaptcha-site-key>',
    remoteLogUrl: '',                          // optional remote log endpoint
  };
}
```

The production environment file is `src/environments/environment.ts`. The build system swaps files automatically via `fileReplacements` in `angular.json`.

---

## 3. Project Structure

```
crm-app/
├── src/
│   ├── app/
│   │   ├── app.component.ts       # Root component
│   │   ├── app.config.ts          # Application providers (standalone)
│   │   ├── app.routes.ts          # Top-level lazy routes
│   │   ├── core/                  # App-wide singleton infrastructure
│   │   │   ├── interceptors/      # JWT, error, loading interceptors
│   │   │   ├── services/          # ThemeService, LoggerService, ToastService, GlobalErrorHandler, LoadingService
│   │   │   ├── models/            # Core-level models
│   │   │   └── utils/             # HttpErrorUtil
│   │   ├── features/              # Lazy-loaded feature slices
│   │   │   ├── auth/              # Login, forgot-password, set-password
│   │   │   ├── master/            # App shell: Home, Dashboard, Profile
│   │   │   ├── admin/             # Users, Roles, Settings, Logs, Backups, Audit
│   │   │   └── project-management/# Projects & Tasks
│   │   ├── shared/                # Reusable across features
│   │   │   ├── components/        # BaseList, BaseDetail, ColumnSorter, Toaster, Loading, ConfirmDialog …
│   │   │   ├── directives/        # SvgIcon, FocusFirstInvalidField
│   │   │   ├── interfaces/        # ICrudService
│   │   │   ├── models/            # BaseDto, SourceInfo, Pagination, FilterModel, EvaluatedPermission …
│   │   │   ├── pipes/             # (expandable)
│   │   │   └── services/          # BaseService, CommonService, NavigationService, UserPermissionService, ToastService, AppTranslateService
│   │   ├── models/                # Route guards (auth.guard)
│   │   └── services/              # PreloadingStrategyService
│   ├── environments/              # environment.ts / environment.development.ts
│   ├── styles/
│   │   ├── _tokens.scss           # Design tokens (spacing, typography, shadows …)
│   │   ├── themes.scss            # 7 theme definitions
│   │   ├── _bootstrap-overrides.scss  # Map tokens → Bootstrap custom properties
│   │   ├── _utilities.scss        # 8px-grid spacing/layout utility classes
│   │   └── fonts.scss             # Local Roboto font faces
│   ├── index.html
│   ├── main.ts                    # Bootstrap
│   └── styles.scss                # Global entry point
├── public/                        # Static assets (fonts, icons)
├── cypress/                       # E2E tests
├── angular.json
├── tsconfig.json
└── package.json
```

---

## 4. Architecture

### Standalone Components (Angular 17+)

This project uses **standalone components** throughout — there are no `NgModule` declarations. Each component imports its own dependencies.

```typescript
@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, /* … */],
  templateUrl: './my.component.html',
})
export class MyComponent {}
```

### Application Bootstrap

`main.ts` bootstraps using `appConfig`:

```typescript
// src/app/app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withPreloading(PreloadingStrategyService)),
    provideHttpClient(
      withFetch(),
      withInterceptors([jwtInterceptor, errorInterceptor, loadingInterceptor])
    ),
    importProvidersFrom(translateModule),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    ToastService,
  ],
};
```

Key decisions:
- **`withFetch()`** — uses the Fetch API instead of XMLHttpRequest for better performance.
- **`withPreloading(PreloadingStrategyService)`** — custom preloading; routes marked `data: { preload: true }` are eagerly prefetched after initial load.
- **`GlobalErrorHandler`** — catches unhandled Angular errors globally.

---

## 5. Routing

### Route Map

```
/                        → LoginComponent (public)
/auth/login              → LoginComponent
/auth/forgot-password    → ForgotPasswordComponent
/auth/forgot-password-confirm → ForgotPasswordConfirmComponent
/auth/set-password       → SetPasswordComponent

/dashboard               → DashboardComponent        [authGuard]
/profile                 → ProfileComponent           [authGuard]

/admin/users             → UserListComponent          [authGuard + ViewAccess/USER]
/admin/users/:id         → UserDetailComponent        [authGuard + ViewAccess/USER]
/admin/roles             → RoleListComponent          [authGuard + ViewAccess/ROLE]
/admin/roles/:id         → RoleDetailComponent        [authGuard + ViewAccess/ROLE]
/admin/mail-setting      → MailSettingComponent       [authGuard + ViewAccess/SETTING]
/admin/logs              → LogsListComponent          [authGuard + ViewAccess/SETTING]
/admin/backups           → BackupListComponent        [authGuard + ViewAccess/SETTING]
/admin/audit-logs        → AuditLogListComponent      [authGuard + ViewAccess/SETTING]

/project-management/**   → (project-management.routes)
```

All routes under the shell (`/dashboard`, `/profile`, `/admin/**`, `/project-management/**`) are children of `HomeComponent` (the main layout with sidebar + header).

### `authGuard`

Located at `src/app/models/route-gaurds/auth.guard.ts`. It verifies:
1. A valid JWT token exists.
2. (Optionally) The required `data.permission` bit is set for `data.permissionFor` entity.

If either check fails, the user is redirected to `/auth/login`.

### Adding a Protected Route

```typescript
// In your feature's routes file:
{
  path: 'invoices',
  canActivate: [authGuard],
  data: {
    permission: EnumPermissions.ViewAccess,
    permissionFor: EnumPermissionFor.PROJECT,  // add your entity to this enum
  },
  loadComponent: () =>
    import('./components/invoice-list/invoice-list.component')
      .then(m => m.InvoiceListComponent),
}
```

---

## 6. HTTP Layer

### API Base URL

```typescript
// Defined once in BaseService:
public static API_V1 = `${environment.Setting.apiServiceUrl}/api/v1`;

public static readonly ApiUrls = {
  Auth: `${BaseService.API_V1}/Auth`,
  Account: `${BaseService.API_V1}/Account`,
  Role: `${BaseService.API_V1}/Role`,
  User: `${BaseService.API_V1}/User`,
  Setting: `${BaseService.API_V1}/Setting`,
  Project: `${BaseService.API_V1}/Project`,
  Task: `${BaseService.API_V1}/Task`,
};
```

Add a new endpoint by appending a key here when creating a new feature.

### HTTP Interceptors (applied in order)

| Interceptor | File | Responsibility |
|---|---|---|
| `jwtInterceptor` | `core/interceptors/jwt.interceptor.ts` | Attaches `Authorization: Bearer <token>` header if token is valid; also resets idle timer |
| `errorInterceptor` | `core/interceptors/error.interceptor.ts` | Catches `HttpErrorResponse`, formats it into `ApiError`, logs it, auto-clears token on 401 |
| `loadingInterceptor` | `core/interceptors/loading.interceptor.ts` | Updates `LoadingService` so global spinner shows/hides automatically |

### Response Wrappers

All API responses are expected to follow a typed envelope:

```typescript
interface ApiOkResponse<T> {
  data: T;
  // … status/message fields
}

interface ApiCreatedResponse<T> { … }
interface ApiError { statusCode, errorMessage, eventMessageId, errorDetail }
```

`CommonService.extractOkResponse()` / `extractCreateResponse()` handle the unwrapping and throw `ApiError` on failure.

---

## 7. State Management (NgRx)

The app configures NgRx in `app.config.ts`:

```typescript
provideStore(),
provideEffects(),
```

Feature states are registered lazily in each feature module. The template includes an example in `base-list-store` and `column-sorter-store` shared components.

### Conventions

- **Store per feature slice** — keep actions, reducers, selectors, and effects co-located inside `features/<name>/store/`.
- Prefer **signals** (`signal()`, `computed()`) for local/UI-only state; use NgRx only for shared, server-synchronised state.

---

## 8. Authentication & Authorization

### Token Lifecycle

1. `AuthService` (`features/auth/services/auth.service.ts`) handles login/logout and stores the JWT in a way accessible via `authService.getCurrentToken`.
2. The token exposes `isValid()` and `token()` methods — the JWT interceptor calls both before attaching the header.
3. On 401, `errorInterceptor` calls `authService.clearToken()`, which should navigate the user to the login page.

### Permission System

Permissions are **bit-flag** based:

```typescript
enum EnumPermissions {
  None            = 0x00,
  ViewAccess      = 0x01,
  UpdateAccess    = 0x02,
  CreateAccess    = 0x04,
  DeleteAccess    = 0x08,
  ApproveAccess   = 0x10,
  CURDAccess      = 0x0F,  // View | Update | Create | Delete
}
```

Each permission is scoped to an entity via `EnumPermissionFor`:

```typescript
enum EnumPermissionFor { USER = 1, ROLE = 2, SETTING = 3, Project = 4, Task = 5 }
```

#### Checking Permissions in a Component

```typescript
// Inject via BaseService or directly
const permission = this.permissionService.getEvaluatedPermission(rolePermissionDto);
permission.viewAccess   // boolean
permission.createAccess // boolean

// Or as Observable
this.permissionService.isGranted(EnumPermissionFor.Project, EnumPermissions.CreateAccess)
  .subscribe(allowed => { … });
```

`GlobalAdministrator` and `EnterpriseAdministrator` users automatically pass all permission checks.

#### Adding a New Permission Entity

1. Add a value to `EnumPermissionFor` in `shared/models/common-enums.ts`.
2. Add the corresponding `ApiUrls` entry in `BaseService.ApiUrls`.
3. Protect your route with `data: { permission, permissionFor }`.

---

## 9. Design System & Styling

The SCSS design system lives in `src/styles/` and is imported by `src/styles.scss` in a fixed order:

```
1. fonts.scss              — Local Roboto font faces
2. _tokens.scss            — Design tokens (spacing, typography, shadows)
3. themes.scss             — 7 theme class definitions + base component styles
4. _bootstrap-overrides.scss — Maps tokens to Bootstrap CSS custom properties
5. _utilities.scss         — Custom spacing/layout utility classes
6. Third-party overrides   — ng-select
7. styles.scss (inline)    — SVG icon utilities, scrollbar styling
```

> **Never change the import order.** Each layer depends on the previous one.

### Design Tokens (`_tokens.scss`)

All tokens are exposed as both **SCSS variables** (compile-time) and **CSS custom properties** (runtime, theme-switchable):

#### Spacing — 8px Grid

| Token | Value | Use |
|---|---|---|
| `--space-0` | 0 | Reset |
| `--space-1` | 4px | Micro (icon-text gaps) |
| `--space-2` | 8px | Base (compact padding) |
| `--space-3` | 16px | Standard (component padding) |
| `--space-4` | 24px | Medium (section gaps) |
| `--space-5` | 32px | Large (section separation) |
| `--space-6` | 48px | XL (page margins) |

**Always use the 8px grid.** Do not introduce arbitrary pixel values.

#### Border Radius

| Token | Value |
|---|---|
| `--radius-sm` | 4px — buttons, chips |
| `--radius-md` | 8px — cards, dialogs |
| `--radius-lg` | 12px — large cards, modals |
| `--radius-xl` | 16px — feature cards |
| `--radius-full` | 9999px — pills, avatars |

#### Typography

| Token | Value | Use |
|---|---|---|
| `--font-size-xs` | 11px | Captions |
| `--font-size-sm` | 12px | Table headers |
| `--font-size-base` | 14px | Body (default) |
| `--font-size-md` | 16px | Emphasised body |
| `--font-size-lg` | 18px | Subheadings |
| `--font-size-xl` | 22px | Page titles |
| `--font-size-2xl` | 28px | Hero headings |

Font family: **Roboto** (local, loaded from `public/fonts`).

#### Layout Tokens

| Token | Value |
|---|---|
| `--sidebar-width` | 240px |
| `--sidebar-width-collapsed` | 72px |
| `--header-height` | 56px |
| `--container-max-width` | 1400px |
| `--page-gutter` | 16px / 24px / 32px (responsive) |

#### Elevation (Shadows)

Use `--elevation-1` through `--elevation-5` to create depth. Each theme overrides them appropriately (e.g., dark mode uses stronger shadows).

### Utility Classes (`_utilities.scss`)

The project has a custom 8px-grid utility layer. Use these instead of Bootstrap's spacing utilities where you need to guarantee grid adherence:

```html
<!-- Spacing -->
<div class="p-2">   8px padding  </div>
<div class="mt-3">  16px margin-top </div>
<div class="gap-2"> 8px gap in flex/grid </div>
```

---

## 10. Theming

### Available Themes

| Class | Name | Description |
|---|---|---|
| `.theme-light` | Light ☀️ | Default. White background, Google Blue primary |
| `.theme-dark` | Dark 🌙 | Dark #1f1f1f background, muted blue accents |
| `.theme-blue` | Teal 💎 | White background, Teal/Cyan primary |
| `.theme-glass` | Glass 🔮 | Purple gradient bg, frosted glass surfaces |
| `.theme-bold` | Bold 🎨 | Light bg, vibrant purple gradient, large radius |
| `.theme-soft` | Soft ☁️ | Neumorphic soft grey, embossed shadows |
| `.theme-corporate` | Corporate 💼 | Navy blue sidebar, professional grey surfaces |

The active theme class is applied to `<body>` at runtime by `ThemeService`.

### How Theming Works

Every theme redefines the same set of CSS custom properties:

```css
--bg          /* page background */
--surface     /* card/panel background */
--text        /* primary text */
--text-secondary
--primary     /* brand/interactive colour */
--primary-hover
--primary-light
--border, --divider
--hover-bg
--elevation-1 … --elevation-4
--sidebar-bg, --sidebar-active-bg, --sidebar-active-text
--input-bg, --input-border, --input-focus-border
```

**Rule:** Always style new components using these CSS variables, never hardcode colours.

```scss
// ✅ Correct
.my-card {
  background-color: var(--surface);
  border: 1px solid var(--border);
  color: var(--text);
}

// ❌ Wrong
.my-card {
  background-color: #ffffff;
  color: #202124;
}
```

### Switching Themes Programmatically

```typescript
import { ThemeService, Theme } from '@core/services/theme.service';

// In any component:
themeService = inject(ThemeService);

setTheme(t: Theme) {
  this.themeService.setTheme(t);  // persists to localStorage
}

// Read the current theme (Angular signal):
activeTheme = this.themeService.activeTheme; // Signal<Theme>
```

### Adding a Custom Theme

1. Open `src/styles/themes.scss`.
2. Add a new class block at the end:

```scss
.theme-myapp {
  --bg: #f0f4f8;
  --surface: #ffffff;
  --text: #1e293b;
  --primary: #0ea5e9;
  --primary-rgb: 14, 165, 233;
  --primary-hover: #0284c7;
  --primary-light: #e0f2fe;
  // … (copy all vars from an existing theme and adjust)
}
```

3. Add the theme to `ThemeService`:

```typescript
// theme.service.ts
export type Theme = 'light' | 'dark' | 'blue' | ... | 'myapp';

export const THEME_OPTIONS: ThemeOption[] = [
  // existing options …
  { id: 'myapp', name: 'My App', icon: '✨' }
];
```

---

## 11. Shared Library

### Base Model Hierarchy

```
BaseDto                          (id?, $selected?)
 └─ BaseDtoWithCommonFields      (+ createdOn, modifiedOn, disabled …)
     ├─ BaseDtoWithAddress       (+ addressLine1 … lat, lng, phone …)
     └─ BaseDtoWithNameDescription (+ name, description)
```

Extend the appropriate base class for new entity DTOs.

### `BaseService<A>` — The Generic CRUD Service

All feature services extend `BaseService<EntityDto>`. It provides:

| Method | HTTP | Description |
|---|---|---|
| `getRecord(id)` | GET `/{id}` | Fetch single record |
| `updateRecord(id, record)` | POST or PUT | Create (id=0) or update |
| `deleteRecord(id)` | DELETE `/{id}` | Delete record |
| `enableDisableRecord(id, isDisable)` | POST | Archive/restore |
| `getList<T>()` | GET | Paginated list |
| `lookUpList<T>()` | GET `/lookup-list` | All records (for dropdowns) |
| `getResponse<T>(base, sub)` | GET | Generic GET |
| `postResponse<T,U>(…)` | POST | Generic POST |
| `putResponse<T,U>(…)` | PUT | Generic PUT |
| `patchResponse<T,U>(…)` | PATCH | Generic PATCH |
| `deleteResponse(…)` | DELETE | Generic DELETE |

#### Creating a Feature Service

```typescript
import { Injectable, Inject } from '@angular/core';
import { BaseService } from '@shared/services/base.service';
import { InvoiceDto } from '../models/invoice-dto';

@Injectable({ providedIn: 'root' })
export class InvoiceService extends BaseService<InvoiceDto> {
  constructor() {
    super(BaseService.ApiUrls.Invoice);  // add key to ApiUrls first
  }
}
```

### `SourceInfo` — List State Object

`SourceInfo` is the single state object powering all list screens:

```typescript
class SourceInfo {
  list: any[];
  filters: FilterModel[];
  filterChanged: Subject<any>;     // emit to trigger API reload
  pagination: Pagination;          // { pageNo, pageSize, totalRecords }
  gridDataStatus: DataStatusEnum;  // Fetching | Loaded | Error | Empty
  sortingState: SortingModel;
  sortQuery: string;               // built by ListHelper; appended to API URL
  filterQuery: string;
  paginationQuery: string;
}
```

Use `ListHelper` static methods to drive filter/sort/paginate, which all ultimately call `sourceInfo.filterChanged.next('')` to reload data.

### Shared Components

| Component | Purpose |
|---|---|
| `BaseListComponent` | Base for all list pages (wires up `SourceInfo`, pagination, column sort) |
| `BaseListStoreComponent` | Same, but driven by NgRx |
| `BaseDetailComponent` | Base for form/detail pages |
| `BaseDetailPopupComponent` | Same inside an ng-bootstrap modal |
| `ColumnSorterComponent` | Sortable table header cell |
| `ColumnSorterStoreComponent` | Same, NgRx variant |
| `ConfirmDialogPopupComponent` | Generic delete/warning confirmation modal |
| `LoadingComponent` | Full-screen loading overlay |
| `ToasterComponent` | Toast notification container |
| `ThemeSwitcherComponent` | UI to cycle through all themes |
| `ErrorDetailComponent` | Displays API error details |

### Directives

| Directive | Selector | Purpose |
|---|---|---|
| `SvgIconDirective` | `[appSvgIcon]` | Inlines SVG icons by name from the icon sprite |
| `FocusFirstInvalidFieldDirective` | `[appFocusFirstInvalidField]` | On form submit, auto-scrolls to and focuses the first invalid control |

### SVG Icons

Icons are managed via `SvgIconDirective`. The icon set lives in `src/styles/themes.scss` (and the SVG library) and provides the complete Material Symbols set baked in. Usage:

```html
<i appSvgIcon="home"></i>
<i appSvgIcon="settings" class="crm-svg-sm"></i>
```

Size helper classes:

| Class | Icon size |
|---|---|
| `.crm-svg` | 1.25rem (default) |
| `.crm-svg-sm` | 1rem |
| `.crm-svg-lg` | 1.5rem |

Wrap with `.crm-svg` for the hover-circle button style:

```html
<span class="crm-svg">
  <i appSvgIcon="edit"></i>
</span>
```

---

## 12. Core Services

### `ThemeService`

Manages the active UI theme. See [Section 10](#10-theming).

### `LoggerService`

Structured logging with severity levels, colour-coded console output, and optional remote logging.

```typescript
logger = inject(LoggerService);

this.logger.debug('Fetching users …');
this.logger.info('User logged in', { userId });
this.logger.warn('Token about to expire');
this.logger.error('Failed to load data', error);
```

- In development builds (`!environment.production`) the level is set to **Debug** (all messages shown).
- In production the level is **Info** (debug messages suppressed).
- If `environment.Setting.remoteLogUrl` is set, `Error`-level messages are POSTed to that endpoint automatically.

### `ToastService` (core)

Low-level toast primitive used internally. Prefer the `BaseService` convenience wrappers:

```typescript
// Inside a service that extends BaseService:
this.showSuccessIdToast('USER_SAVED_SUCCESS');
this.showApiErrorToast(apiError);

// Generic:
this.showSuccessToast('Saved!', 'Your changes were saved.');
this.showErrorToast('Failed', error.message);
```

### `LoadingService`

The `loadingInterceptor` automatically increments/decrements a counter. The `LoadingComponent` subscribes and shows/hides the spinner. No manual invocation needed.

### `GlobalErrorHandler`

Replaces Angular's default `ErrorHandler`. Logs all uncaught errors via `LoggerService`.

### `NavigationService`

Manages sidebar state and active-link detection:

```typescript
nav = inject(NavigationService);

nav.toggleSideNav();              // collapse/expand sidebar
nav.isMobileView()                // Signal<boolean>
nav.hideSideNav()                 // Signal<boolean>
nav.getSideBarMenu()              // MenuItem[]
```

Mobile view (`< 768px`) auto-collapses the sidebar on init.

---

## 13. Feature Modules

### `auth` Feature

| Route | Component |
|---|---|
| `/auth/login` | `LoginComponent` |
| `/auth/forgot-password` | `ForgotPasswordComponent` |
| `/auth/forgot-password-confirm` | `ForgotPasswordConfirmComponent` |
| `/auth/set-password` | `SetPasswordComponent` |

Auth services are in `features/auth/services/`. `AuthService` is the primary entry point used by interceptors and guards.

### `master` Feature

Houses the main **app shell**: `HomeComponent` (sidebar + header wrapper), `DashboardComponent`, and `ProfileComponent`.

### `admin` Feature

CRUD screens for platform administration:

- **Users** — manage application users, invite, set passwords, assign roles
- **Roles** — define role names and bit-flag permissions per entity
- **Mail Setting** — SMTP configuration
- **Logs** — application log viewer
- **Backups** — database backup management
- **Audit Logs** — user action trail

### `project-management` Feature

Domain feature demonstrating the pattern: Projects + Tasks with full CRUD.

---

## 14. Adding a New Feature

Follow this checklist when creating a new feature module.

### Step 1 — Create the folder structure

```
src/app/features/my-feature/
  ├── components/
  │   ├── my-list/
  │   └── my-detail/
  ├── models/
  │   └── my-feature-dto.ts
  ├── services/
  │   └── my-feature.service.ts
  └── my-feature.routes.ts
```

### Step 2 — Create the DTO

```typescript
// models/my-feature-dto.ts
import { BaseDtoWithCommonFields } from '@shared/models/base-model';

export class MyFeatureDto extends BaseDtoWithCommonFields {
  name?: string;
  description?: string;
}
```

### Step 3 — Register the API URL

```typescript
// shared/services/base.service.ts — ApiUrls object
public static readonly ApiUrls = {
  // … existing
  MyFeature: `${BaseService.API_V1}/MyFeature`,
};
```

### Step 4 — Create the Service

```typescript
@Injectable({ providedIn: 'root' })
export class MyFeatureService extends BaseService<MyFeatureDto> {
  constructor() {
    super(BaseService.ApiUrls.MyFeature);
  }
}
```

### Step 5 — Add to `EnumPermissionFor` (if permission-controlled)

```typescript
// shared/models/common-enums.ts
export enum EnumPermissionFor {
  // … existing
  MY_FEATURE = 6,
}
```

### Step 6 — Create the Routes File

```typescript
// my-feature.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from '../../models/route-gaurds/auth.guard';
import { EnumPermissionFor, EnumPermissions } from '@shared/models/common-enums';

export const myFeatureRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    data: { permission: EnumPermissions.ViewAccess, permissionFor: EnumPermissionFor.MY_FEATURE },
    loadComponent: () =>
      import('./components/my-list/my-list.component').then(m => m.MyListComponent),
  },
  {
    path: ':id',
    canActivate: [authGuard],
    data: { permission: EnumPermissions.ViewAccess, permissionFor: EnumPermissionFor.MY_FEATURE },
    loadComponent: () =>
      import('./components/my-detail/my-detail.component').then(m => m.MyDetailComponent),
  },
];
```

### Step 7 — Register in Root Routes

```typescript
// app.routes.ts — inside the HomeComponent children:
{
  path: 'my-feature',
  canActivate: [authGuard],
  loadChildren: () =>
    import('./features/my-feature/my-feature.routes').then(r => r.myFeatureRoutes),
}
```

### Step 8 — Add to Sidebar Menu

Open `src/app/shared/models/menu-helper.ts` and add a `MenuItem` entry with your route link and icon name.

---

## 15. Internationalisation (i18n)

The app uses `@ngx-translate/core` with JSON translation files.

### Translation Files

Translation files live under `public/i18n/` (e.g., `en.json`, `tr.json`). Keys should be `SCREAMING_SNAKE_CASE`.

```json
{
  "BUTTON_SAVE":   "Save",
  "BUTTON_CANCEL": "Cancel",
  "USER_SAVED_SUCCESS": "User saved successfully."
}
```

### Using Translations

In templates:

```html
{{ 'BUTTON_SAVE' | translate }}
<button [title]="'BUTTON_CANCEL' | translate">…</button>
```

In TypeScript (via `AppTranslateService` or `TranslateService`):

```typescript
translateService = inject(TranslateService);
const label = this.translateService.instant('BUTTON_SAVE');
```

### Changing Language at Runtime

```typescript
appTranslate = inject(AppTranslateService);
appTranslate.setLanguage('tr');
```

---

## 16. Logging

`LoggerService` is the only logging mechanism — **never use `console.log` directly** in production code.

```typescript
logger = inject(LoggerService);

// Use appropriate level:
this.logger.debug('verbose — dev only');
this.logger.info('normal operation info');
this.logger.warn('unexpected but recoverable situation');
this.logger.error('failure that needs attention', errorObject);
```

### Remote Logging

Set `remoteLogUrl` in the environment file to a valid endpoint. All `Error`-level entries will be POSTed as:

```json
{
  "level": "Error",
  "message": "…",
  "timestamp": "2026-01-01T00:00:00.000Z",
  "additionalDetails": [/* extra args */]
}
```

---

## 17. Environment Configuration

Two files control environment settings:

| File | When used |
|---|---|
| `src/environments/environment.ts` | Production builds (`ng build`) |
| `src/environments/environment.development.ts` | Development (`ng serve`) |

`angular.json` swaps the files via `fileReplacements` during `development` configuration.

### `IEnvironmentSetting` Interface

```typescript
interface IEnvironmentSetting {
  rootURL: string;           // Application root path
  apiServiceUrl: string;     // Backend API base URL (no trailing slash)
  captchaKey: string;        // Google reCAPTCHA v2 site key
  remoteLogUrl?: string;     // Optional: where to POST error logs
}
```

`environment.production` (`boolean`) controls `LogLevel` — `Debug` in dev, `Info` in production.

---

## 18. Testing

### Unit Tests (Karma/Jasmine)

```bash
npm test                   # interactive watch mode
npm test -- --watch=false  # single run (CI)
```

Test files follow the pattern `*.spec.ts` alongside the source file.

```typescript
// Example:
describe('MyFeatureService', () => {
  let service: MyFeatureService;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [MyFeatureService] });
    service = TestBed.inject(MyFeatureService);
  });
  it('should be created', () => expect(service).toBeTruthy());
});
```

### E2E Tests (Cypress)

```bash
npm run e2e       # open Cypress Test Runner (interactive)
npm run e2e:run   # headless run (CI)
```

Cypress tests live under `cypress/e2e/`. Configuration is in `cypress.config.ts`.

---

## 19. Common Patterns & Conventions

### Component File Naming

Angular CLI schematics are configured to produce:
- `my-component.component.ts` (suffix: `component`)
- `my-directive.directive.ts` (suffix: `directive`)
- `my.service.ts` (suffix: `service`)
- `my.guard.ts` / `my.interceptor.ts` / `my.pipe.ts` / `my.resolver.ts`

### Path Aliases

`tsconfig.json` defines the path alias `@shared` → `src/app/shared`:

```typescript
import { BaseService } from '@shared/services/base.service';
import { EvaluatedPermission } from '@shared/models/base-model';
```

### Form Guidelines

- Use **Reactive Forms** (`ReactiveFormsModule`) — not Template-driven.
- Apply `appFocusFirstInvalidField` directive on the `<form>` element so the first invalid field is focused on submit.

```html
<form [formGroup]="form" (ngSubmit)="onSubmit()" appFocusFirstInvalidField>
  …
</form>
```

### List Page Pattern

All list pages should:
1. Declare a `SourceInfo` instance.
2. Subscribe to `sourceInfo.filterChanged` to call the API.
3. Use `ListHelper.onFilter()`, `ListHelper.onSorting()`, `ListHelper.pageChanged()` to mutate state — these all trigger a reload automatically.

### Confirm Delete Pattern

```typescript
const dlgRef = this.modal.open(ConfirmDialogPopupComponent);
dlgRef.componentInstance.setting = new ConfirmDlgSetting(
  'TITLE_WARNING',
  'DELETE_CONFIRM',
);
dlgRef.result.then((ok) => {
  if (ok) this.myService.deleteRecord(id).subscribe(…);
});
```

### Bootstrap Overrides

Do **not** override Bootstrap variables directly in component SCSS files. All Bootstrap customisations belong in `src/styles/_bootstrap-overrides.scss`, expressed as remaps of existing tokens.

---

*Last updated: February 2026*
