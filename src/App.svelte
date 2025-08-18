<script lang="ts">
  import Camiones from './lib/components/Camiones.svelte'
  import CamionesCards from './lib/components/CamionesCards.svelte'
  import Estructura from './lib/components/Estructura.svelte'
  import EstructuraCards from './lib/components/EstructuraCards.svelte'
  import Pallets from './lib/components/Pallets.svelte'
  import PalletsCards from './lib/components/PalletsCards.svelte'
  import Login from './lib/components/Login.svelte'
  import Dashboard from './lib/components/Dashboard.svelte'
  import { authStore, isAuthenticated, isLoading, currentUser } from './lib/stores/auth'
  
  let currentView: 'dashboard' | 'camiones' | 'estructura' | 'pallets' = 'dashboard'

  function handleLogout() {
    authStore.logout()
  }

  function handleModuleSelected(event: CustomEvent<string>) {
    currentView = event.detail as any;
  }

  function goToDashboard() {
    currentView = 'dashboard';
  }
</script>

{#if $isLoading}
  <div class="loading-container">
    <div class="spinner"></div>
    <p>Cargando...</p>
  </div>
{:else if !$isAuthenticated}
  <Login on:loginSuccess={() => {}} />
{:else}
  <main>
    <header class="app-header">
      <div class="header-left">
        {#if currentView !== 'dashboard'}
          <button class="back-btn" on:click={goToDashboard}>
            ← Dashboard
          </button>
        {/if}
        <h1>
          {#if currentView === 'dashboard'}
            Fotovoltaica
          {:else if currentView === 'camiones'}
            Camiones
          {:else if currentView === 'estructura'}
            Estructura
          {:else if currentView === 'pallets'}
            Pallets
          {/if}
        </h1>
      </div>
      {#if currentView === 'dashboard'}
        <div class="user-info">
          <span class="welcome-text">Bienvenido, {$currentUser?.name || $currentUser?.email}</span>
          <button class="logout-btn" on:click={handleLogout}>Cerrar Sesión</button>
        </div>
      {/if}
    </header>
    
    {#if currentView === 'dashboard'}
      <Dashboard on:moduleSelected={handleModuleSelected} />
    {:else if currentView === 'camiones'}
      <CamionesCards />
    {:else if currentView === 'estructura'}
      <EstructuraCards />
    {:else if currentView === 'pallets'}
      <PalletsCards />
    {/if}
  </main>
{/if}

<style>
  main { 
    padding: 16px; 
    max-width: 1200px;
    margin: 0 auto;
  }
  
  @media (max-width: 768px) {
    main {
      padding: 8px;
    }
  }
  
  @media (max-width: 480px) {
    main {
      padding: 4px;
    }
  }
  
  .app-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    padding: 0 24px 16px;
    border-bottom: 2px solid #eee;
  }
  
  @media (min-width: 1200px) {
    .app-header {
      padding: 0 48px 16px;
    }
  }
  
  .header-left {
    display: flex;
    align-items: center;
    gap: 16px;
  }
  
  .back-btn {
    background: #F3F4F6;
    color: #374151;
    border: 1px solid #D1D5DB;
    padding: 8px 16px;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
    text-decoration: none;
  }
  
  .back-btn:hover {
    background: #E5E7EB;
    transform: translateX(-2px);
  }
  
  .app-header h1 {
    color: #333;
    font-size: 28px;
    font-weight: 600;
    margin: 0;
  }
  
  .user-info {
    display: flex;
    align-items: center;
    gap: 12px;
    font-size: 14px;
    color: #666;
  }
  
  .logout-btn {
    background: linear-gradient(135deg, #ff6b6b, #ee5a52);
    color: white;
    border: none;
    padding: 8px 16px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  
  .logout-btn:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(238, 90, 82, 0.3);
  }
  
  .tabs { 
    display: flex; 
    gap: 8px; 
    margin-bottom: 24px;
  }
  
  .tabs button { 
    padding: 12px 18px; 
    border: 2px solid #e1e5e9; 
    background: #f8f9fa; 
    border-radius: 8px; 
    cursor: pointer;
    font-weight: 500;
    font-size: 14px;
    transition: all 0.2s ease;
    color: #666;
  }
  
  .tabs button:hover {
    border-color: #90caf9;
    background: #f0f7ff;
  }
  
  .tabs button.selected { 
    background: linear-gradient(135deg, #667eea, #764ba2);
    color: white;
    border-color: #667eea;
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
  }
  
  .loading-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    gap: 16px;
  }
  
  .loading-container .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid rgba(102, 126, 234, 0.2);
    border-top: 4px solid #667eea;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }
  
  .loading-container p {
    color: #666;
    font-size: 16px;
    margin: 0;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  @media (max-width: 768px) {
    .app-header {
      flex-direction: column;
      gap: 16px;
      align-items: center;
      text-align: center;
    }
    
    .header-left {
      flex-direction: column;
      gap: 12px;
    }
    
    .app-header h1 {
      font-size: 24px;
    }
    
    .user-info {
      flex-direction: column;
      gap: 8px;
      font-size: 13px;
    }
    
    .tabs {
      flex-wrap: wrap;
    }
    
    .tabs button {
      flex: 1;
      min-width: 120px;
    }
  }
  
  @media (max-width: 640px) {
    .app-header {
      flex-direction: column;
      gap: 12px;
    }
    
    .header-left {
      width: 100%;
      justify-content: center;
    }
    
    .user-info {
      width: 100%;
      justify-content: center;
    }
    
    .app-header h1 {
      font-size: 20px;
    }
  }
</style>
