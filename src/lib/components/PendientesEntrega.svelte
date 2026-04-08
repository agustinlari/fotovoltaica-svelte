<script lang="ts">
  import { onMount } from 'svelte';
  import { apiGet, apiPost, apiPut, apiDelete, type Articulo, type EstructuraPendiente } from '../api';
  import { Plus, Trash2, Check, X, Search } from 'lucide-svelte';

  export let estructuraId: number;

  let pendientes: EstructuraPendiente[] = [];
  let articulos: Articulo[] = [];
  let loading = true;
  let error: string | null = null;

  let showAddForm = false;
  let searchQuery = '';
  let cantidad = 1;
  let selectedArticulo: Articulo | null = null;
  let isSubmitting = false;

  $: filteredArticulos = searchQuery.trim()
    ? articulos.filter(a => {
        const q = searchQuery.toLowerCase();
        return a.reference.toLowerCase().includes(q) ||
               (a.description || '').toLowerCase().includes(q);
      }).slice(0, 8)
    : [];

  async function load() {
    try {
      loading = true;
      error = null;
      const [pendientesData, articulosData] = await Promise.all([
        apiGet<EstructuraPendiente[]>(`/estructura/${estructuraId}/pendientes`),
        articulos.length === 0 ? apiGet<Articulo[]>('/articulos') : Promise.resolve(articulos),
      ]);
      pendientes = pendientesData;
      articulos = articulosData;
    } catch (e: any) {
      error = e?.message || 'Error cargando pendientes';
    } finally {
      loading = false;
    }
  }

  function openAddForm() {
    showAddForm = true;
    searchQuery = '';
    cantidad = 1;
    selectedArticulo = null;
  }

  function closeAddForm() {
    showAddForm = false;
    selectedArticulo = null;
    searchQuery = '';
  }

  function selectArticulo(a: Articulo) {
    selectedArticulo = a;
    searchQuery = `${a.reference}${a.description ? ' - ' + a.description : ''}`;
  }

  async function addPendiente() {
    if (!selectedArticulo || cantidad < 1 || isSubmitting) return;
    try {
      isSubmitting = true;
      await apiPost(`/estructura/${estructuraId}/pendientes`, {
        articulo_id: selectedArticulo.id,
        cantidad,
      });
      await load();
      closeAddForm();
    } catch (e: any) {
      error = e?.message || 'Error añadiendo pendiente';
    } finally {
      isSubmitting = false;
    }
  }

  async function toggleEntregado(p: EstructuraPendiente) {
    try {
      await apiPut(`/estructura/${estructuraId}/pendientes/${p.id}`, {
        entregado: !p.entregado,
      });
      await load();
    } catch (e: any) {
      error = e?.message || 'Error actualizando';
    }
  }

  async function deletePendiente(p: EstructuraPendiente) {
    if (!confirm(`Eliminar pendiente "${p.reference}"?`)) return;
    try {
      await apiDelete(`/estructura/${estructuraId}/pendientes/${p.id}`);
      await load();
    } catch (e: any) {
      error = e?.message || 'Error eliminando';
    }
  }

  onMount(load);
</script>

<div class="pendientes">
  <div class="pendientes-header">
    <h4>Articulos pendientes de entrega</h4>
    {#if !showAddForm}
      <button type="button" class="btn-add" on:click={openAddForm}>
        <Plus size={14} />
        Añadir
      </button>
    {/if}
  </div>

  {#if error}
    <div class="error-msg">{error}</div>
  {/if}

  {#if showAddForm}
    <div class="add-form">
      <div class="search-wrapper">
        <Search size={14} color="#9CA3AF" />
        <input
          type="text"
          placeholder="Buscar referencia o descripcion..."
          bind:value={searchQuery}
          on:input={() => { selectedArticulo = null; }}
          class="search-input"
        />
      </div>

      {#if searchQuery && !selectedArticulo}
        {#if filteredArticulos.length > 0}
          <ul class="suggestions">
            {#each filteredArticulos as a (a.id)}
              <li>
                <button type="button" on:click={() => selectArticulo(a)}>
                  <span class="ref">{a.reference}</span>
                  {#if a.description}
                    <span class="desc">{a.description}</span>
                  {/if}
                </button>
              </li>
            {/each}
          </ul>
        {:else}
          <div class="no-results">Sin coincidencias</div>
        {/if}
      {/if}

      <div class="form-row">
        <label>
          Cantidad
          <input type="number" min="1" bind:value={cantidad} class="cantidad-input" />
        </label>
        <div class="form-actions">
          <button type="button" class="btn-secondary" on:click={closeAddForm}>
            Cancelar
          </button>
          <button
            type="button"
            class="btn-primary"
            on:click={addPendiente}
            disabled={!selectedArticulo || cantidad < 1 || isSubmitting}
          >
            {isSubmitting ? 'Añadiendo...' : 'Añadir'}
          </button>
        </div>
      </div>
    </div>
  {/if}

  {#if loading}
    <div class="loading">Cargando...</div>
  {:else if pendientes.length === 0}
    <div class="empty">No hay articulos pendientes</div>
  {:else}
    <ul class="pendientes-list">
      {#each pendientes as p (p.id)}
        <li class="pendiente-item" class:entregado={p.entregado}>
          <button
            type="button"
            class="check-btn"
            class:active={p.entregado}
            on:click={() => toggleEntregado(p)}
            title={p.entregado ? 'Marcar como pendiente' : 'Marcar como entregado'}
          >
            {#if p.entregado}<Check size={14} />{/if}
          </button>
          <div class="pendiente-info">
            <div class="pendiente-ref">{p.reference}</div>
            {#if p.description}
              <div class="pendiente-desc">{p.description}</div>
            {/if}
          </div>
          <div class="pendiente-cantidad">x{p.cantidad}</div>
          <button
            type="button"
            class="btn-icon-delete"
            on:click={() => deletePendiente(p)}
            title="Eliminar"
          >
            <Trash2 size={14} color="#EF4444" />
          </button>
        </li>
      {/each}
    </ul>
  {/if}
</div>

<style>
  .pendientes { margin-top: 16px; }

  .pendientes-header {
    display: flex; justify-content: space-between; align-items: center;
    margin-bottom: 12px;
  }
  .pendientes-header h4 {
    margin: 0; font-size: 14px; font-weight: 600; color: #374151;
  }

  .btn-add {
    display: flex; align-items: center; gap: 4px;
    background: white; color: #374151; border: 1px solid #D1D5DB;
    padding: 6px 12px; border-radius: 8px; font-size: 13px; font-weight: 500;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-add:hover { background: #F3F4F6; }

  .error-msg {
    background: #FEF2F2; color: #DC2626; padding: 8px 12px;
    border-radius: 8px; font-size: 13px; margin-bottom: 12px;
  }

  .add-form {
    background: #F9FAFB; border: 1px solid #E5E7EB; border-radius: 10px;
    padding: 12px; margin-bottom: 12px;
  }

  .search-wrapper {
    display: flex; align-items: center; gap: 8px;
    background: white; border: 1px solid #D1D5DB; border-radius: 8px;
    padding: 8px 10px;
  }
  .search-input {
    flex: 1; border: none; outline: none; font-size: 14px;
    background: transparent; min-width: 0;
  }

  .suggestions {
    list-style: none; padding: 0; margin: 8px 0 0;
    background: white; border: 1px solid #E5E7EB; border-radius: 8px;
    max-height: 220px; overflow-y: auto;
  }
  .suggestions li { border-bottom: 1px solid #F3F4F6; }
  .suggestions li:last-child { border-bottom: none; }
  .suggestions button {
    display: flex; flex-direction: column; align-items: flex-start; gap: 2px;
    width: 100%; background: transparent; border: none; padding: 8px 12px;
    cursor: pointer; text-align: left; font-size: 13px;
  }
  .suggestions button:hover { background: #F3F4F6; }
  .suggestions .ref { font-weight: 600; color: #1F2937; }
  .suggestions .desc { font-size: 12px; color: #6B7280; }

  .no-results {
    margin-top: 8px; padding: 8px 12px; background: white;
    border: 1px solid #E5E7EB; border-radius: 8px;
    text-align: center; color: #9CA3AF; font-size: 13px;
  }

  .form-row {
    display: flex; gap: 12px; align-items: flex-end; margin-top: 12px;
    flex-wrap: wrap;
  }
  .form-row label {
    display: flex; flex-direction: column; gap: 4px;
    font-size: 12px; font-weight: 600; color: #6B7280;
    text-transform: uppercase; letter-spacing: 0.5px;
  }
  .cantidad-input {
    width: 90px; padding: 8px 10px; border: 1px solid #D1D5DB;
    border-radius: 8px; font-size: 14px; text-transform: none; font-weight: normal; color: #1F2937;
  }
  .form-actions { display: flex; gap: 8px; margin-left: auto; }
  .btn-primary {
    background: #1F2937; color: white; border: none; padding: 8px 16px;
    border-radius: 8px; font-weight: 500; font-size: 13px; cursor: pointer; transition: all 0.2s;
  }
  .btn-primary:hover:not(:disabled) { background: #111827; }
  .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
  .btn-secondary {
    background: white; color: #374151; border: 1px solid #D1D5DB;
    padding: 8px 16px; border-radius: 8px; font-weight: 500; font-size: 13px;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-secondary:hover { background: #F3F4F6; }

  .loading, .empty {
    text-align: center; padding: 16px; color: #9CA3AF; font-size: 13px;
  }

  .pendientes-list {
    list-style: none; padding: 0; margin: 0;
    display: flex; flex-direction: column; gap: 8px;
  }
  .pendiente-item {
    display: flex; align-items: center; gap: 10px;
    background: white; border: 1px solid #E5E7EB; border-radius: 10px;
    padding: 10px 12px;
  }
  .pendiente-item.entregado { background: #F9FAFB; }
  .pendiente-item.entregado .pendiente-ref,
  .pendiente-item.entregado .pendiente-desc {
    text-decoration: line-through; color: #9CA3AF;
  }

  .check-btn {
    width: 22px; height: 22px; border-radius: 6px;
    border: 1.5px solid #D1D5DB; background: white;
    display: flex; align-items: center; justify-content: center;
    cursor: pointer; flex-shrink: 0; color: white; transition: all 0.2s;
  }
  .check-btn:hover { border-color: #9CA3AF; }
  .check-btn.active {
    background: #10B981; border-color: #10B981;
  }

  .pendiente-info { flex: 1; min-width: 0; }
  .pendiente-ref { font-size: 14px; font-weight: 600; color: #1F2937; }
  .pendiente-desc { font-size: 12px; color: #6B7280; }
  .pendiente-cantidad {
    font-size: 14px; font-weight: 600; color: #374151;
    background: #F3F4F6; padding: 4px 10px; border-radius: 6px;
  }

  .btn-icon-delete {
    display: flex; align-items: center; justify-content: center;
    width: 28px; height: 28px; border-radius: 6px;
    border: 1px solid #E5E7EB; background: #FEF2F2;
    cursor: pointer; transition: all 0.2s; flex-shrink: 0;
  }
  .btn-icon-delete:hover { background: #FEE2E2; border-color: #FECACA; }
</style>
