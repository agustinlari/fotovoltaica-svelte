<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { authStore } from '../stores/auth';
  import { Sun } from 'lucide-svelte';

  const dispatch = createEventDispatcher();

  function getRegisterUrl(): string {
    const base = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
      ? 'http://localhost:8080/auth'
      : `${window.location.protocol}//${window.location.host}/auth`;
    const redirect = encodeURIComponent(`${window.location.protocol}//${window.location.host}/fotovoltaica/`);
    return `${base}/realms/master/protocol/openid-connect/registrations?client_id=fotovoltaica-client&response_type=code&scope=openid&redirect_uri=${redirect}`;
  }

  const registerUrl = getRegisterUrl();

  let username = '';
  let password = '';
  let isLoading = false;
  let error = '';

  async function handleLogin() {
    if (!username || !password) {
      error = 'Por favor, introduce usuario y contrasena';
      return;
    }

    isLoading = true;
    error = '';

    try {
      await authStore.login(username, password);
      dispatch('loginSuccess');
    } catch (err: any) {
      error = err.message || 'Error de autenticacion';
      console.error('Login error:', err);
    } finally {
      isLoading = false;
    }
  }

  function handleKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      handleLogin();
    }
  }
</script>

<div class="login-container">
  <div class="login-card">
    <div class="login-header">
      <div class="logo-icon">
        <Sun size={32} strokeWidth={1.5} />
      </div>
      <h1>Fotovoltaica</h1>
      <p>Inicia sesion para acceder al sistema</p>
    </div>

    <form on:submit|preventDefault={handleLogin} class="login-form">
      <div class="form-group">
        <label for="username">Usuario / Email</label>
        <input
          id="username"
          type="text"
          bind:value={username}
          on:keypress={handleKeyPress}
          placeholder="tu.email@ejemplo.com"
          autocomplete="username"
          disabled={isLoading}
        />
      </div>

      <div class="form-group">
        <label for="password">Contrasena</label>
        <input
          id="password"
          type="password"
          bind:value={password}
          on:keypress={handleKeyPress}
          placeholder="Tu contrasena"
          autocomplete="current-password"
          disabled={isLoading}
        />
      </div>

      {#if error}
        <div class="error-message">
          {error}
        </div>
      {/if}

      <button
        type="submit"
        class="login-button"
        disabled={isLoading || !username || !password}
      >
        {#if isLoading}
          <span class="spinner"></span>
          Iniciando sesion...
        {:else}
          Iniciar sesion
        {/if}
      </button>
    </form>

    <div class="register-link">
      <span>No tienes cuenta?</span>
      <a href={registerUrl} rel="noopener">Registrarse</a>
    </div>
  </div>
</div>

<style>
  .login-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: #F8FAFC;
    padding: 20px;
    box-sizing: border-box;
  }

  .login-card {
    background: white;
    border-radius: 16px;
    border: 1px solid #E5E7EB;
    padding: 40px;
    width: 100%;
    max-width: 380px;
  }

  .login-header {
    text-align: center;
    margin-bottom: 32px;
  }

  .logo-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 56px;
    height: 56px;
    background: #FEF3C7;
    color: #D97706;
    border-radius: 14px;
    margin-bottom: 16px;
  }

  .login-header h1 {
    color: #1F2937;
    font-size: 22px;
    font-weight: 600;
    margin: 0 0 6px 0;
  }

  .login-header p {
    color: #6B7280;
    font-size: 14px;
    margin: 0;
  }

  .login-form {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .form-group label {
    font-weight: 500;
    color: #374151;
    font-size: 14px;
  }

  .form-group input {
    padding: 12px 14px;
    border: 1px solid #D1D5DB;
    border-radius: 10px;
    font-size: 16px;
    transition: border-color 0.2s ease;
    background: white;
  }

  .form-group input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .form-group input:disabled {
    background-color: #F9FAFB;
    cursor: not-allowed;
  }

  .error-message {
    background-color: #FEF2F2;
    color: #DC2626;
    padding: 12px 14px;
    border-radius: 10px;
    border: 1px solid #FECACA;
    font-size: 14px;
    text-align: center;
  }

  .login-button {
    background: #1F2937;
    color: white;
    border: none;
    padding: 14px 20px;
    border-radius: 10px;
    font-size: 16px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
  }

  .login-button:hover:not(:disabled) {
    background: #111827;
  }

  .login-button:active:not(:disabled) {
    transform: scale(0.98);
  }

  .login-button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .spinner {
    width: 16px;
    height: 16px;
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-top: 2px solid white;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  .register-link {
    text-align: center;
    margin-top: 24px;
    padding-top: 20px;
    border-top: 1px solid #E5E7EB;
    font-size: 14px;
    color: #6B7280;
  }

  .register-link a {
    color: #3B82F6;
    text-decoration: none;
    font-weight: 500;
    margin-left: 4px;
  }

  .register-link a:hover {
    text-decoration: underline;
  }

  @media (max-width: 480px) {
    .login-container {
      padding: 16px;
      align-items: flex-start;
      padding-top: 10vh;
    }

    .login-card {
      padding: 28px 20px;
      max-width: none;
    }
  }
</style>
