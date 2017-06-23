using System;
using System.Collections.Generic;
using System.Linq;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chip8
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Chip8 emu;
		Texture2D pixel;

        KeyboardState key;
        KeyboardState oldKey;

		public Game1()
		{
            graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 520;
            graphics.PreferredBackBufferHeight = 256;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();

            oldKey = Keyboard.GetState();
            key = Keyboard.GetState();

			base.Initialize();
		}

		/// <summary>		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			pixel = Content.Load<Texture2D>("pixel");

            emu = new Chip8();

			emu.LoadGame("Games/DRAW");
			//TODO: use this.Content to load your game content here 
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
            oldKey = key;
            key = Keyboard.GetState();

#if !__IOS__ && !__TVOS__
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
#endif
            for (int i = 0; i < 2; i++)
            {
                emu.Step();
            }

            Keys keyPressed = key.GetPressedKeys().Length > 0 ? key.GetPressedKeys().First() : Keys.None;
            if (emu.KeyboardTranslation.ContainsKey(keyPressed))
                emu.PressKey(emu.KeyboardTranslation[keyPressed]);
            

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int position = x + (y * 64);

                    if (position >= 2048)
                        break;

                    if(emu.gfxBuf[position] == 1)
                        spriteBatch.Draw(pixel, new Vector2((x * 8), (y * 8)), Color.White);
                }
            }
			if (emu.DrawFlag)
			{
				emu.Draw();
            }
            spriteBatch.End();
			//TODO: Add your drawing code here

			base.Draw(gameTime);
		}
	}
}
