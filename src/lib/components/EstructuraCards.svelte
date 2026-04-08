<script lang="ts">
  import { onMount } from 'svelte';
  import type { Estructura } from '../api';
  import { apiGet, apiPost, apiPut, apiDelete, getUserEmail, formatUpdateDate } from '../api';
  import { formatDdMmYyyyFromIso, formatYyyyMmDdFromIso, toIsoMidnight } from '../date';
  import ProtectedRoute from './ProtectedRoute.svelte';
  import PhotoGallery from './PhotoGallery.svelte';
  import BarcodeScanner from './BarcodeScanner.svelte';
  import PendientesEntrega from './PendientesEntrega.svelte';
  import { Plus, Search, X, ScanLine, Trash2, LayoutGrid, Table, FolderOpen, Camera } from 'lucide-svelte';

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
    items: Estructura[];
    filteredItems: Estructura[];
    error: string | null;
    showCreateModal: boolean;
    showEditModal: boolean;
    showDeleteModal: boolean;
    selectedEstructura: Estructura | null;
    isSubmitting: boolean;
    searchQuery: string;
    showBarcodeScanner: boolean;
    viewMode: 'cards' | 'table';
    isLargeScreen: boolean;
  };

  let state: State = {
    loading: true,
    items: [],
    filteredItems: [],
    error: null,
    showCreateModal: false,
    showEditModal: false,
    showDeleteModal: false,
    selectedEstructura: null,
    isSubmitting: false,
    searchQuery: '',
    showBarcodeScanner: false,
    viewMode: 'cards',
    isLargeScreen: false,
  };

  let newEstructura: Partial<Estructura> = {};
  let editEstructura: Partial<Estructura> = {};
  let photoGalleryRef: any = null;

  async function load() {
    try {
      state.loading = true;
      state.error = null;
      state.items = await apiGet<Estructura[]>('/estructura');
      filterItems();
    } catch (e: any) {
      state.error = e?.message || 'Error cargando estructuras';
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
    state.filteredItems = state.items.filter(estructura => {
      const searchableFields = [
        estructura.DNI,
        estructura.Conductor,
        estructura.Matricula,
        estructura.Proveedor,
        estructura.PackingList,
        estructura.Albaran,
        estructura.id?.toString(),
        // Buscar también en fecha formateada
        estructura.FechaDescarga ? formatDdMmYyyyFromIso(estructura.FechaDescarga) : ''
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
    newEstructura = {
      DNI: '',
      Conductor: '',
      Matricula: '',
      Proveedor: '',
      PackingList: '',
      Albaran: '',
      FechaDescarga: null
    };
    state.showCreateModal = true;
  }

  function openEditModal(estructura: Estructura) {
    editEstructura = {
      ...estructura,
      // Convertir fecha ISO a formato YYYY-MM-DD para el input
      FechaDescarga: estructura.FechaDescarga ? formatYyyyMmDdFromIso(estructura.FechaDescarga) : null
    };
    state.selectedEstructura = estructura;
    state.showEditModal = true;
  }

  function openDeleteModal(estructura: Estructura) {
    state.selectedEstructura = estructura;
    state.showDeleteModal = true;
  }

  async function createEstructura() {
    if (state.isSubmitting) return;

    try {
      state.isSubmitting = true;
      // Procesar fecha si existe
      if (newEstructura.FechaDescarga) {
        newEstructura.FechaDescarga = toIsoMidnight(newEstructura.FechaDescarga);
      }

      const created = await apiPost<Estructura>('/estructura', newEstructura);
      state.items = [...state.items, created];
      filterItems();
      state.showCreateModal = false;
    } catch (e: any) {
      state.error = e?.message || 'Error creando estructura';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function updateEstructura() {
    if (state.isSubmitting || !state.selectedEstructura) return;

    try {
      state.isSubmitting = true;
      // Procesar fecha si existe
      if (editEstructura.FechaDescarga) {
        editEstructura.FechaDescarga = toIsoMidnight(editEstructura.FechaDescarga);
      }

      await apiPut(`/estructura/${state.selectedEstructura.id}`, editEstructura);

      // Actualizar la lista local
      const index = state.items.findIndex(item => item.id === state.selectedEstructura!.id);
      if (index >= 0) {
        state.items[index] = { ...state.items[index], ...editEstructura };
      }

      filterItems();
      state.showEditModal = false;
      state.selectedEstructura = null;
    } catch (e: any) {
      state.error = e?.message || 'Error actualizando estructura';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function deleteEstructura() {
    if (state.isSubmitting || !state.selectedEstructura) return;

    try {
      state.isSubmitting = true;
      await apiDelete(`/estructura/${state.selectedEstructura.id}`);

      // Remover de la lista local
      state.items = state.items.filter(item => item.id !== state.selectedEstructura!.id);

      filterItems();
      state.showDeleteModal = false;
      state.selectedEstructura = null;
    } catch (e: any) {
      state.error = e?.message || 'Error eliminando estructura';
    } finally {
      state.isSubmitting = false;
    }
  }

  function closeModals() {
    state.showCreateModal = false;
    state.showEditModal = false;
    state.showDeleteModal = false;
    state.selectedEstructura = null;
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
    checkScreenSize();
    window.addEventListener('resize', checkScreenSize);

    return () => {
      window.removeEventListener('resize', checkScreenSize);
    };
  });
</script>

<ProtectedRoute>
<div class="container">
  <div class="toolbar">
    <div class="search-box">
      <Search size={16} class="search-icon" />
      <input
        type="text"
        placeholder="Buscar por proveedor, conductor, matricula, DNI..."
        bind:value={state.searchQuery}
        on:input={handleSearch}
        class="search-input"
      />
      {#if state.searchQuery}
        <button class="icon-btn-ghost" on:click={clearSearch}><X size={16} /></button>
      {/if}
      <button class="icon-btn" on:click={openBarcodeScanner} title="Escanear"><ScanLine size={18} /></button>
    </div>
    <button class="btn-primary" on:click={openCreateModal}>
      <Plus size={16} />
      <span>Nuevo</span>
    </button>
  </div>

  {#if state.searchQuery && state.filteredItems.length !== state.items.length}
    <div class="search-results">
      {state.filteredItems.length} de {state.items.length} estructuras
    </div>
  {/if}

  {#if state.loading}
    <div class="empty-state"><div class="spinner"></div><p>Cargando...</p></div>
  {:else if state.error && state.items.length === 0}
    <div class="empty-state">
      <p class="error-text">{state.error}</p>
      <button class="btn-secondary" on:click={load}>Reintentar</button>
    </div>
  {:else if state.items.length === 0}
    <div class="empty-state">
      <h3>No hay estructuras registradas</h3>
      <p>Comienza creando tu primera estructura</p>
      <button class="btn-primary" on:click={openCreateModal}><Plus size={16} /> Crear</button>
    </div>
  {:else if state.filteredItems.length === 0 && state.searchQuery}
    <div class="empty-state">
      <h3>Sin resultados</h3>
      <p>No hay estructuras que coincidan con "{state.searchQuery}"</p>
      <button class="btn-secondary" on:click={clearSearch}>Limpiar</button>
    </div>
  {:else}
    {#if state.isLargeScreen}
      <div class="view-toggle">
        <button class="toggle-btn" class:active={state.viewMode === 'cards'} on:click={() => state.viewMode = 'cards'}>
          <LayoutGrid size={16} /> Tarjetas
        </button>
        <button class="toggle-btn" class:active={state.viewMode === 'table'} on:click={() => state.viewMode = 'table'}>
          <Table size={16} /> Tabla
        </button>
      </div>
    {/if}

    {#if state.viewMode === 'table' && state.isLargeScreen}
      <div class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>ID</th><th>Proveedor</th><th>Conductor</th><th>DNI</th>
              <th>Matricula</th><th>Packing List</th><th>Albaran</th>
              <th>Fecha</th><th>Actualizado</th><th></th>
            </tr>
          </thead>
          <tbody>
            {#each state.filteredItems as estructura (estructura.id)}
              <tr class="table-row" on:click={() => openEditModal(estructura)} role="button" tabindex="0" on:keydown={(e) => { if (e.key === 'Enter') openEditModal(estructura); }}>
                <td class="id-cell">#{estructura.id}</td>
                <td class="proveedor-cell">{estructura.Proveedor || '-'}</td>
                <td>{estructura.Conductor || '-'}</td>
                <td>{estructura.DNI || '-'}</td>
                <td>{estructura.Matricula || '-'}</td>
                <td>{estructura.PackingList || '-'}</td>
                <td>{estructura.Albaran || '-'}</td>
                <td>{estructura.FechaDescarga ? formatDdMmYyyyFromIso(estructura.FechaDescarga) : '-'}</td>
                <td class="update-cell">
                  {#if estructura.modified_at}
                    <span class="update-date">{formatUpdateDate(estructura.modified_at)}</span>
                    {#if estructura.modified_by}
                      {#await getUserDisplayName(estructura.modified_by)}
                        <span class="update-user">...</span>
                      {:then email}
                        <span class="update-user">{email}</span>
                      {:catch}
                        <span class="update-user">{estructura.modified_by}</span>
                      {/await}
                    {/if}
                  {:else}-{/if}
                </td>
                <td>
                  <button class="btn-delete" on:click|stopPropagation={() => openDeleteModal(estructura)} title="Eliminar">
                    <Trash2 size={14} color="#EF4444" />
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>
    {:else}
      <div class="cards-grid">
        {#each state.filteredItems as estructura (estructura.id)}
          <div class="card" on:click={() => openEditModal(estructura)} role="button" tabindex="0" on:keydown={(e) => { if (e.key === 'Enter') openEditModal(estructura); }}>
            <div class="card-header">
              <span class="card-id">#{estructura.id}</span>
              <button class="btn-delete" on:click|stopPropagation={() => openDeleteModal(estructura)} title="Eliminar">
                <Trash2 size={14} color="#EF4444" />
              </button>
            </div>
            <div class="card-body">
              <h3>{estructura.Proveedor || 'Sin proveedor'}</h3>
              <p class="subtitle">{estructura.Conductor || 'Sin conductor'}</p>
              <div class="details-grid">
                <div class="detail"><span class="detail-label">DNI</span><span>{estructura.DNI || '-'}</span></div>
                <div class="detail"><span class="detail-label">Matricula</span><span>{estructura.Matricula || '-'}</span></div>
                <div class="detail"><span class="detail-label">Packing List</span><span>{estructura.PackingList || '-'}</span></div>
                <div class="detail"><span class="detail-label">Albaran</span><span>{estructura.Albaran || '-'}</span></div>
                <div class="detail full-width"><span class="detail-label">Fecha Descarga</span><span>{estructura.FechaDescarga ? formatDdMmYyyyFromIso(estructura.FechaDescarga) : '-'}</span></div>
              </div>
              {#if estructura.modified_at || estructura.modified_by}
                <div class="audit-info">
                  {#if estructura.modified_at}{formatUpdateDate(estructura.modified_at)}{/if}
                  {#if estructura.modified_by}
                    {#await getUserDisplayName(estructura.modified_by)}
                      <span class="audit-user">...</span>
                    {:then email}
                      <span class="audit-user">por {email}</span>
                    {:catch}
                      <span class="audit-user">por {estructura.modified_by}</span>
                    {/await}
                  {/if}
                </div>
              {/if}
              <PhotoGallery tableName="estructura" recordId={estructura.id} readonly={true} />
            </div>
          </div>
        {/each}
      </div>
    {/if}
  {/if}
</div>

{#if state.showCreateModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Nueva Estructura</h3>
        <button class="icon-btn-ghost" on:click={closeModals}><X size={20} /></button>
      </div>
      <form class="modal-form" on:submit|preventDefault={createEstructura}>
        <div class="form-row">
          <div class="form-group"><label for="new-proveedor">Proveedor *</label><input id="new-proveedor" bind:value={newEstructura.Proveedor} required></div>
          <div class="form-group"><label for="new-conductor">Conductor</label><input id="new-conductor" bind:value={newEstructura.Conductor}></div>
        </div>
        <div class="form-row">
          <div class="form-group"><label for="new-dni">DNI</label><input id="new-dni" bind:value={newEstructura.DNI}></div>
          <div class="form-group"><label for="new-matricula">Matricula</label><input id="new-matricula" bind:value={newEstructura.Matricula}></div>
        </div>
        <div class="form-row">
          <div class="form-group"><label for="new-packinglist">Packing List</label><input id="new-packinglist" bind:value={newEstructura.PackingList}></div>
          <div class="form-group"><label for="new-albaran">Albaran</label><input id="new-albaran" bind:value={newEstructura.Albaran}></div>
        </div>
        <div class="form-group"><label for="new-fecha">Fecha Descarga</label><input id="new-fecha" type="date" bind:value={newEstructura.FechaDescarga}></div>
        {#if state.error}<div class="form-error">{state.error}</div>{/if}
        <div class="modal-actions">
          <button type="button" class="btn-secondary" on:click={closeModals}>Cancelar</button>
          <button type="submit" class="btn-primary" disabled={state.isSubmitting}>{state.isSubmitting ? 'Creando...' : 'Crear'}</button>
        </div>
      </form>
    </div>
  </div>
{/if}

{#if state.showEditModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Editar Estructura #{state.selectedEstructura?.id}</h3>
        <button class="icon-btn-ghost" on:click={closeModals}><X size={20} /></button>
      </div>
      <form class="modal-form" on:submit|preventDefault={updateEstructura}>
        <div class="form-row">
          <div class="form-group"><label for="edit-proveedor">Proveedor *</label><input id="edit-proveedor" bind:value={editEstructura.Proveedor} required></div>
          <div class="form-group"><label for="edit-conductor">Conductor</label><input id="edit-conductor" bind:value={editEstructura.Conductor}></div>
        </div>
        <div class="form-row">
          <div class="form-group"><label for="edit-dni">DNI</label><input id="edit-dni" bind:value={editEstructura.DNI}></div>
          <div class="form-group"><label for="edit-matricula">Matricula</label><input id="edit-matricula" bind:value={editEstructura.Matricula}></div>
        </div>
        <div class="form-row">
          <div class="form-group"><label for="edit-packinglist">Packing List</label><input id="edit-packinglist" bind:value={editEstructura.PackingList}></div>
          <div class="form-group"><label for="edit-albaran">Albaran</label><input id="edit-albaran" bind:value={editEstructura.Albaran}></div>
        </div>
        <div class="form-group"><label for="edit-fecha">Fecha Descarga</label><input id="edit-fecha" type="date" bind:value={editEstructura.FechaDescarga}></div>
        <div class="photos-section">
          <h4>Fotos</h4>
          <div class="photo-controls">
            <button type="button" class="btn-secondary" on:click={() => photoGalleryRef?.triggerGallery()} disabled={photoGalleryRef?.getState()?.uploading}>
              <FolderOpen size={16} /> Galeria
            </button>
            <button type="button" class="btn-secondary" on:click={() => photoGalleryRef?.triggerCamera()} disabled={photoGalleryRef?.getState()?.uploading}>
              <Camera size={16} /> Camara
            </button>
          </div>
          <PhotoGallery bind:this={photoGalleryRef} tableName="estructura" recordId={state.selectedEstructura?.id} readonly={false} modalMode={true} />
        </div>
        {#if state.selectedEstructura}
          <div class="pendientes-section">
            <PendientesEntrega estructuraId={state.selectedEstructura.id} />
          </div>
        {/if}
        {#if state.error}<div class="form-error">{state.error}</div>{/if}
        <div class="modal-actions">
          <button type="button" class="btn-secondary" on:click={closeModals}>Cancelar</button>
          <button type="submit" class="btn-primary" disabled={state.isSubmitting}>{state.isSubmitting ? 'Guardando...' : 'Guardar'}</button>
        </div>
      </form>
    </div>
  </div>
{/if}

{#if state.showDeleteModal}
  <div class="modal-overlay" on:click={closeModals}>
    <div class="modal modal-small" on:click|stopPropagation>
      <div class="modal-header">
        <h3>Eliminar estructura</h3>
        <button class="icon-btn-ghost" on:click={closeModals}><X size={20} /></button>
      </div>
      <div class="modal-body">
        <p>Eliminar estructura <strong>#{state.selectedEstructura?.id}</strong>?</p>
        <p class="warning-text">Esta accion no se puede deshacer.</p>
        {#if state.error}<div class="form-error">{state.error}</div>{/if}
      </div>
      <div class="modal-actions">
        <button type="button" class="btn-secondary" on:click={closeModals}>Cancelar</button>
        <button class="btn-danger" on:click={deleteEstructura} disabled={state.isSubmitting}>{state.isSubmitting ? 'Eliminando...' : 'Eliminar'}</button>
      </div>
    </div>
  </div>
{/if}

<BarcodeScanner isOpen={state.showBarcodeScanner} on:scan={handleBarcodeScan} on:close={closeBarcodeScanner} />
</ProtectedRoute>

<style>
  .container { max-width: 1600px; margin: 0 auto; padding: 20px; }

  .toolbar {
    display: flex; gap: 12px; align-items: center; margin-bottom: 20px;
  }
  .search-box {
    flex: 1; display: flex; align-items: center; gap: 8px;
    background: white; border: 1px solid #E5E7EB; border-radius: 10px; padding: 4px 12px;
  }
  .search-box :global(.search-icon) { color: #9CA3AF; flex-shrink: 0; }
  .search-input {
    flex: 1; border: none; outline: none; font-size: 14px; padding: 10px 4px; background: transparent; min-width: 0;
  }
  .search-results { text-align: center; font-size: 13px; color: #6B7280; margin-bottom: 16px; }

  .icon-btn {
    display: flex; align-items: center; justify-content: center;
    width: 40px; height: 40px; border-radius: 10px; border: 1px solid #E5E7EB;
    background: white; color: #374151; cursor: pointer; transition: all 0.2s; flex-shrink: 0;
  }
  .icon-btn:hover { background: #F3F4F6; }

  .icon-btn-ghost {
    display: flex; align-items: center; justify-content: center;
    width: 32px; height: 32px; border-radius: 8px; border: none;
    background: transparent; color: #6B7280; cursor: pointer; transition: all 0.2s;
  }
  .icon-btn-ghost:hover { background: #F3F4F6; }

  .btn-delete {
    display: flex; align-items: center; justify-content: center;
    width: 32px; height: 32px; border-radius: 8px;
    border: 1px solid #E5E7EB; background: #FEF2F2;
    color: #EF4444; cursor: pointer; transition: all 0.2s;
  }
  .btn-delete:hover { background: #FEE2E2; border-color: #FECACA; color: #DC2626; }

  .btn-primary {
    display: flex; align-items: center; gap: 6px;
    background: #1F2937; color: white; border: none; padding: 10px 18px;
    border-radius: 10px; font-weight: 500; font-size: 14px; cursor: pointer;
    transition: all 0.2s; white-space: nowrap;
  }
  .btn-primary:hover:not(:disabled) { background: #111827; }
  .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

  .btn-secondary {
    display: flex; align-items: center; gap: 6px;
    background: white; color: #374151; border: 1px solid #D1D5DB;
    padding: 8px 16px; border-radius: 8px; font-weight: 500; font-size: 14px;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-secondary:hover { background: #F3F4F6; }

  .btn-danger {
    background: #DC2626; color: white; border: none; padding: 10px 20px;
    border-radius: 8px; font-weight: 500; font-size: 14px; cursor: pointer; transition: all 0.2s;
  }
  .btn-danger:hover:not(:disabled) { background: #B91C1C; }

  .empty-state { text-align: center; padding: 60px 20px; color: #6B7280; }
  .empty-state h3 { color: #374151; font-size: 16px; margin: 0 0 8px; }
  .empty-state p { margin: 0 0 16px; font-size: 14px; }
  .error-text { color: #DC2626; }

  .spinner {
    width: 32px; height: 32px; border: 3px solid #E5E7EB; border-top: 3px solid #3B82F6;
    border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px;
  }
  @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

  .view-toggle { display: flex; gap: 4px; margin-bottom: 16px; background: #F3F4F6; border-radius: 10px; padding: 4px; width: fit-content; }
  .toggle-btn {
    display: flex; align-items: center; gap: 6px; padding: 8px 16px; border: none;
    border-radius: 8px; background: transparent; font-size: 13px; font-weight: 500;
    color: #6B7280; cursor: pointer; transition: all 0.2s;
  }
  .toggle-btn.active { background: white; color: #1F2937; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }

  /* Table */
  .table-container { background: white; border-radius: 12px; border: 1px solid #E5E7EB; overflow: hidden; }
  .data-table { width: 100%; border-collapse: collapse; }
  .data-table th { background: #F9FAFB; padding: 12px; text-align: left; font-weight: 500; color: #6B7280; font-size: 13px; border-bottom: 1px solid #E5E7EB; }
  .data-table td { padding: 12px; border-bottom: 1px solid #F3F4F6; font-size: 14px; color: #374151; }
  .table-row { cursor: pointer; transition: background 0.15s; }
  .table-row:hover { background: #F9FAFB; }
  .id-cell { font-weight: 600; color: #3B82F6; }
  .proveedor-cell { font-weight: 600; color: #1F2937; }
  .update-cell { min-width: 160px; }
  .update-date { font-size: 13px; color: #374151; }
  .update-user { font-size: 12px; color: #9CA3AF; display: block; }

  /* Cards */
  .cards-grid { display: grid; grid-template-columns: 1fr; gap: 12px; }
  .card {
    background: white; border: 1px solid #E5E7EB; border-radius: 12px;
    cursor: pointer; transition: all 0.2s;
  }
  .card:hover { border-color: #D1D5DB; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
  .card:active { transform: scale(0.99); }
  .card-header {
    display: flex; justify-content: space-between; align-items: center;
    padding: 12px 16px; border-bottom: 1px solid #F3F4F6;
  }
  .card-id { font-weight: 600; color: #9CA3AF; font-size: 13px; }
  .card-body { padding: 16px; }
  .card-body h3 { font-size: 17px; font-weight: 600; color: #1F2937; margin: 0 0 2px; }
  .subtitle { color: #6B7280; font-size: 14px; margin: 0 0 16px; }
  .details-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
  .detail { display: flex; flex-direction: column; gap: 2px; }
  .detail-label { font-size: 11px; font-weight: 600; color: #9CA3AF; text-transform: uppercase; letter-spacing: 0.5px; }
  .detail span:last-child { font-size: 14px; color: #374151; }
  .detail.full-width { grid-column: span 2; }
  .audit-info { margin-top: 12px; padding-top: 12px; border-top: 1px solid #F3F4F6; font-size: 12px; color: #9CA3AF; }
  .audit-user { color: #9CA3AF; }

  /* Modal */
  .modal-overlay {
    position: fixed; top: 0; left: 0; right: 0; bottom: 0;
    background: rgba(0,0,0,0.4); display: flex; align-items: center;
    justify-content: center; z-index: 1000; padding: 20px;
  }
  .modal {
    background: white; border-radius: 16px; width: 100%; max-width: 560px;
    max-height: 90vh; overflow-y: auto;
  }
  .modal-small { max-width: 380px; }
  .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 20px 20px 0; }
  .modal-header h3 { font-size: 18px; font-weight: 600; color: #1F2937; margin: 0; }
  .modal-form { padding: 20px; }
  .modal-body { padding: 0 20px; }
  .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
  .form-group { margin-bottom: 16px; }
  .form-group label { display: block; font-weight: 500; color: #374151; margin-bottom: 6px; font-size: 14px; }
  .form-group input {
    width: 100%; padding: 10px 12px; border: 1px solid #D1D5DB; border-radius: 8px;
    font-size: 14px; transition: border-color 0.2s; box-sizing: border-box;
  }
  .form-group input:focus { outline: none; border-color: #3B82F6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
  .form-error { background: #FEF2F2; color: #DC2626; padding: 10px 12px; border-radius: 8px; margin-bottom: 16px; font-size: 14px; }
  .warning-text { color: #DC2626; font-size: 14px; margin: 4px 0 0; }
  .photos-section { margin-top: 20px; padding-top: 16px; border-top: 1px solid #E5E7EB; }
  .photos-section h4 { margin: 0 0 12px; font-size: 15px; font-weight: 600; color: #374151; }
  .pendientes-section { margin-top: 20px; padding-top: 16px; border-top: 1px solid #E5E7EB; }
  .photo-controls { display: flex; gap: 8px; margin-bottom: 12px; }
  .modal-actions { display: flex; justify-content: flex-end; gap: 8px; padding: 16px 20px 20px; border-top: 1px solid #F3F4F6; margin-top: 16px; }

  @media (min-width: 768px) {
    .cards-grid { grid-template-columns: repeat(auto-fill, minmax(400px, 1fr)); gap: 16px; }
  }
  @media (max-width: 480px) {
    .container { padding: 12px; }
    .toolbar { flex-direction: column; }
    .btn-primary { width: 100%; justify-content: center; }
    .form-row { grid-template-columns: 1fr; gap: 0; }
    .modal { margin: 10px; max-height: calc(100vh - 20px); }
  }
</style>
