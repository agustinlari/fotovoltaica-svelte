<script lang="ts">
  import { onMount } from 'svelte';
  import type { Camion, Pallet } from '../api';
  import { apiGet, formatUpdateDate } from '../api';
  import { formatDdMmYyyyFromIso } from '../date';
  import ProtectedRoute from './ProtectedRoute.svelte';
  import { Printer } from 'lucide-svelte';

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
      const [camionesData, palletsData] = await Promise.all([
        apiGet<Camion[]>('/camiones'),
        apiGet<Pallet[]>('/pallets')
      ]);
      state.camiones = camionesData;
      state.pallets = palletsData;
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
      .filter(data => data.pallets.length > 0)
      .sort((a, b) => a.camion.id - b.camion.id);
  }

  function printReport() {
    window.print();
  }

  onMount(load);
</script>

<ProtectedRoute>
<div class="container">
  <div class="header no-print">
    <h2>Informes de Descarga</h2>
    <button class="btn-primary" on:click={printReport}>
      <Printer size={16} /> Imprimir
    </button>
  </div>

  {#if state.loading}
    <div class="empty-state no-print"><div class="spinner"></div><p>Generando informes...</p></div>
  {:else if state.error}
    <div class="empty-state no-print">
      <p class="error-text">{state.error}</p>
      <button class="btn-secondary" on:click={load}>Reintentar</button>
    </div>
  {:else if state.reportData.length === 0}
    <div class="empty-state no-print">
      <h3>No hay datos</h3>
      <p>No se encontraron pallets asignados a camiones</p>
    </div>
  {:else}
    <div class="report-content">
      {#each state.reportData as data, index (data.camion.id)}
        <div class="report-section" class:page-break={index > 0}>
          <div class="report-header">
            <h3>Descarga {data.camion.id}</h3>
            <div class="camion-info">
              <p><strong>Matricula:</strong> {data.camion.Matricula || 'No especificada'}</p>
              <p><strong>Conductor:</strong> {data.camion.NombreConductor || 'No especificado'}</p>
              <p><strong>DNI:</strong> {data.camion.DNI || 'No especificado'}</p>
              {#if data.camion.FechaDescarga}
                <p><strong>Fecha:</strong> {formatDdMmYyyyFromIso(data.camion.FechaDescarga)}</p>
              {/if}
            </div>
          </div>

          <table class="report-table">
            <thead>
              <tr><th>ID Pallet</th><th>Estado</th><th>Fecha</th></tr>
            </thead>
            <tbody>
              {#each data.pallets as pallet (pallet.id)}
                <tr class:defecto-row={pallet.Defecto}>
                  <td class="pallet-id">{pallet.id}</td>
                  <td class="estado-cell">
                    <span class="badge" class:defecto={pallet.Defecto}>
                      {pallet.Defecto ? 'CON DEFECTO' : 'SIN DEFECTO'}
                    </span>
                  </td>
                  <td class="fecha-cell">
                    {#if pallet.updated_at}{formatDdMmYyyyFromIso(pallet.updated_at)}{:else}-{/if}
                  </td>
                </tr>
              {/each}
            </tbody>
            <tfoot>
              <tr class="summary-row">
                <td><strong>Total:</strong></td>
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
  .container { max-width: 1200px; margin: 0 auto; padding: 20px; }

  .header {
    display: flex; justify-content: space-between; align-items: center;
    margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px solid #E5E7EB;
  }
  .header h2 { font-size: 18px; font-weight: 600; color: #1F2937; margin: 0; }

  .btn-primary {
    display: flex; align-items: center; gap: 6px;
    background: #1F2937; color: white; border: none; padding: 10px 18px;
    border-radius: 10px; font-weight: 500; font-size: 14px; cursor: pointer; transition: all 0.2s;
  }
  .btn-primary:hover { background: #111827; }

  .btn-secondary {
    background: white; color: #374151; border: 1px solid #D1D5DB;
    padding: 8px 16px; border-radius: 8px; font-weight: 500; font-size: 14px;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-secondary:hover { background: #F3F4F6; }

  .empty-state { text-align: center; padding: 60px 20px; color: #6B7280; }
  .empty-state h3 { color: #374151; font-size: 16px; margin: 0 0 8px; }
  .empty-state p { margin: 0 0 16px; font-size: 14px; }
  .error-text { color: #DC2626; }

  .spinner {
    width: 32px; height: 32px; border: 3px solid #E5E7EB; border-top: 3px solid #3B82F6;
    border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px;
  }
  @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

  .report-section { margin-bottom: 40px; }
  .report-header { margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid #E5E7EB; }
  .report-header h3 { font-size: 16px; font-weight: 600; color: #1F2937; margin: 0 0 8px; }
  .camion-info { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 4px; }
  .camion-info p { margin: 0; font-size: 14px; color: #6B7280; }

  .report-table { width: 100%; border-collapse: collapse; font-size: 14px; }
  .report-table th, .report-table td { border: 1px solid #E5E7EB; padding: 10px 12px; text-align: left; }
  .report-table th { background: #F9FAFB; font-weight: 500; color: #6B7280; font-size: 13px; }
  .pallet-id { font-family: monospace; font-weight: 600; color: #1F2937; }
  .estado-cell { text-align: center; }
  .badge {
    display: inline-block; padding: 3px 10px; border-radius: 10px;
    font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;
    background: #D1FAE5; color: #065F46;
  }
  .badge.defecto { background: #FEE2E2; color: #991B1B; }
  .fecha-cell { font-family: monospace; color: #6B7280; }
  .defecto-row { background: #FEF2F2; }
  .summary-row { background: #F9FAFB; border-top: 2px solid #E5E7EB; }
  .summary-row td { font-size: 13px; color: #374151; }

  @media print {
    .no-print { display: none !important; }
    .container { max-width: none; padding: 0; margin: 0; }
    .page-break { page-break-before: always; }
    .report-section { margin-bottom: 24px; }
    .report-table { font-size: 12px; }
    .report-table th, .report-table td { padding: 6px 8px; }
    .report-header h3 { font-size: 16px; }
    .badge { font-size: 10px; padding: 2px 8px; }
  }

  @media (max-width: 768px) {
    .container { padding: 16px; }
    .header { flex-direction: column; gap: 12px; align-items: flex-start; }
    .camion-info { grid-template-columns: 1fr; }
    .report-table { font-size: 12px; }
    .report-table th, .report-table td { padding: 8px 6px; }
  }
</style>
