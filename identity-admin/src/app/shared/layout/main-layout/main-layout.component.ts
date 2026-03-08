import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';
import { SettingsPanelComponent } from '../settings-panel/settings-panel.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent, SettingsPanelComponent],
  template: `
    <div class="flex min-h-screen bg-slate-50 dark:bg-slate-900">
      <app-sidebar />
      <div class="flex-1 flex flex-col min-w-0">
        <app-header />
        <main class="flex-1 p-6 overflow-auto">
          <router-outlet />
        </main>
      </div>
      <app-settings-panel />
    </div>
  `
})
export class MainLayoutComponent {}
