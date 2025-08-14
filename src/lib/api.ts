// Detectar automáticamente la URL base según el entorno
function getApiBase(): string {
  // Si hay una variable de entorno definida, usarla
  if (import.meta.env.VITE_API_BASE) {
    return import.meta.env.VITE_API_BASE;
  }
  
  // Si estamos en producción (usando HTTPS), usar la URL de producción
  if (window.location.protocol === 'https:' && window.location.hostname === 'aplicaciones.osmos.es') {
    return `${window.location.protocol}//${window.location.host}/api/fotovoltaica`;
  }
  
  // Fallback para desarrollo local
  return 'http://localhost:8787';
}

const API_BASE = getApiBase();

// Token management
let authToken: string | null = null;

export function setAuthToken(token: string | null) {
  authToken = token;
  if (token) {
    localStorage.setItem('auth_token', token);
  } else {
    localStorage.removeItem('auth_token');
  }
}

export function getAuthToken(): string | null {
  if (!authToken) {
    authToken = localStorage.getItem('auth_token');
  }
  return authToken;
}

function getAuthHeaders(): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };
  
  const token = getAuthToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  return headers;
}

export async function apiGet<T>(path: string): Promise<T> {
  const token = getAuthToken();
  const headers: Record<string, string> = {};
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_BASE}${path}`, { 
    headers,
    credentials: 'include',
    mode: 'cors' 
  });
  
  if (res.status === 401) {
    setAuthToken(null);
    throw new Error('Token inválido o expirado');
  }
  
  if (!res.ok) throw new Error(`GET ${path} failed: ${res.status}`);
  return res.json() as Promise<T>;
}

export async function apiPut<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify(body),
    credentials: 'include',
    mode: 'cors',
  });
  
  if (res.status === 401) {
    setAuthToken(null);
    throw new Error('Token inválido o expirado');
  }
  
  if (!res.ok) throw new Error(`PUT ${path} failed: ${res.status}`);
  return res.json() as Promise<T>;
}

export async function apiPost<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(body),
    credentials: 'include',
    mode: 'cors',
  });
  
  if (res.status === 401) {
    setAuthToken(null);
    throw new Error('Token inválido o expirado');
  }
  
  if (!res.ok) throw new Error(`POST ${path} failed: ${res.status}`);
  return res.json() as Promise<T>;
}

export async function apiDelete<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'DELETE',
    headers: getAuthHeaders(),
    credentials: 'include',
    mode: 'cors',
  });
  
  if (res.status === 401) {
    setAuthToken(null);
    throw new Error('Token inválido o expirado');
  }
  
  if (!res.ok) throw new Error(`DELETE ${path} failed: ${res.status}`);
  return res.json() as Promise<T>;
}

// Authentication API
export interface AuthResponse {
  success: boolean;
  access_token: string;
  refresh_token: string;
  expires_in: number;
  user: {
    id: number;
    email: string;
    name: string;
    rol: string;
  };
}

export interface UserProfile {
  success: boolean;
  user: {
    id: number;
    email: string;
    name: string;
    rol: string;
  };
}

export async function login(username: string, password: string): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/auth/keycloak/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ username, password }),
    credentials: 'include',
    mode: 'cors',
  });

  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.error || 'Error de autenticación');
  }

  return res.json() as Promise<AuthResponse>;
}

export async function logout(): Promise<void> {
  try {
    await fetch(`${API_BASE}/auth/keycloak/logout`, {
      method: 'POST',
      headers: getAuthHeaders(),
      credentials: 'include',
      mode: 'cors',
    });
  } catch (error) {
    console.error('Error during logout:', error);
  } finally {
    setAuthToken(null);
  }
}

export async function getCurrentUser(): Promise<UserProfile> {
  return apiGet<UserProfile>('/auth/keycloak/me');
}

export async function refreshToken(refreshToken: string): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/auth/keycloak/refresh`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ refresh_token: refreshToken }),
    credentials: 'include',
    mode: 'cors',
  });

  if (!res.ok) {
    throw new Error('No se pudo refrescar el token');
  }

  return res.json() as Promise<AuthResponse>;
}

export type Camion = {
  id: number;
  DNI?: string | null;
  Matricula?: string | null;
  UbicacionCampa?: string | null;
  FechaDescarga?: string | null;
  Container?: string | null;
  Albaran?: string | null;
  NombreConductor?: string | null;
};

export type Estructura = {
  id: number;
  DNI?: string | null;
  Conductor?: string | null;
  Matricula?: string | null;
  Proveedor?: string | null;
  PackingList?: string | null;
  Albaran?: string | null;
  FechaDescarga?: string | null;
};

export type Pallet = {
  id: string;
  Descarga?: number | null;
  Defecto?: boolean | null;
};


