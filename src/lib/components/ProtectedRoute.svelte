<script lang="ts">
  import { isAuthenticated, isLoading, authError } from '../stores/auth';
  
  export let requiredRole: string | undefined = undefined;
  export let fallback: string = 'No tienes permisos para acceder a esta sección.';
  
  // Si se especifica un rol requerido, verificar que el usuario lo tenga
  $: hasPermission = !requiredRole || ($isAuthenticated && checkUserRole(requiredRole));
  
  function checkUserRole(role: string): boolean {
    // Por ahora, implementación básica
    // Podrías extender esto para verificar roles específicos del usuario
    return true;
  }
</script>

{#if $isLoading}
  <div class="loading">
    <div class="spinner"></div>
    <p>Cargando...</p>
  </div>
{:else if !$isAuthenticated}
  <div class="auth-required">
    <p>Debes iniciar sesión para acceder a esta funcionalidad.</p>
  </div>
{:else if !hasPermission}
  <div class="permission-denied">
    <p>{fallback}</p>
  </div>
{:else}
  <slot />
{/if}

{#if $authError}
  <div class="error-banner">
    <p>Error de autenticación: {$authError}</p>
  </div>
{/if}

<style>
  .loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px;
    gap: 16px;
  }
  
  .spinner {
    width: 32px;
    height: 32px;
    border: 3px solid rgba(102, 126, 234, 0.2);
    border-top: 3px solid #667eea;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .auth-required,
  .permission-denied {
    text-align: center;
    padding: 40px;
    background: #f8f9fa;
    border-radius: 8px;
    border: 1px solid #e9ecef;
  }
  
  .auth-required p,
  .permission-denied p {
    color: #666;
    font-size: 16px;
    margin: 0;
  }
  
  .error-banner {
    background: #fee;
    color: #c33;
    padding: 12px 16px;
    border-radius: 6px;
    border: 1px solid #fcc;
    margin-bottom: 16px;
  }
  
  .error-banner p {
    margin: 0;
    font-size: 14px;
  }
</style>