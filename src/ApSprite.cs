using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WeddingWitchArchipelago;

/// The mod's own art: the Archipelago logo in colour for a shop shelf and a potion
/// banner, in white for the Shop menu button, and an inventory icon for the Upgrades
/// button — which lists what you hold rather than what is for sale. The supplied
/// six-colour charm represents an achievement location check.
///
/// Embedded in the assembly rather than loaded from disk so the plugin stays a single
/// file to install. Replace the files in res/ and rebuild.
public static class ApSprite
{
    private const string Colour = "WeddingWitchArchipelago.res.ap-icon.png";
    private const string White = "WeddingWitchArchipelago.res.ap-icon-white.png";
    private const string Inventory = "WeddingWitchArchipelago.res.inventory.png";
    private const string Achievement = "WeddingWitchArchipelago.res.achievement-check.png";

    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    // New Unity builds also expose ReadOnlySpan<byte> overloads, which cannot be
    // resolved against net472's reference assemblies. Bind the byte[] API once
    // so image loading keeps working without adding a runtime dependency.
    private static readonly Func<Texture2D, byte[], bool> LoadImage =
        (Func<Texture2D, byte[], bool>)Delegate.CreateDelegate(
            typeof(Func<Texture2D, byte[], bool>),
            typeof(ImageConversion).GetMethod(nameof(ImageConversion.LoadImage),
                new[] { typeof(Texture2D), typeof(byte[]) }));

    public static Sprite Get() => Load(Colour);

    public static Sprite GetWhite() => Load(White);

    public static Sprite GetInventory() => Load(Inventory);

    public static Sprite GetAchievement() => Load(Achievement);

    private static Sprite Load(string resource)
    {
        if (Cache.TryGetValue(resource, out var cached)) return cached;

        // Cached even on failure, so a missing resource is warned about once rather
        // than on every repaint.
        Cache[resource] = Decode(resource);
        return Cache[resource];
    }

    private static Sprite Decode(string resource)
    {
        try
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null)
                {
                    Plugin.Logger.LogWarning($"Embedded resource '{resource}' not found");
                    return null;
                }

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                // Size is irrelevant — LoadImage replaces the texture wholesale.
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                if (!LoadImage(texture, bytes))
                {
                    Plugin.Logger.LogWarning($"Failed to decode '{resource}'");
                    return null;
                }

                texture.filterMode = FilterMode.Bilinear;
                var sprite = Sprite.Create(texture,
                                           new Rect(0, 0, texture.width, texture.height),
                                           new Vector2(0.5f, 0.5f),
                                           pixelsPerUnit: 100f);
                sprite.name = resource;
                Plugin.Logger.LogInfo($"Loaded {resource} ({texture.width}x{texture.height})");
                return sprite;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"Could not load '{resource}': {ex.Message}");
            return null;
        }
    }
}

