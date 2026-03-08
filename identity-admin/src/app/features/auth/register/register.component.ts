import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  form: FormGroup;
  loading = false;
  message: string | null = null;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.nonNullable.group({
      userName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatch });
  }

  passwordMatch(control: AbstractControl): { [key: string]: boolean } | null {
    const g = control as FormGroup;
    const p = g.get('password')?.value;
    const c = g.get('confirmPassword')?.value;
    return p && c && p === c ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.form.invalid || this.loading) return;
    this.loading = true;
    this.message = 'Register is not yet available. Please contact your administrator to create an account.';
    this.loading = false;
  }
}
