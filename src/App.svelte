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
  import { ArrowLeft, LogOut } from 'lucide-svelte'

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
    <header class="app-header">
      {#if currentView === 'dashboard'}
        <h1>Fotovoltaica</h1>
        <div class="user-info">
          <span class="welcome-text">{$currentUser?.name || $currentUser?.email}</span>
          <button class="logout-btn" on:click={handleLogout} title="Cerrar sesión">
            <LogOut size={18} />
          </button>
        </div>
      {:else}
        <button class="back-btn" on:click={goToDashboard}>
          <ArrowLeft size={18} />
          <span>Volver</span>
        </button>
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
        <div class="spacer"></div>
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
    max-width: 1400px;
    margin: 0 auto;
    background: #F8FAFC;
    min-height: 100vh;
  }

  .app-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px 20px;
    background: white;
    border-bottom: 1px solid #E5E7EB;
    position: sticky;
    top: 0;
    z-index: 50;
  }

  .app-header h1 {
    font-size: 18px;
    font-weight: 600;
    color: #1F2937;
    margin: 0;
  }

  .user-info {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .welcome-text {
    font-size: 14px;
    color: #6B7280;
  }

  .logout-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    background: none;
    border: 1px solid #E5E7EB;
    color: #6B7280;
    width: 36px;
    height: 36px;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s ease;
  }

  .logout-btn:hover {
    background: #FEE2E2;
    border-color: #FECACA;
    color: #DC2626;
  }

  .back-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    background: none;
    border: 1px solid #E5E7EB;
    color: #374151;
    padding: 8px 14px;
    border-radius: 8px;
    font-weight: 500;
    font-size: 14px;
    cursor: pointer;
    transition: all 0.2s ease;
  }

  .back-btn:hover {
    background: #F3F4F6;
  }

  .spacer {
    width: 80px;
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
    width: 32px;
    height: 32px;
    border: 3px solid #E5E7EB;
    border-top: 3px solid #3B82F6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  .loading-container p {
    color: #6B7280;
    font-size: 14px;
    margin: 0;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  @media (max-width: 480px) {
    .app-header {
      padding: 12px 16px;
    }

    .welcome-text {
      display: none;
    }

    .spacer {
      width: 40px;
    }
  }
</style>
