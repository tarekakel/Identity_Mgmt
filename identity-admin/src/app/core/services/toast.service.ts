import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration: number;
  createdAt: number;
}

const DEFAULT_DURATION_MS = 4500;
const ERROR_DURATION_MS = 6000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly toasts$ = new BehaviorSubject<Toast[]>([]);
  private idCounter = 0;
  private timeouts = new Map<string, ReturnType<typeof setTimeout>>();

  getToasts$() {
    return this.toasts$.asObservable();
  }

  get toasts(): Toast[] {
    return this.toasts$.value;
  }

  private add(message: string, type: ToastType, durationMs?: number): void {
    const id = `toast-${++this.idCounter}`;
    const duration = durationMs ?? (type === 'error' ? ERROR_DURATION_MS : DEFAULT_DURATION_MS);
    const toast: Toast = {
      id,
      message,
      type,
      duration,
      createdAt: Date.now()
    };
    this.toasts$.next([...this.toasts$.value, toast]);

    const timeout = setTimeout(() => {
      this.dismiss(id);
      this.timeouts.delete(id);
    }, duration);
    this.timeouts.set(id, timeout);
  }

  success(message: string, durationMs?: number): void {
    this.add(message, 'success', durationMs);
  }

  error(message: string, durationMs?: number): void {
    this.add(message, 'error', durationMs);
  }

  info(message: string, durationMs?: number): void {
    this.add(message, 'info', durationMs);
  }

  dismiss(id: string): void {
    const t = this.timeouts.get(id);
    if (t) {
      clearTimeout(t);
      this.timeouts.delete(id);
    }
    this.toasts$.next(this.toasts$.value.filter((toast) => toast.id !== id));
  }
}
