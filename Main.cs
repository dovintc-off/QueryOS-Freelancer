// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using QueryOS_Freelancer.TextRendering;

namespace QueryOS_Freelancer.Main;

public class Main : Game {
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;

    public Main() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        Window.Title = "QueryOS: Freelancer";
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowsResize;

        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.HardwareModeSwitch = false;
        _graphics.IsFullScreen = false;

        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();

        base.Initialize();

        UpdateGlobalWindowSize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        FontRegistry.Initialize();
    }

    protected override void Update(GameTime gameTime) {
        ValidateInitialization();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        ValidateInitialization();

        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        _spriteBatch.DrawText(Font.Tokushupikuse, 32, "Created by Dov1ntc",
            new Vector2(20,
                GlobalStates.GlobalStates.Window.height - (FontRegistry.Get(Font.Tokushupikuse, 32).MeasureString("Created by Dov1ntc").Y + 20)),
            Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
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
            throw new InvalidOperationException("SpriteBatch is not initialized.");
        }
    }
}
