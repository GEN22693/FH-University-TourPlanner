import { Injectable, computed, signal } from '@angular/core';
import { AppUser, LoginData, RegisterData } from '../../models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly usersKey = 'tourplanner_users';
  private readonly currentUserKey = 'tourplanner_current_user';

  private readonly currentUserSignal = signal<AppUser | null>(this.loadCurrentUser());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUserSignal() !== null);

  register(data: RegisterData): string | null {
    const users = this.loadUsers();

    const emailExists = users.some((user) => user.email.toLowerCase() === data.email.toLowerCase());

    if (emailExists) {
      return 'This email is already registered.';
    }

    const newUser: AppUser = {
      id: crypto.randomUUID(),
      username: data.username,
      email: data.email,
      password: data.password,
    };

    users.push(newUser);
    localStorage.setItem(this.usersKey, JSON.stringify(users));

    return null;
  }

  login(data: LoginData): string | null {
    const users = this.loadUsers();

    const foundUser = users.find(
      (user) =>
        user.email.toLowerCase() === data.email.toLowerCase() && user.password === data.password,
    );

    if (!foundUser) {
      return 'Email or password is wrong.';
    }

    localStorage.setItem(this.currentUserKey, JSON.stringify(foundUser));
    this.currentUserSignal.set(foundUser);

    return null;
  }

  logout(): void {
    localStorage.removeItem(this.currentUserKey);
    this.currentUserSignal.set(null);
  }

  private loadUsers(): AppUser[] {
    const rawUsers = localStorage.getItem(this.usersKey);

    if (!rawUsers) {
      return [];
    }

    return JSON.parse(rawUsers) as AppUser[];
  }

  private loadCurrentUser(): AppUser | null {
    const rawUser = localStorage.getItem(this.currentUserKey);

    if (!rawUser) {
      return null;
    }

    return JSON.parse(rawUser) as AppUser;
  }
}
