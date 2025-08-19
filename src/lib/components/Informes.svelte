<script lang="ts">
  import { onMount } from 'svelte';
  import type { Camion, Pallet } from '../api';
  import { apiGet, formatUpdateDate } from '../api';
  import { formatDdMmYyyyFromIso } from '../date';
  import ProtectedRoute from './ProtectedRoute.svelte';

  type State = {
    loading: boolean;
    camiones: Camion[];
    pallets: Pallet[];
    error: string | null;
    reportData: ReportData[];
  };

  type ReportData = {
    camion: Camion;
    pallets: Pallet[];
  };

  let state: State = {
    loading: true,
    camiones: [],
    pallets: [],
    error: null,
    reportData: [],
  };

  async function load() {
    try {
      state.loading = true;
      state.error = null;
      
      // Cargar camiones y pallets en paralelo
      const [camionesData, palletsData] = await Promise.all([
        apiGet<Camion[]>('/camiones'),
        apiGet<Pallet[]>('/pallets')
      ]);
      
      state.camiones = camionesData;
      state.pallets = palletsData;
      
      // Generar datos del reporte
      generateReportData();
    } catch (e: any) {
      state.error = e?.message || 'Error cargando datos para informes';
    } finally {
      state.loading = false;
    }
  }

  function generateReportData() {
    state.reportData = state.camiones
      .map(camion => ({
        camion,
        pallets: state.pallets.filter(pallet => pallet.Descarga === camion.id)
      }))
      .filter(data => data.pallets.length > 0) // Solo mostrar camiones con pallets
      .sort((a, b) => a.camion.id - b.camion.id); // Ordenar por ID de camión
  }

  function printReport() {
    window.print();
  }

  onMount(load);
</script>

<ProtectedRoute>
<div class="informes-container">
  <div class="informes-header no-print">
    <h1>Informes de Descarga</h1>
    <div class="header-actions">
      <button class="btn-print" on:click={printReport}>
        🖨️ Imprimir
      </button>
    </div>
  </div>

  {#if state.loading}
    <div class="loading-state no-print">
      <div class="spinner"></div>
      <p>Generando informes...</p>
    </div>
  {:else if state.error}
    <div class="error-state no-print">
      <p>❌ {state.error}</p>
      <button class="btn-secondary" on:click={load}>Reintentar</button>
    </div>
  {:else if state.reportData.length === 0}
    <div class="empty-state no-print">
      <h3>No hay datos para generar informes</h3>
      <p>No se encontraron pallets asignados a camiones</p>
    </div>
  {:else}
    <div class="report-content">
      {#each state.reportData as data, index (data.camion.id)}
        <div class="report-section" class:page-break={index > 0}>
          <div class="report-header">
            <h2>Descarga {data.camion.id}</h2>
            <div class="camion-info">
              <p><strong>Matrícula:</strong> {data.camion.Matricula || 'No especificada'}</p>
              <p><strong>Conductor:</strong> {data.camion.NombreConductor || 'No especificado'}</p>
              <p><strong>DNI:</strong> {data.camion.DNI || 'No especificado'}</p>
              {#if data.camion.FechaDescarga}
                <p><strong>Fecha de Descarga:</strong> {formatDdMmYyyyFromIso(data.camion.FechaDescarga)}</p>
              {/if}
            </div>
          </div>

          <table class="report-table">
            <thead>
              <tr>
                <th>ID Pallet</th>
                <th>Estado</th>
                <th>Fecha Registro</th>
              </tr>
            </thead>
            <tbody>
              {#each data.pallets as pallet (pallet.id)}
                <tr class:defecto-row={pallet.Defecto}>
                  <td class="pallet-id">{pallet.id}</td>
                  <td class="estado-cell">
                    <span class="estado-badge" class:defecto={pallet.Defecto}>
                      {pallet.Defecto ? 'CON DEFECTO' : 'SIN DEFECTO'}
                    </span>
                  </td>
                  <td class="fecha-cell">
                    {#if pallet.updated_at}
                      {formatDdMmYyyyFromIso(pallet.updated_at)}
                    {:else}
                      -
                    {/if}
                  </td>
                </tr>
              {/each}
            </tbody>
            <tfoot>
              <tr class="summary-row">
                <td><strong>Total de Pallets:</strong></td>
                <td colspan="2">
                  <strong>{data.pallets.length}</strong>
                  ({data.pallets.filter(p => p.Defecto).length} con defecto, 
                   {data.pallets.filter(p => !p.Defecto).length} sin defecto)
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      {/each}
    </div>
  {/if}
</div>
</ProtectedRoute>

<style>
  .informes-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 32px;
  }

  .informes-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 32px;
    padding-bottom: 20px;
    border-bottom: 2px solid #E5E7EB;
  }

  .informes-header h1 {
    color: #1F2937;
    font-size: 28px;
    font-weight: 700;
    margin: 0;
  }

  .header-actions {
    display: flex;
    gap: 12px;
  }

  .btn-print {
    background: linear-gradient(135deg, #3B82F6, #1D4ED8);
    color: white;
    border: none;
    padding: 12px 24px;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .btn-print:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(59, 130, 246, 0.4);
  }

  .btn-secondary {
    background: #F3F4F6;
    color: #374151;
    border: 1px solid #D1D5DB;
    padding: 8px 16px;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
  }

  .btn-secondary:hover {
    background: #E5E7EB;
  }

  .loading-state, .error-state, .empty-state {
    text-align: center;
    padding: 60px 20px;
  }

  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #E5E7EB;
    border-top: 4px solid #3B82F6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin: 0 auto 20px;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  .report-content {
    background: white;
  }

  .report-section {
    margin-bottom: 48px;
  }

  .report-header {
    margin-bottom: 24px;
    border-bottom: 2px solid #E5E7EB;
    padding-bottom: 16px;
  }

  .report-header h2 {
    color: #1F2937;
    font-size: 24px;
    font-weight: 700;
    margin: 0 0 12px 0;
  }

  .camion-info {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 8px;
    margin-top: 12px;
  }

  .camion-info p {
    margin: 0;
    font-size: 14px;
    color: #6B7280;
  }

  .report-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
    margin: 16px 0;
  }

  .report-table th,
  .report-table td {
    border: 1px solid #D1D5DB;
    padding: 12px;
    text-align: left;
  }

  .report-table th {
    background: #F9FAFB;
    font-weight: 600;
    color: #374151;
  }

  .pallet-id {
    font-family: monospace;
    font-weight: 600;
    color: #1F2937;
  }

  .estado-cell {
    text-align: center;
  }

  .estado-badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .estado-badge:not(.defecto) {
    background: #D1FAE5;
    color: #065F46;
  }

  .estado-badge.defecto {
    background: #FEE2E2;
    color: #991B1B;
  }

  .fecha-cell {
    font-family: monospace;
    color: #6B7280;
  }

  .defecto-row {
    background: #FEF2F2;
  }

  .summary-row {
    background: #F9FAFB;
    border-top: 2px solid #D1D5DB;
  }

  .summary-row td {
    font-weight: 600;
    color: #374151;
  }

  /* Estilos para impresión */
  @media print {
    .no-print {
      display: none !important;
    }

    .informes-container {
      max-width: none;
      padding: 0;
      margin: 0;
    }

    .report-content {
      background: white;
    }

    .page-break {
      page-break-before: always;
    }

    .report-section {
      margin-bottom: 32px;
    }

    .report-table {
      font-size: 12px;
    }

    .report-table th,
    .report-table td {
      padding: 8px;
    }

    .report-header h2 {
      font-size: 20px;
    }

    .camion-info {
      grid-template-columns: repeat(2, 1fr);
    }

    .estado-badge {
      font-size: 10px;
      padding: 2px 8px;
    }
  }

  @media (max-width: 768px) {
    .informes-container {
      padding: 16px;
    }

    .informes-header {
      flex-direction: column;
      gap: 16px;
      align-items: flex-start;
    }

    .camion-info {
      grid-template-columns: 1fr;
    }

    .report-table {
      font-size: 12px;
    }

    .report-table th,
    .report-table td {
      padding: 8px 6px;
    }
  }
</style>