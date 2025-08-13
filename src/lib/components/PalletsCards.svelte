<script lang="ts">
  import { onMount } from 'svelte';
  import type { Pallet, Camion } from '../api';
  import { apiGet, apiPost, apiPut, apiDelete } from '../api';
  import ProtectedRoute from './ProtectedRoute.svelte';

  type State = {
    loading: boolean;
    items: Pallet[];
    camiones: Camion[];
    error: string | null;
    showCreateModal: boolean;
    showEditModal: boolean;
    showDeleteModal: boolean;
    selectedPallet: Pallet | null;
    isSubmitting: boolean;
    loadingCamiones: boolean;
  };

  let state: State = {
    loading: true,
    items: [],
    camiones: [],
    error: null,
    showCreateModal: false,
    showEditModal: false,
    showDeleteModal: false,
    selectedPallet: null,
    isSubmitting: false,
    loadingCamiones: false,
  };

  let newPallet: Partial<Pallet> = {};
  let editPallet: Partial<Pallet> = {};

  async function load() {
    try {
      state.loading = true;
      state.error = null;
      state.items = await apiGet<Pallet[]>('/pallets');
    } catch (e: any) {
      state.error = e?.message || 'Error cargando pallets';
    } finally {
      state.loading = false;
    }
  }

  async function loadCamiones() {
    if (state.camiones.length > 0) return; // Ya cargados
    
    try {
      state.loadingCamiones = true;
      state.camiones = await apiGet<Camion[]>('/camiones');
    } catch (e: any) {
      console.error('Error cargando camiones:', e);
    } finally {
      state.loadingCamiones = false;
    }
  }

  function getCamionInfo(descargaId: number | null): string {
    if (!descargaId) return 'Sin asignar';
    const camion = state.camiones.find(c => c.id === descargaId);
    return camion ? `#${camion.id} - ${camion.Matricula || 'Sin matrícula'}` : `Camión #${descargaId}`;
  }

  function openCreateModal() {
    newPallet = {
      id: '',
      Descarga: null,
      Defecto: false
    };
    state.showCreateModal = true;
    loadCamiones();
  }

  function openEditModal(pallet: Pallet) {
    editPallet = { ...pallet };
    state.selectedPallet = pallet;
    state.showEditModal = true;
    loadCamiones();
  }

  function openDeleteModal(pallet: Pallet) {
    state.selectedPallet = pallet;
    state.showDeleteModal = true;
  }

  async function createPallet() {
    if (state.isSubmitting) return;
    
    try {
      state.isSubmitting = true;
      
      // Convertir strings vacíos a null para Descarga
      if (newPallet.Descarga === '') {
        newPallet.Descarga = null;
      } else if (newPallet.Descarga) {
        newPallet.Descarga = Number(newPallet.Descarga);
      }
      
      const created = await apiPost<Pallet>('/pallets', newPallet);
      state.items = [...state.items, created];
      state.showCreateModal = false;
    } catch (e: any) {
      state.error = e?.message || 'Error creando pallet';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function updatePallet() {
    if (state.isSubmitting || !state.selectedPallet) return;
    
    try {
      state.isSubmitting = true;
      
      // Convertir strings vacíos a null para Descarga
      if (editPallet.Descarga === '') {
        editPallet.Descarga = null;
      } else if (editPallet.Descarga) {
        editPallet.Descarga = Number(editPallet.Descarga);
      }
      
      await apiPut(`/pallets/${state.selectedPallet.id}`, editPallet);
      
      // Actualizar la lista local
      const index = state.items.findIndex(item => item.id === state.selectedPallet!.id);
      if (index >= 0) {
        state.items[index] = { ...state.items[index], ...editPallet };
      }
      
      state.showEditModal = false;
      state.selectedPallet = null;
    } catch (e: any) {
      state.error = e?.message || 'Error actualizando pallet';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function deletePallet() {
    if (state.isSubmitting || !state.selectedPallet) return;
    
    try {
      state.isSubmitting = true;
      await apiDelete(`/pallets/${state.selectedPallet.id}`);
      
      // Remover de la lista local
      state.items = state.items.filter(item => item.id !== state.selectedPallet!.id);
      
      state.showDeleteModal = false;
      state.selectedPallet = null;
    } catch (e: any) {
      state.error = e?.message || 'Error eliminando pallet';
    } finally {
      state.isSubmitting = false;
    }
  }

  function closeModals() {
    state.showCreateModal = false;
    state.showEditModal = false;
    state.showDeleteModal = false;
    state.selectedPallet = null;
    state.error = null;
  }

  onMount(() => {
    load();
    loadCamiones();
  });
</script>

<ProtectedRoute>
<div class="pallets-container">
  <div class="pallets-header">
    <div class="header-content">
      <h2>Gestión de Pallets</h2>
      <p>Administra la información de los pallets y su contenido</p>
    </div>
    <button class="btn-primary" on:click={openCreateModal}>
      + Nuevo Pallet
    </button>
  </div>

  {#if state.loading}
    <div class="loading-state">
      <div class="spinner"></div>
      <p>Cargando pallets...</p>
    </div>
  {:else if state.error}
    <div class="error-state">
      <p>❌ {state.error}</p>
      <button class="btn-secondary" on:click={load}>Reintentar</button>
    </div>
  {:else if state.items.length === 0}
    <div class="empty-state">
      <h3>No hay pallets registrados</h3>
      <p>Comienza creando tu primer pallet</p>
      <button class="btn-primary" on:click={openCreateModal}>
        + Crear Primer Pallet
      </button>
    </div>
  {:else}
    <div class="pallets-grid">
      {#each state.items as pallet (pallet.id)}
        <div class="pallet-card" class:defecto={pallet.Defecto}>
          <div class="card-header">
            <div class="pallet-id">{pallet.id}</div>
            <div class="card-actions">
              <button class="btn-edit" on:click={() => openEditModal(pallet)} title="Editar">
                ✏️
              </button>
              <button class="btn-delete" on:click={() => openDeleteModal(pallet)} title="Eliminar">
                🗑️
              </button>
            </div>
          </div>
          
          <div class="card-content">
            <div class="main-info">
              <h3>Pallet {pallet.id}</h3>
              <div class="status-badges">
                {#if pallet.Defecto}
                  <span class="badge badge-danger">Con Defecto</span>
                {:else}
                  <span class="badge badge-success">Sin Defecto</span>
                {/if}
              </div>
            </div>
            
            <div class="details-grid">
              <div class="detail-item full-width">
                <label>Camión Asignado</label>
                <span class="camion-info">
                  {getCamionInfo(pallet.Descarga)}
                </span>
              </div>
              
              {#if pallet.updated_at}
                <div class="detail-item full-width">
                  <label>Última Actualización</label>
                  <span>{new Date(pallet.updated_at).toLocaleDateString()}</span>
                </div>
              {/if}
            </div>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>

<!-- Modal Crear Pallet -->
{#if state.showCreateModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Crear Nuevo Pallet</h3>
        <button class="modal-close" on:click={closeModals}>×</button>
      </div>
      
      <form class="modal-form" on:submit|preventDefault={createPallet}>
        <div class="form-group">
          <label for="new-id">ID del Pallet *</label>
          <input 
            id="new-id" 
            bind:value={newPallet.id} 
            required
            placeholder="Ej: PLT001, PALLET-123..."
          >
        </div>
        
        <div class="form-group">
          <label for="new-descarga">Camión de Descarga</label>
          {#if state.loadingCamiones}
            <p class="loading-text">Cargando camiones...</p>
          {:else}
            <select id="new-descarga" bind:value={newPallet.Descarga}>
              <option value="">Sin asignar</option>
              {#each state.camiones as camion}
                <option value={camion.id}>
                  #{camion.id} - {camion.Matricula || 'Sin matrícula'} 
                  {camion.NombreConductor ? `(${camion.NombreConductor})` : ''}
                </option>
              {/each}
            </select>
          {/if}
        </div>
        
        <div class="form-group">
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              bind:checked={newPallet.Defecto}
            >
            <span class="checkbox-text">Marcar como defectuoso</span>
          </label>
        </div>
        
        {#if state.error}
          <div class="form-error">{state.error}</div>
        {/if}
        
        <div class="modal-actions">
          <button type="button" class="btn-secondary" on:click={closeModals}>
            Cancelar
          </button>
          <button type="submit" class="btn-primary" disabled={state.isSubmitting}>
            {state.isSubmitting ? 'Creando...' : 'Crear Pallet'}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

<!-- Modal Editar Pallet -->
{#if state.showEditModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Editar Pallet {state.selectedPallet?.id}</h3>
        <button class="modal-close" on:click={closeModals}>×</button>
      </div>
      
      <form class="modal-form" on:submit|preventDefault={updatePallet}>
        <div class="form-group">
          <label for="edit-id">ID del Pallet *</label>
          <input 
            id="edit-id" 
            bind:value={editPallet.id} 
            required
            placeholder="Ej: PLT001, PALLET-123..."
          >
        </div>
        
        <div class="form-group">
          <label for="edit-descarga">Camión de Descarga</label>
          {#if state.loadingCamiones}
            <p class="loading-text">Cargando camiones...</p>
          {:else}
            <select id="edit-descarga" bind:value={editPallet.Descarga}>
              <option value="">Sin asignar</option>
              {#each state.camiones as camion}
                <option value={camion.id}>
                  #{camion.id} - {camion.Matricula || 'Sin matrícula'} 
                  {camion.NombreConductor ? `(${camion.NombreConductor})` : ''}
                </option>
              {/each}
            </select>
          {/if}
        </div>
        
        <div class="form-group">
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              bind:checked={editPallet.Defecto}
            >
            <span class="checkbox-text">Marcar como defectuoso</span>
          </label>
        </div>
        
        {#if state.error}
          <div class="form-error">{state.error}</div>
        {/if}
        
        <div class="modal-actions">
          <button type="button" class="btn-secondary" on:click={closeModals}>
            Cancelar
          </button>
          <button type="submit" class="btn-primary" disabled={state.isSubmitting}>
            {state.isSubmitting ? 'Guardando...' : 'Guardar Cambios'}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

<!-- Modal Eliminar Pallet -->
{#if state.showDeleteModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal modal-small" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Confirmar Eliminación</h3>
        <button class="modal-close" on:click={closeModals}>×</button>
      </div>
      
      <div class="modal-content">
        <p>¿Estás seguro de que quieres eliminar el pallet <strong>{state.selectedPallet?.id}</strong>?</p>
        <p class="warning-text">Esta acción no se puede deshacer.</p>
        
        {#if state.error}
          <div class="form-error">{state.error}</div>
        {/if}
      </div>
      
      <div class="modal-actions">
        <button type="button" class="btn-secondary" on:click={closeModals}>
          Cancelar
        </button>
        <button class="btn-danger" on:click={deletePallet} disabled={state.isSubmitting}>
          {state.isSubmitting ? 'Eliminando...' : 'Eliminar'}
        </button>
      </div>
    </div>
  </div>
{/if}
</ProtectedRoute>

<style>
  .pallets-container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 24px;
  }
  
  .pallets-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 32px;
    padding-bottom: 20px;
    border-bottom: 2px solid #E5E7EB;
  }
  
  .header-content h2 {
    font-size: 28px;
    font-weight: 700;
    color: #1F2937;
    margin: 0 0 8px 0;
  }
  
  .header-content p {
    color: #6B7280;
    margin: 0;
    font-size: 16px;
  }
  
  .btn-primary {
    background: linear-gradient(135deg, #F59E0B, #D97706);
    color: white;
    border: none;
    padding: 12px 24px;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
  }
  
  .btn-primary:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(245, 158, 11, 0.4);
  }
  
  .btn-primary:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
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
  
  .btn-danger {
    background: linear-gradient(135deg, #EF4444, #DC2626);
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
  }
  
  .btn-danger:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 6px 16px rgba(239, 68, 68, 0.4);
  }
  
  .loading-state, .error-state, .empty-state {
    text-align: center;
    padding: 60px 20px;
  }
  
  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #E5E7EB;
    border-top: 4px solid #F59E0B;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin: 0 auto 20px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .pallets-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
    gap: 24px;
  }
  
  .pallet-card {
    background: white;
    border: 1px solid #E5E7EB;
    border-radius: 12px;
    overflow: hidden;
    transition: all 0.3s ease;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .pallet-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
    border-color: #F59E0B;
  }
  
  .pallet-card.defecto {
    border-color: #EF4444;
  }
  
  .pallet-card.defecto:hover {
    border-color: #DC2626;
  }
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px 20px;
    background: #FFFBEB;
    border-bottom: 1px solid #FED7AA;
  }
  
  .pallet-card.defecto .card-header {
    background: #FEF2F2;
    border-bottom-color: #FECACA;
  }
  
  .pallet-id {
    font-weight: 600;
    color: #D97706;
    font-size: 14px;
    font-family: 'Courier New', monospace;
  }
  
  .pallet-card.defecto .pallet-id {
    color: #DC2626;
  }
  
  .card-actions {
    display: flex;
    gap: 8px;
  }
  
  .btn-edit, .btn-delete {
    background: none;
    border: none;
    padding: 8px;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 16px;
  }
  
  .btn-edit:hover {
    background: #FEF3C7;
  }
  
  .btn-delete:hover {
    background: #FEE2E2;
  }
  
  .card-content {
    padding: 20px;
  }
  
  .main-info {
    margin-bottom: 20px;
  }
  
  .main-info h3 {
    font-size: 18px;
    font-weight: 600;
    color: #1F2937;
    margin: 0 0 12px 0;
    font-family: 'Courier New', monospace;
  }
  
  .status-badges {
    display: flex;
    gap: 8px;
  }
  
  .badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .badge-success {
    background: #D1FAE5;
    color: #065F46;
  }
  
  .badge-danger {
    background: #FEE2E2;
    color: #991B1B;
  }
  
  .details-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .detail-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }
  
  .detail-item.full-width {
    grid-column: span 1;
  }
  
  .detail-item label {
    font-size: 12px;
    font-weight: 600;
    color: #6B7280;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .detail-item span {
    font-size: 14px;
    color: #1F2937;
    font-weight: 500;
  }
  
  .camion-info {
    font-family: 'Courier New', monospace;
    background: #F3F4F6;
    padding: 8px 12px;
    border-radius: 6px;
    border: 1px solid #D1D5DB;
  }
  
  /* Modal Styles */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    padding: 20px;
  }
  
  .modal {
    background: white;
    border-radius: 12px;
    width: 100%;
    max-width: 500px;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  }
  
  .modal-small {
    max-width: 400px;
  }
  
  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 24px 24px 0;
    margin-bottom: 20px;
  }
  
  .modal-header h3 {
    font-size: 20px;
    font-weight: 600;
    color: #1F2937;
    margin: 0;
  }
  
  .modal-close {
    background: none;
    border: none;
    font-size: 24px;
    cursor: pointer;
    color: #6B7280;
    padding: 4px;
    border-radius: 4px;
  }
  
  .modal-close:hover {
    background: #F3F4F6;
  }
  
  .modal-form {
    padding: 0 24px;
  }
  
  .modal-content {
    padding: 0 24px;
  }
  
  .form-group {
    margin-bottom: 20px;
  }
  
  .form-group label {
    display: block;
    font-weight: 600;
    color: #374151;
    margin-bottom: 6px;
    font-size: 14px;
  }
  
  .form-group input, .form-group select {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid #D1D5DB;
    border-radius: 6px;
    font-size: 14px;
    transition: border-color 0.2s ease;
    box-sizing: border-box;
  }
  
  .form-group input:focus, .form-group select:focus {
    outline: none;
    border-color: #F59E0B;
    box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.1);
  }
  
  .checkbox-label {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
    padding: 12px;
    border: 1px solid #D1D5DB;
    border-radius: 6px;
    transition: all 0.2s ease;
  }
  
  .checkbox-label:hover {
    background: #F9FAFB;
  }
  
  .checkbox-label input[type="checkbox"] {
    width: auto;
    margin: 0;
  }
  
  .checkbox-text {
    font-weight: 500;
    color: #374151;
  }
  
  .loading-text {
    color: #6B7280;
    font-style: italic;
    margin: 0;
    padding: 10px 12px;
  }
  
  .form-error {
    background: #FEE2E2;
    color: #DC2626;
    padding: 12px;
    border-radius: 6px;
    margin-bottom: 20px;
    font-size: 14px;
  }
  
  .warning-text {
    color: #DC2626;
    font-size: 14px;
    margin: 8px 0 0 0;
  }
  
  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding: 20px 24px 24px;
    border-top: 1px solid #E5E7EB;
    margin-top: 20px;
  }
  
  @media (max-width: 768px) {
    .pallets-header {
      flex-direction: column;
      gap: 16px;
      align-items: flex-start;
    }
    
    .pallets-grid {
      grid-template-columns: 1fr;
      gap: 16px;
    }
    
    .modal {
      margin: 20px;
      max-height: calc(100vh - 40px);
    }
  }
</style>