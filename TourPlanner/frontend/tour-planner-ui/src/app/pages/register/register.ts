import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

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

    const error = this.authService.register(registerData);

    if (error) {
      this.errorMessage.set(error);
      return;
    }

    this.router.navigate(['/login']);
  }
}
