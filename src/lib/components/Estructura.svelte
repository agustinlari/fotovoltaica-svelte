<script lang="ts">
  import { onMount } from 'svelte';
  import type { Estructura } from '../api';
  import { apiGet, apiPut } from '../api';
  import { formatDdMmYyyyFromIso, toIsoMidnight } from '../date';

  type State = {
    loading: boolean;
    items: Estructura[];
    selectedId: number | null;
    error: string | null;
    info: string | null;
  };

  let state: State = { loading: false, items: [], selectedId: null, error: null, info: null };
  let form: Partial<Estructura & { FechaDescargaUi?: string }> = {};

  async function load() {
    state.loading = true;
    state.error = null;
    try {
      state.items = await apiGet<Estructura[]>('/estructura');
      if (state.items.length && state.selectedId == null) select(state.items[0].id);
    } catch (e: any) {
      state.error = e?.message || 'Error cargando estructura';
    } finally {
      state.loading = false;
    }
  }

  function select(id: number) {
    state.selectedId = id;
    const item = state.items.find((i) => i.id === id);
    if (!item) return;
    form = {
      ...item,
      FechaDescargaUi: formatDdMmYyyyFromIso(item.FechaDescarga ?? null)
    };
  }

  async function save() {
    if (state.selectedId == null) return;
    state.info = null;
    state.error = null;
    const payload: any = { ...form };
    if (payload.FechaDescargaUi !== undefined) {
      const iso = toIsoMidnight(payload.FechaDescargaUi);
      if (!iso) {
        state.error = 'Fecha inválida (usa dd/MM/yyyy)';
        return;
      }
      payload.FechaDescarga = iso;
      delete payload.FechaDescargaUi;
    }
    try {
      await apiPut(`/estructura/${state.selectedId}`, payload);
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
    <h3>Estructura</h3>
    {#if state.loading}
      <p>Cargando…</p>
    {:else if state.error}
      <p class="error">{state.error}</p>
    {:else}
      <ul>
        {#each state.items as it}
          <li class:selected={it.id === state.selectedId}>
            <button on:click={() => select(it.id)}>{it.id} {it.Matricula ? `(${it.Matricula})` : ''}</button>
          </li>
        {/each}
      </ul>
    {/if}
  </div>
  <div class="content">
    <h3>Editar</h3>
    {#if state.selectedId === null}
      <p>Selecciona un registro de estructura</p>
    {:else}
      <div class="form">
        <label>
          Matrícula
          <input bind:value={form.Matricula} />
        </label>
        <label>
          Conductor
          <input bind:value={form.Conductor} />
        </label>
        <label>
          DNI
          <input bind:value={form.DNI} />
        </label>
        <label>
          Proveedor
          <input bind:value={form.Proveedor} />
        </label>
        <label>
          Packing List
          <input bind:value={form.PackingList} />
        </label>
        <label>
          Albarán
          <input bind:value={form.Albaran} />
        </label>
        <label>
          Fecha descarga (dd/MM/yyyy)
          <input bind:value={form.FechaDescargaUi} placeholder="dd/MM/yyyy" />
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


