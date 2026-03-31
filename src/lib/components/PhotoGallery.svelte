<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { FolderOpen, Camera, X } from 'lucide-svelte';

  export let tableName: string;
  export let recordId: string | number;
  export let readonly: boolean = false;
  export let modalMode: boolean = false;

  type Photo = {
    id: number;
    filename: string;
    original_name: string;
    mime_type: string;
    size: number;
    created_at: string;
  };

  type State = {
    photos: Photo[];
    loading: boolean;
    error: string | null;
    uploading: boolean;
    showLightbox: boolean;
    lightboxImage: string | null;
  };

  let state: State = {
    photos: [],
    loading: true,
    error: null,
    uploading: false,
    showLightbox: false,
    lightboxImage: null
  };

  let fileInput: HTMLInputElement;
  const dispatch = createEventDispatcher();

  function getApiBase(): string {
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
      return 'http://localhost:8787';
    }
    return `${window.location.protocol}//${window.location.host}/api/fotovoltaica`;
  }

  function getUploadsBase(): string {
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
      return 'http://localhost:8787/uploads';
    }
    return `${window.location.protocol}//${window.location.host}/public/fotovoltaica-uploads`;
  }

  async function loadPhotos() {
    try {
      state.loading = true;
      state.error = null;
      const response = await fetch(`${getApiBase()}/files/${tableName}/${recordId}`);
      if (response.ok) {
        state.photos = await response.json();
      } else {
        state.error = 'Error cargando fotos';
      }
    } catch (error) {
      state.error = 'Error de conexion';
      console.error('Error cargando fotos:', error);
    } finally {
      state.loading = false;
    }
  }

  async function uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    try {
      state.uploading = true;
      state.error = null;
      const response = await fetch(`${getApiBase()}/upload/${tableName}/${recordId}`, {
        method: 'POST',
        body: formData
      });

      if (response.ok) {
        await loadPhotos();
        dispatch('photoAdded');
      } else {
        const errorData = await response.json();
        state.error = errorData.error || 'Error subiendo archivo';
      }
    } catch (error) {
      state.error = 'Error de conexion';
      console.error('Error subiendo archivo:', error);
    } finally {
      state.uploading = false;
    }
  }

  function handleFileSelect(event: Event) {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (file) {
      uploadFile(file);
    }
  }

  async function deletePhoto(photoId: number) {
    if (!confirm('Eliminar esta foto?')) return;

    try {
      const response = await fetch(`${getApiBase()}/files/${photoId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        await loadPhotos();
        dispatch('photoDeleted');
      } else {
        state.error = 'Error eliminando foto';
      }
    } catch (error) {
      state.error = 'Error de conexion';
      console.error('Error eliminando foto:', error);
    }
  }

  function openLightbox(filename: string) {
    state.lightboxImage = filename;
    state.showLightbox = true;
  }

  function closeLightbox() {
    state.showLightbox = false;
    state.lightboxImage = null;
  }

  function handleLightboxClick(event: MouseEvent) {
    if (event.target === event.currentTarget) {
      closeLightbox();
    }
  }

  function triggerFileInput(useCamera: boolean = false) {
    if (!fileInput) {
      console.error('File input not available');
      return;
    }
    if (useCamera) {
      fileInput.setAttribute('capture', 'environment');
    } else {
      fileInput.removeAttribute('capture');
    }
    fileInput.click();
  }

  $: if (tableName && recordId) {
    loadPhotos();
  }

  export function triggerGallery() { triggerFileInput(false); }
  export function triggerCamera() { triggerFileInput(true); }
  export function getState() { return state; }
  export function getPhotos() { return state.photos; }
</script>

<div class="photo-gallery" class:modal-mode={modalMode}>
  {#if !readonly}
    <input
      bind:this={fileInput}
      type="file"
      accept="image/*"
      style="display: none;"
      on:change={handleFileSelect}
    />

    {#if !modalMode}
      <div class="upload-section">
        <div class="upload-buttons">
          <button class="btn-upload" on:click={() => triggerFileInput(false)} disabled={state.uploading}>
            <FolderOpen size={14} /> Galeria
          </button>
          <button class="btn-upload" on:click={() => triggerFileInput(true)} disabled={state.uploading}>
            <Camera size={14} /> Camara
          </button>
        </div>
        {#if state.uploading}
          <div class="upload-progress">
            <div class="spinner-small"></div>
            <span>Subiendo...</span>
          </div>
        {/if}
      </div>
    {/if}
  {/if}

  {#if state.error}
    <div class="error-message">{state.error}</div>
  {/if}

  <div class="photos-grid">
    {#if state.loading}
      <div class="loading-photos">
        <div class="spinner-small"></div>
        <span>Cargando fotos...</span>
      </div>
    {:else if state.photos.length === 0}
      <div class="no-photos">
        {#if readonly}
          <span>Sin fotos</span>
        {:else}
          <span>No hay fotos</span>
        {/if}
      </div>
    {:else}
      {#each state.photos as photo (photo.id)}
        <div class="photo-item">
          <img
            src="{getUploadsBase()}/{photo.filename}"
            alt={photo.original_name}
            loading="lazy"
            on:click={() => openLightbox(photo.filename)}
            on:keydown={(e) => e.key === 'Enter' && openLightbox(photo.filename)}
            tabindex="0"
            role="button"
          />
          {#if !readonly}
            <button class="delete-photo" on:click={() => deletePhoto(photo.id)} title="Eliminar foto">
              <X size={12} />
            </button>
          {/if}
          <div class="photo-info">
            <span class="photo-name">{photo.original_name}</span>
            <span class="photo-size">{Math.round(photo.size / 1024)} KB</span>
          </div>
        </div>
      {/each}
    {/if}
  </div>
</div>

{#if state.showLightbox && state.lightboxImage}
  <div class="lightbox-overlay" on:click={handleLightboxClick} role="dialog" aria-modal="true">
    <div class="lightbox-content">
      <button class="lightbox-close" on:click={closeLightbox}><X size={18} /></button>
      <img src="{getUploadsBase()}/{state.lightboxImage}" alt="Vista ampliada" />
    </div>
  </div>
{/if}

<style>
  .photo-gallery { margin-top: 12px; }
  .photo-gallery.modal-mode { margin-top: 0; }

  .upload-section {
    margin-bottom: 12px; padding: 12px;
    background: #F9FAFB; border: 1px dashed #D1D5DB; border-radius: 8px;
  }
  .upload-buttons { display: flex; gap: 8px; margin-bottom: 8px; }
  .btn-upload {
    display: flex; align-items: center; gap: 4px;
    background: white; color: #374151; border: 1px solid #D1D5DB;
    padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 500;
    cursor: pointer; transition: all 0.2s;
  }
  .btn-upload:hover:not(:disabled) { background: #F3F4F6; }
  .btn-upload:disabled { opacity: 0.5; cursor: not-allowed; }
  .upload-progress { display: flex; align-items: center; gap: 8px; font-size: 12px; color: #6B7280; }

  .spinner-small {
    width: 14px; height: 14px; border: 2px solid #E5E7EB;
    border-top: 2px solid #3B82F6; border-radius: 50%; animation: spin 1s linear infinite;
  }
  @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }

  .error-message {
    background: #FEF2F2; color: #DC2626; padding: 8px 12px;
    border-radius: 8px; font-size: 12px; margin-bottom: 12px;
  }

  .photos-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(90px, 1fr)); gap: 8px; }
  .loading-photos, .no-photos { grid-column: 1 / -1; text-align: center; padding: 16px; color: #9CA3AF; font-size: 13px; }
  .loading-photos { display: flex; align-items: center; justify-content: center; gap: 8px; }

  .photo-item {
    position: relative; background: white; border-radius: 8px; overflow: hidden;
    border: 1px solid #E5E7EB; transition: border-color 0.2s;
  }
  .photo-item:hover { border-color: #D1D5DB; }
  .photo-item img { width: 100%; height: 72px; object-fit: cover; cursor: pointer; display: block; }

  .delete-photo {
    position: absolute; top: 4px; right: 4px;
    background: rgba(0,0,0,0.5); color: white; border: none;
    width: 20px; height: 20px; border-radius: 50%;
    cursor: pointer; display: flex; align-items: center; justify-content: center;
    opacity: 0; transition: opacity 0.2s;
  }
  .photo-item:hover .delete-photo { opacity: 1; }
  .delete-photo:hover { background: rgba(220, 38, 38, 0.9); }

  .photo-info { padding: 4px 6px; }
  .photo-name { font-size: 10px; color: #374151; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; display: block; }
  .photo-size { font-size: 9px; color: #9CA3AF; }

  .lightbox-overlay {
    position: fixed; top: 0; left: 0; right: 0; bottom: 0;
    background: rgba(0,0,0,0.9); display: flex; align-items: center;
    justify-content: center; z-index: 2000; padding: 20px;
  }
  .lightbox-content { position: relative; max-width: 90vw; max-height: 90vh; }
  .lightbox-content img { max-width: 100%; max-height: 100%; object-fit: contain; border-radius: 8px; }
  .lightbox-close {
    position: absolute; top: -40px; right: 0;
    background: rgba(255,255,255,0.9); border: none; width: 32px; height: 32px;
    border-radius: 50%; cursor: pointer; display: flex; align-items: center;
    justify-content: center; color: #374151;
  }
  .lightbox-close:hover { background: white; }

  @media (max-width: 640px) {
    .photos-grid { grid-template-columns: repeat(auto-fill, minmax(72px, 1fr)); gap: 6px; }
    .photo-item img { height: 56px; }
  }
</style>
