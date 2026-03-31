<script lang="ts">
  import { onMount } from 'svelte';
  import type { Camion } from '../api';
  import { apiGet, apiPost, apiPut, apiDelete, getUserEmail, formatUpdateDate } from '../api';
  import { formatDdMmYyyyFromIso, formatYyyyMmDdFromIso, toIsoMidnight } from '../date';
  import ProtectedRoute from './ProtectedRoute.svelte';
  import PhotoGallery from './PhotoGallery.svelte';
  import BarcodeScanner from './BarcodeScanner.svelte';
  import { Plus, Search, X, ScanLine, Trash2, LayoutGrid, Table, FolderOpen, Camera } from 'lucide-svelte';

  let userEmails: Map<string, string> = new Map();

  async function getUserDisplayName(keycloakId: string | null): Promise<string> {
    if (!keycloakId) return 'Usuario desconocido';
    if (userEmails.has(keycloakId)) return userEmails.get(keycloakId)!;
    const email = await getUserEmail(keycloakId);
    userEmails.set(keycloakId, email);
    userEmails = userEmails;
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
    selectedCamion: null,
    isSubmitting: false,
    searchQuery: '',
    showBarcodeScanner: false,
    viewMode: 'cards',
    isLargeScreen: false,
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
        camion.DNI, camion.Matricula, camion.UbicacionCampa,
        camion.Container, camion.Albaran, camion.NombreConductor,
        camion.id?.toString(),
        camion.FechaDescarga ? formatDdMmYyyyFromIso(camion.FechaDescarga) : ''
      ];
      return searchableFields.some(field => field && field.toString().toLowerCase().includes(query));
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

  function openBarcodeScanner() { state.showBarcodeScanner = true; }

  function handleBarcodeScan(event: CustomEvent<string>) {
    state.searchQuery = event.detail;
    filterItems();
    state.showBarcodeScanner = false;
  }

  function closeBarcodeScanner() { state.showBarcodeScanner = false; }

  function openCreateModal() {
    newCamion = { DNI: '', Matricula: '', UbicacionCampa: '', Container: '', Albaran: '', NombreConductor: '', FechaDescarga: null };
    state.showCreateModal = true;
  }

  function openEditModal(camion: Camion) {
    editCamion = { ...camion, FechaDescarga: camion.FechaDescarga ? formatYyyyMmDdFromIso(camion.FechaDescarga) : null };
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
      if (newCamion.FechaDescarga) newCamion.FechaDescarga = toIsoMidnight(newCamion.FechaDescarga);
      const created = await apiPost<Camion>('/camiones', newCamion);
      state.items = [...state.items, created];
      filterItems();
      state.showCreateModal = false;
    } catch (e: any) {
      state.error = e?.message || 'Error creando camion';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function updateCamion() {
    if (state.isSubmitting || !state.selectedCamion) return;
    try {
      state.isSubmitting = true;
      if (editCamion.FechaDescarga) editCamion.FechaDescarga = toIsoMidnight(editCamion.FechaDescarga);
      await apiPut(`/camiones/${state.selectedCamion.id}`, editCamion);
      const index = state.items.findIndex(item => item.id === state.selectedCamion!.id);
      if (index >= 0) state.items[index] = { ...state.items[index], ...editCamion };
      filterItems();
      state.showEditModal = false;
      state.selectedCamion = null;
    } catch (e: any) {
      state.error = e?.message || 'Error actualizando camion';
    } finally {
      state.isSubmitting = false;
    }
  }

  async function deleteCamion() {
    if (state.isSubmitting || !state.selectedCamion) return;
    try {
      state.isSubmitting = true;
      await apiDelete(`/camiones/${state.selectedCamion.id}`);
      state.items = state.items.filter(item => item.id !== state.selectedCamion!.id);
      filterItems();
      state.showDeleteModal = false;
      state.selectedCamion = null;
    } catch (e: any) {
      state.error = e?.message || 'Error eliminando camion';
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

  onMount(() => {
    load();
    checkScreenSize();
    window.addEventListener('resize', checkScreenSize);
  });

  function checkScreenSize() {
    state.isLargeScreen = window.innerWidth >= 1200;
    state.viewMode = state.isLargeScreen ? 'table' : 'cards';
  }
</script>

<ProtectedRoute>
<div class="container">
  <div class="toolbar">
    <div class="search-box">
      <Search size={16} class="search-icon" />
      <input
        type="text"
        placeholder="Buscar por matricula, conductor, DNI..."
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
      {state.filteredItems.length} de {state.items.length} camiones
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
      <h3>No hay camiones registrados</h3>
      <p>Comienza creando tu primer camion</p>
      <button class="btn-primary" on:click={openCreateModal}><Plus size={16} /> Crear</button>
    </div>
  {:else if state.filteredItems.length === 0 && state.searchQuery}
    <div class="empty-state">
      <h3>Sin resultados</h3>
      <p>No hay camiones que coincidan con "{state.searchQuery}"</p>
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
              <th>ID</th><th>DNI</th><th>Ubicacion</th><th>Container</th>
              <th>Albaran</th><th>Fecha</th><th>Actualizado</th><th></th>
            </tr>
          </thead>
          <tbody>
            {#each state.filteredItems as camion (camion.id)}
              <tr class="table-row" on:click={() => openEditModal(camion)}>
                <td class="id-cell">#{camion.id}</td>
                <td>{camion.DNI || '-'}</td>
                <td>{camion.UbicacionCampa || '-'}</td>
                <td>{camion.Container || '-'}</td>
                <td>{camion.Albaran || '-'}</td>
                <td>{camion.FechaDescarga ? formatDdMmYyyyFromIso(camion.FechaDescarga) : '-'}</td>
                <td class="update-cell">
                  {#if camion.updated_at}
                    <span class="update-date">{formatUpdateDate(camion.updated_at)}</span>
                    {#if camion.updated_by}
                      {#await getUserDisplayName(camion.updated_by)}
                        <span class="update-user">...</span>
                      {:then email}
                        <span class="update-user">{email}</span>
                      {:catch}
                        <span class="update-user">{camion.updated_by}</span>
                      {/await}
                    {/if}
                  {:else}-{/if}
                </td>
                <td>
                  <button class="btn-delete" on:click|stopPropagation={() => openDeleteModal(camion)} title="Eliminar">
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
        {#each state.filteredItems as camion (camion.id)}
          <div class="card" on:click={() => openEditModal(camion)} role="button" tabindex="0" on:keydown={(e) => { if (e.key === 'Enter') openEditModal(camion); }}>
            <div class="card-header">
              <span class="card-id">#{camion.id}</span>
              <button class="btn-delete" on:click|stopPropagation={() => openDeleteModal(camion)} title="Eliminar">
                <Trash2 size={14} color="#EF4444" />
              </button>
            </div>
            <div class="card-body">
              <h3>{camion.Matricula || 'Sin matricula'}</h3>
              <p class="subtitle">{camion.NombreConductor || 'Sin conductor'}</p>
              <div class="details-grid">
                <div class="detail"><span class="detail-label">DNI</span><span>{camion.DNI || '-'}</span></div>
                <div class="detail"><span class="detail-label">Ubicacion</span><span>{camion.UbicacionCampa || '-'}</span></div>
                <div class="detail"><span class="detail-label">Container</span><span>{camion.Container || '-'}</span></div>
                <div class="detail"><span class="detail-label">Albaran</span><span>{camion.Albaran || '-'}</span></div>
                <div class="detail full-width"><span class="detail-label">Fecha Descarga</span><span>{camion.FechaDescarga ? formatDdMmYyyyFromIso(camion.FechaDescarga) : '-'}</span></div>
              </div>
              {#if camion.updated_at || camion.updated_by}
                <div class="audit-info">
                  {#if camion.updated_at}{formatUpdateDate(camion.updated_at)}{/if}
                  {#if camion.updated_by}
                    {#await getUserDisplayName(camion.updated_by)}
                      <span class="audit-user">...</span>
                    {:then email}
                      <span class="audit-user">por {email}</span>
                    {:catch}
                      <span class="audit-user">por {camion.updated_by}</span>
                    {/await}
                  {/if}
                </div>
              {/if}
              <PhotoGallery tableName="camiones" recordId={camion.id} readonly={true} />
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
        <h3>Nuevo Camion</h3>
        <button class="icon-btn-ghost" on:click={closeModals}><X size={20} /></button>
      </div>
      <form class="modal-form" on:submit|preventDefault={createCamion}>
        <div class="form-row">
          <div class="form-group"><label for="new-matricula">Matricula *</label><input id="new-matricula" bind:value={newCamion.Matricula} required></div>
          <div class="form-group"><label for="new-dni">DNI</label><input id="new-dni" bind:value={newCamion.DNI}></div>
        </div>
        <div class="form-group"><label for="new-conductor">Conductor</label><input id="new-conductor" bind:value={newCamion.NombreConductor}></div>
        <div class="form-row">
          <div class="form-group"><label for="new-ubicacion">Ubicacion</label><input id="new-ubicacion" bind:value={newCamion.UbicacionCampa}></div>
          <div class="form-group"><label for="new-container">Container</label><input id="new-container" bind:value={newCamion.Container}></div>
        </div>
        <div class="form-row">
          <div class="form-group"><label for="new-albaran">Albaran</label><input id="new-albaran" bind:value={newCamion.Albaran}></div>
          <div class="form-group"><label for="new-fecha">Fecha Descarga</label><input id="new-fecha" type="date" bind:value={newCamion.FechaDescarga}></div>
        </div>
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
        <h3>Editar Camion #{state.selectedCamion?.id}</h3>
        <button class="icon-btn-ghost" on:click={closeModals}><X size={20} /></button>
      </div>
      <form class="modal-form" on:submit|preventDefault={updateCamion}>
        <div class="form-row">
          <div class="form-group"><label for="edit-matricula">Matricula *</label><input id="edit-matricula" bind:value={editCamion.Matricula} required></div>
          <div class="form-group"><label for="edit-dni">DNI</label><input id="edit-dni" bind:value={editCamion.DNI}></div>
        </div>
        <div class="form-group"><label for="edit-conductor">Conductor</label><input id="edit-conductor" bind:value={editCamion.NombreConductor}></div>
        <div class="form-row">
          <div class="form-group"><label for="edit-ubicacion">Ubicacion</label><input id="edit-ubicacion" bind:value={editCamion.UbicacionCampa}></div>
          <div class="form-group"><label for="edit-container">Container</label><input id="edit-container" bind:value={editCamion.Container}></div>
        </div>
        <div class="form-row">
          <div class="form-group"><label for="edit-albaran">Albaran</label><input id="edit-albaran" bind:value={editCamion.Albaran}></div>
          <div class="form-group"><label for="edit-fecha">Fecha Descarga</label><input id="edit-fecha" type="date" bind:value={editCamion.FechaDescarga}></div>
        </div>
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
          <PhotoGallery bind:this={photoGalleryRef} tableName="camiones" recordId={state.selectedCamion?.id} readonly={false} modalMode={true} />
        </div>
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
        <h3>Eliminar camion</h3>
        <button class="icon-btn-ghost" on:click={closeModals}><X size={20} /></button>
      </div>
      <div class="modal-body">
        <p>Eliminar camion <strong>#{state.selectedCamion?.id}</strong>?</p>
        <p class="warning-text">Esta accion no se puede deshacer.</p>
        {#if state.error}<div class="form-error">{state.error}</div>{/if}
      </div>
      <div class="modal-actions">
        <button type="button" class="btn-secondary" on:click={closeModals}>Cancelar</button>
        <button class="btn-danger" on:click={deleteCamion} disabled={state.isSubmitting}>{state.isSubmitting ? 'Eliminando...' : 'Eliminar'}</button>
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
