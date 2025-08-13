import { writable, derived } from 'svelte/store';
import { login as apiLogin, logout as apiLogout, getCurrentUser, setAuthToken, getAuthToken, refreshToken as apiRefreshToken, type AuthResponse, type UserProfile } from '../api';

interface AuthState {
  isAuthenticated: boolean;
  user: {
    id: number;
    email: string;
    name: string;
    rol: string;
  } | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  token: null,
  refreshToken: null,
  isLoading: true,
  error: null
};

function createAuthStore() {
  const { subscribe, set, update } = writable<AuthState>(initialState);

  return {
    subscribe,
    
    async init() {
      update(state => ({ ...state, isLoading: true, error: null }));
      
      try {
        const token = getAuthToken();
        
        if (token) {
          // Verificar si el token es válido obteniendo el perfil del usuario
          const userProfile = await getCurrentUser();
          
          update(state => ({
            ...state,
            isAuthenticated: true,
            user: userProfile.user,
            token,
            isLoading: false,
            error: null
          }));
        } else {
          update(state => ({
            ...state,
            isAuthenticated: false,
            user: null,
            token: null,
            isLoading: false,
            error: null
          }));
        }
      } catch (error: any) {
        console.error('Error initializing auth:', error);
        
        // Si el token es inválido, limpiarlo
        setAuthToken(null);
        
        update(state => ({
          ...state,
          isAuthenticated: false,
          user: null,
          token: null,
          isLoading: false,
          error: error.message || 'Error de autenticación'
        }));
      }
    },

    async login(username: string, password: string) {
      update(state => ({ ...state, isLoading: true, error: null }));
      
      try {
        const response: AuthResponse = await apiLogin(username, password);
        
        // Guardar token
        setAuthToken(response.access_token);
        
        // Guardar refresh token en localStorage
        if (response.refresh_token) {
          localStorage.setItem('refresh_token', response.refresh_token);
        }
        
        update(state => ({
          ...state,
          isAuthenticated: true,
          user: response.user,
          token: response.access_token,
          refreshToken: response.refresh_token,
          isLoading: false,
          error: null
        }));
        
        console.log('Login exitoso para usuario:', response.user.email);
        
      } catch (error: any) {
        console.error('Error en login:', error);
        
        update(state => ({
          ...state,
          isAuthenticated: false,
          user: null,
          token: null,
          refreshToken: null,
          isLoading: false,
          error: error.message || 'Error de autenticación'
        }));
        
        throw error;
      }
    },

    async logout() {
      update(state => ({ ...state, isLoading: true }));
      
      try {
        await apiLogout();
      } catch (error) {
        console.error('Error durante logout:', error);
      }
      
      // Limpiar localStorage
      localStorage.removeItem('refresh_token');
      
      set({
        isAuthenticated: false,
        user: null,
        token: null,
        refreshToken: null,
        isLoading: false,
        error: null
      });
      
      console.log('Logout completado');
    },

    async refreshToken() {
      const refreshToken = localStorage.getItem('refresh_token');
      
      if (!refreshToken) {
        throw new Error('No hay refresh token disponible');
      }
      
      try {
        const response: AuthResponse = await apiRefreshToken(refreshToken);
        
        setAuthToken(response.access_token);
        
        if (response.refresh_token) {
          localStorage.setItem('refresh_token', response.refresh_token);
        }
        
        update(state => ({
          ...state,
          token: response.access_token,
          refreshToken: response.refresh_token,
          user: response.user,
          isAuthenticated: true,
          error: null
        }));
        
        return response;
        
      } catch (error: any) {
        console.error('Error refrescando token:', error);
        
        // Si no se puede refrescar, hacer logout
        await this.logout();
        throw error;
      }
    },

    clearError() {
      update(state => ({ ...state, error: null }));
    }
  };
}

export const authStore = createAuthStore();

// Store derivado para facilitar el acceso a propiedades comunes
export const isAuthenticated = derived(authStore, $auth => $auth.isAuthenticated);
export const currentUser = derived(authStore, $auth => $auth.user);
export const isLoading = derived(authStore, $auth => $auth.isLoading);
export const authError = derived(authStore, $auth => $auth.error);

// Inicializar el store cuando se carga la aplicación
if (typeof window !== 'undefined') {
  authStore.init();
}