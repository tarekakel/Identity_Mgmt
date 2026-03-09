import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  errorMessage: string | null = null;

  private readonly notification = inject(NotificationService);
  private readonly translate = inject(TranslateService);

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading) return;
    this.errorMessage = null;
    this.loading = true;
    this.auth
      .login({
        email: this.form.value.email!,
        password: this.form.value.password!,
        tenantCode: environment.defaultTenantCode
      })
      .subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            this.notification.success(this.translate.instant('auth.loginSuccess'));
            this.router.navigate(['/dashboard']);
          } else {
            const msg = res.message || this.translate.instant('errors.loginFailed');
            this.errorMessage = msg;
            this.notification.error(msg);
          }
        },
        error: (err) => {
          this.loading = false;
          const msg = err.error?.message || err.error?.errors?.[0] || this.translate.instant('errors.loginFailed');
          this.errorMessage = msg;
          this.notification.error(msg);
        }
      });
  }
}
