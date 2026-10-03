import { Component, inject } from '@angular/core';

import { RouterOutlet } from '@angular/router';
import { AppTranslateService } from '@shared/services/app-translate.service';
import { ToasterComponent } from '@shared/components/toaster/toaster.component';
import { ThemeService } from './core/services/theme.service';

import { CommonModule } from '@angular/common';

import { ThemeSwitcherComponent } from '@shared/components/theme-switcher/theme-switcher.component';

@Component({
  selector: 'crm-root',
  imports: [RouterOutlet, ToasterComponent, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'crm-app';

  private _translate = inject(AppTranslateService);
  private _themeService = inject(ThemeService);

  constructor() {
  }
}
