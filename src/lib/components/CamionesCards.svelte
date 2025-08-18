<script lang="ts">
  import { onMount } from 'svelte';
  import type { Camion } from '../api';
  import { apiGet, apiPost, apiPut, apiDelete, getUserEmail, formatUpdateDate } from '../api';
  import { formatDdMmYyyyFromIso, formatYyyyMmDdFromIso, toIsoMidnight } from '../date';
  import ProtectedRoute from './ProtectedRoute.svelte';
  import PhotoGallery from './PhotoGallery.svelte';
  import BarcodeScanner from './BarcodeScanner.svelte';
  
  // Estado para almacenar emails de usuarios
  let userEmails: Map<string, string> = new Map();
  
  // Función para obtener y cachear el email del usuario
  async function getUserDisplayName(keycloakId: string | null): Promise<string> {
    if (!keycloakId) return 'Usuario desconocido';
    
    if (userEmails.has(keycloakId)) {
      return userEmails.get(keycloakId)!;
    }
    
    const email = await getUserEmail(keycloakId);
    userEmails.set(keycloakId, email);
    userEmails = userEmails; // Reactivity
    return email;
  }

  type State = {
    loading: boolean;
    items: Camion[];
    filteredItems: Camion[];
    error: string | null;
    showCreateModal: boolean;
    showEditModal: boolean;
    showDeleteModal: boolean;
    selectedCamion: Camion | null;
    isSubmitting: boolean;
    searchQuery: string;
    showBarcodeScanner: boolean;
  };

  let state: State = {
    loading: true,
    items: [],
    filteredItems: [],
    error: null,
    showCreateModal: false,
    showEditModal: false,
    showDeleteModal: false,
    selectedCamion: null,
    isSubmitting: false,
    searchQuery: '',
    showBarcodeScanner: false,
  };

  let newCamion: Partial<Camion> = {};
  let editCamion: Partial<Camion> = {};
  let photoGalleryRef: any = null;

  async function load() {
    try {
      state.loading = true;
      state.error = null;
      state.items = await apiGet<Camion[]>('/camiones');
      filterItems();
    } catch (e: any) {
      state.error = e?.message || 'Error cargando camiones';
    } finally {
      state.loading = false;
    }
  }

  function filterItems() {
    if (!state.searchQuery.trim()) {
      state.filteredItems = [...state.items];
      return;
    }

    const query = state.searchQuery.toLowerCase().trim();
    state.filteredItems = state.items.filter(camion => {
      const searchableFields = [
        camion.DNI,
        camion.Matricula,
        camion.UbicacionCampa,
        camion.Container,
        camion.Albaran,
        camion.NombreConductor,
        camion.id?.toString(),
        // Buscar también en fecha formateada
        camion.FechaDescarga ? formatDdMmYyyyFromIso(camion.FechaDescarga) : ''
      ];

      return searchableFields.some(field => 
        field && field.toString().toLowerCase().includes(query)
      );
    });
  }

  function handleSearch(event: Event) {
    const target = event.target as HTMLInputElement;
    state.searchQuery = target.value;
    filterItems();
  }

  function clearSearch() {
    state.searchQuery = '';
    filterItems();
  }

  function openBarcodeScanner() {
    state.showBarcodeScanner = true;
  }

  function handleBarcodeScan(event: CustomEvent<string>) {
    const scannedCode = event.detail;
    state.searchQuery = scannedCode;
    filterItems();
    state.showBarcodeScanner = false;
  }

  function closeBarcodeScanner() {
    state.showBarcodeScanner = false;
  }

  function openCreateModal() {
    newCamion = {
      DNI: '',
      Matricula: '',
      UbicacionCampa: '',
      Container: '',
      Albaran: '',
      NombreConductor: '',
      FechaDescarga: null
    };
    state.showCreateModal = true;
  }

  function openEditModal(camion: Camion) {
    editCamion = { 
      ...camion,
      // Convertir fecha ISO a formato YYYY-MM-DD para el input
      FechaDescarga: camion.FechaDescarga ? formatYyyyMmDdFromIso(camion.FechaDescarga) : null
    };
    state.selectedCamion = camion;
    state.showEditModal = true;
  }

  function openDeleteModal(camion: Camion) {
    state.selectedCamion = camion;
    state.showDeleteModal = true;
  }

  async function createCamion() {
    if (state.isSubmitting) return;
    
    try {
      state.isSubmitting = true;
      // Procesar fecha si existe
      if (newCamion.FechaDescarga) {
        newCamion.FechaDescarga = toIsoMidnight(newCamion.FechaDescarga);
      }
      
      const created = await apiPost<Camion>('/camiones', newCamion);
      state.items = [...state.items, created];
      filterItems();
      state.showCreateModal = false;
    } catch (e: any) {
      state.error = e?.message || 'Error creando camión';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function updateCamion() {
    if (state.isSubmitting || !state.selectedCamion) return;
    
    try {
      state.isSubmitting = true;
      // Procesar fecha si existe
      if (editCamion.FechaDescarga) {
        editCamion.FechaDescarga = toIsoMidnight(editCamion.FechaDescarga);
      }
      
      await apiPut(`/camiones/${state.selectedCamion.id}`, editCamion);
      
      // Actualizar la lista local
      const index = state.items.findIndex(item => item.id === state.selectedCamion!.id);
      if (index >= 0) {
        state.items[index] = { ...state.items[index], ...editCamion };
      }
      
      filterItems();
      state.showEditModal = false;
      state.selectedCamion = null;
    } catch (e: any) {
      state.error = e?.message || 'Error actualizando camión';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function deleteCamion() {
    if (state.isSubmitting || !state.selectedCamion) return;
    
    try {
      state.isSubmitting = true;
      await apiDelete(`/camiones/${state.selectedCamion.id}`);
      
      // Remover de la lista local
      state.items = state.items.filter(item => item.id !== state.selectedCamion!.id);
      
      filterItems();
      state.showDeleteModal = false;
      state.selectedCamion = null;
    } catch (e: any) {
      state.error = e?.message || 'Error eliminando camión';
    } finally {
      state.isSubmitting = false;
    }
  }

  function closeModals() {
    state.showCreateModal = false;
    state.showEditModal = false;
    state.showDeleteModal = false;
    state.selectedCamion = null;
    state.error = null;
  }

  onMount(load);
</script>

<ProtectedRoute>
<div class="camiones-container">
  <div class="camiones-header">
    <div class="header-content">
      <h2>Gestión de Camiones</h2>
      <p>Administra la información de los camiones y sus descargas</p>
    </div>
    <button class="btn-primary" on:click={openCreateModal}>
      + Nuevo Camión
    </button>
  </div>

  <!-- Buscador -->
  <div class="search-container">
    <div class="search-box">
      <div class="search-input-wrapper">
        <input
          type="text"
          placeholder="Buscar camiones por matrícula, conductor, DNI, ubicación..."
          bind:value={state.searchQuery}
          on:input={handleSearch}
          class="search-input"
        />
        {#if state.searchQuery}
          <button class="search-clear" on:click={clearSearch} title="Limpiar búsqueda">
            ×
          </button>
        {/if}
      </div>
      <button class="barcode-scanner-btn" on:click={openBarcodeScanner} title="Escanear código de barras">
        📷
      </button>
    </div>
    {#if state.searchQuery && state.filteredItems.length !== state.items.length}
      <div class="search-results">
        Mostrando {state.filteredItems.length} de {state.items.length} camiones
      </div>
    {/if}
  </div>

  {#if state.loading}
    <div class="loading-state">
      <div class="spinner"></div>
      <p>Cargando camiones...</p>
    </div>
  {:else if state.error}
    <div class="error-state">
      <p>❌ {state.error}</p>
      <button class="btn-secondary" on:click={load}>Reintentar</button>
    </div>
  {:else if state.items.length === 0}
    <div class="empty-state">
      <h3>No hay camiones registrados</h3>
      <p>Comienza creando tu primer camión</p>
      <button class="btn-primary" on:click={openCreateModal}>
        + Crear Primer Camión
      </button>
    </div>
  {:else if state.filteredItems.length === 0 && state.searchQuery}
    <div class="empty-state">
      <h3>No se encontraron camiones</h3>
      <p>No hay camiones que coincidan con "{state.searchQuery}"</p>
      <button class="btn-secondary" on:click={clearSearch}>
        Limpiar búsqueda
      </button>
    </div>
  {:else}
    <div class="camiones-grid">
      {#each state.filteredItems as camion (camion.id)}
        <div class="camion-card">
          <div class="card-header">
            <div class="camion-id">#{camion.id}</div>
            <div class="card-actions">
              <button class="btn-edit" on:click={() => openEditModal(camion)} title="Editar">
                ✏️
              </button>
              <button class="btn-delete" on:click={() => openDeleteModal(camion)} title="Eliminar">
                🗑️
              </button>
            </div>
          </div>
          
          <div class="card-content">
            <div class="main-info">
              <h3>{camion.Matricula || 'Sin matrícula'}</h3>
              <p class="conductor">{camion.NombreConductor || 'Sin conductor'}</p>
            </div>
            
            <div class="details-grid">
              <div class="detail-item">
                <label>DNI</label>
                <span>{camion.DNI || '-'}</span>
              </div>
              <div class="detail-item">
                <label>Ubicación</label>
                <span>{camion.UbicacionCampa || '-'}</span>
              </div>
              <div class="detail-item">
                <label>Container</label>
                <span>{camion.Container || '-'}</span>
              </div>
              <div class="detail-item">
                <label>Albarán</label>
                <span>{camion.Albaran || '-'}</span>
              </div>
              <div class="detail-item full-width">
                <label>Fecha Descarga</label>
                <span>{camion.FechaDescarga ? formatDdMmYyyyFromIso(camion.FechaDescarga) : '-'}</span>
              </div>
            </div>

            <!-- Información de auditoría -->
            {#if camion.updated_at || camion.updated_by}
              <div class="audit-info">
                <div class="audit-item">
                  <label>Última actualización</label>
                  <span class="audit-details">
                    {#if camion.updated_at}
                      {formatUpdateDate(camion.updated_at)}
                    {/if}
                    {#if camion.updated_by}
                      {#await getUserDisplayName(camion.updated_by)}
                        <span class="user-loading">...</span>
                      {:then email}
                        <span class="user-name">por {email}</span>
                      {:catch}
                        <span class="user-name">por {camion.updated_by}</span>
                      {/await}
                    {/if}
                  </span>
                </div>
              </div>
            {/if}

            <!-- Vista previa de fotos (solo lectura) -->
            <PhotoGallery tableName="camiones" recordId={camion.id} readonly={true} />
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>

<!-- Modal Crear Camión -->
{#if state.showCreateModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Crear Nuevo Camión</h3>
        <button class="modal-close" on:click={closeModals}>×</button>
      </div>
      
      <form class="modal-form" on:submit|preventDefault={createCamion}>
        <div class="form-row">
          <div class="form-group">
            <label for="new-matricula">Matrícula *</label>
            <input id="new-matricula" bind:value={newCamion.Matricula} required>
          </div>
          <div class="form-group">
            <label for="new-dni">DNI</label>
            <input id="new-dni" bind:value={newCamion.DNI}>
          </div>
        </div>
        
        <div class="form-group">
          <label for="new-conductor">Nombre Conductor</label>
          <input id="new-conductor" bind:value={newCamion.NombreConductor}>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="new-ubicacion">Ubicación Campa</label>
            <input id="new-ubicacion" bind:value={newCamion.UbicacionCampa}>
          </div>
          <div class="form-group">
            <label for="new-container">Container</label>
            <input id="new-container" bind:value={newCamion.Container}>
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="new-albaran">Albarán</label>
            <input id="new-albaran" bind:value={newCamion.Albaran}>
          </div>
          <div class="form-group">
            <label for="new-fecha">Fecha Descarga</label>
            <input id="new-fecha" type="date" bind:value={newCamion.FechaDescarga}>
          </div>
        </div>
        
        {#if state.error}
          <div class="form-error">{state.error}</div>
        {/if}
        
        <div class="modal-actions">
          <button type="button" class="btn-secondary" on:click={closeModals}>
            Cancelar
          </button>
          <button type="submit" class="btn-primary" disabled={state.isSubmitting}>
            {state.isSubmitting ? 'Creando...' : 'Crear Camión'}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

<!-- Modal Editar Camión -->
{#if state.showEditModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Editar Camión #{state.selectedCamion?.id}</h3>
        <button class="modal-close" on:click={closeModals}>×</button>
      </div>
      
      <form class="modal-form" on:submit|preventDefault={updateCamion}>
        <div class="form-row">
          <div class="form-group">
            <label for="edit-matricula">Matrícula *</label>
            <input id="edit-matricula" bind:value={editCamion.Matricula} required>
          </div>
          <div class="form-group">
            <label for="edit-dni">DNI</label>
            <input id="edit-dni" bind:value={editCamion.DNI}>
          </div>
        </div>
        
        <div class="form-group">
          <label for="edit-conductor">Nombre Conductor</label>
          <input id="edit-conductor" bind:value={editCamion.NombreConductor}>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="edit-ubicacion">Ubicación Campa</label>
            <input id="edit-ubicacion" bind:value={editCamion.UbicacionCampa}>
          </div>
          <div class="form-group">
            <label for="edit-container">Container</label>
            <input id="edit-container" bind:value={editCamion.Container}>
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="edit-albaran">Albarán</label>
            <input id="edit-albaran" bind:value={editCamion.Albaran}>
          </div>
          <div class="form-group">
            <label for="edit-fecha">Fecha Descarga</label>
            <input 
              id="edit-fecha" 
              type="date" 
              bind:value={editCamion.FechaDescarga}
            >
          </div>
        </div>
        
        <!-- Gestión de Fotos -->
        <div class="photos-section">
          <h4>Fotos</h4>
          <div class="photo-controls">
            <button type="button" class="btn-upload" on:click={() => photoGalleryRef?.triggerGallery()} disabled={photoGalleryRef?.getState()?.uploading}>
              📁 Galería
            </button>
            <button type="button" class="btn-upload" on:click={() => photoGalleryRef?.triggerCamera()} disabled={photoGalleryRef?.getState()?.uploading}>
              📷 Cámara
            </button>
          </div>
          <PhotoGallery bind:this={photoGalleryRef} tableName="camiones" recordId={state.selectedCamion?.id} readonly={false} modalMode={true} />
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

<!-- Modal Eliminar Camión -->
{#if state.showDeleteModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal modal-small" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Confirmar Eliminación</h3>
        <button class="modal-close" on:click={closeModals}>×</button>
      </div>
      
      <div class="modal-content">
        <p>¿Estás seguro de que quieres eliminar el camión <strong>#{state.selectedCamion?.id}</strong>?</p>
        <p class="warning-text">Esta acción no se puede deshacer.</p>
        
        {#if state.error}
          <div class="form-error">{state.error}</div>
        {/if}
      </div>
      
      <div class="modal-actions">
        <button type="button" class="btn-secondary" on:click={closeModals}>
          Cancelar
        </button>
        <button class="btn-danger" on:click={deleteCamion} disabled={state.isSubmitting}>
          {state.isSubmitting ? 'Eliminando...' : 'Eliminar'}
        </button>
      </div>
    </div>
  </div>
{/if}

<!-- Escáner de códigos de barras -->
<BarcodeScanner
  isOpen={state.showBarcodeScanner}
  on:scan={handleBarcodeScan}
  on:close={closeBarcodeScanner}
/>
</ProtectedRoute>

<style>
  .camiones-container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 24px;
  }
  
  @media (max-width: 768px) {
    .camiones-container {
      padding: 12px;
    }
  }
  
  @media (max-width: 480px) {
    .camiones-container {
      padding: 8px;
    }
  }

  /* Search Styles */
  .search-container {
    margin-bottom: 24px;
  }

  .search-box {
    display: flex;
    align-items: center;
    gap: 12px;
    max-width: 600px;
    margin: 0 auto;
  }

  .search-input-wrapper {
    position: relative;
    flex: 1;
  }

  .search-input {
    width: 100%;
    padding: 12px 16px;
    padding-right: 40px;
    border: 2px solid #E5E7EB;
    border-radius: 8px;
    font-size: 16px;
    transition: border-color 0.2s ease;
    box-sizing: border-box;
  }

  .search-input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .search-clear {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    font-size: 20px;
    color: #6B7280;
    cursor: pointer;
    padding: 4px;
    border-radius: 4px;
    transition: all 0.2s ease;
  }

  .search-clear:hover {
    background: #F3F4F6;
    color: #374151;
  }

  .barcode-scanner-btn {
    font-size: 20px;
    color: #6B7280;
    padding: 12px;
    background: #F0FDF4;
    border: 2px solid #DCFCE7;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s ease;
  }

  .barcode-scanner-btn:hover {
    background: #10B981;
    color: white;
    border-color: #10B981;
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(16, 185, 129, 0.3);
  }

  .search-results {
    text-align: center;
    margin-top: 12px;
    font-size: 14px;
    color: #6B7280;
    padding: 8px 16px;
    background: #F0F9FF;
    border: 1px solid #BAE6FD;
    border-radius: 6px;
    max-width: 600px;
    margin: 12px auto 0;
  }
  
  .camiones-header {
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
    background: linear-gradient(135deg, #3B82F6, #1D4ED8);
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
    box-shadow: 0 8px 20px rgba(59, 130, 246, 0.4);
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
    border-top: 4px solid #3B82F6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin: 0 auto 20px;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
  
  .camiones-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
    gap: 24px;
  }
  
  @media (min-width: 1200px) {
    .camiones-grid {
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 20px;
    }
  }
  
  .camion-card {
    background: white;
    border: 1px solid #E5E7EB;
    border-radius: 12px;
    overflow: hidden;
    transition: all 0.3s ease;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .camion-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
    border-color: #3B82F6;
  }
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px 20px;
    background: #F8FAFC;
    border-bottom: 1px solid #E2E8F0;
  }
  
  .camion-id {
    font-weight: 600;
    color: #64748B;
    font-size: 14px;
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
  
  @media (min-width: 1200px) {
    .card-content {
      padding: 16px;
    }
  }
  
  .main-info {
    margin-bottom: 20px;
  }
  
  @media (min-width: 1200px) {
    .main-info {
      margin-bottom: 16px;
    }
  }
  
  .main-info h3 {
    font-size: 20px;
    font-weight: 600;
    color: #1F2937;
    margin: 0 0 4px 0;
  }
  
  .conductor {
    color: #6B7280;
    font-size: 14px;
    margin: 0;
  }
  
  .details-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
  }
  
  @media (min-width: 1200px) {
    .details-grid {
      gap: 12px;
    }
  }
  
  .detail-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }
  
  @media (min-width: 1200px) {
    .detail-item {
      gap: 2px;
    }
  }
  
  .detail-item.full-width {
    grid-column: span 2;
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
  
  .audit-info {
    margin-top: 16px;
    padding-top: 12px;
    border-top: 1px solid #E5E7EB;
  }
  
  .audit-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }
  
  .audit-item label {
    font-size: 11px;
    font-weight: 600;
    color: #6B7280;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  
  .audit-details {
    font-size: 12px;
    color: #374151;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }
  
  .user-name {
    font-style: italic;
    color: #6B7280;
  }
  
  .user-loading {
    color: #9CA3AF;
    font-style: italic;
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
    max-width: 600px;
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
  
  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
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
  
  .form-group input {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid #D1D5DB;
    border-radius: 6px;
    font-size: 14px;
    transition: border-color 0.2s ease;
    box-sizing: border-box;
  }
  
  .form-group input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
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
  
  .photos-section {
    margin-top: 24px;
    padding-top: 20px;
    border-top: 1px solid #E5E7EB;
  }
  
  .photos-section h4 {
    margin: 0 0 12px 0;
    font-size: 16px;
    font-weight: 600;
    color: #374151;
  }
  
  .photo-controls {
    display: flex;
    gap: 8px;
    margin-bottom: 16px;
  }
  
  .btn-upload {
    background: #F3F4F6;
    color: #374151;
    border: 1px solid #D1D5DB;
    padding: 8px 16px;
    border-radius: 6px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    gap: 4px;
  }
  
  .btn-upload:hover:not(:disabled) {
    background: #E5E7EB;
    transform: translateY(-1px);
  }
  
  .btn-upload:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    transform: none;
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
    .camiones-header {
      flex-direction: column;
      gap: 16px;
      align-items: flex-start;
    }
    
    .camiones-grid {
      grid-template-columns: 1fr;
      gap: 16px;
    }
    
    .form-row {
      grid-template-columns: 1fr;
      gap: 0;
    }
    
    .modal {
      margin: 20px;
      max-height: calc(100vh - 40px);
    }
    
    .details-grid {
      grid-template-columns: 1fr;
      gap: 12px;
    }
    
    .detail-item.full-width {
      grid-column: span 1;
    }
  }
</style>