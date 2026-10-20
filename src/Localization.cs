// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
using System.Globalization;

namespace QueryOS_Freelancer.Loc;

public static class Localization {

    private static Dictionary<string, string> _localizedStrings = new Dictionary<string, string>();

    public static string Get(string key) {
        if (_localizedStrings.TryGetValue(key, out string? value)) {
            if (string.IsNullOrEmpty(value)) {
                DevConsole.Log(LogType.Warning, $"Localization key '{key}' has a null value.");
                return key;
            }
            return value;
        }
        return key;
    }

    public static string Get(string key, params object[] args) {
        string template = Get(key);

        if (template == key) return key;

        try {
            return string.Format(template, args);
        } catch (FormatException) {
            DevConsole.Log(LogType.Warning, $"Localization format error for key '{key}'. Check arguments count.");
            return template;
        }
    }

    public static void Reload(string lang) {
        LoadBundle(lang);
    }

    public static Dictionary<string, string> LoadBundle(string lang = "auto") {
        _localizedStrings.Clear();

        string Lang = "";

        if (lang == "auto") {
            Lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
        } else {
            if (lang.Length == 2 || lang.Length == 5){
                Lang = lang.ToLower();
            }
        }
        
        string file = $"assets/bundles/bundle_{Lang}.properties";
        string defaultFile = "assets/bundles/bundle.properties";

        if (!File.Exists(file)) {
            if (!File.Exists(defaultFile)) {
                DevConsole.Log(LogType.Warning, $"Default localization file not found: {defaultFile}");
                return null!;
            }
            file = defaultFile;
        }

        string[] lines = File.ReadAllLines(file, System.Text.Encoding.UTF8);
        foreach (string line in lines) {
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split('=', 2);
            if (parts.Length == 2) {
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                _localizedStrings[key] = value;
            } else {
                DevConsole.Log(LogType.Warning, $"Invalid line in localization file: {line}");
            }
        }
        return _localizedStrings;
    }
}