<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { authStore } from '../stores/auth';

  const dispatch = createEventDispatcher();

  let username = '';
  let password = '';
  let isLoading = false;
  let error = '';

  async function handleLogin() {
    if (!username || !password) {
      error = 'Por favor, introduce usuario y contraseña';
      return;
    }

    isLoading = true;
    error = '';

    try {
      await authStore.login(username, password);
      dispatch('loginSuccess');
    } catch (err: any) {
      error = err.message || 'Error de autenticación';
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
      <h1>Instalación Fotovoltaica</h1>
      <p>Inicia sesión para acceder al sistema</p>
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
        <label for="password">Contraseña</label>
        <input
          id="password"
          type="password"
          bind:value={password}
          on:keypress={handleKeyPress}
          placeholder="Tu contraseña"
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
          Iniciando sesión...
        {:else}
          Iniciar Sesión
        {/if}
      </button>
    </form>

    <div class="login-footer">
      <p>Sistema de gestión de instalación fotovoltaica</p>
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
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 20px;
    box-sizing: border-box;
  }

  .login-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 15px 35px rgba(0, 0, 0, 0.1);
    padding: 40px;
    width: 100%;
    max-width: 400px;
    animation: slideIn 0.3s ease-out;
  }

  @keyframes slideIn {
    from {
      opacity: 0;
      transform: translateY(-20px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .login-header {
    text-align: center;
    margin-bottom: 30px;
  }

  .login-header h1 {
    color: #333;
    font-size: 24px;
    font-weight: 600;
    margin: 0 0 8px 0;
  }

  .login-header p {
    color: #666;
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
    color: #333;
    font-size: 14px;
  }

  .form-group input {
    padding: 12px 16px;
    border: 2px solid #e1e5e9;
    border-radius: 8px;
    font-size: 14px;
    transition: border-color 0.2s ease;
  }

  .form-group input:focus {
    outline: none;
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  }

  .form-group input:disabled {
    background-color: #f5f5f5;
    cursor: not-allowed;
  }

  .error-message {
    background-color: #fee;
    color: #c33;
    padding: 12px 16px;
    border-radius: 8px;
    border: 1px solid #fcc;
    font-size: 14px;
    text-align: center;
  }

  .login-button {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    padding: 14px 20px;
    border-radius: 8px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
  }

  .login-button:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
  }

  .login-button:active:not(:disabled) {
    transform: translateY(0);
  }

  .login-button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
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

  .login-footer {
    text-align: center;
    margin-top: 30px;
    padding-top: 20px;
    border-top: 1px solid #eee;
  }

  .login-footer p {
    color: #999;
    font-size: 12px;
    margin: 0;
  }

  @media (max-width: 480px) {
    .login-container {
      padding: 16px;
      align-items: flex-start;
      padding-top: 10vh;
    }
    
    .login-card {
      padding: 24px 20px;
      width: 100%;
      max-width: none;
    }
    
    .login-header h1 {
      font-size: 22px;
    }
    
    .form-group input {
      padding: 14px 16px;
      font-size: 16px; /* Previene zoom en iOS */
    }
    
    .login-button {
      padding: 16px 20px;
      font-size: 16px;
    }
  }
  
  @media (max-width: 320px) {
    .login-card {
      padding: 20px 16px;
    }
    
    .login-header h1 {
      font-size: 20px;
    }
  }
</style>