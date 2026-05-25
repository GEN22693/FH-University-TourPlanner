export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthUser {
  id: number;
  email: string;
  username: string;
}

export interface LoginResponse {
  token: string;
  user: AuthUser;
}
