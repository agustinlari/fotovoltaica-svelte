using PdfSharpCore.Fonts;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CustomFontResolver : IFontResolver
{
    // 1. Añade esta propiedad obligatoria
    public string DefaultFontName => "arial"; // Nombre debe coincidir con tu archivo .ttf

    private static bool _initialized = false;

    public byte[] GetFont(string faceName)
    {
        string fontPath = Path.Combine(Application.persistentDataPath, $"{faceName}.ttf");

        if (File.Exists(fontPath))
        {
            return File.ReadAllBytes(fontPath);
        }
        else
        {
            Debug.LogError($"Fuente no encontrada en: {fontPath}");
            return null;
        }
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // 2. Usa DefaultFontName como fallback
        if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
        {
            return new FontResolverInfo(DefaultFontName);
        }
        return new FontResolverInfo(DefaultFontName);
    }

    public IEnumerator InitializeFonts()
    {
        if (_initialized) yield break;

#if UNITY_ANDROID || UNITY_IOS
        string[] requiredFonts = { DefaultFontName }; // Usa la propiedad aquí

        foreach (var font in requiredFonts)
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, $"{font}.ttf");
            string destPath = Path.Combine(Application.persistentDataPath, $"{font}.ttf");

            if (!File.Exists(destPath))
            {
                UnityWebRequest www = UnityWebRequest.Get(sourcePath);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(destPath, www.downloadHandler.data);
                    Debug.Log($"Fuente {font} copiada a: {destPath}");
                }
                else
                {
                    Debug.LogError($"Error al copiar {font}: {www.error}");
                }
            }
        }
#endif

        GlobalFontSettings.FontResolver = new CustomFontResolver();
        _initialized = true;
    }
}