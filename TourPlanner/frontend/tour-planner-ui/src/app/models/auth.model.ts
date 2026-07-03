export interface AppUser {
  id: number;
  username: string;
  email: string;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface RegisterData {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  username: string;
  email: string;
  token: string;
}

export interface AuthMeResponse {
  userId: number;
  username: string;
  email: string;
}
