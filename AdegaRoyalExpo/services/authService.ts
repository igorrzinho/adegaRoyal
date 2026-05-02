import AsyncStorage from '@react-native-async-storage/async-storage';
import { apiRequest } from './api';

// ─── Tipos ──────────────────────────────────────────────────────────────────────

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

export interface RegisterResponse {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface DecodedToken {
  sub: string;
  name: string;
  email: string;
  role: string;
  exp: number;
}

// ─── Constantes ─────────────────────────────────────────────────────────────────

const STORAGE_KEYS = {
  ACCESS_TOKEN: '@adega_access_token',
  REFRESH_TOKEN: '@adega_refresh_token',
};

// ─── Helpers ────────────────────────────────────────────────────────────────────

function decodeJwt(token: string): DecodedToken | null {
  try {
    const payload = token.split('.')[1];
    const decoded = atob(payload);
    return JSON.parse(decoded) as DecodedToken;
  } catch {
    return null;
  }
}

// ─── Serviço ────────────────────────────────────────────────────────────────────

export const authService = {
  async login(email: string, password: string) {
    const result = await apiRequest<LoginResponse>('/api/Auth/login', {
      method: 'POST',
      body: JSON.stringify({ email: email.trim(), password }),
    });

    if (result.data) {
      await AsyncStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, result.data.accessToken);
      await AsyncStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, result.data.refreshToken);
    }

    return result;
  },

  async register(name: string, email: string, password: string) {
    return apiRequest<RegisterResponse>('/api/Auth/register/customer', {
      method: 'POST',
      body: JSON.stringify({ name: name.trim(), email: email.trim(), password }),
    });
  },

  async getToken(): Promise<string | null> {
    return AsyncStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  },

  async getUser(): Promise<DecodedToken | null> {
    const token = await AsyncStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    if (!token) return null;
    const decoded = decodeJwt(token);
    if (!decoded) return null;
    if (decoded.exp * 1000 < Date.now()) {
      await this.logout();
      return null;
    }
    return decoded;
  },

  async isAuthenticated(): Promise<boolean> {
    const user = await this.getUser();
    return user !== null;
  },

  async logout(): Promise<void> {
    await AsyncStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    await AsyncStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
  },
};
