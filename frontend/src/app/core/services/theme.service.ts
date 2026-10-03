import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark' | 'blue' | 'glass' | 'bold' | 'soft' | 'corporate' | 'navy';

export interface ThemeOption {
    id: Theme;
    name: string;
    icon: string;
}

export const THEME_OPTIONS: ThemeOption[] = [
    { id: 'light', name: 'Light', icon: '☀️' },
    { id: 'dark', name: 'Dark', icon: '🌙' },
    { id: 'blue', name: 'Teal', icon: '💎' },
    { id: 'glass', name: 'Glass', icon: '🔮' },
    { id: 'bold', name: 'Bold', icon: '🎨' },
    { id: 'soft', name: 'Soft', icon: '☁️' },
    { id: 'corporate', name: 'Corporate', icon: '💼' },
    { id: 'navy', name: 'Navy', icon: '⚓' }
];

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    private readonly THEME_KEY = 'selected-theme';
    private readonly ALL_THEMES: Theme[] = ['light', 'dark', 'blue', 'glass', 'bold', 'soft', 'corporate', 'navy'];

    // Signal to hold the current theme
    activeTheme = signal<Theme>('navy');

    // Available theme options
    themeOptions = THEME_OPTIONS;

    constructor() {
        this.loadTheme();
    }

    private loadTheme() {
        const savedTheme = localStorage.getItem(this.THEME_KEY) as Theme;
        if (savedTheme && this.ALL_THEMES.includes(savedTheme)) {
            this.setTheme(savedTheme);
        } else {
            this.setTheme('navy');
        }
    }

    setTheme(theme: Theme) {
        this.activeTheme.set(theme);
        localStorage.setItem(this.THEME_KEY, theme);
        this.updateBodyClass(theme);
    }

    private updateBodyClass(theme: Theme) {
        // Remove all theme classes
        this.ALL_THEMES.forEach(t => {
            document.body.classList.remove(`theme-${t}`);
        });
        // Add the active theme class
        document.body.classList.add(`theme-${theme}`);
    }
}
