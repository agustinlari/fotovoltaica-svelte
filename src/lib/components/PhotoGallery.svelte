<script lang="ts">
  import { createEventDispatcher } from 'svelte';

  export let tableName: string;
  export let recordId: string | number;
  export let readonly: boolean = false;

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

  // Función para obtener la URL base de la API
  function getApiBase(): string {
    if (window.location.protocol === 'https:' && window.location.hostname === 'aplicaciones.osmos.es') {
      return `${window.location.protocol}//${window.location.host}/api/fotovoltaica`;
    }
    return 'http://localhost:8787';
  }

  // Función para obtener la URL base de uploads
  function getUploadsBase(): string {
    if (window.location.protocol === 'https:' && window.location.hostname === 'aplicaciones.osmos.es') {
      return `${window.location.protocol}//${window.location.host}/public/fotovoltaica-uploads`;
    }
    return 'http://localhost:8787/uploads';
  }

  // Cargar fotos al montar el componente
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
      state.error = 'Error de conexión';
      console.error('Error cargando fotos:', error);
    } finally {
      state.loading = false;
    }
  }

  // Subir archivo
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
        await loadPhotos(); // Recargar la lista
        dispatch('photoAdded');
      } else {
        const errorData = await response.json();
        state.error = errorData.error || 'Error subiendo archivo';
      }
    } catch (error) {
      state.error = 'Error de conexión';
      console.error('Error subiendo archivo:', error);
    } finally {
      state.uploading = false;
    }
  }

  // Manejar selección de archivo
  function handleFileSelect(event: Event) {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (file) {
      uploadFile(file);
    }
  }

  // Eliminar foto
  async function deletePhoto(photoId: number) {
    if (!confirm('¿Estás seguro de que quieres eliminar esta foto?')) return;

    try {
      const response = await fetch(`${getApiBase()}/files/${photoId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        await loadPhotos(); // Recargar la lista
        dispatch('photoDeleted');
      } else {
        state.error = 'Error eliminando foto';
      }
    } catch (error) {
      state.error = 'Error de conexión';
      console.error('Error eliminando foto:', error);
    }
  }

  // Abrir lightbox
  function openLightbox(filename: string) {
    state.lightboxImage = filename;
    state.showLightbox = true;
  }

  // Cerrar lightbox
  function closeLightbox() {
    state.showLightbox = false;
    state.lightboxImage = null;
  }

  // Manejar click en overlay del lightbox
  function handleLightboxClick(event: MouseEvent) {
    if (event.target === event.currentTarget) {
      closeLightbox();
    }
  }

  // Activar input de archivo desde galería o cámara
  function triggerFileInput(useCamera: boolean = false) {
    if (useCamera) {
      fileInput.setAttribute('capture', 'environment');
    } else {
      fileInput.removeAttribute('capture');
    }
    fileInput.click();
  }

  // Cargar fotos al montar
  $: if (tableName && recordId) {
    loadPhotos();
  }
</script>

<div class="photo-gallery">
  {#if !readonly}
    <div class="upload-section">
      <div class="upload-buttons">
        <button class="btn-upload" on:click={() => triggerFileInput(false)} disabled={state.uploading}>
          📁 Galería
        </button>
        <button class="btn-upload" on:click={() => triggerFileInput(true)} disabled={state.uploading}>
          📷 Cámara
        </button>
      </div>
      
      <input
        bind:this={fileInput}
        type="file"
        accept="image/*"
        style="display: none;"
        on:change={handleFileSelect}
      />
      
      {#if state.uploading}
        <div class="upload-progress">
          <div class="spinner-small"></div>
          <span>Subiendo...</span>
        </div>
      {/if}
    </div>
  {/if}

  {#if state.error}
    <div class="error-message">
      {state.error}
    </div>
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
          <span>No hay fotos. ¡Sube la primera!</span>
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
            <button
              class="delete-photo"
              on:click={() => deletePhoto(photo.id)}
              title="Eliminar foto"
            >
              ×
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

<!-- Lightbox -->
{#if state.showLightbox && state.lightboxImage}
  <div class="lightbox-overlay" on:click={handleLightboxClick} role="dialog" aria-modal="true">
    <div class="lightbox-content">
      <button class="lightbox-close" on:click={closeLightbox}>×</button>
      <img src="{getUploadsBase()}/{state.lightboxImage}" alt="Vista ampliada" />
    </div>
  </div>
{/if}

<style>
  .photo-gallery {
    margin-top: 16px;
  }

  .upload-section {
    margin-bottom: 16px;
    padding: 12px;
    background: #F9FAFB;
    border: 1px dashed #D1D5DB;
    border-radius: 8px;
  }

  .upload-buttons {
    display: flex;
    gap: 8px;
    margin-bottom: 8px;
  }

  .btn-upload {
    background: #3B82F6;
    color: white;
    border: none;
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 12px;
    cursor: pointer;
    transition: all 0.2s ease;
  }

  .btn-upload:hover:not(:disabled) {
    background: #2563EB;
  }

  .btn-upload:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  .upload-progress {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 12px;
    color: #6B7280;
  }

  .spinner-small {
    width: 16px;
    height: 16px;
    border: 2px solid #E5E7EB;
    border-top: 2px solid #3B82F6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  .error-message {
    background: #FEE2E2;
    color: #DC2626;
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 12px;
    margin-bottom: 12px;
  }

  .photos-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
    gap: 12px;
  }

  .loading-photos, .no-photos {
    grid-column: 1 / -1;
    text-align: center;
    padding: 20px;
    color: #6B7280;
    font-size: 14px;
  }

  .loading-photos {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
  }

  .photo-item {
    position: relative;
    background: white;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    transition: transform 0.2s ease;
  }

  .photo-item:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  }

  .photo-item img {
    width: 100%;
    height: 80px;
    object-fit: cover;
    cursor: pointer;
    display: block;
  }

  .delete-photo {
    position: absolute;
    top: 4px;
    right: 4px;
    background: rgba(239, 68, 68, 0.9);
    color: white;
    border: none;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    font-size: 14px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: opacity 0.2s ease;
  }

  .photo-item:hover .delete-photo {
    opacity: 1;
  }

  .delete-photo:hover {
    background: rgba(220, 38, 38, 0.9);
  }

  .photo-info {
    padding: 6px 8px;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .photo-name {
    font-size: 10px;
    color: #374151;
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .photo-size {
    font-size: 9px;
    color: #6B7280;
  }

  /* Lightbox */
  .lightbox-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.9);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2000;
    padding: 20px;
  }

  .lightbox-content {
    position: relative;
    max-width: 90vw;
    max-height: 90vh;
  }

  .lightbox-content img {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    border-radius: 8px;
  }

  .lightbox-close {
    position: absolute;
    top: -40px;
    right: 0;
    background: rgba(255, 255, 255, 0.9);
    border: none;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    font-size: 20px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #374151;
  }

  .lightbox-close:hover {
    background: white;
  }

  @media (max-width: 640px) {
    .photos-grid {
      grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
      gap: 8px;
    }

    .photo-item img {
      height: 60px;
    }

    .upload-buttons {
      flex-direction: column;
    }
  }
</style>