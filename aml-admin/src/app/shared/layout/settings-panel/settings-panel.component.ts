import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SettingsPanelService } from '../../../core/services/settings-panel.service';
import { SettingsService } from '../../../core/services/settings.service';
import { ThemeService } from '../../../core/services/theme.service';

const FONT_SIZE_MIN = 12;
const FONT_SIZE_MAX = 24;

interface FontColorPreset {
  value: string;
  labelKey: string;
}

const FONT_COLOR_PRESETS: FontColorPreset[] = [
  { value: '', labelKey: 'settings.themeDefault' },
  { value: '#0f172a', labelKey: 'settings.presetSlate' },
  { value: '#1e293b', labelKey: 'settings.presetSlateDark' },
  { value: '#334155', labelKey: 'settings.presetSlateMid' },
  { value: '#0c4a6e', labelKey: 'settings.presetNavy' }
];

@Component({
  selector: 'app-settings-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    @if (panelService.isOpen()) {
      <div
        role="button"
        tabindex="-1"
        aria-label="Close settings"
        class="fixed inset-0 z-40 bg-black/30 transition-opacity"
        (click)="panelService.close()"
      ></div>
      <aside
        role="dialog"
        aria-label="General settings"
        class="fixed inset-y-0 right-0 z-50 w-80 max-w-[100vw] bg-white dark:bg-slate-800 shadow-xl transform transition-transform duration-200 ease-out border-l border-slate-200 dark:border-slate-700 flex flex-col"
      >
        <div class="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
          <h2 class="text-lg font-semibold text-slate-800 dark:text-slate-100">{{ 'settings.general' | translate }}</h2>
          <button
            type="button"
            (click)="panelService.close()"
            class="p-2 rounded-lg text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-700 transition"
            aria-label="Close"
          >
            ✕
          </button>
        </div>
        <div class="flex-1 overflow-y-auto p-4 space-y-5">
          <div>
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">{{ 'settings.theme' | translate }}</label>
            <div class="flex gap-2">
              <button
                type="button"
                (click)="setTheme('light')"
                [class.ring-2]="themeService.currentTheme() === 'light'"
                class="flex-1 px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-800 dark:text-slate-200 text-sm transition ring-indigo-500"
              >
                {{ 'settings.light' | translate }}
              </button>
              <button
                type="button"
                (click)="setTheme('dark')"
                [class.ring-2]="themeService.currentTheme() === 'dark'"
                class="flex-1 px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-slate-800 dark:bg-slate-700 text-slate-200 text-sm transition ring-indigo-500"
              >
                {{ 'settings.dark' | translate }}
              </button>
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">{{ 'settings.language' | translate }}</label>
            <select
              [ngModel]="settingsService.currentSettings().language"
              (ngModelChange)="onLanguageChange($event)"
              class="w-full px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-800 dark:text-slate-200 text-sm"
            >
              <option value="en">English</option>
              <option value="ar">العربية</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">{{ 'settings.fontSizePx' | translate }}</label>
            <div class="flex items-center gap-2">
              <input
                type="number"
                [ngModel]="settingsService.currentSettings().fontSizePx"
                (ngModelChange)="onFontSizePxChange($event)"
                [min]="FONT_SIZE_MIN"
                [max]="FONT_SIZE_MAX"
                step="1"
                class="w-20 px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-800 dark:text-slate-200 text-sm"
              />
              <span class="text-slate-500 dark:text-slate-400 text-sm">px</span>
            </div>
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">{{ 'settings.fontSizePxHint' | translate }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-1">{{ 'settings.fontColor' | translate }}</label>
            <div class="flex flex-wrap gap-2 mb-2">
              @for (preset of fontColorPresets; track preset.value) {
                <button
                  type="button"
                  (click)="settingsService.setFontColor(preset.value)"
                  [class.ring-2]="(settingsService.currentSettings().fontColor || '') === preset.value"
                  [style.background]="preset.value || 'transparent'"
                  class="h-8 w-8 rounded-full border border-slate-300 dark:border-slate-600 transition"
                  [title]="preset.labelKey | translate"
                >
                  @if (!preset.value) {
                    <span class="text-xs text-slate-500">A</span>
                  }
                </button>
              }
            </div>
            <div class="flex items-center gap-2">
              <input
                type="color"
                [value]="fontColorInputValue()"
                (input)="onColorInput($event)"
                class="h-9 w-14 cursor-pointer rounded border border-slate-300 dark:border-slate-600 bg-white"
              />
              <input
                type="text"
                [ngModel]="settingsService.currentSettings().fontColor"
                (ngModelChange)="settingsService.setFontColor($event)"
                placeholder="#000000"
                class="flex-1 px-3 py-2 rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-800 dark:text-slate-200 text-sm font-mono"
              />
            </div>
          </div>
        </div>
      </aside>
    }
  `
})
export class SettingsPanelComponent {
  readonly FONT_SIZE_MIN = FONT_SIZE_MIN;
  readonly FONT_SIZE_MAX = FONT_SIZE_MAX;
  readonly fontColorPresets: FontColorPreset[] = FONT_COLOR_PRESETS;

  constructor(
    public panelService: SettingsPanelService,
    public settingsService: SettingsService,
    public themeService: ThemeService
  ) {}

  fontColorInputValue(): string {
    const c = this.settingsService.currentSettings().fontColor;
    return c && /^#[0-9A-Fa-f]{6}$/.test(c) ? c : '#0f172a';
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.panelService.close();
  }

  setTheme(theme: 'light' | 'dark'): void {
    this.themeService.setTheme(theme);
    this.settingsService.setTheme(theme);
  }

  onLanguageChange(lang: string): void {
    this.settingsService.setLanguage(lang);
  }

  onFontSizePxChange(value: number | string): void {
    const px = typeof value === 'string' ? parseInt(value, 10) : value;
    if (!Number.isNaN(px)) this.settingsService.setFontSizePx(px);
  }

  onColorInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.value) this.settingsService.setFontColor(input.value);
  }
}
