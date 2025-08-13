<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  const dispatch = createEventDispatcher();
  
  const modules = [
    {
      id: 'camiones',
      title: 'Camiones',
      description: 'Gestión de camiones y descargas',
      icon: '🚛',
      color: '#3B82F6'
    },
    {
      id: 'estructura',
      title: 'Estructura',
      description: 'Control de estructuras fotovoltaicas',
      icon: '⚡',
      color: '#10B981'
    },
    {
      id: 'pallets',
      title: 'Pallets',
      description: 'Seguimiento de pallets y contenido',
      icon: '📦',
      color: '#F59E0B'
    }
  ];
  
  function selectModule(moduleId: string) {
    dispatch('moduleSelected', moduleId);
  }
</script>

<div class="dashboard">
  <div class="dashboard-header">
    <h1>Sistema de Gestión Fotovoltaica</h1>
    <p>Selecciona un módulo para comenzar</p>
  </div>
  
  <div class="modules-grid">
    {#each modules as module}
      <button 
        class="module-card"
        style="--module-color: {module.color}"
        on:click={() => selectModule(module.id)}
      >
        <div class="module-icon">{module.icon}</div>
        <div class="module-content">
          <h3>{module.title}</h3>
          <p>{module.description}</p>
        </div>
        <div class="module-arrow">→</div>
      </button>
    {/each}
  </div>
  
  <div class="dashboard-footer">
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-number">--</div>
        <div class="stat-label">Camiones Registrados</div>
      </div>
      <div class="stat-card">
        <div class="stat-number">--</div>
        <div class="stat-label">Estructuras Activas</div>
      </div>
      <div class="stat-card">
        <div class="stat-number">--</div>
        <div class="stat-label">Pallets en Sistema</div>
      </div>
    </div>
  </div>
</div>

<style>
  .dashboard {
    max-width: 1200px;
    margin: 0 auto;
    padding: 40px 20px;
  }
  
  .dashboard-header {
    text-align: center;
    margin-bottom: 50px;
  }
  
  .dashboard-header h1 {
    font-size: 32px;
    font-weight: 700;
    color: #1F2937;
    margin: 0 0 12px 0;
  }
  
  .dashboard-header p {
    font-size: 18px;
    color: #6B7280;
    margin: 0;
  }
  
  .modules-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
    gap: 24px;
    margin-bottom: 60px;
  }
  
  .module-card {
    display: flex;
    align-items: center;
    padding: 30px;
    background: white;
    border: 2px solid #E5E7EB;
    border-radius: 16px;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    text-align: left;
    width: 100%;
  }
  
  .module-card:hover {
    border-color: var(--module-color);
    transform: translateY(-4px);
    box-shadow: 0 12px 24px -4px rgba(0, 0, 0, 0.15);
  }
  
  .module-icon {
    font-size: 48px;
    margin-right: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 80px;
    height: 80px;
    background: linear-gradient(135deg, var(--module-color)15, var(--module-color)25);
    border-radius: 12px;
  }
  
  .module-content {
    flex: 1;
  }
  
  .module-content h3 {
    font-size: 22px;
    font-weight: 600;
    color: #1F2937;
    margin: 0 0 8px 0;
  }
  
  .module-content p {
    font-size: 15px;
    color: #6B7280;
    margin: 0;
    line-height: 1.4;
  }
  
  .module-arrow {
    font-size: 24px;
    color: var(--module-color);
    font-weight: bold;
    opacity: 0;
    transition: all 0.3s ease;
    transform: translateX(-10px);
  }
  
  .module-card:hover .module-arrow {
    opacity: 1;
    transform: translateX(0);
  }
  
  .dashboard-footer {
    border-top: 1px solid #E5E7EB;
    padding-top: 40px;
  }
  
  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 20px;
  }
  
  .stat-card {
    background: #F9FAFB;
    padding: 24px;
    border-radius: 12px;
    text-align: center;
    border: 1px solid #E5E7EB;
  }
  
  .stat-number {
    font-size: 28px;
    font-weight: 700;
    color: #1F2937;
    margin-bottom: 8px;
  }
  
  .stat-label {
    font-size: 14px;
    color: #6B7280;
    font-weight: 500;
  }
  
  @media (max-width: 768px) {
    .dashboard {
      padding: 20px 16px;
    }
    
    .dashboard-header h1 {
      font-size: 28px;
    }
    
    .dashboard-header p {
      font-size: 16px;
    }
    
    .modules-grid {
      grid-template-columns: 1fr;
      gap: 16px;
    }
    
    .module-card {
      padding: 24px;
      flex-direction: column;
      text-align: center;
    }
    
    .module-icon {
      margin-right: 0;
      margin-bottom: 16px;
    }
    
    .module-arrow {
      display: none;
    }
  }
</style>