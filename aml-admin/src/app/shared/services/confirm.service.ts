import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export interface ConfirmState {
  visible: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private result$ = new Subject<boolean>();
  private _state: ConfirmState = {
    visible: false,
    title: '',
    message: ''
  };

  get state(): ConfirmState {
    return this._state;
  }

  confirm(
    title: string,
    message: string,
    confirmLabel?: string,
    cancelLabel?: string
  ): Observable<boolean> {
    this._state = {
      visible: true,
      title,
      message,
      confirmLabel,
      cancelLabel
    };
    return new Observable<boolean>((subscriber) => {
      const sub = this.result$.subscribe({
        next: (value) => {
          subscriber.next(value);
          subscriber.complete();
        }
      });
      return () => sub.unsubscribe();
    });
  }

  accept(): void {
    this._state = { ...this._state, visible: false };
    this.result$.next(true);
  }

  cancel(): void {
    this._state = { ...this._state, visible: false };
    this.result$.next(false);
  }
}
