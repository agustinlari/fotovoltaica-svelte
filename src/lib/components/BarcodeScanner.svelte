<script lang="ts">
  import { createEventDispatcher, onDestroy, onMount } from 'svelte';

  export let isOpen = false;

  const dispatch = createEventDispatcher<{ scan: string; close: void }>();

  let scannerContainer: HTMLDivElement;
  let scanning = false;
  let error = '';
  let Quagga: any = null;

  // Cargar QuaggaJS dinámicamente
  onMount(async () => {
    try {
      const quaggaModule = await import('@ericblade/quagga2');
      Quagga = quaggaModule.default;
      console.log('QuaggaJS cargado correctamente');
    } catch (err) {
      console.error('Error cargando QuaggaJS:', err);
      error = 'Error cargando el escáner. Usa entrada manual.';
    }
  });

  // Función para iniciar el escáner con QuaggaJS
  async function startCamera() {
    if (!Quagga || !scannerContainer) {
      error = 'Escáner no disponible. Usa entrada manual.';
      return;
    }

    try {
      error = '';
      scanning = true;

      await Quagga.init({
        inputStream: {
          name: "Live",
          type: "LiveStream",
          target: scannerContainer,
          constraints: {
            width: 640,
            height: 480,
            facingMode: "environment" // Cámara trasera
          }
        },
        decoder: {
          readers: [
            "code_128_reader",
            "ean_reader",
            "ean_8_reader", 
            "code_39_reader",
            "code_39_vin_reader",
            "codabar_reader",
            "upc_reader",
            "upc_e_reader",
            "i2of5_reader"
          ]
        },
        locate: true,
        locator: {
          patchSize: "medium",
          halfSample: true
        },
        numOfWorkers: 2,
        frequency: 10
      });

      Quagga.start();
      
      // Configurar el evento de detección
      Quagga.onDetected((data: any) => {
        const code = data.codeResult.code;
        console.log('Código detectado:', code);
        dispatch('scan', code);
        close();
      });

    } catch (err) {
      console.error('Error iniciando el escáner:', err);
      error = 'No se pudo acceder a la cámara. Usa entrada manual.';
      scanning = false;
    }
  }

  // Función para detener el escáner
  function stopCamera() {
    if (Quagga && scanning) {
      try {
        Quagga.stop();
        Quagga.offDetected();
      } catch (err) {
        console.error('Error deteniendo el escáner:', err);
      }
    }
    scanning = false;
  }

  // Función para cerrar el escáner
  function close() {
    stopCamera();
    dispatch('close');
  }

  // Función para entrada manual de código
  function manualInput() {
    const code = prompt('Introduce el código de barras manualmente:');
    if (code && code.trim()) {
      dispatch('scan', code.trim());
      close();
    }
  }

  // Iniciar la cámara cuando se abre
  $: if (isOpen && scannerContainer) {
    startCamera();
  }

  // Limpiar al cerrar el componente
  onDestroy(() => {
    stopCamera();
  });
</script>

{#if isOpen}
  <div class="scanner-overlay">
    <div class="scanner-container">
      <div class="scanner-header">
        <h3>Escanear Código de Barras</h3>
        <button class="close-btn" on:click={close}>×</button>
      </div>
      
      <div class="scanner-content">
        <div class="camera-container">
          <div bind:this={scannerContainer} class="scanner-viewport"></div>
          
          <!-- Overlay de guía para el código de barras -->
          <div class="scan-guide">
            <div class="scan-frame"></div>
            <p>Centra el código de barras en el marco</p>
          </div>
        </div>
        
        {#if error}
          <div class="error-message">
            {error}
          </div>
        {/if}
        
        <div class="scanner-controls">
          <button class="btn-manual" on:click={manualInput}>
            ⌨️ Código Manual
          </button>
          <button class="btn-cancel" on:click={close}>
            Cancelar
          </button>
        </div>
      </div>
    </div>
  </div>
{/if}

<style>
  .scanner-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.9);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 3000;
    padding: 20px;
  }

  .scanner-container {
    background: white;
    border-radius: 12px;
    width: 100%;
    max-width: 500px;
    max-height: 90vh;
    overflow: hidden;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
  }

  .scanner-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 20px;
    border-bottom: 1px solid #E5E7EB;
    background: #F9FAFB;
  }

  .scanner-header h3 {
    margin: 0;
    font-size: 18px;
    font-weight: 600;
    color: #1F2937;
  }

  .close-btn {
    background: none;
    border: none;
    font-size: 24px;
    cursor: pointer;
    color: #6B7280;
    padding: 4px;
    border-radius: 4px;
  }

  .close-btn:hover {
    background: #E5E7EB;
  }

  .scanner-content {
    padding: 20px;
  }

  .camera-container {
    position: relative;
    border-radius: 8px;
    overflow: hidden;
    background: #000;
    aspect-ratio: 4/3;
    margin-bottom: 20px;
  }

  .scanner-viewport {
    width: 100%;
    height: 100%;
  }

  .scanner-viewport :global(video) {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .scanner-viewport :global(canvas) {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
  }

  .scan-guide {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    pointer-events: none;
  }

  .scan-frame {
    width: 80%;
    height: 100px;
    border: 2px solid #10B981;
    border-radius: 8px;
    background: rgba(16, 185, 129, 0.1);
    position: relative;
    margin-bottom: 20px;
  }

  .scan-frame::before {
    content: '';
    position: absolute;
    top: 50%;
    left: 0;
    right: 0;
    height: 2px;
    background: #10B981;
    transform: translateY(-50%);
    animation: scan-line 2s ease-in-out infinite;
  }

  @keyframes scan-line {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.3; }
  }

  .scan-guide p {
    color: white;
    background: rgba(0, 0, 0, 0.7);
    padding: 8px 16px;
    border-radius: 20px;
    font-size: 14px;
    margin: 0;
  }

  .error-message {
    background: #FEE2E2;
    color: #DC2626;
    padding: 12px;
    border-radius: 6px;
    font-size: 14px;
    margin-bottom: 20px;
    text-align: center;
  }

  .scanner-controls {
    display: flex;
    gap: 12px;
    justify-content: center;
  }

  .btn-manual, .btn-cancel {
    padding: 12px 24px;
    border: none;
    border-radius: 6px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
  }

  .btn-manual {
    background: #10B981;
    color: white;
  }

  .btn-manual:hover {
    background: #059669;
  }

  .btn-cancel {
    background: #F3F4F6;
    color: #374151;
    border: 1px solid #D1D5DB;
  }

  .btn-cancel:hover {
    background: #E5E7EB;
  }

  @media (max-width: 640px) {
    .scanner-overlay {
      padding: 10px;
    }
    
    .scanner-container {
      max-height: 95vh;
    }
    
    .scanner-header {
      padding: 15px;
    }
    
    .scanner-content {
      padding: 15px;
    }
    
    .scanner-controls {
      flex-direction: column;
    }
    
    .btn-manual, .btn-cancel {
      width: 100%;
    }
  }
</style>