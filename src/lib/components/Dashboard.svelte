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
    max-width: 1400px;
    margin: 0 auto;
    padding: 20px;
  }
  

  
  .modules-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(380px, 1fr));
    gap: 32px;
    margin-bottom: 48px;
  }
  
  @media (min-width: 1200px) {
    .modules-grid {
      grid-template-columns: repeat(3, 1fr);
    }
  }
  
  .module-card {
    display: flex;
    align-items: center;
    padding: 36px;
    background: linear-gradient(135deg, #FFFFFF, #F9FAFB);
    border: 2px solid #E5E7EB;
    border-radius: 20px;
    cursor: pointer;
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
    text-align: left;
    width: 100%;
    position: relative;
    overflow: hidden;
  }
  
  .module-card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: linear-gradient(90deg, var(--module-color), rgba(255,255,255,0));
    opacity: 0;
    transition: opacity 0.3s ease;
  }
  
  .module-card:hover {
    border-color: var(--module-color);
    transform: translateY(-8px) scale(1.02);
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    background: linear-gradient(135deg, #FFFFFF, #F8FAFC);
  }
  
  .module-card:hover::before {
    opacity: 1;
  }
  
  .module-icon {
    font-size: 52px;
    margin-right: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 88px;
    height: 88px;
    background: linear-gradient(135deg, rgba(59, 130, 246, 0.1), rgba(59, 130, 246, 0.2));
    border-radius: 16px;
    transition: all 0.3s ease;
    position: relative;
  }
  
  .module-card:hover .module-icon {
    transform: scale(1.1) rotate(5deg);
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
    background: linear-gradient(135deg, #FFFFFF, #F8FAFC);
    padding: 32px 24px;
    border-radius: 16px;
    text-align: center;
    border: 2px solid #E5E7EB;
    transition: all 0.3s ease;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  }
  
  .stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    border-color: #D1D5DB;
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