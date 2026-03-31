<script lang="ts">
  import { onMount } from 'svelte';
  import type { Pallet, Camion } from '../api';
  import { apiGet, apiPost, apiPut, apiDelete, getUserEmail, formatUpdateDate } from '../api';
  import ProtectedRoute from './ProtectedRoute.svelte';
  import PhotoGallery from './PhotoGallery.svelte';
  import BarcodeScanner from './BarcodeScanner.svelte';
  import { Plus, Search, X, ScanLine, Trash2, LayoutGrid, Table, FolderOpen, Camera, AlertTriangle } from 'lucide-svelte';

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
    items: Pallet[];
    filteredItems: Pallet[];
    camiones: Camion[];
    error: string | null;
    showCreateModal: boolean;
    showEditModal: boolean;
    showDeleteModal: boolean;
    selectedPallet: Pallet | null;
    isSubmitting: boolean;
    loadingCamiones: boolean;
    searchQuery: string;
    showBarcodeScanner: boolean;
    viewMode: 'cards' | 'table';
    isLargeScreen: boolean;
    showDefectosOnly: boolean;
  };

  let state: State = {
    loading: true,
    items: [],
    filteredItems: [],
    camiones: [],
    error: null,
    showCreateModal: false,
    showEditModal: false,
    showDeleteModal: false,
    selectedPallet: null,
    isSubmitting: false,
    loadingCamiones: false,
    searchQuery: '',
    showBarcodeScanner: false,
    viewMode: 'cards',
    isLargeScreen: false,
    showDefectosOnly: false,
  };

  let newPallet: Partial<Pallet> = {};
  let editPallet: Partial<Pallet> = {};
  let photoGalleryRef: any = null;

  async function load() {
    try {
      state.loading = true;
      state.error = null;
      state.items = await apiGet<Pallet[]>('/pallets');
      filterItems();
    } catch (e: any) {
      state.error = e?.message || 'Error cargando pallets';
    } finally {
      state.loading = false;
    }
  }

  function filterItems() {
    let filtered = [...state.items];

    // Filtrar por defectos si está activado
    if (state.showDefectosOnly) {
      filtered = filtered.filter(pallet => pallet.Defecto);
    }

    // Filtrar por búsqueda de texto si hay query
    if (state.searchQuery.trim()) {
      const query = state.searchQuery.toLowerCase().trim();
      filtered = filtered.filter(pallet => {
        const camionInfo = getCamionInfo(pallet.Descarga);
        const defectoText = pallet.Defecto ? 'defectuoso' : 'sin defecto';

        const searchableFields = [
          pallet.id,
          camionInfo,
          defectoText,
          // Buscar en información del camión asignado
          pallet.Descarga ? `camion ${pallet.Descarga}` : 'sin asignar',
          pallet.Defecto ? 'defecto' : 'correcto',
        ];

        return searchableFields.some(field =>
          field && field.toString().toLowerCase().includes(query)
        );
      });
    }

    state.filteredItems = filtered;
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

  function toggleDefectosFilter() {
    state.showDefectosOnly = !state.showDefectosOnly;
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
      filterItems();
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

      filterItems();
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

      filterItems();
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

  function checkScreenSize() {
    state.isLargeScreen = window.innerWidth >= 1200;
    state.viewMode = state.isLargeScreen ? 'table' : 'cards';
  }

  function toggleViewMode() {
    state.viewMode = state.viewMode === 'cards' ? 'table' : 'cards';
  }

  onMount(() => {
    load();
    loadCamiones();
    checkScreenSize();
    window.addEventListener('resize', checkScreenSize);

    return () => {
      window.removeEventListener('resize', checkScreenSize);
    };
  });
</script>

<ProtectedRoute>
<div class="pallets-container">
  <!-- Toolbar -->
  <div class="toolbar">
    <div class="search-box">
      <Search size={18} class="search-icon" />
      <input
        type="text"
        placeholder="Buscar pallets por ID, camion, estado..."
        bind:value={state.searchQuery}
        on:input={handleSearch}
        class="search-input"
      />
      {#if state.searchQuery}
        <button class="icon-btn-ghost" on:click={clearSearch} title="Limpiar busqueda">
          <X size={16} />
        </button>
      {/if}
      <button class="icon-btn" on:click={openBarcodeScanner} title="Escanear codigo de barras">
        <ScanLine size={20} />
      </button>
    </div>
    <button class="btn-primary" on:click={openCreateModal}>
      <Plus size={18} />
      Nuevo
    </button>
  </div>

  <!-- Filter bar -->
  <div class="filter-bar">
    {#if state.isLargeScreen}
      <div class="view-toggle">
        <button
          class="toggle-btn"
          class:active={state.viewMode === 'cards'}
          on:click={() => state.viewMode = 'cards'}
        >
          <LayoutGrid size={16} />
          Tarjetas
        </button>
        <button
          class="toggle-btn"
          class:active={state.viewMode === 'table'}
          on:click={() => state.viewMode = 'table'}
        >
          <Table size={16} />
          Tabla
        </button>
      </div>
    {/if}

    <button
      class="toggle-btn defectos-toggle"
      class:active={state.showDefectosOnly}
      on:click={toggleDefectosFilter}
    >
      <AlertTriangle size={16} />
      Defectos
      <span class="count-pill">{state.items.filter(p => p.Defecto).length}</span>
    </button>
  </div>

  {#if (state.searchQuery && state.filteredItems.length !== state.items.length) || state.showDefectosOnly}
    <div class="search-results">
      Mostrando {state.filteredItems.length} de {state.items.length} pallets
      {#if state.showDefectosOnly}
        <span class="filter-active">- Solo defectuosos</span>
      {/if}
    </div>
  {/if}

  {#if state.loading}
    <div class="loading-state">
      <div class="spinner"></div>
      <p>Cargando pallets...</p>
    </div>
  {:else if state.error}
    <div class="error-state">
      <p class="error-text">{state.error}</p>
      <button class="btn-secondary" on:click={load}>Reintentar</button>
    </div>
  {:else if state.items.length === 0}
    <div class="empty-state">
      <h3>No hay pallets registrados</h3>
      <p>Comienza creando tu primer pallet</p>
      <button class="btn-primary" on:click={openCreateModal}>
        <Plus size={18} />
        Crear Primer Pallet
      </button>
    </div>
  {:else if state.filteredItems.length === 0 && state.searchQuery}
    <div class="empty-state">
      <h3>No se encontraron pallets</h3>
      <p>No hay pallets que coincidan con "{state.searchQuery}"</p>
      <button class="btn-secondary" on:click={clearSearch}>
        Limpiar busqueda
      </button>
    </div>
  {:else}
    <!-- Vista de tabla para pantallas grandes -->
    {#if state.viewMode === 'table'}
      <div class="pallets-table-container">
        <table class="pallets-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Estado</th>
              <th>Descarga</th>
              <th>Camion Asignado</th>
              <th>Actualizado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {#each state.filteredItems as pallet (pallet.id)}
              <tr class="table-row" class:defecto-row={pallet.Defecto} on:click={() => openEditModal(pallet)} role="button" tabindex="0" on:keydown={(e) => { if (e.key === 'Enter') openEditModal(pallet); }}>
                <td class="id-cell">{pallet.id}</td>
                <td class="estado-cell">
                  {#if pallet.Defecto}
                    <span class="status-badge status-danger">Con Defecto</span>
                  {:else}
                    <span class="status-badge status-success">Sin Defecto</span>
                  {/if}
                </td>
                <td class="descarga-cell">{pallet.Descarga || '-'}</td>
                <td class="camion-cell">
                  <span class="camion-info-table">{getCamionInfo(pallet.Descarga)}</span>
                </td>
                <td class="updated-cell">
                  {#if pallet.updated_at}
                    <div class="update-info">
                      <span class="update-date">{formatUpdateDate(pallet.updated_at)}</span>
                      {#if pallet.updated_by}
                        {#await getUserDisplayName(pallet.updated_by)}
                          <span class="update-user">...</span>
                        {:then email}
                          <span class="update-user">{email}</span>
                        {:catch}
                          <span class="update-user">{pallet.updated_by}</span>
                        {/await}
                      {/if}
                    </div>
                  {:else}
                    -
                  {/if}
                </td>
                <td class="actions-cell">
                  <button class="btn-delete" on:click|stopPropagation={() => openDeleteModal(pallet)} title="Eliminar">
                    <Trash2 size={14} color="#EF4444" />
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>
    {:else}
      <!-- Vista de tarjetas -->
      <div class="pallets-grid">
        {#each state.filteredItems as pallet (pallet.id)}
          <div class="pallet-card" on:click={() => openEditModal(pallet)} role="button" tabindex="0" on:keydown={(e) => { if (e.key === 'Enter') openEditModal(pallet); }}>
            <div class="card-header">
              <div class="card-header-left">
                <div class="pallet-id">{pallet.id}</div>
                {#if pallet.Defecto}
                  <span class="defecto-pill defecto-bad">Con Defecto</span>
                {:else}
                  <span class="defecto-pill defecto-ok">OK</span>
                {/if}
              </div>
              <div class="card-actions">
                <button class="btn-delete" on:click|stopPropagation={() => openDeleteModal(pallet)} title="Eliminar">
                  <Trash2 size={14} color="#EF4444" />
                </button>
              </div>
            </div>

            <div class="card-content">
              <div class="details-grid">
                <div class="detail-item">
                  <label>DESCARGA</label>
                  <span>{pallet.Descarga || '-'}</span>
                </div>
                <div class="detail-item">
                  <label>CAMION ASIGNADO</label>
                  <span class="camion-info">
                    {getCamionInfo(pallet.Descarga)}
                  </span>
                </div>
              </div>

              <!-- Informacion de auditoria -->
              {#if pallet.updated_at || pallet.updated_by}
                <div class="audit-info">
                  <div class="audit-item">
                    <label>ULTIMA ACTUALIZACION</label>
                    <span class="audit-details">
                      {#if pallet.updated_at}
                        {formatUpdateDate(pallet.updated_at)}
                      {/if}
                      {#if pallet.updated_by}
                        {#await getUserDisplayName(pallet.updated_by)}
                          <span class="user-loading">...</span>
                        {:then email}
                          <span class="user-name">por {email}</span>
                        {:catch}
                          <span class="user-name">por {pallet.updated_by}</span>
                        {/await}
                      {/if}
                    </span>
                  </div>
                </div>
              {/if}

              <!-- Vista previa de fotos (solo lectura) -->
              <PhotoGallery tableName="pallets" recordId={pallet.id} readonly={true} />
            </div>
          </div>
        {/each}
      </div>
    {/if}
  {/if}
</div>

<!-- Modal Crear Pallet -->
{#if state.showCreateModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Crear Nuevo Pallet</h3>
        <button class="icon-btn-ghost" on:click={closeModals}>
          <X size={20} />
        </button>
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
          <label for="new-descarga">Camion de Descarga</label>
          {#if state.loadingCamiones}
            <p class="loading-text">Cargando camiones...</p>
          {:else}
            <select id="new-descarga" bind:value={newPallet.Descarga}>
              <option value="">Sin asignar</option>
              {#each state.camiones as camion}
                <option value={camion.id}>
                  #{camion.id} - {camion.Matricula || 'Sin matricula'}
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
        <button class="icon-btn-ghost" on:click={closeModals}>
          <X size={20} />
        </button>
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
          <label for="edit-descarga">Camion de Descarga</label>
          {#if state.loadingCamiones}
            <p class="loading-text">Cargando camiones...</p>
          {:else}
            <select id="edit-descarga" bind:value={editPallet.Descarga}>
              <option value="">Sin asignar</option>
              {#each state.camiones as camion}
                <option value={camion.id}>
                  #{camion.id} - {camion.Matricula || 'Sin matricula'}
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

        <!-- Gestion de Fotos -->
        <div class="photos-section">
          <h4>Fotos</h4>
          <div class="photo-controls">
            <button type="button" class="btn-secondary btn-with-icon" on:click={() => photoGalleryRef?.triggerGallery()} disabled={photoGalleryRef?.getState()?.uploading}>
              <FolderOpen size={16} />
              Galeria
            </button>
            <button type="button" class="btn-secondary btn-with-icon" on:click={() => photoGalleryRef?.triggerCamera()} disabled={photoGalleryRef?.getState()?.uploading}>
              <Camera size={16} />
              Camara
            </button>
          </div>
          <PhotoGallery bind:this={photoGalleryRef} tableName="pallets" recordId={state.selectedPallet?.id} readonly={false} modalMode={true} />
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
        <h3>Confirmar Eliminacion</h3>
        <button class="icon-btn-ghost" on:click={closeModals}>
          <X size={20} />
        </button>
      </div>

      <div class="modal-content">
        <p>Estas seguro de que quieres eliminar el pallet <strong>{state.selectedPallet?.id}</strong>?</p>
        <p class="warning-text">Esta accion no se puede deshacer.</p>

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

<!-- Escaner de codigos de barras -->
<BarcodeScanner
  isOpen={state.showBarcodeScanner}
  on:scan={handleBarcodeScan}
  on:close={closeBarcodeScanner}
/>
</ProtectedRoute>

<style>
  .pallets-container {
    max-width: 1600px;
    margin: 0 auto;
    padding: 32px;
  }

  @media (min-width: 1400px) {
    .pallets-container {
      padding: 40px;
    }
  }

  @media (max-width: 768px) {
    .pallets-container {
      padding: 12px;
    }
  }

  @media (max-width: 480px) {
    .pallets-container {
      padding: 8px;
    }
  }

  /* Toolbar */
  .toolbar {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px;
  }

  .search-box {
    flex: 1;
    display: flex;
    align-items: center;
    gap: 0;
    background: white;
    border: 1px solid #E5E7EB;
    border-radius: 10px;
    padding: 0 4px 0 14px;
    height: 44px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  }

  .search-box :global(.search-icon) {
    color: #9CA3AF;
    flex-shrink: 0;
  }

  .search-input {
    flex: 1;
    border: none;
    outline: none;
    font-size: 14px;
    color: #1F2937;
    padding: 0 10px;
    background: transparent;
    min-width: 0;
  }

  .search-input::placeholder {
    color: #9CA3AF;
  }

  /* Icon buttons */
  .icon-btn {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #E5E7EB;
    border-radius: 10px;
    background: white;
    color: #6B7280;
    cursor: pointer;
    transition: all 0.15s ease;
    flex-shrink: 0;
  }

  .icon-btn:hover {
    background: #F9FAFB;
    color: #1F2937;
    border-color: #D1D5DB;
  }

  .icon-btn-ghost {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    border-radius: 8px;
    background: transparent;
    color: #6B7280;
    cursor: pointer;
    transition: all 0.15s ease;
    flex-shrink: 0;
    padding: 0;
  }

  .icon-btn-ghost:hover {
    background: #F3F4F6;
    color: #1F2937;
  }

  .btn-delete {
    display: flex; align-items: center; justify-content: center;
    width: 32px; height: 32px; border-radius: 8px;
    border: 1px solid #E5E7EB; background: #FEF2F2;
    color: #EF4444; cursor: pointer; transition: all 0.2s;
  }
  .btn-delete:hover { background: #FEE2E2; border-color: #FECACA; color: #DC2626; }

  /* Filter bar */
  .filter-bar {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 20px;
  }

  .view-toggle {
    display: flex;
    background: #F3F4F6;
    border-radius: 10px;
    padding: 3px;
    gap: 2px;
  }

  .toggle-btn {
    display: flex;
    align-items: center;
    gap: 6px;
    background: transparent;
    border: none;
    padding: 7px 14px;
    border-radius: 8px;
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s ease;
    color: #6B7280;
    white-space: nowrap;
  }

  .toggle-btn.active {
    background: white;
    color: #1F2937;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  }

  .toggle-btn:hover:not(.active) {
    color: #374151;
  }

  .defectos-toggle {
    background: #F3F4F6;
    border-radius: 10px;
    padding: 7px 14px;
  }

  .defectos-toggle.active {
    background: #FEE2E2;
    color: #991B1B;
    box-shadow: none;
  }

  .count-pill {
    background: rgba(0, 0, 0, 0.08);
    padding: 1px 7px;
    border-radius: 10px;
    font-size: 11px;
    font-weight: 600;
  }

  .defectos-toggle.active .count-pill {
    background: rgba(153, 27, 27, 0.15);
  }

  .search-results {
    text-align: center;
    margin-bottom: 16px;
    font-size: 13px;
    color: #6B7280;
    padding: 8px 16px;
    background: white;
    border: 1px solid #E5E7EB;
    border-radius: 8px;
  }

  .filter-active {
    color: #991B1B;
    font-weight: 500;
  }

  /* Buttons */
  .btn-primary {
    display: flex;
    align-items: center;
    gap: 6px;
    background: #1F2937;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 10px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.15s ease;
    font-size: 14px;
    white-space: nowrap;
  }

  .btn-primary:hover:not(:disabled) {
    background: #111827;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  }

  .btn-primary:active:not(:disabled) {
    transform: scale(0.99);
  }

  .btn-primary:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  .btn-secondary {
    display: flex;
    align-items: center;
    gap: 6px;
    background: white;
    color: #374151;
    border: 1px solid #D1D5DB;
    padding: 8px 16px;
    border-radius: 10px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.15s ease;
    font-size: 14px;
  }

  .btn-secondary:hover:not(:disabled) {
    background: #F9FAFB;
    border-color: #9CA3AF;
  }

  .btn-secondary:active:not(:disabled) {
    transform: scale(0.99);
  }

  .btn-secondary:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .btn-with-icon {
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }

  .btn-danger {
    background: #DC2626;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 10px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.15s ease;
    font-size: 14px;
  }

  .btn-danger:hover:not(:disabled) {
    background: #B91C1C;
    box-shadow: 0 2px 8px rgba(220, 38, 38, 0.25);
  }

  .btn-danger:active:not(:disabled) {
    transform: scale(0.99);
  }

  .btn-danger:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  /* States */
  .loading-state, .error-state, .empty-state {
    text-align: center;
    padding: 60px 20px;
    color: #6B7280;
  }

  .error-text {
    color: #DC2626;
    font-weight: 500;
  }

  .spinner {
    width: 36px;
    height: 36px;
    border: 3px solid #E5E7EB;
    border-top: 3px solid #1F2937;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
    margin: 0 auto 20px;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  /* Cards grid */
  .pallets-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
    gap: 16px;
  }

  @media (min-width: 1200px) {
    .pallets-grid {
      grid-template-columns: repeat(auto-fill, minmax(700px, 1fr));
      gap: 20px;
    }
  }

  @media (min-width: 1600px) {
    .pallets-grid {
      grid-template-columns: repeat(auto-fill, minmax(850px, 1fr));
      gap: 24px;
    }
  }

  .pallet-card {
    background: white;
    border: 1px solid #E5E7EB;
    border-radius: 12px;
    overflow: hidden;
    transition: all 0.15s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    cursor: pointer;
  }

  .pallet-card:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    border-color: #D1D5DB;
  }

  .pallet-card:active {
    transform: scale(0.99);
  }

  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 14px 18px;
    border-bottom: 1px solid #E5E7EB;
  }

  .card-header-left {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .pallet-id {
    font-weight: 700;
    color: #1F2937;
    font-size: 14px;
    font-family: 'Courier New', monospace;
  }

  .defecto-pill {
    display: inline-block;
    padding: 2px 10px;
    border-radius: 10px;
    font-size: 11px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }

  .defecto-ok {
    background: #D1FAE5;
    color: #065F46;
  }

  .defecto-bad {
    background: #FEE2E2;
    color: #991B1B;
  }

  .card-actions {
    display: flex;
    gap: 4px;
  }

  .card-content {
    padding: 18px;
  }

  @media (min-width: 1200px) {
    .card-content {
      padding: 16px;
    }
  }

  .details-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
  }

  @media (min-width: 1200px) {
    .details-grid {
      grid-template-columns: repeat(3, 1fr);
      gap: 20px;
    }
  }

  @media (min-width: 1600px) {
    .details-grid {
      grid-template-columns: repeat(4, 1fr);
      gap: 24px;
    }
  }

  .detail-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .detail-item label {
    font-size: 11px;
    font-weight: 600;
    color: #9CA3AF;
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
    background: #F9FAFB;
    padding: 6px 10px;
    border-radius: 8px;
    border: 1px solid #E5E7EB;
    font-size: 13px;
  }

  .audit-info {
    margin-top: 14px;
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
    color: #9CA3AF;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .audit-details {
    font-size: 12px;
    color: #6B7280;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .user-name {
    font-style: italic;
    color: #9CA3AF;
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
    background: rgba(0, 0, 0, 0.4);
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
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
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
    font-size: 17px;
    font-weight: 600;
    color: #1F2937;
    margin: 0;
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
    border-radius: 8px;
    font-size: 14px;
    transition: border-color 0.15s ease;
    box-sizing: border-box;
    color: #1F2937;
    background: white;
  }

  .form-group input:focus, .form-group select:focus {
    outline: none;
    border-color: #1F2937;
    box-shadow: 0 0 0 3px rgba(31, 41, 55, 0.08);
  }

  .checkbox-label {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
    padding: 12px;
    border: 1px solid #D1D5DB;
    border-radius: 8px;
    transition: all 0.15s ease;
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
    border-radius: 8px;
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
    font-size: 15px;
    font-weight: 600;
    color: #1F2937;
  }

  .photo-controls {
    display: flex;
    gap: 8px;
    margin-bottom: 16px;
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    padding: 20px 24px 24px;
    border-top: 1px solid #E5E7EB;
    margin-top: 20px;
  }

  /* Table styles */
  .pallets-table-container {
    background: white;
    border-radius: 12px;
    border: 1px solid #E5E7EB;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  }

  .pallets-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
  }

  .pallets-table thead {
    background: #F9FAFB;
  }

  .pallets-table th {
    padding: 12px;
    text-align: left;
    font-weight: 600;
    color: #6B7280;
    border-bottom: 1px solid #E5E7EB;
    white-space: nowrap;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .pallets-table td {
    padding: 12px;
    border-bottom: 1px solid #F3F4F6;
    vertical-align: middle;
  }

  .table-row {
    cursor: pointer;
    transition: all 0.1s ease;
  }

  .table-row:hover {
    background: #F9FAFB;
  }

  .table-row.defecto-row {
    background: #FEF2F2;
  }

  .table-row.defecto-row:hover {
    background: #FEE2E2;
  }

  .table-row:focus {
    outline: 2px solid #1F2937;
    outline-offset: -2px;
  }

  .id-cell {
    font-weight: 600;
    color: #1F2937;
    font-family: 'Courier New', monospace;
    min-width: 120px;
  }

  .estado-cell {
    min-width: 120px;
  }

  .status-badge {
    display: inline-block;
    padding: 3px 10px;
    border-radius: 10px;
    font-size: 11px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }

  .status-success {
    background: #D1FAE5;
    color: #065F46;
  }

  .status-danger {
    background: #FEE2E2;
    color: #991B1B;
  }

  .descarga-cell {
    font-family: monospace;
    min-width: 100px;
    color: #6B7280;
  }

  .camion-cell {
    min-width: 200px;
  }

  .camion-info-table {
    font-family: 'Courier New', monospace;
    background: #F9FAFB;
    padding: 4px 8px;
    border-radius: 6px;
    border: 1px solid #E5E7EB;
    font-size: 12px;
  }

  .updated-cell {
    min-width: 140px;
  }

  .update-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .update-date {
    font-size: 12px;
    color: #374151;
  }

  .update-user {
    font-size: 11px;
    color: #9CA3AF;
    font-style: italic;
  }

  .actions-cell {
    text-align: center;
    min-width: 60px;
  }

  @media (max-width: 768px) {
    .toolbar {
      flex-wrap: wrap;
    }

    .search-box {
      flex: 1 1 100%;
      order: 1;
    }

    .toolbar .btn-primary {
      order: 0;
      margin-left: auto;
    }

    .filter-bar {
      flex-wrap: wrap;
    }

    .view-toggle {
      display: none;
    }

    .pallets-grid {
      grid-template-columns: 1fr;
      gap: 12px;
    }

    .modal {
      margin: 20px;
      max-height: calc(100vh - 40px);
    }

    .details-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
