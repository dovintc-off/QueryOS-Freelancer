// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://www.gnu.org/licenses/gpl-3.0.txt
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QueryOS_Freelancer.TextRendering;

public static class FontRegistry {
    private static readonly object SyncRoot = new();
    private static readonly IReadOnlyDictionary<Font, string> FontPaths = new Dictionary<Font, string> {
        [Font.PlanetKosmos] = Path.Combine("assets", "font", "planet-kosmos", "font.ttf"),
        [Font.Sonderpaedagogik] = Path.Combine("assets", "font", "sonderpaedagogik", "font.otf"),
        [Font.Tahoma] = Path.Combine("assets", "font", "tahoma", "font.ttf"),
        [Font.Tokushupikuse] = Path.Combine("assets", "font", "tokushupikuse", "font.otf"),
        [Font.Consolas] = Path.Combine("assets", "font", "consolas", "font.ttf")
    };
    private static readonly Dictionary<Font, FontSystem> FontSystems = new();
    private static readonly Dictionary<(Font Font, int Size), SpriteFontBase> FontCache = new();
    private static bool _initialized;

    public static void Initialize() {
        lock (SyncRoot) {
            if (_initialized)
                return;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var loadedSystems = new Dictionary<Font, FontSystem>();
            foreach (var entry in FontPaths) {
                string path = Path.Combine(baseDirectory, entry.Value);
                if (!File.Exists(path))
                    throw new FileNotFoundException(Localization.Get("Crash.FontFileNotFound", [entry.Key]), path);

                var fontSystem = new FontSystem();
                fontSystem.AddFont(File.ReadAllBytes(path));
                loadedSystems.Add(entry.Key, fontSystem);
            }

            foreach (var entry in loadedSystems)
                FontSystems.Add(entry.Key, entry.Value);
            _initialized = true;
        }
    }

    public static SpriteFontBase Get(Font font, int size) {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), Localization.Get("Crash.FontIsSmall"));

        lock (SyncRoot) {
            if (!_initialized)
                throw new InvalidOperationException(Localization.Get("Crash.ManagerFontsNotInited"));
            if (!FontSystems.TryGetValue(font, out FontSystem? fontSystem))
                throw new ArgumentOutOfRangeException(nameof(font), font, Localization.Get("Crash.FontNotReg"));

            var key = (font, size);
            if (FontCache.TryGetValue(key, out SpriteFontBase? cachedFont))
                return cachedFont;

            SpriteFontBase spriteFont = fontSystem.GetFont(size);
            FontCache.Add(key, spriteFont);
            return spriteFont;
        }
    }
}

public static class SpriteBatchExtensions {
    public static void DrawText(this SpriteBatch spriteBatch, Font font, int size, string text, Vector2 position, Color color) {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        ArgumentNullException.ThrowIfNull(text);
        FontRegistry.Get(font, size).DrawText(spriteBatch, text, position, color);
    }
}

public enum Font {
    PlanetKosmos,
    Sonderpaedagogik,
    Tahoma,
    Tokushupikuse,
    Consolas
}