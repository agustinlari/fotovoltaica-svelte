using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using UnityEngine.UI;
using TMPro;
using System;
using static UnityEngine.Rendering.DebugUI;
using System.Linq;
using System.Collections;
using System.IO;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System.Drawing;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using NUnit.Framework;
using System.Globalization;
using System.Text.RegularExpressions;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;
using UnityEngine.EventSystems;
using Supabase.Interfaces;
using UnityEngine.Networking;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Runtime.InteropServices.ComTypes;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using System.IO;
using System.ComponentModel;
using SixLabors.ImageSharp;

public class ComunicacionDB : MonoBehaviour
{
    
    //Varios
    public Canvas Galeria;
    public UnityEngine.UI.Image[] imageContainers;
    public GameObject loadingPrefab;
    private GameObject currentLoadingInstance;
    private string FichaPrevia = "";
    public TMP_Text t1;
    public TMP_Text t2;
    public TMP_Text t3;
    public TMP_Text t4;
    public RectTransform contentTransform;

    public TMP_Text tInformacion;
    public GameObject barras;
    public GameObject botonAceptar;
    public GameObject botonCancelar;

    public TMP_Text tInformacionPallets;
    public GameObject barrasPallet;
    public GameObject botonAceptarPallet;
    public GameObject botonCancelarPallet;

    //Lista selección
    
    public TMP_Text tituloListaSeleccion;
    public GameObject itemLista;
    public Transform botonesListaSeleccion;
    private int numeroPaginaLista = 0;
    public TMP_Text tPaginaActual;
    private List<string> filteredOptionsCamiones;
    private List<string> filteredOptionsPallets;
    public GameObject pnlGuardadoCamion;

    //Controles camiones
    private List<Camiones> camiones;
    public TMP_Text camionSeleccionado;
    public TMP_InputField tbMatricula;
    public TMP_InputField tbFechaDescarga;
    public TMP_InputField tbNombreConductor;
    public TMP_InputField tbDNIConductor;
    public TMP_InputField tbContainer;
    public TMP_InputField tbCampa;
    public TMP_InputField tbAlbaran;
    //private System.Data.DataTable datosCamiones;
    public Canvas CanvasNuevoCamion;

    //Controles estructura
    private List<Estructura> estructura;
    private List<string> filteredOptionsEstructura;
    public TMP_Text estructuraSeleccionada;
    public TMP_InputField tbDNIConductorEstructura;
    public TMP_InputField tbNombreConductorEstructura;
    public TMP_InputField tbMatriculaEstructura;
    public TMP_InputField tbProveedorEstructura;
    public TMP_InputField tbFechaDescargaEstructura;
    public TMP_InputField tbPackingListEstructura;
    public TMP_InputField tbAlbaranEstructura;
    public Canvas CanvasNuevaEstructura;
    public GameObject pnlGuardadoEstructura;
    public GameObject botonAceptarEstructura;
    public GameObject botonCancelarEstructura;
    public TMP_Text tInformacionEstructura;
    public GameObject barrasEstructura;
    public TMP_InputField tbInputPackingEstructura;
    public TMP_Text tituloListaSeleccionEstructura;
    public TMP_Text tPaginaActualEstructura;
    //Controles Pallets
    public TMP_InputField tbFiltro;
    public Toggle checkDefecto;
    public GameObject pnlGuardadoPallet;
    private List<Pallets> pallets;
    public TMP_InputField tbSelDescargaPallet;
    public Transform botonesListaSeleccionEstructura;
    public Supabase.Client supabase;
    public System.Data.DataTable datosPallets;
    private bool inhibeEvento;

    List<string> resultadosCompletosPallets;
    public TMP_Text escaneando;
    public RawImage rawImage;
    private WebCamTexture camTexture;
    private bool scanning = false;
    private float tiempoInicio = 0.0f;
    private Texture2D frameTexture;
    private BarcodeReader barcodeReader;
    public Canvas CanvasNuevoPallet;
    public TMP_Text palletSeleccionado;
    private GameObject botonPulsado;

    //Canvas Informes
    public TMP_InputField tbFechaInformePallets;
    List<Camiones> camionesEncontrados;
    PdfDocument document;
    XGraphics gfx;
    private const string FILENAME = "datos_guardados.txt";
    public GameObject panelInfoInforme;
    //Datos de la hoja de Excel
    string sheetID = "1GYgug_31QHLlQqEHHc8cRY0Yvz3cKYtqlbfJzAhXIyo"; // Reemplázalo
    string apiKey = "AIzaSyCuTKrMsXL_J3zdY4SyGcIN1HHP8yvdkAM"; // Clave de API de Google Cloud
    public TMP_InputField InputLinkInforme;
    string fileUrl;
    void Awake()
    {
        escaneando.text = "...";
        filteredOptionsCamiones = new List<string>();
        filteredOptionsPallets = new List<string>();
        filteredOptionsEstructura = new List<string>();
        Debug.Log("Inicio de programa.");
        inhibeEvento = false;
        ConectaDDBB();
        camiones = new List<Camiones>();
        pallets = new List<Pallets>();
        estructura = new List<Estructura>();

    }
    void Start()
    {
        RequestPermissions();
    }
    public void ShowLoading()
    {
        if (currentLoadingInstance != null) return; // Evitar duplicados

        // Instanciar el prefab como hijo del Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            currentLoadingInstance = Instantiate(loadingPrefab, canvas.transform);
            currentLoadingInstance.transform.SetAsLastSibling(); // Para que aparezca encima de todo
        }
        else
        {
            Debug.LogError("No se encontró un Canvas en la escena.");
        }
    }
    public void HideLoading()
    {
        if (currentLoadingInstance != null)
        {
            Destroy(currentLoadingInstance);
            currentLoadingInstance = null;
        }
    }
    public async Task GetFilteredImages(string searchText)
    {
        try
        {
            // --- Limpiar imágenes existentes primero ---
            foreach (var container in imageContainers)
            {
                container.sprite = null;
                container.color = new UnityEngine.Color(1, 1, 1, 0);
            }

            // Lista para almacenar todos los archivos
            var allFiles = new List<Supabase.Storage.FileObject>();
            var storage = supabase.Storage.From("defectospallets");

            // Paginación: obtener todos los archivos
            int offset = 0;
            const int limit = 100; // Límite por página
            bool hasMoreFiles = true;

            while (hasMoreFiles)
            {
                // Obtener archivos con paginación
                var files = await storage.List("", new Supabase.Storage.SearchOptions
                {
                    Limit = limit,
                    Offset = offset
                });

                if (files != null && files.Count > 0)
                {
                    allFiles.AddRange(files);
                    offset += limit;

                    // Si obtuvimos menos del límite, no hay más archivos
                    hasMoreFiles = files.Count == limit;
                }
                else
                {
                    hasMoreFiles = false;
                }
            }

            Debug.Log($"Total de archivos encontrados: {allFiles.Count}");

            // Filtrar por texto en el nombre (case insensitive)
            var filteredFiles = allFiles
                .Where(f => f.Name.ToLower().Contains(searchText.ToLower()))
                .Take(3) // Tomar solo las 3 primeras
                .ToList();

            Debug.Log($"Archivos filtrados encontrados: {filteredFiles.Count}");

            // Cargar las nuevas imágenes
            for (int i = 0; i < filteredFiles.Count && i < imageContainers.Length; i++)
            {
                var file = filteredFiles[i];
                var publicUrl = storage.GetPublicUrl(file.Name);
                await LoadImageToContainer(publicUrl, imageContainers[i]);
                imageContainers[i].color = UnityEngine.Color.white;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al obtener imágenes: {ex.Message}");
        }
    }
    private async Task LoadImageToContainer(string imageUrl, UnityEngine.UI.Image container)
    {
        using (var request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error al cargar imagen: {request.error}");
                return;
            }

            var texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
            container.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero
            );
        }
    }
    public async void TakePhotoWithLoader(String tipoFoto)
    {
        ShowLoading();

        NativeCamera.TakePicture(async (path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("No se pudo tomar la foto.");
                HideLoading();
                return;
            }
            string uniqueFileName = "";
            if (tipoFoto == "Pallet") uniqueFileName = $"{palletSeleccionado.text.Trim(' ')}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            if (tipoFoto == "Camion") uniqueFileName = $"Descarga_{camionSeleccionado.text.Trim(' ')}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            if (tipoFoto == "Estructura") uniqueFileName = $"Estructura_{estructuraSeleccionada.text.Trim(' ')}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string mobilePath = Path.Combine(Application.persistentDataPath, uniqueFileName);

            try
            {
                File.Copy(path, mobilePath, overwrite: true);

                // Elimina la declaración previa de uploadTask y usa await directamente
                await supabase.Storage
                    .From("defectospallets")
                    .Upload(mobilePath, uniqueFileName, null);

                NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(
                    path,
                    "MiApp",
                    uniqueFileName
                );
                InformaMensaje(pnlGuardadoPallet, "FOTO SUBIDA CON ÉXITO", "acierto");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al procesar la foto: " + ex.Message);
            }
            finally
            {
                HideLoading();
            }
        }, 1024);
    }
    void RequestPermissions()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            // Permiso para la cámara
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.CAMERA"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.CAMERA");
            }

            // Permiso para leer imágenes (Android 13+)
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_IMAGES"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_MEDIA_IMAGES");
            }

            // Permiso para guardar imágenes en la galería (Android 14+)
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_VISUAL_USER_SELECTED"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_MEDIA_VISUAL_USER_SELECTED");
            }
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.READ_EXTERNAL_STORAGE"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.READ_EXTERNAL_STORAGE");
            }
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.WRITE_EXTERNAL_STORAGE");
            }
        }
    }
    public async void CargaGaleria()
    {
        ShowLoading();
        if(FichaPrevia=="Pallet") await GetFilteredImages(palletSeleccionado.text.Trim(' '));
        if (FichaPrevia == "Camion") await GetFilteredImages("Descarga_" + camionSeleccionado.text.Trim(' '));
        if (FichaPrevia == "Estructura") await GetFilteredImages("Estructura_" + estructuraSeleccionada.text.Trim(' '));
        AbrirNuevoCanvas(Galeria);
        HideLoading();
    }
    private void OnImageClicked(string fileName)
    {
        Debug.Log("Imagen seleccionada: " + fileName);
        // Aquí puedes abrir la imagen en grande o eliminarla
    }
    private async Task<bool> ActualizaCamiones()//Rellena filteredoptions con los camiones
    {
        try
        {
            // 1. Obtener datos directamente como List<Camiones>
            camiones = await LeeDatosPaginadosDDBB<Camiones>();

            // 2. Limpiar y procesar la lista directamente
            filteredOptionsCamiones.Clear();

            // 3. Extraer, ordenar y convertir IDs - todo en una operación LINQ
            var idsOrdenados = camiones
                .Where(c => c.id > 0) // Filtro opcional para IDs válidos
                .Select(c => c.id)
                .OrderBy(id => id)
                .ToList();

            // 4. Convertir a strings y asignar
            filteredOptionsCamiones.AddRange(idsOrdenados.Select(id => id.ToString()));
            tituloListaSeleccion.text = "CAMIONES";

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en ActualizaCamiones: {ex.Message}");
            return false;
        }
    }
    private async Task<bool> ActualizaEstructura()//Rellena filteredoptions con los camiones
    {
        try
        {
            // 1. Obtener datos directamente como List<Camiones>
            estructura = await LeeDatosPaginadosDDBB<Estructura>();

            // 2. Limpiar y procesar la lista directamente
            filteredOptionsEstructura.Clear();

            // 3. Extraer, ordenar y convertir IDs - todo en una operación LINQ
            var idsOrdenados = estructura
                .Where(c => c.id > 0) // Filtro opcional para IDs válidos
                .Select(c => c.id)
                .OrderBy(id => id)
                .ToList();

            // 4. Convertir a strings y asignar
            filteredOptionsEstructura.AddRange(idsOrdenados.Select(id => id.ToString()));
            tituloListaSeleccionEstructura.text = "ESTRUCTURA";

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en ActualizaEstructura: {ex.Message}");
            return false;
        }
    }
    private async Task<bool> ActualizaPallets()
    {
        try
        {
            // 1. Obtener datos directamente como List<Pallets>
            pallets = await LeeDatosPaginadosDDBB<Pallets>();

            // 2. Limpiar y procesar la lista directamente
            filteredOptionsPallets.Clear();

            // 3. Extraer, ordenar y convertir IDs - todo en una operación LINQ
            var idsOrdenados = pallets
                .Select(c => c.id)
                .OrderBy(id => id)
                .ToList();

            // 4. Convertir a strings y asignar
            filteredOptionsPallets.AddRange(idsOrdenados.Select(id => id.ToString()));
            tituloListaSeleccion.text = "PALLETS";

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en ActualizaPallets: {ex.Message}");
            return false;
        }
    }
    public async void CargaInicialCamiones()
    {
        ShowLoading();
        FichaPrevia = "Camiones";
        bool respuesta = await ActualizaCamiones();

        // Validación con camiones en lugar de datosCamiones
        if (!respuesta || filteredOptionsCamiones.Count == 0 || camiones == null || !camiones.Any())
        {
            Debug.LogWarning("No hay datos de camiones disponibles.");
            return;
        }
        try
        {
            // Buscar el camión directamente en la lista
            Camiones camionEncontrado = camiones
                .FirstOrDefault(c => c.id.ToString() == filteredOptionsCamiones[0]);

            if (camionEncontrado != null)
            {
                string nombrecompleto = $"{filteredOptionsCamiones[0]} ({camionEncontrado.Matricula})";
                SelectItem(nombrecompleto, "CAMIONES");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en CargaInicialCamiones: {ex.Message}");
        }
        HideLoading();
    }
    public async void CargaInicialPallets()
    {
        ShowLoading();
        FichaPrevia = "Pallets";
        bool respuesta2 = await ActualizaCamiones();
        bool respuesta = await ActualizaPallets();
        // Validación con camiones en lugar de datosCamiones
        if (!respuesta || filteredOptionsPallets.Count == 0 || pallets == null || !pallets.Any())
        {
            Debug.LogWarning("No hay datos de pallets disponibles.");
            return;
        }
        try
        {
            // Buscar el camión directamente en la lista
            Pallets palletEncontrado = pallets
                .FirstOrDefault(c => c.id.ToString() == filteredOptionsPallets[0]);

            if (palletEncontrado != null)
            {
                string nombrecompleto = $"{filteredOptionsPallets[0]} ({palletEncontrado.Descarga})";
                SelectItem(nombrecompleto, "PALLETS");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en CargaInicialCamiones: {ex.Message}");
        }
        HideLoading();
    }
    public async void CargaInicialEstructura()
    {
        ShowLoading();
        FichaPrevia = "Estructura";
        bool respuesta = await ActualizaEstructura();

        // Validación con camiones en lugar de datosCamiones
        if (!respuesta || filteredOptionsEstructura.Count == 0 || estructura == null || !estructura.Any())
        {
            Debug.LogWarning("No hay datos de estructura disponibles.");
            return;
        }
        try
        {
            // Buscar el camión directamente en la lista
            Estructura estructuraEncontrada = estructura
                .FirstOrDefault(c => c.id.ToString() == filteredOptionsEstructura[0]);

            if (estructuraEncontrada != null)
            {
                string nombrecompleto = $"{filteredOptionsEstructura[0]} ({estructuraEncontrada.Matricula})";
                SelectItemEstructura(nombrecompleto);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en CargaInicialEstructura: {ex.Message}");
        }
        HideLoading();
    }
    public void StartCamera()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            camTexture = new WebCamTexture(WebCamTexture.devices[0].name, 1280, 720);
            rawImage.texture = camTexture;
            rawImage.material.mainTexture = camTexture;
            camTexture.Play();
            scanning = true;

            // Ajustar la relación de aspecto
            AdjustCameraTexture();

            // Crear la textura para procesar los píxeles
            frameTexture = new Texture2D(camTexture.width, camTexture.height, TextureFormat.RGBA32, false);
            escaneando.text = "INICIANDO...";
        }
        else
        {
            Debug.LogError("No se encontró ninguna cámara.");
        }
    }
    void Update()
    {
        if (scanning)
        {
            float tiempoPasado = Time.time - tiempoInicio;

            if (tiempoPasado > 0.033f) // ~30 FPS
            {
                tiempoInicio = Time.time;

                // Obtener píxeles de la cámara
                Color32[] pixels = camTexture.GetPixels32();
                int width = camTexture.width;
                int height = camTexture.height;

                // Ajuste de la relación de aspecto al recortar la imagen si es necesario
                int cropWidth = width;
                int cropHeight = Mathf.RoundToInt(width / (float)rawImage.rectTransform.rect.width * rawImage.rectTransform.rect.height);
                if (cropHeight > height)
                {
                    cropHeight = height;
                    cropWidth = Mathf.RoundToInt(height / (float)rawImage.rectTransform.rect.height * rawImage.rectTransform.rect.width);
                }

                // Extraer solo la parte central de la imagen
                var croppedPixels = CropPixels(pixels, width, height, cropWidth, cropHeight);

                // Crear fuente de datos para ZXing
                //var source = new Color32LuminanceSource(croppedPixels, cropWidth, cropHeight);

                var barcodeReader = new BarcodeReader { AutoRotate = true, Options = new ZXing.Common.DecodingOptions { TryHarder = false } };

                try
                {
                    // Intentar decodificar el código de barras
                    //var result = barcodeReader.Decode(source);
                    var result = barcodeReader.Decode(croppedPixels, cropWidth, cropHeight);
                    if (result != null)
                    {
                        StopCamera();
                        scanning = false;
                        if (CheckeaCodigo(result.Text))
                        {
                            palletSeleccionado.text = result.Text;
                            escaneando.text = "ENCONTRADO";
                        }
                        else
                        {
                            InformaMensaje(pnlGuardadoPallet, "CODIGO INVALIDO", "error");
                        }
                        //cbPalletCambia();
                        
                    }
                    else
                    {
                        if (escaneando.text == "INICIANDO...")
                        {
                            escaneando.text = "ESCANEANDO";
                            return;
                        }
                        if (escaneando.text == "ESCANEANDO")
                        {
                            escaneando.text = "ESCANEANDO...";
                            return;
                        }
                        if (escaneando.text == "ESCANEANDO...")
                        {
                            escaneando.text = "ESCANEANDO";
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error al decodificar el código de barras: " + ex.Message);
                }
            }
        }
    }
    private bool CheckeaCodigo(string codigo)
    {
        return pallets != null && pallets.Any(p => p.id == codigo);
    }
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }
    public void StopCamera()
    {

        scanning = false;
        AbrirNuevoCanvas(CanvasNuevoPallet);
        rawImage.texture = null;
        if (camTexture != null)
        {
            camTexture.Stop();
        }
    }
    void AdjustCameraTexture()
    {
        // Ajustar la relación de aspecto de la cámara para que coincida con la de la RawImage
        float cameraRatio = (float)camTexture.width / camTexture.height;
        float rawImageRatio = (float)rawImage.rectTransform.rect.width / rawImage.rectTransform.rect.height;

        if (cameraRatio > rawImageRatio)
        {
            rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.rect.height * cameraRatio, rawImage.rectTransform.rect.height);
        }
        else
        {
            rawImage.rectTransform.sizeDelta = new Vector2(rawImage.rectTransform.rect.width, rawImage.rectTransform.rect.width / cameraRatio);
        }
    }
    Color32[] CropPixels(Color32[] pixels, int width, int height, int cropWidth, int cropHeight)
    {
        int xStart = (width - cropWidth) / 2;
        int yStart = (height - cropHeight) / 2;

        Color32[] croppedPixels = new Color32[cropWidth * cropHeight];

        for (int y = 0; y < cropHeight; y++)
        {
            for (int x = 0; x < cropWidth; x++)
            {
                int pixelIndex = (yStart + y) * width + (xStart + x);
                int croppedIndex = y * cropWidth + x;
                croppedPixels[croppedIndex] = pixels[pixelIndex];
            }
        }
        return croppedPixels;
    }
    private async void ConectaDDBB()
    {
        var url = @"https://zstbxqkiudhmhwkuwtye.supabase.co";
        var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InpzdGJ4cWtpdWRobWh3a3V3dHllIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mzg3Mzc4MTksImV4cCI6MjA1NDMxMzgxOX0.NtFLYFuTPv_0T7rDnrHM51SWlRvUXCl-awHWZuMjHSg";
        var options = new Supabase.SupabaseOptions { AutoConnectRealtime = true };
        supabase = new Supabase.Client(url, key, options);
        await supabase.InitializeAsync();
    }
    private async Task<List<T>> LeeDatosPaginadosDDBB<T>() where T : Supabase.Postgrest.Models.BaseModel, new()
    {
        List<T> allData = new List<T>();
        int batchSize = 1000; // Número máximo de registros por consulta
        int start = 0;

        while (true)
        {
            var response = await supabase.From<T>()
                .Select("*")
                .Range(start, start + batchSize - 1) // Paginación
                .Get();

            var data = response.Models;

            if (data.Count() == 0)
                break; // Si no hay más registros, termina el bucle

            allData.AddRange(data);
            start += batchSize; // Avanza a la siguiente página
        }

        return allData;
    }
    public async void GeneraLista(string opcion) //Carga los botones en la lista de selección de pallets, camiones, etc.
    {
        // Limpiar elementos anteriores
        foreach (Transform child in botonesListaSeleccion)
        {
            Destroy(child.gameObject);
        }

        // Calcular rango de elementos a mostrar (paginación)
        int startIndex = numeroPaginaLista * 15;
        int endIndex = 0;
        if (opcion == "CAMIONES")
        {
            tituloListaSeleccion.text = "CAMIONES";
            endIndex = Mathf.Min(startIndex + 15, filteredOptionsCamiones.Count);
        }
        if (opcion == "DESCARGAS")
        {
            tituloListaSeleccion.text = "DESCARGAS";
            endIndex = Mathf.Min(startIndex + 15, filteredOptionsCamiones.Count);
        }
        if (opcion == "PALLETS")
        {
            endIndex = Mathf.Min(startIndex + 15, filteredOptionsPallets.Count);
        }
        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject newItem = Instantiate(itemLista, botonesListaSeleccion);
            if (opcion == "CAMIONES" || opcion == "DESCARGAS")
            {
                Camiones camionEncontrado = camiones.FirstOrDefault(c => c.id.ToString() == filteredOptionsCamiones[i]);
                if (camionEncontrado != null)
                {
                    // Asumo que [4] era el campo "matrícula" o similar - ajusta según tu modelo
                    string nombrecompleto = $"{filteredOptionsCamiones[i]} ({camionEncontrado.Matricula})";
                    TMP_Text textComponent = newItem.GetComponentInChildren<TMP_Text>();
                    textComponent.text = nombrecompleto;

                    // Configurar el botón
                    newItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => SelectItem(nombrecompleto, opcion));

                }
                tPaginaActual.text = $"{numeroPaginaLista + 1} de {1 + Mathf.CeilToInt(filteredOptionsCamiones.Count / 15f)}";
            }
            if (opcion == "PALLETS")
            {
                // Buscar el camión en la lista usando el ID de filteredOptions
                Pallets palletEncontrado = pallets.FirstOrDefault(c => c.id == filteredOptionsPallets[i]);
                tituloListaSeleccion.text = "PALLETS";
                if (palletEncontrado != null)
                {
                    // Numero de descarga
                    string nombrecompleto = $"{filteredOptionsPallets[i]} ({palletEncontrado.Descarga})";
                    TMP_Text textComponent = newItem.GetComponentInChildren<TMP_Text>();
                    textComponent.text = nombrecompleto;

                    // Configurar el botón
                    newItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => SelectItem(nombrecompleto, "PALLETS"));
                }
                tPaginaActual.text = $"{numeroPaginaLista + 1} de {1 + Mathf.CeilToInt(filteredOptionsPallets.Count / 15f)}";
            }
        }
    }
    public async void GeneraListaEstructura() //Carga los botones en la lista de selección de pallets, camiones, etc.
    {
        // Limpiar elementos anteriores
        foreach (Transform child in botonesListaSeleccionEstructura)
        {
            Destroy(child.gameObject);
        }
        int startIndex = numeroPaginaLista * 15;
        int endIndex = 0;
        endIndex = Mathf.Min(startIndex + 15, filteredOptionsEstructura.Count);
        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject newItem = Instantiate(itemLista, botonesListaSeleccionEstructura);

                Estructura estructuraEncontrada = estructura.FirstOrDefault(c => c.id.ToString() == filteredOptionsEstructura[i]);
                if (estructuraEncontrada != null)
                {
                    // Asumo que [4] era el campo "matrícula" o similar - ajusta según tu modelo
                    string nombrecompleto = $"{filteredOptionsEstructura[i]} ({estructuraEncontrada.Matricula})";
                    TMP_Text textComponent = newItem.GetComponentInChildren<TMP_Text>();
                    textComponent.text = nombrecompleto;

                    // Configurar el botón
                    newItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => SelectItemEstructura(nombrecompleto));
                }
                tPaginaActualEstructura.text = $"{numeroPaginaLista + 1} de {1 + Mathf.CeilToInt(filteredOptionsEstructura.Count / 15f)}";
        }
    }
    void SelectItemEstructura(string itemName)
    {
        FichaPrevia = "Estructura";
        string[] partes = itemName.Split('(');
        string itemLimpio = partes[0];
        estructuraSeleccionada.text = itemLimpio;
        EstructuraCambia();
        AbrirNuevoCanvas(CanvasNuevaEstructura);
        return;
    }
    void SelectItem(string itemName, string tipoLista)
    {
        if (tipoLista == "CAMIONES")
        {
            FichaPrevia = "Camion";
            string[] partes = itemName.Split('(');
            string itemLimpio = partes[0];
            camionSeleccionado.text = itemLimpio;
            CamionCambia();
            AbrirNuevoCanvas(CanvasNuevoCamion);
            return;
        }
        if (tipoLista == "PALLETS")
        {
            FichaPrevia = "Pallet";
            palletSeleccionado.text = itemName;
            string[] partes = itemName.Split('(');
            string itemLimpio = partes[0];
            palletSeleccionado.text = itemLimpio;
            PalletCambia();
            AbrirNuevoCanvas(CanvasNuevoPallet);
            return;
        }
        if (tipoLista == "DESCARGAS")
        {
            FichaPrevia = "Descarga";
            string[] partes = itemName.Split('(');
            string itemLimpio = partes[0];
            tbSelDescargaPallet.text = itemLimpio;
            AbrirNuevoCanvas(CanvasNuevoPallet);
            return;
        }
    }
    public void PaginaSiguiente()
    {
        if (tituloListaSeleccion.text == "CAMIONES" || tituloListaSeleccion.text == "DESCARGAS")
        {
            if (numeroPaginaLista * 15 >= filteredOptionsCamiones.Count - 15) return;
            numeroPaginaLista++;
            GeneraLista(tituloListaSeleccion.text);
        }
    }
    public void PaginaSiguienteEstructura()
    {
            if (numeroPaginaLista * 15 >= filteredOptionsEstructura.Count - 15) return;
            numeroPaginaLista++;
            GeneraListaEstructura();
    }
    public void PaginaAnteriorEstructura()
    {
            if (numeroPaginaLista == 0) return;
            numeroPaginaLista--;
            GeneraListaEstructura();
    }
    public void PaginaAnterior()
    {
        if (tituloListaSeleccion.text == "CAMIONES" || tituloListaSeleccion.text == "DESCARGAS")
        {
            if (numeroPaginaLista == 0) return;
            numeroPaginaLista--;
            GeneraLista(tituloListaSeleccion.text);
        }
    }
    public async void CamionCambia()
    {
        tbMatricula.text = "";
        tbFechaDescarga.text = "";
        tbNombreConductor.text = "";
        tbDNIConductor.text = "";
        tbContainer.text = "";
        tbCampa.text = "";
        tbAlbaran.text = "";
        try
        {
            int clave = Convert.ToInt32(camionSeleccionado.text);
            var result = await supabase.From<Camiones>()
              .Where(x => x.id == clave)
              .Get();
            if (result.Content.Length > 0)
            {
                DateTime fechatest = DateTime.Now;
                try
                {
                    DateTimeOffset fecha = DateTimeOffset.ParseExact(result.Model.FechaDescarga, "yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
                    tbFechaDescarga.text = fecha.ToString("dd/MM/yyyy");
                } catch (Exception ex) { }
                try { tbMatricula.text = result.Model.Matricula.ToString(); } catch (Exception ex) { }
                try { tbNombreConductor.text = result.Model.NombreConductor.ToString(); } catch (Exception ex) { }
                try { tbDNIConductor.text = result.Model.DNI.ToString(); } catch (Exception ex) { }
                try { tbContainer.text = result.Model.Container.ToString(); } catch (Exception ex) { }
                try { tbCampa.text = result.Model.UbicacionCampa.ToString(); } catch (Exception ex) { }
                try { tbAlbaran.text = result.Model.Albaran.ToString(); } catch (Exception ex) { }
            }
        }
        catch (Exception ex)
        {
            string fallo = ex.Message;
            return;
        }
    }
    public async void EstructuraCambia()
    {
        tbAlbaranEstructura.text = "";
        tbMatriculaEstructura.text = "";
        tbNombreConductorEstructura.text = "";
        tbDNIConductorEstructura.text = "";
        tbFechaDescargaEstructura.text = "";
        tbPackingListEstructura.text = "";
        tbProveedorEstructura.text = "";
        try
        {
            int clave = Convert.ToInt32(estructuraSeleccionada.text);
            var result = await supabase.From<Estructura>()
              .Where(x => x.id == clave)
              .Get();
            if (result.Content.Length > 0)
            {
                DateTime fechatest = DateTime.Now;
                try
                {
                    DateTimeOffset fecha = DateTimeOffset.ParseExact(result.Model.FechaDescarga, "yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
                    tbFechaDescargaEstructura.text = fecha.ToString("dd/MM/yyyy");
                }
                catch (Exception ex) { }
                try { tbMatriculaEstructura.text = result.Model.Matricula.ToString(); } catch (Exception ex) { }
                try { tbNombreConductorEstructura.text = result.Model.Conductor.ToString(); } catch (Exception ex) { }
                try { tbDNIConductorEstructura.text = result.Model.DNI.ToString(); } catch (Exception ex) { }
                try { tbPackingListEstructura.text = result.Model.PackingList.ToString(); } catch (Exception ex) { }
                try { tbProveedorEstructura.text = result.Model.Proveedor.ToString(); } catch (Exception ex) { }
                try { tbAlbaranEstructura.text = result.Model.Albaran.ToString(); } catch (Exception ex) { }
            }
        }
        catch (Exception ex)
        {
            string fallo = ex.Message;
            return;
        }
    }
    public async void PalletCambia()
    {
        tbSelDescargaPallet.text = "";
        checkDefecto.isOn = false;

        try
        {
            string clave = palletSeleccionado.text.Trim(' ');
            var result = await supabase.From<Pallets>()
              .Where(x => x.id == clave)
              .Get();
            if (result.Content.Length == 2)
            {
                InformaMensaje(pnlGuardadoPallet, "ERROR! PALLET DESCONOCIDO", "error");
                return;
            }
            try { tbSelDescargaPallet.text = result.Model.Descarga.ToString(); } catch (Exception ex) { }
            try { checkDefecto.isOn = result.Model.Defecto; } catch (Exception ex) { }
        }
        catch (Exception ex)
        {
            string fallo = ex.Message;
            return;
        }
    }
    public async void SalvarDatosEstructura()
    {
        UnityEngine.UI.Image panelimage = pnlGuardadoEstructura.GetComponent<UnityEngine.UI.Image>();
        panelimage.color = new UnityEngine.Color(50f / 255f, 150f / 255f, 200f / 255f, 1f);
        int clave = Convert.ToInt32(estructuraSeleccionada.text);
        botonAceptarEstructura.SetActive(false);
        pnlGuardadoEstructura.SetActive(true);
        tInformacionEstructura.text = "";
        barrasEstructura.SetActive(true);
        //Checkeamos si tiene datos
        var result = await supabase.From<Estructura>()
        .Where(x => x.id == clave)
        .Get();
        barrasEstructura.SetActive(false);

        if (!DateTime.TryParseExact(tbFechaDescargaEstructura.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
        {
            panelimage.color = new UnityEngine.Color(255f / 255f, 100f / 255f, 50f / 255f, 1f);
            tInformacionEstructura.text = "La fecha seleccionada no es valida";
            botonAceptarEstructura.SetActive(false);
            return;
        }
        if (result.Content.Length > 0)
        {
            var updatedDescarga = result.Model;
            bool matriculavacia = string.IsNullOrEmpty(updatedDescarga.Matricula);
            if (matriculavacia)
            {
                tInformacionEstructura.text = "Se procedera a guardar la descarga de estructura " + clave + ". Desea continuar?";
                botonAceptarEstructura.SetActive(true);
                
            }
            else
            {
                panelimage.color = new UnityEngine.Color(250f/255f, 220f/255f, 50f/255f, 1f);
                tInformacionEstructura.text = "Atencion. La descarga " + clave + " ya tiene una matricula asignada. Deseas sobreescribirla con los nuevos datos?";
                botonAceptarEstructura.SetActive(true);
            }
        }
        else
        {
            panelimage.color = new UnityEngine.Color(255f / 255f, 100f / 255f, 50f / 255f, 1f);
            tInformacionEstructura.text = "El numero de descarga no es valido.";
            botonAceptarEstructura.SetActive(false);
        }
    }
    public async void SalvarDatosCamion()
    {
        UnityEngine.UI.Image panelimage = pnlGuardadoCamion.GetComponent<UnityEngine.UI.Image>();
        panelimage.color = new UnityEngine.Color(50f / 255f, 150f / 255f, 200f / 255f, 1f);
        int clave = Convert.ToInt32(camionSeleccionado.text);
        botonAceptar.SetActive(false);
        pnlGuardadoCamion.SetActive(true);
        tInformacion.text = "";
        barras.SetActive(true);
        //Checkeamos si tiene datos
        var result = await supabase.From<Camiones>()
        .Where(x => x.id == clave)
        .Get();
        barras.SetActive(false);

        if (!DateTime.TryParseExact(tbFechaDescarga.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
        {
            panelimage.color = new UnityEngine.Color(255f / 255f, 100f / 255f, 50f / 255f, 1f);
            tInformacion.text = "La fecha seleccionada no es valida";
            botonAceptar.SetActive(false);
            return;
        }
        if (result.Content.Length > 0)
        {
            var updatedCamion = result.Model;
            bool matriculavacia = string.IsNullOrEmpty(updatedCamion.Matricula);
            if (matriculavacia)
            {
                tInformacion.text = "Se procedera a guardar la descarga " + clave + ". Desea continuar?";
                botonAceptar.SetActive(true);

            }
            else
            {
                panelimage.color = new UnityEngine.Color(250f / 255f, 220f / 255f, 50f / 255f, 1f);
                tInformacion.text = "Atencion. La descarga " + clave + " ya tiene una matricula asignada. Deseas sobreescribirla con los nuevos datos?";
                botonAceptar.SetActive(true);
            }
        }
        else
        {
            panelimage.color = new UnityEngine.Color(255f / 255f, 100f / 255f, 50f / 255f, 1f);
            tInformacion.text = "El numero de descarga no es valido.";
            botonAceptar.SetActive(false);
        }
    }
    public void onClicGuardado()
    {
        _ = IniciaProcesoGuardadoCamion(); // Llamada "fire-and-forget"
    }
    public void onClicGuardadoEstructura()
    {
        _ = IniciaProcesoGuardadoEstructura(); // Llamada "fire-and-forget"
    }
    private async Task IniciaProcesoGuardadoEstructura()
    {
        barrasEstructura.SetActive(true);
        int clave = Convert.ToInt32(estructuraSeleccionada.text);
        DateTime.TryParseExact(tbFechaDescargaEstructura.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha);
        string fechaDescarga = fecha.ToString("MM/dd/yyyy") + " 0:00:00";
        UnityEngine.UI.Image panelimage = pnlGuardadoEstructura.GetComponent<UnityEngine.UI.Image>();

        var updateResponse = await supabase
            .From<Estructura>()
            .Where(x => x.id == clave)
            .Set(x => x.Albaran, tbAlbaranEstructura.text)
            .Set(x => x.Matricula, tbMatriculaEstructura.text)
            .Set(x => x.FechaDescarga, fechaDescarga)
            .Set(x => x.Conductor, tbNombreConductorEstructura.text)
            .Set(x => x.DNI, tbDNIConductorEstructura.text)
            .Set(x => x.PackingList, tbInputPackingEstructura.text)
            .Set(x => x.Proveedor, tbProveedorEstructura.text)
            .Update();

        barrasEstructura.SetActive(false);
        if (!updateResponse.ResponseMessage.IsSuccessStatusCode)
        {
            panelimage.color = new UnityEngine.Color(255f / 255f, 100f / 255f, 50f / 255f, 1f);
            tInformacionEstructura.text = "error. la informacion no se ha guardado. comprueba la conexion y vuelve a intentarlo.";
            return;
        }

        // Mostrar éxito
        panelimage.color = new UnityEngine.Color(36f / 255f, 72f / 255f, 84f / 255f, 1f);
        tInformacionEstructura.text = "los datos se han guardado correctamente";
        bool respuesta = await ActualizaEstructura();
        botonAceptarEstructura.SetActive(false);
    }
    private async Task IniciaProcesoGuardadoCamion()
    {
        barras.SetActive(true);
        int clave = Convert.ToInt32(camionSeleccionado.text);
        DateTime.TryParseExact(tbFechaDescarga.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha);
        string fechaDescarga = fecha.ToString("MM/dd/yyyy") + " 0:00:00";
        UnityEngine.UI.Image panelimage = pnlGuardadoCamion.GetComponent<UnityEngine.UI.Image>();

        var updateResponse = await supabase
            .From<Camiones>()
            .Where(x => x.id == clave)
            .Set(x => x.Albaran, tbAlbaran.text)
            .Set(x => x.Matricula, tbMatricula.text)
            .Set(x => x.FechaDescarga, fechaDescarga)
            .Set(x => x.NombreConductor, tbNombreConductor.text)
            .Set(x => x.DNI, tbDNIConductor.text)
            .Set(x => x.Container, tbContainer.text)
            .Set(x => x.UbicacionCampa, tbCampa.text)
            .Update();

        barras.SetActive(false);
        if (!updateResponse.ResponseMessage.IsSuccessStatusCode)
        {
            panelimage.color = new UnityEngine.Color(255f / 255f, 100f / 255f, 50f / 255f, 1f);
            tInformacion.text = "error. la informacion no se ha guardado. comprueba la conexion y vuelve a intentarlo.";
            return;
        }

        // Mostrar éxito
        panelimage.color = new UnityEngine.Color(36f / 255f, 72f / 255f, 84f / 255f, 1f);
        tInformacion.text = "los datos se han guardado correctamente";
        bool respuesta = await ActualizaCamiones();
        botonAceptar.SetActive(false);
    }
    public void ocultaPanelGuardado()
    {
        pnlGuardadoCamion.SetActive(false);
    }
    public void ocultaPanelGuardadoEstructura()
    {
        pnlGuardadoEstructura.SetActive(false);
    }
    public void ocultaPanelGuardadoPallet()
    {
        pnlGuardadoPallet.SetActive(false);
    }
    private void MostrarMensajePallet(string mensaje, UnityEngine.Color color, bool mostrarBotonAceptar)
    {
        var panelImage = pnlGuardadoPallet.GetComponent<UnityEngine.UI.Image>();
        panelImage.color = color;
        tInformacionPallets.text = mensaje;
        botonAceptarPallet.SetActive(mostrarBotonAceptar);
        pnlGuardadoPallet.SetActive(true);
    }
    public async Task SalvarDatosPallet()
    {
        UnityEngine.Color colorError = new UnityEngine.Color(1f, 100f / 255f, 50f / 255f, 1f);
        UnityEngine.Color colorAdvertencia = new UnityEngine.Color(250f / 255f, 220f / 255f, 50f / 255f, 1f);
        UnityEngine.Color colorExito = new UnityEngine.Color(50f / 255f, 150f / 255f, 200f / 255f, 1f);

        pnlGuardadoPallet.SetActive(true);
        botonAceptarPallet.SetActive(false);
        tInformacionPallets.text = "";

        bool defecto = checkDefecto.isOn;
        string clave = Regex.Replace(palletSeleccionado.text, @"\s+", "");
        string descargaTexto = Regex.Replace(tbSelDescargaPallet.text, @"\s+", "");

        if (!int.TryParse(descargaTexto, out int numero))
        {
            MostrarMensajePallet("Número de descarga no válido.", colorError, false);
            return;
        }

        barrasPallet.SetActive(true);

        var result = await supabase.From<Pallets>()
            .Where(x => x.id == clave)
            .Get();

        barrasPallet.SetActive(false);

        if (result.Content.Length == 0)
        {
            MostrarMensajePallet("El número de pallet no es válido.", colorError, false);
            return;
        }

        var updatedPallet = result.Model;
        bool tieneDescarga = updatedPallet.Descarga.ToString() != "0";

        if (tieneDescarga)
        {
            MostrarMensajePallet($"Atencion. El pallet {clave} ya tiene una descarga asignada. Desea sobreescribir los datos?", colorAdvertencia, true);
        }
        else
        {
            MostrarMensajePallet($"Se procedera a guardar el pallet {clave}. Desea continuar?", colorExito, true);
            botonAceptarPallet.SetActive(true);
        }
    }
    public async Task AceptarGuardadoPallet()
    {
        barrasPallet.SetActive(true);
        UnityEngine.Color colorError = new UnityEngine.Color(1f, 100f / 255f, 50f / 255f, 1f);
        UnityEngine.Color colorAdvertencia = new UnityEngine.Color(250f / 255f, 220f / 255f, 50f / 255f, 1f);
        UnityEngine.Color colorExito = new UnityEngine.Color(36f / 255f, 72f / 255f, 84f / 255f, 1f);

        pnlGuardadoPallet.SetActive(true);
        botonAceptarPallet.SetActive(false);
        tInformacionPallets.text = "";

        bool defecto = checkDefecto.isOn;
        string clave = Regex.Replace(palletSeleccionado.text, @"\s+", "");
        string descargaTexto = Regex.Replace(tbSelDescargaPallet.text, @"\s+", "");
        int.TryParse(descargaTexto, out int numero);
        var update = await supabase
    .From<Pallets>()
    .Where(x => x.id == clave)
    .Set(x => x.Descarga, numero)
    .Set(x => x.Defecto, defecto)
    .Update();

        barrasPallet.SetActive(false);

        if (!update.ResponseMessage.IsSuccessStatusCode)
        {
            MostrarMensajePallet("Error. La información no se ha guardado. Comprueba la conexión e inténtalo de nuevo.", colorError, false);
            return;
        }

        MostrarMensajePallet("Los datos se han guardado correctamente.", colorExito, false);
        botonAceptarPallet.SetActive(false);

        bool respuesta = await ActualizaPallets();
    }
    public void OnClickGuardarPallet()
    {
        _ = SalvarDatosPallet(); // fire-and-forget
    }
    public void OnClickAceptarGuardadoPallet()
    {
        _ = AceptarGuardadoPallet();
    }
    private void InformaMensaje(GameObject panel, string texto, string tipoMensaje) //tipoMensaje: error, acierto, advertencia
    {
        TMP_Text textos = panel.GetComponentInChildren<TMP_Text>();
        UnityEngine.UI.Image panelImage = panel.GetComponent<UnityEngine.UI.Image>();

        UnityEngine.Color errorColor = new UnityEngine.Color(255f / 255f, 0f / 255f, 0f / 255f, 1f);  // Color rojo
        UnityEngine.Color aciertoColor = new UnityEngine.Color(37f / 255f, 73f / 255f, 84f / 255f, 1f);  // Color verde
        UnityEngine.Color advertenciaColor = new UnityEngine.Color(1f, 1f, 128f, 1f);  // Color amarillo
        panelImage.color = errorColor;
        if (tipoMensaje == "acierto") panelImage.color = aciertoColor;
        if (tipoMensaje == "advertencia") panelImage.color = advertenciaColor;
        textos.text = texto;
        panel.SetActive(true);
        StartCoroutine(DesactivarPanelGuardado(panel));
    }
    private IEnumerator DesactivarPanelGuardado(GameObject panel)
    {
        yield return new WaitForSeconds(1);
        panel.SetActive(false);
    }
    public void tbFiltroCambia()
    {
        string filtro = tbFiltro.text.Trim().ToLower();

        // Filtrar según el contexto (CAMIONES u otros)
        if (tituloListaSeleccion.text == "CAMIONES" || tituloListaSeleccion.text == "DESCARGAS")
        {
            if (filtro.Length <= 0)
            {
                ResetearFiltro();
                return;
            }
            filteredOptionsCamiones = camiones
                .Where(c => c != null) // Primero filtramos los nulos
                .Where(c =>
                    (c.id.ToString()?.Contains(filtro) ?? false) ||
                    (!string.IsNullOrEmpty(c.Matricula) && c.Matricula.ToLower().Contains(filtro)))
                .OrderBy(c => c.id)
                .Select(c => $"{c.id}")
                .ToList();
        }
        if (tituloListaSeleccion.text == "PALLETS")
        {
            if (filtro.Length <= 2)
            {
                ResetearFiltro();
                return;
            }
            filteredOptionsPallets = pallets
                .Where(c => c != null) // Primero filtramos los nulos
                .Where(c =>
                    (c.id.ToString()?.Contains(filtro) ?? false) ||
                    (!string.IsNullOrEmpty(c.id) && c.id.ToLower().Contains(filtro)))
                .OrderBy(c => c.id)
                .Select(c => $"{c.id}")
                .ToList();
        }

        numeroPaginaLista = 0;
        GeneraLista(tituloListaSeleccion.text);
    }
    private void ResetearFiltro()
    {
        if (tituloListaSeleccion.text == "CAMIONES" || tituloListaSeleccion.text == "DESCARGAS")
        {
            filteredOptionsCamiones.Clear();
            // Restaurar lista original de camiones
            filteredOptionsCamiones = camiones
                .OrderBy(c => c.id)
                .Select(c => $"{c.id}")
                .ToList();
        }
        if (tituloListaSeleccion.text == "PALLETS")
        {
            filteredOptionsPallets.Clear();
            // Restaurar lista original de camiones
            filteredOptionsPallets = pallets
                .OrderBy(c => c.id)
                .Select(c => $"{c.id}")
                .ToList();
        }

        numeroPaginaLista = 0;
        GeneraLista(tituloListaSeleccion.text);

    }
    public void GestionaAtrasGaleria()
    {
        if (FichaPrevia == "Camion") AbrirNuevoCanvas(CanvasNuevoCamion);
        if (FichaPrevia == "Pallet") AbrirNuevoCanvas(CanvasNuevoPallet);
        if (FichaPrevia == "Estructura") AbrirNuevoCanvas(CanvasNuevaEstructura);
    }
    public void AbrirNuevoCanvas(Canvas canvasAbrir)
    {
        if (canvasAbrir != null)
        {
            // Desactivar todos los Canvas en la escena
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject != canvasAbrir) // Si el Canvas no es 'Pantalla 2'
                {
                    canvas.gameObject.SetActive(false); // Desactivamos el Canvas
                }
            }

            // Activar 'Pantalla 2'
            canvasAbrir.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el Canvas en el Inspector.");
        }
    }
    public void FechaAbajoCamiones()
    {
        bool valida = TryConvertToDateTime(tbFechaDescarga.text, out DateTime fecha);
        if (valida)
        {
            tbFechaDescarga.text = fecha.AddDays(-1).ToString("dd/MM/yyyy");
        }
        else
        {
            tbFechaDescarga.text = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
        }
    }
    public void FechaArribaCamiones()
    {
        bool valida = TryConvertToDateTime(tbFechaDescarga.text, out DateTime fecha);
        if (valida)
        {
            tbFechaDescarga.text = fecha.AddDays(1).ToString("dd/MM/yyyy");
        }
        else
        {
            tbFechaDescarga.text = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
        }
    }
    public void FechaAbajoEstructura()
    {
        bool valida = TryConvertToDateTime(tbFechaDescargaEstructura.text, out DateTime fecha);
        if (valida)
        {
            tbFechaDescargaEstructura.text = fecha.AddDays(-1).ToString("dd/MM/yyyy");
        }
        else
        {
            tbFechaDescargaEstructura.text = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
        }
    }
    public void FechaArribaEstructura()
    {
        bool valida = TryConvertToDateTime(tbFechaDescargaEstructura.text, out DateTime fecha);
        if (valida)
        {
            tbFechaDescargaEstructura.text = fecha.AddDays(1).ToString("dd/MM/yyyy");
        }
        else
        {
            tbFechaDescargaEstructura.text = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
        }
    }
    public void PonerDiaActualCamiones()
    {
        tbFechaDescarga.text = DateTime.Now.ToString("dd/MM/yyyy");
    }
    public void PonerDiaActualEstructura()
    {
        tbFechaDescargaEstructura.text = DateTime.Now.ToString("dd/MM/yyyy");
    }
    public void FechaAbajoInformePallets()
    {
        bool valida = TryConvertToDateTime(tbFechaInformePallets.text, out DateTime fecha);
        if (valida)
        {
            tbFechaInformePallets.text = fecha.AddDays(-1).ToString("dd/MM/yyyy");
        }
        else
        {
            tbFechaInformePallets.text = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
        }
    }
    public void FechaArribaInformePallets()
    {
        bool valida = TryConvertToDateTime(tbFechaInformePallets.text, out DateTime fecha);
        if (valida)
        {
            tbFechaInformePallets.text = fecha.AddDays(1).ToString("dd/MM/yyyy");
        }
        else
        {
            tbFechaInformePallets.text = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
        }
    }
    public void PonerDiaActualInformePallets()
    {
        tbFechaInformePallets.text = DateTime.Now.ToString("dd/MM/yyyy");
    }
    public bool TryConvertToDateTime(string fechaString, out DateTime fechaConvertida)
    {
        // Define el formato esperado: "día/mes/año" (ej: "25/12/2023")
        string formato = "dd/MM/yyyy";

        // Intenta convertir el string a DateTime
        bool esValida = DateTime.TryParseExact(
            fechaString,
            formato,
            CultureInfo.InvariantCulture, // Evita problemas con formatos regionales
            DateTimeStyles.None,         // Sin estilos adicionales
            out fechaConvertida          // Variable de salida con la fecha convertida
        );

        return esValida;
    }
    public void GestionaAtrasListaSeleccion()
    {
        if (tituloListaSeleccion.text == "CAMIONES")
        {
            AbrirNuevoCanvas(CanvasNuevoCamion);
            return;
        }
        if (tituloListaSeleccion.text == "PALLETS" || tituloListaSeleccion.text == "DESCARGAS")
        {
            AbrirNuevoCanvas(CanvasNuevoPallet);
            return;
        }
    }
    public void GestionaAtrasEstructura()
    {
        AbrirNuevoCanvas(CanvasNuevaEstructura);
        return;
    }
    public void Vibrate()
    {
        Handheld.Vibrate();
    }
    public void OnBotonGenerarPdf()
    {
        StartCoroutine(GenerateSimplePdf());
    }
    private IEnumerator EsperarTask(Task task)
    {
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.Exception != null)
        {
            Debug.LogError("Error en task: " + task.Exception);
        }
    }
    public IEnumerator GenerateSimplePdf()
    {
        // Crear documento
#if UNITY_ANDROID || UNITY_IOS
        CustomFontResolver resolver = new CustomFontResolver();
        yield return StartCoroutine(resolver.InitializeFonts());
#endif
        document = new PdfDocument();
        PdfPage page = document.AddPage();
        gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("arial", 12);
        XFont fontSmall = new XFont("arial", 8);
        XFont fontNegrita = new XFont("arial", 16);
        // Título
        int yPosition = 50;
        XRect rect = new XRect(50, yPosition, page.Width - 100, page.Height - 150);
        gfx.DrawString("CAMIONES RECEPCIONADOS", fontNegrita, XBrushes.Black, rect, PdfSharpCore.Drawing.XStringFormats.TopCenter);
        yPosition = yPosition + 50;
        gfx.DrawString(tbFechaInformePallets.text, fontNegrita, XBrushes.Black, (page.Width - 100) / 2, yPosition);

        //Logo de Osmos
        //string path = Path.Combine(Application.streamingAssetsPath, "LOGO COLOR.jpg");
        //XImage image = XImage.FromFile(path);
        //gfx.DrawImage(image, 50, page.Height-75, 100, 30);

        yield return EsperarTask(ActualizaCamiones());
        yield return EsperarTask(ActualizaPallets());
        DateTime fecha;
        string fechaDescarga = "";
        if (DateTime.TryParseExact(tbFechaInformePallets.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            fechaDescarga = fecha.ToString("MM/dd/yyyy");
        }
        camionesEncontrados = camiones?
            .Where(c => c != null &&
                       !string.IsNullOrEmpty(c.FechaDescarga) &&
                       DateTime.TryParse(c.FechaDescarga, out DateTime fechaCamion) &&
                       fechaCamion.ToString("MM/dd/yyyy") == fechaDescarga?.Trim())
            .OrderBy(c => c.id) // Ordena por el campo Id
            .ToList() ?? new List<Camiones>();

        yPosition = yPosition + 50;
        DibujaCuadricula(50, yPosition, gfx, fontSmall, camionesEncontrados, page.Width - 100);
        gfx.DrawString("FV Cierzo II", font, XBrushes.Black, (page.Width - 50), page.Height - 75, XStringFormats.CenterRight);
        gfx.DrawString("Tudela, Navarra", font, XBrushes.Black, (page.Width - 50), page.Height - 50, XStringFormats.CenterRight);
        gfx.DrawString("1/" + (1 + camionesEncontrados.Count).ToString(), font, XBrushes.Black, page.Width / 2, page.Height - 100, XStringFormats.Center);
        // Guardar archivo (en Android usa Application.persistentDataPath)
        int contadorhojas = 2;
        foreach (Camiones cam in camionesEncontrados)
        {
            GeneraPDFPallets(cam, contadorhojas);
            contadorhojas++;
        }
        string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_InformePallets.pdf";
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        document.Save(fullPath);
        Debug.Log("PDF generado en: " + fullPath);
        StartCoroutine(UploadInformeCoroutine(fullPath, fileName));
    }
    private IEnumerator UploadInformeCoroutine(string fullPath, string fileName)
    {
        Task uploadTask = SubirYEsperarInforme(fullPath, fileName);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
    }
    private async Task SubirYEsperarInforme(string fullPath, string fileName)
    {
        try
        {
            await supabase.Storage
                .From("informes")
                .Upload(fullPath, fileName, null);

            fileUrl = await EsperaInforme(fileName);
            CopyToClipboard(fileUrl);
            InputLinkInforme.text = fileUrl;
            MostrarToast("Link del informe copiado en portapapeles");
        }
        catch (Exception ex)
        {
            InformaMensaje(panelInfoInforme, "ERROR AL GUARDAR", "error");
            Debug.LogError("Error al procesar pdf: " + ex.Message);
        }
    }
    private async Task<string> EsperaInforme(string fileName)
    {
        try
        {
            return supabase.Storage.From("informes").GetPublicUrl(fileName);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al obtener URL pública: " + ex.Message);
            throw; // Re-throw the exception to be handled by the caller
        }
    }
    public void CopyToClipboard(string text)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject clipboardManager = context.Call<AndroidJavaObject>("getSystemService", "clipboard"))
            using (AndroidJavaClass clipDataClass = new AndroidJavaClass("android.content.ClipData"))
            using (AndroidJavaObject clip = clipDataClass.CallStatic<AndroidJavaObject>("newPlainText", "label", text))
            {
                clipboardManager.Call("setPrimaryClip", clip);
            }
        }
        else
        {
            GUIUtility.systemCopyBuffer = text;
        }
    }
    private AndroidJavaObject GetAndroidContext()
    {
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
    }
    private void DibujaCuadricula(Double x, Double y, XGraphics gfx, XFont font, List<Camiones> camiones, PdfSharpCore.Drawing.XUnit anchoTotal)
    {
        // Configuración de estilos
        XFont fontCabecera = new XFont(font.Name, font.Size);
        XBrush brushCabecera = XBrushes.White;
        XBrush brushFondoCabecera = XBrushes.Navy;
        double alturaFila = 20;
        double paddingHorizontal = 10;

        // Definición de columnas con sus anchos relativos
        var columnas = new[]
        {
        new { Titulo = "ID", AnchoRelativo = 0.8, Alineacion = XStringFormats.Center },
        new { Titulo = "Matrícula", AnchoRelativo = 1.2, Alineacion = XStringFormats.Center },
        new { Titulo = "Nombre conductor", AnchoRelativo = 2.0, Alineacion = XStringFormats.CenterLeft },
        new { Titulo = "DNI", AnchoRelativo = 1.5, Alineacion = XStringFormats.Center },
        //new { Titulo = "Ubicación", AnchoRelativo = 1.8, Alineacion = XStringFormats.CenterLeft },
        new { Titulo = "Fecha", AnchoRelativo = 1.5, Alineacion = XStringFormats.Center },
        //new { Titulo = "Contenedor", AnchoRelativo = 1.2, Alineacion = XStringFormats.Center },
        new { Titulo = "Albarán", AnchoRelativo = 1.2, Alineacion = XStringFormats.Center }
        //new { Titulo = "Actualizado", AnchoRelativo = 1.5, Alineacion = XStringFormats.Center }
    };

        // Calcular anchos reales basados en el ancho total disponible
        double anchoTotalDisponible = anchoTotal - (columnas.Length * 1); // Ajuste para bordes
        double factorConversion = anchoTotalDisponible / columnas.Sum(c => c.AnchoRelativo);

        double[] anchosReales = columnas.Select(c => c.AnchoRelativo * factorConversion).ToArray();

        // Dibujar cabeceras
        double posXActual = x;
        for (int i = 0; i < columnas.Length; i++)
        {
            // Fondo de cabecera
            gfx.DrawRectangle(brushFondoCabecera, posXActual, y, anchosReales[i], alturaFila);

            // Texto de cabecera
            gfx.DrawString(columnas[i].Titulo, fontCabecera, brushCabecera,
                new XRect(posXActual, y, anchosReales[i], alturaFila),
                columnas[i].Alineacion);

            posXActual += anchosReales[i];
        }

        // Dibujar filas de datos
        for (int row = 0; row < camiones.Count; row++)
        {
            var camion = camiones[row];
            double posYActual = y + ((row + 1) * alturaFila);

            // Color de fondo alternado
            XBrush fondoFila = row % 2 == 0 ? XBrushes.White : XBrushes.LightGray;

            posXActual = x;
            for (int col = 0; col < columnas.Length; col++)
            {
                // Dibujar fondo y borde
                gfx.DrawRectangle(fondoFila, posXActual, posYActual, anchosReales[col], alturaFila);
                gfx.DrawRectangle(XPens.Gray, posXActual, posYActual, anchosReales[col], alturaFila);

                // Obtener valor formateado
                string valor = ObtenerValorCamion(camion, columnas[col].Titulo).ToUpper();

                // Dibujar texto
                gfx.DrawString(valor, font, XBrushes.Black,
                    new XRect(posXActual + 2, posYActual, anchosReales[col] - 4, alturaFila),
                    columnas[col].Alineacion);

                posXActual += anchosReales[col];
            }
        }

    }
    private string ObtenerValorCamion(Camiones camion, string columna)
    {
        if (camion == null) return string.Empty;

        return columna switch
        {
            "ID" => camion.id.ToString(),
            "Matrícula" => camion.Matricula ?? "N/A",
            "Nombre conductor" => camion.NombreConductor ?? "Sin especificar",
            "DNI" => camion.DNI ?? "N/A",
            "Ubicacion" => camion.UbicacionCampa ?? "N/A",
            "Fecha" => FormatearFecha(camion.FechaDescarga),
            "Contenedor" => camion.Container ?? "N/A",
            "Albarán" => camion.Albaran ?? "N/A",
            "updated_at" => FormatearFecha(camion.updated_at),
            _ => string.Empty
        };
    }
    private string FormatearFecha(string fechaOriginal)
    {
        if (string.IsNullOrEmpty(fechaOriginal))
            return "N/A";

        if (DateTime.TryParse(fechaOriginal, out DateTime fecha))
        {
            return fecha.ToString("dd/MM/yyyy");
        }

        return fechaOriginal; // Devuelve el valor original si no se puede parsear
    }
    private async void GeneraPDFPallets(Camiones cam, int contador)
    {
        // Crear documento
        PdfPage page = document.AddPage();
        gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("arial", 12);
        XFont fontSmall = new XFont("arial", 8);
        XFont fontNegrita = new XFont("arial", 16);

        // Título
        int yPosition = 50;
        XRect rect = new XRect(50, yPosition, page.Width - 120, page.Height - 150);
        gfx.DrawString("PALLETS DESCARGA Nº: " + cam.id.ToString(), fontNegrita, XBrushes.Black, rect, XStringFormats.TopCenter);
        yPosition = yPosition + 50;
        gfx.DrawString(tbFechaInformePallets.text, fontNegrita, XBrushes.Black, (page.Width - 100) / 2, yPosition);
        //Logo de Osmos
        //string path = Path.Combine(Application.streamingAssetsPath, "LOGO COLOR.jpg");
        //XImage image = XImage.FromFile(path);
        //gfx.DrawImage(image, 50, page.Height - 75, 100, 30);

        DateTime fecha;
        string fechaDescarga = "";
        if (DateTime.TryParseExact(tbFechaInformePallets.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
        {
            fechaDescarga = fecha.ToString("MM/dd/yyyy");
        }


        // Buscar los pallets cuya propiedad "Descarga" coincide con alguno de esos IDs
        List<Pallets> palletsAsociados = new List<Pallets>();
        palletsAsociados = pallets.Where(p => !string.IsNullOrEmpty(p.id) && p.Descarga == cam.id).ToList();

        yPosition = yPosition + 50;

        DibujaCuadriculaPallets(50, yPosition, gfx, fontSmall, palletsAsociados, page.Width - 100); //Necesito implementar esta función

        gfx.DrawString("FV Cierzo II", font, XBrushes.Black, (page.Width - 50), page.Height - 75, XStringFormats.CenterRight);
        gfx.DrawString("Tudela, Navarra", font, XBrushes.Black, (page.Width - 50), page.Height - 50, XStringFormats.CenterRight);
        gfx.DrawString(contador.ToString() + "/" + (1 + camionesEncontrados.Count).ToString(), font, XBrushes.Black, page.Width / 2, page.Height - 100, XStringFormats.Center);
        // Guardar archivo (en Android usa Application.persistentDataPath)
    }
    private void DibujaCuadriculaPallets(double x, double y, XGraphics gfx, XFont font, List<Pallets> pallets, XUnit anchoTotal)
    {
        // 1. Validaciones iniciales para evitar NullReferenceException
        if (gfx == null)
        {
            Debug.LogError("XGraphics (gfx) no está inicializado");
            return;
        }

        // Configuración de estilos (con inicialización explícita)
        XFont fontCabecera = new XFont(font.Name, font.Size);
        XBrush brushCabecera = XBrushes.White;
        XBrush brushFondoCabecera = XBrushes.Navy; // Asegurar que no sea null
        double alturaFila = 20;

        // Definición de columnas
        var columnas = new[]
        {
        new { Titulo = "ID Pallet", AnchoRelativo = 1.5, Alineacion = XStringFormats.Center },
        new { Titulo = "Defectuoso", AnchoRelativo = 1.0, Alineacion = XStringFormats.Center }
        //new { Titulo = "Última actualización", AnchoRelativo = 2.0, Alineacion = XStringFormats.Center }
    };

        // Calcular anchos
        double anchoTotalDisponible = anchoTotal - (columnas.Length * 1);
        double factorConversion = anchoTotalDisponible / columnas.Sum(c => c.AnchoRelativo);
        double[] anchosReales = columnas.Select(c => c.AnchoRelativo * factorConversion).ToArray();
        // Dibujar cabeceras (con try-catch para depuración)
        try
        {
            double posXActual = x;
            for (int i = 0; i < columnas.Length; i++)
            {
                // Dibujar fondo de cabecera (validar brush)
                if (brushFondoCabecera != null)
                {
                    gfx.DrawRectangle(brushFondoCabecera, posXActual, y, anchosReales[i], alturaFila);
                }
                else
                {
                    gfx.DrawRectangle(XBrushes.Black, posXActual, y, anchosReales[i], alturaFila); // Fallback
                }

                // Dibujar texto de cabecera
                gfx.DrawString(
                    columnas[i].Titulo,
                    fontCabecera,
                    brushCabecera ?? XBrushes.Black, // Fallback
                    new XRect(posXActual, y, anchosReales[i], alturaFila),
                    columnas[i].Alineacion
                );

                posXActual += anchosReales[i];
            }

            // Dibujar filas de datos
            for (int row = 0; row < pallets.Count; row++)
            {
                var pallet = pallets[row];
                double posYActual = y + ((row + 1) * alturaFila);

                // Color de fondo alternado
                XBrush fondoFila = row % 2 == 0 ? XBrushes.White : XBrushes.LightGray;

                posXActual = x;
                for (int col = 0; col < columnas.Length; col++)
                {
                    // Dibujar fondo y borde
                    gfx.DrawRectangle(fondoFila, posXActual, posYActual, anchosReales[col], alturaFila);
                    gfx.DrawRectangle(XPens.Gray, posXActual, posYActual, anchosReales[col], alturaFila);

                    // Obtener valor formateado
                    string valor = ObtenerValorPallet(pallet, columnas[col].Titulo).ToUpper();

                    // Dibujar texto
                    gfx.DrawString(valor, font, XBrushes.Black,
                        new XRect(posXActual + 2, posYActual, anchosReales[col] - 4, alturaFila),
                        columnas[col].Alineacion);

                    posXActual += anchosReales[col];
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al dibujar cuadrícula: {ex.Message}");
        }
    }
    private string ObtenerValorPallet(Pallets pallet, string columna)
    {
        if (pallet == null) return string.Empty;

        return columna switch
        {
            "ID Pallet" => pallet.id ?? "N/A",
            "Defectuoso" => pallet.Defecto ? "SÍ" : "NO",
            "Última actualización" => FormatearFecha(pallet.updated_at),
            _ => string.Empty
        };
    }
    public void MostrarToast(string mensaje)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    using (AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast"))
                    using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                    using (AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", context, mensaje, toastClass.GetStatic<int>("LENGTH_SHORT")))
                    {
                        toast.Call("show");
                    }
                }));
            }
        }
    }
    public void muestraAyuda1(string menu)
    {
        Vector2 anchored = contentTransform.anchoredPosition;
        anchored.y = 0f;
        contentTransform.anchoredPosition = anchored;
        if (menu == "1")
        {
            t1.gameObject.SetActive(true);
            t2.gameObject.SetActive(false);
            t3.gameObject.SetActive(false);
            t4.gameObject.SetActive(false);
        }
        if (menu == "2")
        {
            t1.gameObject.SetActive(false);
            t2.gameObject.SetActive(true);
            t3.gameObject.SetActive(false);
            t4.gameObject.SetActive(false);
        }
        if (menu == "3")
        {
            t1.gameObject.SetActive(false);
            t2.gameObject.SetActive(false);
            t3.gameObject.SetActive(true);
            t4.gameObject.SetActive(false);
        }
        if (menu == "4")
        {
            t1.gameObject.SetActive(false);
            t2.gameObject.SetActive(false);
            t3.gameObject.SetActive(false);
            t4.gameObject.SetActive(true);
        }
    }
}