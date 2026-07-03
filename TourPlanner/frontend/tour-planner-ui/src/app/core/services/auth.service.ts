import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { API_BASE_URL } from '../api/api.config';
import { AppUser, AuthMeResponse, AuthResponse, LoginData, RegisterData } from '../../models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly tokenKey = 'tourplanner_auth_token';
  private readonly currentUserKey = 'tourplanner_current_user';

  private readonly currentUserSignal = signal<AppUser | null>(this.loadCurrentUser());
  private readonly tokenSignal = signal<string | null>(this.loadToken());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly token = this.tokenSignal.asReadonly();

  readonly isLoggedIn = computed(() => {
    return this.currentUserSignal() !== null && this.tokenSignal() !== null;
  });

  register(data: RegisterData): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/register`, data);
  }

  login(data: LoginData): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/login`, data).pipe(
      tap((response) => this.saveAuth(response)),
    );
  }

  loadMe(): Observable<AuthMeResponse> {
    return this.http.get<AuthMeResponse>(`${API_BASE_URL}/auth/me`).pipe(
      tap((response) => {
        const user: AppUser = {
          id: response.userId,
          username: response.username,
          email: response.email,
        };

        this.currentUserSignal.set(user);
        localStorage.setItem(this.currentUserKey, JSON.stringify(user));
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.currentUserKey);

    this.tokenSignal.set(null);
    this.currentUserSignal.set(null);
  }

  private saveAuth(response: AuthResponse): void {
    const user: AppUser = {
      id: response.userId,
      username: response.username,
      email: response.email,
    };

    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.currentUserKey, JSON.stringify(user));

    this.tokenSignal.set(response.token);
    this.currentUserSignal.set(user);
  }

  private loadToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private loadCurrentUser(): AppUser | null {
    const rawUser = localStorage.getItem(this.currentUserKey);

    if (!rawUser) {
      return null;
    }

    try {
      return JSON.parse(rawUser) as AppUser;
    } catch {
      localStorage.removeItem(this.currentUserKey);
      return null;
    }
  }
}
