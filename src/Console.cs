using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using QueryOS_Freelancer.TextRendering;

namespace QueryOS_Freelancer._Console;

public static class Console {
    private const int MaxLogs = 500;
    private const int FontSize = 18;
    private const float Padding = 10f;
    private const int InputPanelHeight = 52;
    private const int InputPanelMargin = 8;
    public static float BackgroundOpacity = 0.7f;
    private static readonly object SyncRoot = new();
    private static readonly List<LogEntry> _logs = new();
    private static readonly SolidBrush _backgroundBrush = new(Color.Black * BackgroundOpacity);
    private static Desktop _desktop = null!;
    private static Panel _consolePanel = null!;
    private static Panel _inputPanel = null!;
    private static TextBox _inputBox = null!;
    private static KeyboardState _previousKeyboardState;
    private static bool _suppressToggleCharacter;

    public static bool IsOpen { get; private set; }

    public static void Initialize(Game game) {
        ArgumentNullException.ThrowIfNull(game);

        MyraEnvironment.Game = game;
        _desktop = new Desktop();

        _consolePanel = new Panel {
            Background = _backgroundBrush
        };

        _inputPanel = new Panel {
            Background = new SolidBrush(Color.Transparent),
            Border = new SolidBrush(Color.Transparent),
            BorderThickness = Thickness.Zero,
            Height = InputPanelHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(InputPanelMargin)
        };

        _inputBox = new TextBox {
            Background = new SolidBrush(Color.Transparent),
            FocusedBackground = new SolidBrush(Color.Transparent),
            Border = new SolidBrush(Color.Transparent),
            FocusedBorder = new SolidBrush(Color.Transparent),
            BorderThickness = Thickness.Zero,
            Padding = new Thickness(10, 6),
            TextColor = Color.White,
            FocusedTextColor = Color.White,
            Selection = new SolidBrush(new Color(55, 115, 220, 220)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(8, 6)
        };
        _inputBox.ValueChanging += (_, args) => {
            if (_suppressToggleCharacter)
                args.Cancel = true;
        };
        _inputBox.KeyDown += (_, args) => {
            if (args.Data != Keys.Enter || string.IsNullOrEmpty(_inputBox.Text))
                return;

            string command = _inputBox.Text;
            Log(LogType.Command, command);
            ExecuteCommand(command);
            _inputBox.Text = string.Empty;
        };

        _inputPanel.Widgets.Add(_inputBox);
        _consolePanel.Widgets.Add(_inputPanel);
        _desktop.Root = _consolePanel;
        _previousKeyboardState = Keyboard.GetState();
    }

    public static void Log(LogType type, string message, [CallerFilePath] string callerPath = "") {
        ArgumentNullException.ThrowIfNull(message);
        string fileName = Path.GetFileName(callerPath);
        lock (SyncRoot) {
            _logs.Add(new LogEntry {
                Type = type,
                Text = message,
                Time = DateTime.Now,
                ExecutorFile = fileName
            });
            if (_logs.Count > MaxLogs)
                _logs.RemoveAt(0);
        }
    }

    public static void Write(string message, [CallerFilePath] string path = "") => Log(LogType.Info, message, path);

    public static void WriteLine(string message, [CallerFilePath] string path = "") => Log(LogType.Info, message, path);

    public static void Update(GameTime gameTime) {
        KeyboardState keyboardState = Keyboard.GetState();
        bool togglePressed = keyboardState.IsKeyDown(Keys.OemTilde) &&
            _previousKeyboardState.IsKeyUp(Keys.OemTilde);
        _previousKeyboardState = keyboardState;

        if (togglePressed)
            IsOpen = !IsOpen;
        _suppressToggleCharacter = togglePressed;

        if (IsOpen) {
            if (!ReferenceEquals(_desktop.FocusedKeyboardWidget, _inputBox))
                _desktop.FocusedKeyboardWidget = _inputBox;
            _desktop.UpdateInput();
        }
    }

    public static void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        if (!IsOpen)
            return;

        _backgroundBrush.Color = Color.Black * Math.Clamp(BackgroundOpacity, 0f, 1f);
        _desktop.Render();
        _suppressToggleCharacter = false;

        int screenWidth = graphicsDevice.Viewport.Width;
        int screenHeight = graphicsDevice.Viewport.Height;
        SpriteFontBase font = FontRegistry.Get(Font.Tahoma, FontSize);
        Vector2 lineSize = font.MeasureString("Ag");
        float lineHeight = lineSize.Y + 2f;
        LogEntry[] logs;
        lock (SyncRoot) {
            logs = _logs.ToArray();
        }

        int inputAreaHeight = InputPanelHeight + InputPanelMargin;
        int visibleCount = Math.Min(logs.Length,
            Math.Max(0, (int)((screenHeight - inputAreaHeight - Padding) / lineHeight)));
        float logY = screenHeight - inputAreaHeight - lineHeight - 4f;

        spriteBatch.Begin();
        for (int index = 0; index < visibleCount; index++) {
            LogEntry entry = logs[logs.Length - 1 - index];
            Color baseColor = GetLogColor(entry.Type);
            DrawLogLine(spriteBatch, font, entry, baseColor, Padding, logY - lineHeight * index,
                screenWidth - Padding);
        }
        spriteBatch.End();
    }

    private static void ExecuteCommand(string command) { }

    private static void DrawLogLine(SpriteBatch spriteBatch, SpriteFontBase font, LogEntry entry, Color baseColor, float x, float y, float rightEdge) {
        string fileTag = string.IsNullOrEmpty(entry.ExecutorFile) ? string.Empty : $"[{entry.ExecutorFile}] ";
        if (fileTag.Length > 0) {
            font.DrawText(spriteBatch, fileTag, new Vector2(x, y), Color.Gray);
            x += font.MeasureString(fileTag).X;
        }

        foreach (ColorSegment segment in ParseColorSegments(entry.Text, baseColor)) {
            if (x >= rightEdge)
                break;
            font.DrawText(spriteBatch, segment.Text, new Vector2(x, y), segment.Color);
            x += font.MeasureString(segment.Text).X;
        }
    }

    private static List<ColorSegment> ParseColorSegments(string text, Color baseColor) {
        var segments = new List<ColorSegment>();
        var buffer = new StringBuilder();
        Color currentColor = baseColor;

        void Flush() {
            if (buffer.Length == 0)
                return;
            segments.Add(new ColorSegment(buffer.ToString(), currentColor));
            buffer.Clear();
        }

        for (int index = 0; index < text.Length;) {
            if (text[index] != '[') {
                buffer.Append(text[index++]);
                continue;
            }

            int close = text.IndexOf(']', index + 1);
            if (close < 0) {
                buffer.Append(text[index++]);
                continue;
            }

            string tag = text.Substring(index + 1, close - index - 1);
            if (tag.StartsWith('\\') && close + 1 < text.Length && text[close + 1] == '\\' &&
                TryParseColor(tag[1..], out _)) {
                Flush();
                segments.Add(new ColorSegment($"[{tag[1..]}]", baseColor));
                index = close + 2;
                continue;
            }
            if (TryParseColor(tag, out Color parsedColor)) {
                Flush();
                currentColor = parsedColor;
                index = close + 1;
                continue;
            }

            buffer.Append(text, index, close - index + 1);
            index = close + 1;
        }

        Flush();
        return segments;
    }

    private static bool TryParseColor(string tag, out Color color) {
        switch (tag.ToLowerInvariant()) {
            case "red": color = Color.Red; return true;
            case "green": color = Color.Green; return true;
            case "blue": color = Color.Blue; return true;
            case "yellow": color = Color.Yellow; return true;
            case "white": color = Color.White; return true;
        }

        color = Color.White;
        if (tag.Length is not (7 or 9) || tag[0] != '#')
            return false;
        if (!byte.TryParse(tag.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte red) ||
            !byte.TryParse(tag.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte green) ||
            !byte.TryParse(tag.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte blue))
            return false;

        byte alpha = 255;
        if (tag.Length == 9 && !byte.TryParse(tag.AsSpan(7, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out alpha))
            return false;
        color = new Color(red, green, blue, alpha);
        return true;
    }

    private static Color GetLogColor(LogType type) => type switch {
        LogType.Warning => Color.Yellow,
        LogType.Error => Color.Red,
        LogType.Command => Color.Green,
        _ => Color.White
    };

    private readonly record struct ColorSegment(string Text, Color Color);
}

public struct LogEntry {
    public LogType Type;
    public string Text;
    public DateTime Time;
    public string ExecutorFile;

    public LogEntry() {
        Type = LogType.Info;
        Text = string.Empty;
        Time = default;
        ExecutorFile = string.Empty;
    }
}

public enum LogType {
    Info,
    Warning,
    Error,
    Command
}