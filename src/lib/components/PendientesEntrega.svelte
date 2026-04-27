<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { apiGet, apiPost, apiPut, apiDelete, type Articulo, type EstructuraPendiente } from '../api';
  import { Plus, Trash2, Check, X, Search, ChevronRight } from 'lucide-svelte';

  export let estructuraId: number;

  let pendientes: EstructuraPendiente[] = [];
  let articulos: Articulo[] = [];
  let loading = true;
  let error: string | null = null;

  let showAddForm = false;
  let cantidad = 1;
  let selectedArticulo: Articulo | null = null;
  let isSubmitting = false;

  // Modal picker state
  let showPicker = false;
  let pickerSearch = '';
  let pickerSearchInput: HTMLInputElement | null = null;

  $: pickerFilteredArticulos = pickerSearch.trim()
    ? articulos.filter(a => {
        const q = pickerSearch.toLowerCase();
        return a.reference.toLowerCase().includes(q) ||
               (a.description || '').toLowerCase().includes(q);
      })
    : articulos;

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
    cantidad = 1;
    selectedArticulo = null;
  }

  function closeAddForm() {
    showAddForm = false;
    selectedArticulo = null;
  }

  async function openPicker() {
    pickerSearch = '';
    showPicker = true;
    await tick();
    pickerSearchInput?.focus();
  }

  function closePicker() {
    showPicker = false;
  }

  function pickArticulo(a: Articulo) {
    selectedArticulo = a;
    showPicker = false;
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
      <button
        type="button"
        class="picker-trigger"
        class:has-value={selectedArticulo !== null}
        on:click={openPicker}
      >
        {#if selectedArticulo}
          <div class="picker-trigger-content">
            <span class="picker-ref">{selectedArticulo.reference}</span>
            {#if selectedArticulo.description}
              <span class="picker-desc">{selectedArticulo.description}</span>
            {/if}
          </div>
        {:else}
          <span class="picker-placeholder">Seleccionar articulo</span>
        {/if}
        <ChevronRight size={16} color="#9CA3AF" />
      </button>

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

{#if showPicker}
  <div class="picker-overlay" on:click={closePicker} role="dialog" aria-modal="true">
    <div class="picker-modal" on:click|stopPropagation>
      <div class="picker-header">
        <button type="button" class="picker-close" on:click={closePicker} title="Cerrar">
          <X size={20} />
        </button>
        <h3>Seleccionar articulo</h3>
        <div class="picker-spacer"></div>
      </div>

      <div class="picker-search">
        <Search size={16} color="#9CA3AF" />
        <input
          bind:this={pickerSearchInput}
          type="text"
          placeholder="Buscar referencia o descripcion..."
          bind:value={pickerSearch}
          class="picker-search-input"
        />
        {#if pickerSearch}
          <button type="button" class="picker-search-clear" on:click={() => { pickerSearch = ''; }}>
            <X size={14} />
          </button>
        {/if}
      </div>

      <div class="picker-meta">
        {pickerFilteredArticulos.length} de {articulos.length} articulos
      </div>

      <ul class="picker-list">
        {#if pickerFilteredArticulos.length === 0}
          <li class="picker-empty">Sin coincidencias</li>
        {:else}
          {#each pickerFilteredArticulos as a (a.id)}
            <li>
              <button type="button" class="picker-item" on:click={() => pickArticulo(a)}>
                <div class="picker-item-content">
                  <span class="picker-item-ref">{a.reference}</span>
                  {#if a.description}
                    <span class="picker-item-desc">{a.description}</span>
                  {/if}
                </div>
                <ChevronRight size={16} color="#9CA3AF" />
              </button>
            </li>
          {/each}
        {/if}
      </ul>
    </div>
  </div>
{/if}

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

  .picker-trigger {
    display: flex; align-items: center; justify-content: space-between; gap: 8px;
    width: 100%; background: white; border: 1px solid #D1D5DB;
    border-radius: 8px; padding: 10px 12px; cursor: pointer;
    text-align: left; transition: all 0.2s;
  }
  .picker-trigger:hover { border-color: #9CA3AF; }
  .picker-trigger.has-value { border-color: #1F2937; }
  .picker-trigger-content { display: flex; flex-direction: column; gap: 2px; min-width: 0; flex: 1; }
  .picker-ref { font-size: 14px; font-weight: 600; color: #1F2937; }
  .picker-desc {
    font-size: 12px; color: #6B7280;
    white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
  }
  .picker-placeholder { font-size: 14px; color: #9CA3AF; flex: 1; }

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

  /* Picker modal */
  .picker-overlay {
    position: fixed; top: 0; left: 0; right: 0; bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex; align-items: center; justify-content: center;
    z-index: 2000; padding: 0;
  }
  .picker-modal {
    background: white;
    width: 100%; height: 100%;
    max-width: 560px; max-height: 100vh;
    display: flex; flex-direction: column;
    overflow: hidden;
  }
  @media (min-width: 640px) {
    .picker-overlay { padding: 20px; }
    .picker-modal {
      max-height: 85vh; height: auto;
      border-radius: 16px;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    }
  }

  .picker-header {
    display: grid;
    grid-template-columns: 40px 1fr 40px;
    align-items: center;
    padding: 12px 16px;
    border-bottom: 1px solid #E5E7EB;
    flex-shrink: 0;
  }
  .picker-header h3 {
    margin: 0; font-size: 16px; font-weight: 600; color: #1F2937;
    text-align: center;
  }
  .picker-close {
    display: flex; align-items: center; justify-content: center;
    width: 36px; height: 36px; border-radius: 8px; border: none;
    background: transparent; color: #6B7280; cursor: pointer; transition: all 0.2s;
  }
  .picker-close:hover { background: #F3F4F6; color: #1F2937; }
  .picker-spacer { width: 36px; }

  .picker-search {
    display: flex; align-items: center; gap: 8px;
    margin: 12px 16px;
    background: #F3F4F6; border-radius: 10px;
    padding: 10px 12px;
    flex-shrink: 0;
  }
  .picker-search-input {
    flex: 1; border: none; outline: none; background: transparent;
    font-size: 16px; min-width: 0; color: #1F2937;
  }
  .picker-search-clear {
    display: flex; align-items: center; justify-content: center;
    width: 24px; height: 24px; border-radius: 6px; border: none;
    background: #E5E7EB; color: #6B7280; cursor: pointer; flex-shrink: 0;
  }
  .picker-search-clear:hover { background: #D1D5DB; }

  .picker-meta {
    padding: 0 16px 8px;
    font-size: 12px; color: #9CA3AF;
    flex-shrink: 0;
  }

  .picker-list {
    list-style: none; margin: 0; padding: 0;
    overflow-y: auto;
    flex: 1;
    -webkit-overflow-scrolling: touch;
  }
  .picker-empty {
    text-align: center; padding: 32px 16px; color: #9CA3AF; font-size: 14px;
  }
  .picker-list li {
    border-bottom: 1px solid #F3F4F6;
  }
  .picker-list li:last-child { border-bottom: none; }
  .picker-item {
    display: flex; align-items: center; justify-content: space-between; gap: 12px;
    width: 100%; background: transparent; border: none;
    padding: 14px 16px; cursor: pointer; text-align: left;
    transition: background 0.15s;
  }
  .picker-item:hover { background: #F9FAFB; }
  .picker-item:active { background: #F3F4F6; }
  .picker-item-content {
    display: flex; flex-direction: column; gap: 2px;
    min-width: 0; flex: 1;
  }
  .picker-item-ref { font-size: 15px; font-weight: 600; color: #1F2937; }
  .picker-item-desc {
    font-size: 13px; color: #6B7280;
    white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
  }
</style>
