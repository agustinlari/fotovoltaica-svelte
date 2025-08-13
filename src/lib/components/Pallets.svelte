<script lang="ts">
  import { onMount } from 'svelte';
  import type { Pallet } from '../api';
  import { apiGet, apiPut } from '../api';

  type State = {
    loading: boolean;
    items: Pallet[];
    selectedId: string | null;
    error: string | null;
    info: string | null;
  };

  let state: State = { loading: false, items: [], selectedId: null, error: null, info: null };
  let form: Partial<Pallet> = {};

  async function load() {
    state.loading = true;
    state.error = null;
    try {
      state.items = await apiGet<Pallet[]>('/pallets');
      if (state.items.length && state.selectedId == null) select(state.items[0].id);
    } catch (e: any) {
      state.error = e?.message || 'Error cargando pallets';
    } finally {
      state.loading = false;
    }
  }

  function select(id: string) {
    state.selectedId = id;
    const item = state.items.find((i) => i.id === id);
    if (!item) return;
    form = { ...item };
  }

  async function save() {
    if (state.selectedId == null) return;
    state.info = null;
    state.error = null;
    try {
      await apiPut(`/pallets/${state.selectedId}`, form);
      state.info = 'Guardado';
      await load();
      select(state.selectedId);
    } catch (e: any) {
      state.error = e?.message || 'Error guardando';
    }
  }

  onMount(load);
</script>

<div class="wrap">
  <div class="sidebar">
    <h3>Pallets</h3>
    {#if state.loading}
      <p>Cargando…</p>
    {:else if state.error}
      <p class="error">{state.error}</p>
    {:else}
      <ul>
        {#each state.items as it}
          <li class:selected={it.id === state.selectedId}>
            <button on:click={() => select(it.id)}>{it.id} {it.Descarga ? `(Descarga ${it.Descarga})` : ''}</button>
          </li>
        {/each}
      </ul>
    {/if}
  </div>
  <div class="content">
    <h3>Editar</h3>
    {#if state.selectedId === null}
      <p>Selecciona un pallet</p>
    {:else}
      <div class="form">
        <label>
          Descarga
          <input type="number" bind:value={form.Descarga} />
        </label>
        <label>
          Defecto
          <select bind:value={form.Defecto}>
            <option value={true}>Sí</option>
            <option value={false}>No</option>
          </select>
        </label>
        <div class="actions">
          <button on:click={save}>Guardar</button>
          {#if state.info}<span class="info">{state.info}</span>{/if}
          {#if state.error}<span class="error">{state.error}</span>{/if}
        </div>
      </div>
    {/if}
  </div>
</div>

<style>
  .wrap { display: grid; grid-template-columns: 260px 1fr; gap: 16px; }
  .sidebar { border-right: 1px solid #ddd; padding-right: 12px; }
  .sidebar ul { list-style: none; padding: 0; margin: 0; max-height: 70vh; overflow: auto; }
  .sidebar li { margin: 0; }
  .sidebar li.selected button { font-weight: 600; }
  .sidebar button { width: 100%; text-align: left; padding: 6px 8px; background: transparent; border: none; cursor: pointer; }
  .content { padding-left: 8px; }
  .form { display: grid; gap: 10px; max-width: 520px; }
  label { display: grid; gap: 4px; }
  input, select { padding: 6px 8px; border: 1px solid #ccc; border-radius: 6px; }
  .actions { display: flex; align-items: center; gap: 10px; margin-top: 8px; }
  .error { color: #b00020; }
  .info { color: #2f7d32; }
</style>


