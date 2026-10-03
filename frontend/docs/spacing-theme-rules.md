# Spacing & Theme Rules

## Design Tokens Summary

### Spacing (8px Grid)

| Token      | Value | Usage                              |
|------------|-------|-------------------------------------|
| `--space-0`| 0     | Reset, no spacing                   |
| `--space-1`| 4px   | Micro: icon-text gaps              |
| `--space-2`| 8px   | Base unit: compact padding         |
| `--space-3`| 16px  | Standard: component padding        |
| `--space-4`| 24px  | Medium: section gaps               |
| `--space-5`| 32px  | Large: major section separations   |
| `--space-6`| 48px  | XL: page margins, hero spacing     |

### Border Radius

| Token          | Value   | Usage                    |
|----------------|---------|--------------------------|
| `--radius-sm`  | 4px     | Buttons, inputs, chips   |
| `--radius-md`  | 8px     | Cards, dialogs           |
| `--radius-lg`  | 12px    | Large cards, modals      |
| `--radius-xl`  | 16px    | Extra large containers   |
| `--radius-full`| 9999px  | Pills, avatars, circles  |

### Elevation / Shadows

| Token           | Usage                          |
|-----------------|--------------------------------|
| `--elevation-1` | Subtle hover states            |
| `--elevation-2` | Cards, dropdowns               |
| `--elevation-3` | Modals, menus                  |
| `--elevation-4` | Dialog overlays                |

---

## Bootstrap Utility Mapping

| Token-based Class | Bootstrap Equivalent | Value  |
|-------------------|---------------------|--------|
| `.p-2`            | padding             | 8px    |
| `.p-3`            | padding             | 16px   |
| `.gap-2`          | gap                 | 8px    |
| `.gap-3`          | gap                 | 16px   |
| `.rounded`        | border-radius       | 8px    |
| `.rounded-sm`     | border-radius       | 4px    |

---

## Theme Color Tokens

The project supports **7 active themes**. Below are the primary tokens for the core 3. For `glass`, `bold`, `soft`, and `corporate`, refer to `src/styles/themes.scss`.

| Token              | Light        | Dark         | Teal (Blue)  |
|--------------------|--------------|--------------|--------------|
| `--bg`             | #ffffff      | #1f1f1f      | #ffffff      |
| `--surface`        | #ffffff      | #292929      | #ffffff      |
| `--text`           | #202124      | #e8eaed      | #202124      |
| `--text-secondary` | #5f6368      | #9aa0a6      | #5f6368      |
| `--border`         | #dadce0      | #444444      | #b2ebf2      |
| `--primary`        | #1a73e8      | #8ab4f8      | #00bcd4      |
| `--hover-bg`       | #f1f3f4      | #3c3c3c      | #e0f7fa      |

**Other Active Themes:** `glass`, `bold`, `soft`, `corporate`.

---

## Component Spacing Rules

| Component      | Horizontal | Vertical | Notes                |
|----------------|------------|----------|----------------------|
| Page container | 16-32px    | 16px     | Responsive gutters   |
| Cards          | 16px       | 16px     | Internal padding     |
| Table cells    | 16px       | 8-12px   | Balanced density     |
| Buttons        | 16px       | 8px      | Min height 36px      |
| Form inputs    | 12px       | 8px      | Min height 40px      |

---

## Usage Examples

```html
<!-- Card with proper spacing -->
<div class="card p-3 rounded-md elevation-2">
  <h4 class="mb-2">Title</h4>
  <p class="text-secondary">Content</p>
</div>

<!-- Button group with gaps -->
<div class="d-flex gap-2">
  <button class="btn btn-primary">Save</button>
  <button class="btn btn-secondary">Cancel</button>
</div>

<!-- Icon button -->
<button class="icon-btn">
  <i crmSvgIcon="crm_plus"></i>
</button>
```
