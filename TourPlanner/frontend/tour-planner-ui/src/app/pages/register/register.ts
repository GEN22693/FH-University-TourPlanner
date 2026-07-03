import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../core/services/auth.service';
import { RegisterData } from '../../models/auth.model';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly username = signal('');
  readonly email = signal('');
  readonly password = signal('');
  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);

  createAccount(): void {
    this.errorMessage.set('');

    const registerData: RegisterData = {
      username: this.username().trim(),
      email: this.email().trim(),
      password: this.password(),
    };

    if (!registerData.username || !registerData.email || !registerData.password) {
      this.errorMessage.set('Please fill in all fields.');
      return;
    }

    if (registerData.password.length < 6) {
      this.errorMessage.set('Password must have at least 6 characters.');
      return;
    }

    this.isSubmitting.set(true);

    this.authService
      .register(registerData)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.getErrorMessage(error, 'Registration failed.'));
        },
      });
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      const backendMessage = error.error?.message;

      if (typeof backendMessage === 'string' && backendMessage.trim()) {
        return backendMessage;
      }
    }

    return fallbackMessage;
  }
}
