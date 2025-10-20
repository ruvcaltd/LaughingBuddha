import { Component, inject } from '@angular/core';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  template: `
    <button
      (click)="themeService.toggleTheme()"
      class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 dark:focus:ring-offset-gray-800"
      [class]="themeService.isDark() ? 'bg-primary-600' : 'bg-gray-200'"
      role="switch"
      [attr.aria-checked]="themeService.isDark()"
    >
      <span class="sr-only">Toggle dark mode</span>
      <span
        class="inline-block h-4 w-4 transform rounded-full bg-white transition-transform"
        [class]="themeService.isDark() ? 'translate-x-6' : 'translate-x-1'"
      ></span>
    </button>
  `,
})
export class ThemeToggleComponent {
  protected readonly themeService = inject(ThemeService);
}

