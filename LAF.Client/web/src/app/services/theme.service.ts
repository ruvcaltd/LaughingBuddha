import { Injectable, signal, computed, effect } from '@angular/core';
import themes from "devextreme/ui/themes";

export type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'laf_theme';
  
  private themeSignal = signal<Theme>(this.getInitialTheme());
  
  public theme = computed(() => this.themeSignal());
  public isDark = computed(() => this.themeSignal() === 'dark');
  
  // Effect for theme changes
  private themeChangeEffect = effect(() => {
    // This will run whenever themeSignal changes
    this.themeSignal();
  });
  
  constructor() {
    this.applyTheme(this.themeSignal());
  }
  
  private getInitialTheme(): Theme {
    const saved = localStorage.getItem(this.THEME_KEY);
    if (saved && (saved === 'light' || saved === 'dark')) {
      return saved;
    }
    
    // Check system preference
    if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    
    return 'light';
  }
  
  toggleTheme(): void {
    const newTheme = this.themeSignal() === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }
  
  setTheme(theme: Theme): void {
    this.themeSignal.set(theme);
    localStorage.setItem(this.THEME_KEY, theme);
    this.applyTheme(theme);
  }
  
  private applyTheme(theme: Theme): void {
    const body = document.body;
    if (theme === 'dark') {
      body.classList.add('dark');

    } else {
      body.classList.remove('dark');

    }
  }
}

