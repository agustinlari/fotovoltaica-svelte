<script lang="ts">
  import { createEventDispatcher, onMount } from 'svelte';
  import { apiGet } from '../api';
  import { Truck, Zap, Package, BarChart3, ChevronRight } from 'lucide-svelte';
  import type { Camion } from '../types/Camion';
  import type { Estructura } from '../types/Estructura';
  import type { Pallet } from '../types/Pallet';

  const dispatch = createEventDispatcher();

  let stats = {
    camiones: 0,
    estructuras: 0,
    pallets: 0
  };

  onMount(async () => {
    await loadStats();
  });

  async function loadStats() {
    try {
      const [camionesData, estructurasData, palletsData] = await Promise.all([
        apiGet<Camion[]>('/camiones').catch(() => []),
        apiGet<Estructura[]>('/estructura').catch(() => []),
        apiGet<Pallet[]>('/pallets').catch(() => [])
      ]);

      stats.camiones = camionesData.length || 0;
      stats.estructuras = estructurasData.length || 0;
      stats.pallets = palletsData.length || 0;
    } catch (error) {
      console.error('Error cargando estadísticas:', error);
      stats.camiones = 0;
      stats.estructuras = 0;
      stats.pallets = 0;
    }
  }

  const modules = [
    {
      id: 'camiones',
      title: 'Camiones',
      description: 'Gestión de camiones y descargas',
      icon: Truck,
      color: '#3B82F6'
    },
    {
      id: 'estructura',
      title: 'Estructura',
      description: 'Descargas de estructuras fotovoltaicas',
      icon: Zap,
      color: '#10B981'
    },
    {
      id: 'pallets',
      title: 'Módulos',
      description: 'Seguimiento de módulos y contenido',
      icon: Package,
      color: '#F59E0B'
    },
    {
      id: 'informes',
      title: 'Informes',
      description: 'Reportes de descarga por camión',
      icon: BarChart3,
      color: '#8B5CF6'
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
        <div class="module-icon">
          <svelte:component this={module.icon} size={28} strokeWidth={1.5} />
        </div>
        <div class="module-content">
          <h3>{module.title}</h3>
          <p>{module.description}</p>
        </div>
        <div class="module-arrow">
          <ChevronRight size={20} />
        </div>
      </button>
    {/each}
  </div>

  <div class="dashboard-footer">
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-number">{stats.camiones}</div>
        <div class="stat-label">Camiones</div>
      </div>
      <div class="stat-card">
        <div class="stat-number">{stats.estructuras}</div>
        <div class="stat-label">Estructuras</div>
      </div>
      <div class="stat-card">
        <div class="stat-number">{stats.pallets}</div>
        <div class="stat-label">Pallets</div>
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
    grid-template-columns: 1fr;
    gap: 12px;
    margin-bottom: 32px;
  }

  .module-card {
    display: flex;
    align-items: center;
    padding: 20px;
    background: white;
    border: 1px solid #E5E7EB;
    border-radius: 12px;
    cursor: pointer;
    transition: all 0.2s ease;
    text-align: left;
    width: 100%;
  }

  .module-card:hover {
    border-color: var(--module-color);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  }

  .module-card:active {
    transform: scale(0.98);
  }

  .module-icon {
    margin-right: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 52px;
    height: 52px;
    background: color-mix(in srgb, var(--module-color) 10%, transparent);
    border-radius: 12px;
    color: var(--module-color);
    flex-shrink: 0;
  }

  .module-content {
    flex: 1;
    min-width: 0;
  }

  .module-content h3 {
    font-size: 16px;
    font-weight: 600;
    color: #1F2937;
    margin: 0 0 4px 0;
  }

  .module-content p {
    font-size: 13px;
    color: #6B7280;
    margin: 0;
    line-height: 1.4;
  }

  .module-arrow {
    color: #9CA3AF;
    flex-shrink: 0;
    margin-left: 8px;
  }

  .dashboard-footer {
    border-top: 1px solid #E5E7EB;
    padding-top: 24px;
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 12px;
  }

  .stat-card {
    background: white;
    padding: 20px 16px;
    border-radius: 12px;
    text-align: center;
    border: 1px solid #E5E7EB;
  }

  .stat-number {
    font-size: 28px;
    font-weight: 700;
    color: #1F2937;
    margin-bottom: 4px;
  }

  .stat-label {
    font-size: 12px;
    color: #6B7280;
    font-weight: 500;
  }

  @media (min-width: 768px) {
    .modules-grid {
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .module-card {
      padding: 24px;
    }
  }

  @media (max-width: 480px) {
    .dashboard {
      padding: 16px;
    }

    .stats-grid {
      gap: 8px;
    }

    .stat-card {
      padding: 16px 12px;
    }

    .stat-number {
      font-size: 24px;
    }
  }
</style>
