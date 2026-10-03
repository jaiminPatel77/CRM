import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, Theme, THEME_OPTIONS } from '../../../core/services/theme.service';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'crm-theme-switcher',
  standalone: true,
  imports: [CommonModule, NgbDropdownModule, TranslateModule],
  template: `
    <div ngbDropdown class="d-inline-block">
      <button type="button" class="btn btn-sm btn-outline-secondary d-flex align-items-center gap-2" 
              id="themeDropdown" ngbDropdownToggle>
        <span>{{ getCurrentThemeIcon() }}</span>
        <span class="d-none d-md-inline">Theme</span>
      </button>
      <div ngbDropdownMenu aria-labelledby="themeDropdown" class="theme-menu">
        @for(theme of themeOptions; track theme.id) {
          <button ngbDropdownItem 
                  (click)="switchTheme(theme.id)" 
                  [class.active]="activeTheme() === theme.id"
                  class="d-flex align-items-center gap-2">
            <span>{{ theme.icon }}</span>
            <span>{{ theme.name }}</span>
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    .theme-menu {
      min-width: 140px;
    }
    
    .active {
      background-color: var(--primary-light, #e8f0fe);
      color: var(--primary, #1a73e8);
    }
    
    button.active:hover {
      background-color: var(--primary-light, #e8f0fe);
    }
  `]
})
export class ThemeSwitcherComponent {
  themeService = inject(ThemeService);
  activeTheme = this.themeService.activeTheme;
  themeOptions = THEME_OPTIONS;

  switchTheme(theme: Theme) {
    this.themeService.setTheme(theme);
  }

  getCurrentThemeIcon(): string {
    const current = this.themeOptions.find(t => t.id === this.activeTheme());
    return current?.icon || '☀️';
  }
}
