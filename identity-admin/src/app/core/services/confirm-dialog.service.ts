import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

export interface ConfirmDialogConfig {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
}

export interface ConfirmDialogState {
  visible: boolean;
  title: string;
  message: string;
  confirmLabel: string;
  cancelLabel: string;
  type: ConfirmDialogType;
}

export type ConfirmDialogType = 'confirm' | 'alert';

const initialState: ConfirmDialogState = {
  visible: false,
  title: '',
  message: '',
  confirmLabel: 'common.delete',
  cancelLabel: 'common.cancel',
  type: 'confirm'
};

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly result$ = new Subject<boolean>();
  private readonly state$ = new BehaviorSubject<ConfirmDialogState>(initialState);

  confirm(config: ConfirmDialogConfig): Observable<boolean> {
    this.state$.next({
      visible: true,
      title: config.title,
      message: config.message,
      confirmLabel: config.confirmLabel ?? 'common.delete',
      cancelLabel: config.cancelLabel ?? 'common.cancel',
      type: 'confirm'
    });
    return new Observable<boolean>((subscriber) => {
      const sub = this.result$.subscribe((value) => {
        subscriber.next(value);
        subscriber.complete();
      });
      return () => sub.unsubscribe();
    });
  }

  alert(config: { title: string; message: string }): Observable<void> {
    this.state$.next({
      visible: true,
      title: config.title,
      message: config.message,
      confirmLabel: 'common.ok',
      cancelLabel: '',
      type: 'alert'
    });
    return new Observable<void>((subscriber) => {
      const sub = this.result$.subscribe(() => {
        subscriber.next();
        subscriber.complete();
      });
      return () => sub.unsubscribe();
    });
  }

  close(result: boolean): void {
    const current = this.state$.value;
    this.state$.next({ ...current, visible: false });
    this.result$.next(result);
  }

  getState$(): Observable<ConfirmDialogState> {
    return this.state$.asObservable();
  }

  get visible(): boolean {
    return this.state$.value.visible;
  }
}
