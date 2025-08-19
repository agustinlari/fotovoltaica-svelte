<script lang="ts">
  import Camiones from './lib/components/Camiones.svelte'
  import CamionesCards from './lib/components/CamionesCards.svelte'
  import Estructura from './lib/components/Estructura.svelte'
  import EstructuraCards from './lib/components/EstructuraCards.svelte'
  import Pallets from './lib/components/Pallets.svelte'
  import PalletsCards from './lib/components/PalletsCards.svelte'
  import Informes from './lib/components/Informes.svelte'
  import Login from './lib/components/Login.svelte'
  import Dashboard from './lib/components/Dashboard.svelte'
  import { authStore, isAuthenticated, isLoading, currentUser } from './lib/stores/auth'
  
  let currentView: 'dashboard' | 'camiones' | 'estructura' | 'pallets' | 'informes' = 'dashboard'

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
    <header class="app-header" class:centered-header={currentView !== 'dashboard'}>
      {#if currentView === 'dashboard'}
        <div class="header-left">
          <h1>Fotovoltaica</h1>
        </div>
        <div class="user-info">
          <span class="welcome-text">Bienvenido, {$currentUser?.name || $currentUser?.email}</span>
          <button class="logout-btn" on:click={handleLogout}>Cerrar Sesión</button>
        </div>
      {:else}
        <div class="back-section">
          <button class="back-btn" on:click={goToDashboard}>
            ← Dashboard
          </button>
        </div>
        <div class="center-section">
          <h1>
            {#if currentView === 'camiones'}
              Camiones
            {:else if currentView === 'estructura'}
              Estructura
            {:else if currentView === 'pallets'}
              Pallets
            {:else if currentView === 'informes'}
              Informes
            {/if}
          </h1>
        </div>
        <div class="spacer-section"></div>
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
    {:else if currentView === 'informes'}
      <Informes />
    {/if}
  </main>
{/if}

<style>
  main { 
    padding: 24px; 
    max-width: 1400px;
    margin: 0 auto;
    background: #F8FAFC;
    min-height: 100vh;
  }
  
  @media (max-width: 768px) {
    main {
      padding: 16px;
      background: #F1F5F9;
    }
  }
  
  @media (max-width: 480px) {
    main {
      padding: 12px;
      background: #F1F5F9;
    }
  }
  
  .app-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 32px;
    padding: 24px 32px;
    background: white;
    border-radius: 16px;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    border: 1px solid #E5E7EB;
  }
  
  @media (min-width: 1200px) {
    .app-header {
      padding: 32px 48px;
    }
  }
  
  .header-left {
    display: flex;
    align-items: center;
    gap: 16px;
  }
  
  .centered-header {
    display: grid;
    grid-template-columns: 1fr 2fr 1fr;
    align-items: center;
  }
  
  .back-section {
    display: flex;
    justify-content: flex-start;
  }
  
  .center-section {
    display: flex;
    justify-content: center;
  }
  
  .spacer-section {
    /* Empty spacer for grid balance */
  }
  
  .back-btn {
    background: linear-gradient(135deg, #F3F4F6, #E5E7EB);
    color: #374151;
    border: 1px solid #D1D5DB;
    padding: 10px 20px;
    border-radius: 10px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    font-size: 14px;
    text-decoration: none;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  }
  
  .back-btn:hover {
    background: linear-gradient(135deg, #E5E7EB, #D1D5DB);
    transform: translateX(-2px) translateY(-1px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  }
  
  .app-header h1 {
    color: #1F2937;
    font-size: 32px;
    font-weight: 700;
    margin: 0;
    background: linear-gradient(135deg, #3B82F6, #1D4ED8);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
  }
  
  .user-info {
    display: flex;
    align-items: center;
    gap: 20px;
    font-size: 14px;
    color: #666;
  }
  
  @media (min-width: 1200px) {
    .user-info {
      gap: 32px;
    }
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
