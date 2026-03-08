import { Injectable, signal, effect } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const STORAGE_KEY = 'aml_admin_settings';

const FONT_SIZE_MIN = 12;
const FONT_SIZE_MAX = 24;

export interface UserSettings {
  theme: 'light' | 'dark';
  fontSizePx: number;
  fontColor: string;
  language: string;
}

const DEFAULT_SETTINGS: UserSettings = {
  theme: 'light',
  fontSizePx: 16,
  fontColor: '',
  language: 'en'
};

function clampFontSizePx(px: number): number {
  return Math.min(FONT_SIZE_MAX, Math.max(FONT_SIZE_MIN, px));
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly settings = signal<UserSettings>(this.loadSettings());

  currentSettings = this.settings.asReadonly();

  constructor(private translate: TranslateService) {
    effect(() => {
      const s = this.settings();
      this.applyDirection(s.language);
      this.applyCssVars(s.fontSizePx, s.fontColor);
    });
  }

  setSettings(partial: Partial<UserSettings>): void {
    this.settings.update((prev) => {
      const next: UserSettings = {
        ...prev,
        ...partial,
        fontSizePx: partial.fontSizePx != null ? clampFontSizePx(partial.fontSizePx) : prev.fontSizePx
      };
      this.persist(next);
      return next;
    });
  }

  setLanguage(lang: string): void {
    this.translate.use(lang);
    this.setSettings({ language: lang });
  }

  setFontSizePx(px: number): void {
    this.setSettings({ fontSizePx: clampFontSizePx(px) });
  }

  setFontColor(color: string): void {
    this.setSettings({ fontColor: color ?? '' });
  }

  setTheme(theme: 'light' | 'dark'): void {
    this.setSettings({ theme });
  }

  private loadSettings(): UserSettings {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as Record<string, unknown>;
        const migrated = migrateFromOldFormat(parsed);
        return { ...DEFAULT_SETTINGS, ...migrated, fontSizePx: clampFontSizePx((migrated.fontSizePx as number) ?? DEFAULT_SETTINGS.fontSizePx) };
      }
    } catch {}
    return { ...DEFAULT_SETTINGS };
  }

  private persist(s: UserSettings): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(s));
    } catch {}
  }

  private applyDirection(lang: string): void {
    if (typeof document === 'undefined') return;
    const dir = lang === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', dir);
    document.documentElement.setAttribute('lang', lang);
  }

  private applyCssVars(fontSizePx: number, fontColor: string): void {
    if (typeof document === 'undefined') return;
    const doc = document.documentElement;
    doc.style.setProperty('--font-size-base', `${clampFontSizePx(fontSizePx)}px`);
    if (fontColor && fontColor.trim()) doc.style.setProperty('--user-font-color', fontColor.trim());
    else doc.style.removeProperty('--user-font-color');
  }
}

function migrateFromOldFormat(parsed: Record<string, unknown>): Partial<UserSettings> {
  const result: Partial<UserSettings> = {};
  const theme = parsed['theme'];
  if (theme === 'light' || theme === 'dark') result.theme = theme;
  const language = parsed['language'];
  if (typeof language === 'string') result.language = language;
  const fontSizePx = parsed['fontSizePx'];
  if (typeof fontSizePx === 'number') result.fontSizePx = fontSizePx;
  const fontColor = parsed['fontColor'];
  if (typeof fontColor === 'string') result.fontColor = fontColor;
  return result;
}
