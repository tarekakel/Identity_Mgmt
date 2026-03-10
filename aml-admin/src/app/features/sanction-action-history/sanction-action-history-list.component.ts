import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/notification.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import type { ApiResponse, SanctionActionAuditLogDto } from '../../shared/models/api.model';

@Component({
  selector: 'app-sanction-action-history-list',
  standalone: true,
  imports: [CommonModule, TranslateModule, LoaderComponent],
  templateUrl: './sanction-action-history-list.component.html'
})
export class SanctionActionHistoryListComponent implements OnInit {
  list = signal<SanctionActionAuditLogDto[]>([]);
  loading = signal(true);

  private readonly api = inject(ApiService);
  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getSanctionActionAuditLogs({}).subscribe({
      next: (res: ApiResponse<SanctionActionAuditLogDto[]>) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.list.set(res.data);
        } else {
          this.list.set([]);
        }
      },
      error: () => {
        this.loading.set(false);
        this.notification.error(this.translate.instant('common.errorGeneric'));
      }
    });
  }
}
