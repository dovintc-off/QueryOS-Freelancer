// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org

namespace QueryOS_Freelancer.Core._Terminal;

public class Terminal {
    private readonly List<string> _screenLines = new();
    private readonly List<string> CommandHistory = new();
    private int HistoryIndex = -1;
    public int MaxLines {get; set;} = 30;
    public IReadOnlyList<string> ScreenLines => _screenLines;

    public void WriteLine(string text) {
        if (text == null) return;

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        foreach (var line in lines) {
            _screenLines.Add(line);
        }
        while (_screenLines.Count > MaxLines) {
            _screenLines.RemoveAt(0);
        }
    }

    public void Clear(){
        _screenLines.Clear();
    }

    public void AddToHistory(string commandLine) {
        if (string.IsNullOrWhiteSpace(commandLine)) return;

        if (CommandHistory.Count == 0 || CommandHistory[^1] != commandLine){
            CommandHistory.Add(commandLine);
        }
        
        HistoryIndex = CommandHistory.Count;
    }

    public string GetHistoryUp() {
        if (CommandHistory.Count == 0) return string.Empty;
        
        if (HistoryIndex > 0) 
            HistoryIndex--;
            
        return CommandHistory[HistoryIndex];
    }

    public string GetHistoryDown() {
        if (CommandHistory.Count == 0) return string.Empty;

        if (HistoryIndex < CommandHistory.Count - 1) {
            HistoryIndex++;
            return CommandHistory[HistoryIndex];
        }
        HistoryIndex = CommandHistory.Count;
        return string.Empty;
    }
}