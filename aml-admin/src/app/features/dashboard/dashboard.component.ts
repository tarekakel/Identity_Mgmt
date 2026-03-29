import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { BaseChartDirective } from 'ng2-charts';
import type { ChartConfiguration } from 'chart.js';
import { take } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import type { CustomerDashboardKpisDto } from '../../shared/models/api.model';

const KPI_CHART_LABEL_KEYS = [
  'app.dashboardKpis.autoApproved',
  'app.dashboardKpis.approved',
  'app.dashboardKpis.rejected',
  'app.dashboardKpis.pendingMaker',
  'app.dashboardKpis.pendingChecker',
  'app.dashboardKpis.pendingScheduler',
  'app.dashboardKpis.otherStatuses'
] as const;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, TranslateModule, BaseChartDirective],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly errorKey = signal<string | null>(null);
  readonly kpis = signal<CustomerDashboardKpisDto | null>(null);

  readonly chartType = 'pie' as const;
  readonly chartData = signal<ChartConfiguration<'pie'>['data']>({ labels: [], datasets: [] });
  readonly chartOptions = signal<ChartConfiguration<'pie'>['options']>({});

  ngOnInit(): void {
    this.translate.onLangChange.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const k = this.kpis();
      if (k) {
        this.translate
          .get([...KPI_CHART_LABEL_KEYS])
          .pipe(take(1))
          .subscribe((tr) => this.applyChart(k, tr));
      }
    });
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.errorKey.set(null);
    this.api
      .getCustomerDashboardKpis()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.kpis.set(res.data);
            this.translate
              .get([...KPI_CHART_LABEL_KEYS])
              .pipe(take(1))
              .subscribe((tr) => this.applyChart(res.data!, tr));
          } else {
            this.errorKey.set('app.dashboardKpis.loadError');
          }
        },
        error: () => {
          this.loading.set(false);
          this.errorKey.set('app.dashboardKpis.loadError');
        }
      });
  }

  private applyChart(d: CustomerDashboardKpisDto, tr: Record<string, string>): void {
    const palette = ['#6366f1', '#22c55e', '#ef4444', '#f59e0b', '#8b5cf6', '#06b6d4', '#64748b', '#94a3b8'];
    const segs: { labelKey: (typeof KPI_CHART_LABEL_KEYS)[number]; v: number }[] = [
      { labelKey: 'app.dashboardKpis.autoApproved', v: d.autoApproved },
      { labelKey: 'app.dashboardKpis.approved', v: d.approved },
      { labelKey: 'app.dashboardKpis.rejected', v: d.rejected },
      { labelKey: 'app.dashboardKpis.pendingMaker', v: d.pendingMaker },
      { labelKey: 'app.dashboardKpis.pendingChecker', v: d.pendingChecker },
      { labelKey: 'app.dashboardKpis.pendingScheduler', v: d.pendingScheduler }
    ];
    const sumKpi = segs.reduce((a, s) => a + s.v, 0);
    const other = Math.max(0, d.totalCustomers - sumKpi);

    const labels: string[] = [];
    const data: number[] = [];
    const colors: string[] = [];
    let ci = 0;
    for (const s of segs) {
      if (s.v > 0) {
        labels.push(tr[s.labelKey] ?? s.labelKey);
        data.push(s.v);
        colors.push(palette[ci % palette.length]);
        ci++;
      }
    }
    if (other > 0) {
      labels.push(tr['app.dashboardKpis.otherStatuses'] ?? 'Other');
      data.push(other);
      colors.push(palette[ci % palette.length]);
    }

    const total = d.totalCustomers;
    this.chartData.set({
      labels,
      datasets: [
        {
          data,
          backgroundColor: colors,
          borderColor: 'rgba(15,23,42,0.12)',
          borderWidth: 1
        }
      ]
    });
    this.chartOptions.set({
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: { color: '#64748b', boxWidth: 12, padding: 12 }
        },
        tooltip: {
          callbacks: {
            label: (ctx) => {
              const val = Number(ctx.raw);
              const pct = total > 0 ? ((val / total) * 100).toFixed(1) : '0';
              const label = ctx.label ?? '';
              return `${label}: ${val} (${pct}%)`;
            }
          }
        }
      }
    });
  }
}
