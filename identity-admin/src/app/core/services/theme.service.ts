import { Injectable, signal, effect } from '@angular/core';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'identity_admin_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly theme = signal<Theme>(this.loadTheme());

  currentTheme = this.theme.asReadonly();

  constructor() {
    effect(() => {
      const t = this.theme();
      this.applyTheme(t);
    });
  }

  setTheme(theme: Theme): void {
    this.theme.set(theme);
    try {
      localStorage.setItem(STORAGE_KEY, theme);
    } catch {}
  }

  toggleTheme(): void {
    this.setTheme(this.theme() === 'light' ? 'dark' : 'light');
  }

  private loadTheme(): Theme {
    try {
      const stored = localStorage.getItem(STORAGE_KEY) as Theme | null;
      if (stored === 'light' || stored === 'dark') return stored;
      if (typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches)
        return 'dark';
    } catch {}
    return 'light';
  }

  private applyTheme(theme: Theme): void {
    if (typeof document === 'undefined') return;
    const doc = document.documentElement;
    doc.classList.remove('light', 'dark');
    doc.classList.add(theme);
  }
}
