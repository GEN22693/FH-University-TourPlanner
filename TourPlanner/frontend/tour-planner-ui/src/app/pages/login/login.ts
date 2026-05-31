import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { LoginData } from '../../models/auth.model';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly email = signal('');
  readonly password = signal('');
  readonly errorMessage = signal('');

  signIn(): void {
    this.errorMessage.set('');

    const loginData: LoginData = {
      email: this.email().trim(),
      password: this.password(),
    };

    if (!loginData.email || !loginData.password) {
      this.errorMessage.set('Please enter email and password.');
      return;
    }

    const error = this.authService.login(loginData);

    if (error) {
      this.errorMessage.set(error);
      return;
    }

    this.router.navigate(['/tours']);
  }
}
