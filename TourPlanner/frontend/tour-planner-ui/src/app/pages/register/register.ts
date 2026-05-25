import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  username = '';
  email = '';
  password = '';
  errorMessage = '';

  createAccount(): void {
    this.errorMessage = '';

    if (!this.username || !this.email || !this.password) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    console.log('Register data:', {
      username: this.username,
      email: this.email,
      password: this.password,
    });
  }
}
