// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
global using DevConsole = QueryOS_Freelancer._DevConsole.DevConsole;
global using LogType = QueryOS_Freelancer._DevConsole.LogType;
global using QueryOS_Freelancer.Loc;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using QueryOS_Freelancer.TextRendering;
using QueryOS_Freelancer.Core.FileSystem;
using Task = QueryOS_Freelancer.Core._Task.Task;
using TaskManager = QueryOS_Freelancer.Core._Task.TaskManager;
using System.Data.Common;
using System.Security;

namespace QueryOS_Freelancer.Main;

public class Main : Game {
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;

    private Myra.Graphics2D.UI.Desktop _playerDesktop = null!;
    private Myra.Graphics2D.UI.TextBox _playerInputBox = null!;

    private string dots = ".";
    private float dotsTimer = 0f;

    public Main() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        DevConsole.Log(LogType.Info, Localization.Get("Log.GameLoading"));
        Window.Title = "QueryOS: Freelancer";
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowsResize;

        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.HardwareModeSwitch = false;
        _graphics.IsFullScreen = true;

        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();

        System systemKernel = new System();
        systemKernel.Run(new string[] {"boot"});

        GlobalStates.GlobalStates.User.stage = 1;

        base.Initialize();

        DevConsole.Log(LogType.Info, Localization.Get("Log.InitConsole"));
        DevConsole.Initialize(this);

        DevConsole.Log(LogType.Info, Localization.Get("Log.InitFileSystem"));

        UpdateGlobalWindowSize();

        _playerDesktop = new Myra.Graphics2D.UI.Desktop();
        _playerInputBox = new Myra.Graphics2D.UI.TextBox {
            Background = new Myra.Graphics2D.Brushes.SolidBrush(Color.Transparent),
            TextColor = Color.Transparent,
            FocusedTextColor = Color.Transparent,
            Cursor = null,
            HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Stretch,
            VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Stretch
        };

        _playerInputBox.ValueChanging += (_, args) => {
            if (TaskManager.ActiveTask is ConsoleTask shell) {
                shell.CurrentInputBuffer = args.NewValue; 
            }
        };

        _playerInputBox.KeyDown += (_, args) => {
            if (args.Data != Keys.Enter) return;

            if (TaskManager.ActiveTask is ConsoleTask shell) {
                string finalInput = _playerInputBox.Text;
                
                _playerInputBox.Text = string.Empty;
                shell.CurrentInputBuffer = string.Empty;

                shell.HandleInput(finalInput);
            }
        };

        _playerDesktop.Root = _playerInputBox;
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        FontRegistry.Initialize();
        DevConsole.Log(LogType.Info, Localization.Get("Log.LoadContentSuccessfully"));
    }

    protected override void Update(GameTime gameTime) {
        ValidateInitialization();
        DevConsole.Update(gameTime);

        if (GlobalStates.GlobalStates.User.stage == 0) {
            dotsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dotsTimer >= 0.8f) {
                if (dots.Length == 0) {dots = ".";}
                else if (dots.Length == 1) {dots = "..";}
                else if (dots.Length == 2) {dots = "...";}
                else if (dots.Length == 3) {dots = "";}
                dotsTimer = 0f;
            }
        }

        if (GlobalStates.GlobalStates.User.stage == 1) {
            if (DevConsole.IsOpen) {
                _playerInputBox.Visible = false;
            } else {
                _playerInputBox.Visible = true;
                if (_playerDesktop.FocusedKeyboardWidget != _playerInputBox) {
                    _playerDesktop.FocusedKeyboardWidget = _playerInputBox;
                }
                _playerDesktop.UpdateInput();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        ValidateInitialization();

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        _spriteBatch.DrawText(Font.Tokushupikuse, 32, "Created by Dov1ntc",
            new Vector2(10,
                GlobalStates.GlobalStates.Window.height - (FontRegistry.Get(Font.Tokushupikuse, 32).MeasureString("Created by Dov1ntc").Y + 10)),
            Color.White);

        if (GlobalStates.GlobalStates.User.stage == 0) {
            var font = FontRegistry.Get(Font.Sonderpaedagogik, 100);
            string text = "QueryOS Loading"+dots;
            Vector2 textSize = font.MeasureString(text);

            float centerX = GlobalStates.GlobalStates.Window.width / 2f;
            float centerY = GlobalStates.GlobalStates.Window.height / 2f;
    
            _spriteBatch.DrawText(Font.Sonderpaedagogik, 100, text, new Vector2(centerX - (textSize.X / 2f), centerY - (textSize.Y / 2f)), Color.White);
        }

        if (TaskManager.ActiveTask != null && GlobalStates.GlobalStates.User.stage == 1) {
            var lines = TaskManager.ActiveTask.Terminal.ScreenLines;
            float startY = 10f;
            float lineHeight = 25f; 

            int i = 0;
            for (; i < lines.Count; i++) {
                _spriteBatch.DrawText(Font.Consolas, 26, lines[i], new Vector2(20, startY + (i * lineHeight)), Color.LimeGreen);
            }

            if (TaskManager.ActiveTask is ConsoleTask shell) {
                string user = GlobalStates.GlobalStates.User.name;
                string path = shell.FileManager.VirtualCurrentPath;
                
                string prompt = $"{user}@queryos:{path}$ ";
                
                float currentInputY = startY + (i * lineHeight);

                _spriteBatch.DrawText(Font.Consolas, 26, prompt, new Vector2(20, currentInputY), Color.LimeGreen);
                
                float promptWidth = FontRegistry.Get(Font.Consolas, 26).MeasureString(prompt).X;
                
                _spriteBatch.DrawText(Font.Consolas, 26, shell.CurrentInputBuffer, new Vector2(20 + promptWidth, currentInputY), Color.White);
            }
        }

        _spriteBatch.End();

        if (TaskManager.ActiveTask != null && GlobalStates.GlobalStates.User.stage == 1 && !DevConsole.IsOpen) {
            _playerDesktop.Render();
        }

        base.Draw(gameTime);

        DevConsole.Draw(_spriteBatch, GraphicsDevice);
    }

    protected override void UnloadContent() {
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private void OnWindowsResize(object? sender, EventArgs e) {
        if (Window.ClientBounds.Width == 0 || Window.ClientBounds.Height == 0)
            return;

        Window.ClientSizeChanged -= OnWindowsResize;

        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();

        UpdateGlobalWindowSize();

        Window.ClientSizeChanged += OnWindowsResize;
    }

    private void UpdateGlobalWindowSize() {
        GlobalStates.GlobalStates.Window.width = GraphicsDevice.Viewport.Width;
        GlobalStates.GlobalStates.Window.height = GraphicsDevice.Viewport.Height;
    }

    [MemberNotNull(nameof(_spriteBatch))]
    private void ValidateInitialization() {
        if (_spriteBatch == null) {
            throw new InvalidOperationException(Localization.Get("Crash.BatchNotInit"));
        }
    }
}

public class System : Task {

    public SystemFileManager FileManager { get; private set; }

    public System() : base("SYSTEM") {
        this.FileManager = new SystemFileManager();
    }

    protected override void Execute(string[] args) {
        FileManager.SetTerminalWith(this);
        FileManager.Run(new string[] { "boot" });

        ConsoleTask shell = new ConsoleTask(this.FileManager);
        TaskManager.ActiveTask = shell;
        shell.Run(new string[] { "start" });
    }

}

public class ConsoleTask : Task {
    public SystemFileManager FileManager;

    public string CurrentInputBuffer { get; set; } = "";

    public ConsoleTask(SystemFileManager fileManager) : base("sh") {
        FileManager = fileManager;
    }

    protected override void Execute(string[] args) {
        Terminal.WriteLine(Localization.Get("OS.Console.IsInited"));
    }

    public override void HandleInput(string input) {
        input = input.Trim();
        
        string user = GlobalStates.GlobalStates.User.name;
        string path = FileManager.VirtualCurrentPath;
        Terminal.WriteLine($"{user}@queryos:{path}$ {input}");

        if (string.IsNullOrEmpty(input)) return;

        Terminal.AddToHistory(input);

        string[] parts = input.Split(' ');
        string command = parts[0].ToLower();

        if (command == "clear") {
            Terminal.Clear();
        } 
        else if (command == "help") {
            Terminal.WriteLine(Localization.Get("OS.Console.Commands"));
        } 
        else if (command == "ls") {
            DevConsole.Log(LogType.Info, "Used `ls`");
            string[] items = FileManager.ListDirectory();
            foreach (var item in items) {
                string[] meta = item.Split('|');
                string type = meta[1] == "D" ? "<DIR> " : "<FILE>";
                Terminal.WriteLine($"{meta[0]}   {type}\t{meta[2]}");
                DevConsole.Log(LogType.Info, $"{meta[0]}   {type}\t{meta[2]}");
            }
        } 
        else if (command == "cd") {
            if (parts.Length > 1) {
                FileManager.ChangeDirectory(parts[1]);
            }
        } 

        else if (command == "exit") {
            throw new Exception();
        }

        else if (command == "echo") {
            DevConsole.Log(LogType.Info, "Used `echo` creation syntax");

            int firstQuote = input.IndexOf('"');
            int lastQuote = input.LastIndexOf('"');
            int operatorIndex = input.IndexOf(">>");

            if (firstQuote != -1 && lastQuote != -1 && firstQuote != lastQuote && operatorIndex > lastQuote) {
                
                string content = input.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                
                string filename = input.Substring(operatorIndex + 2).Trim();

                if (!string.IsNullOrEmpty(filename)) {
                    FileManager.CreateFile(filename, content);
                } else {
                    Terminal.WriteLine($"{Localization.Get("OS.Console.MessedFileName")}");
                }
            } else {
                Terminal.WriteLine(Localization.Get("OS.Console.echo.SyntaxError1"));
            }
        }
        else {
            Terminal.WriteLine(Localization.Get("OS.Console.CommandNotFound", command));
        }
        
        CurrentInputBuffer = "";
    }

}