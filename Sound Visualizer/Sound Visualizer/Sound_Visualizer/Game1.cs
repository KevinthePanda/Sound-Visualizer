using System;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
namespace Sound_Visualizer
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        KeyboardState oldKeyState = Keyboard.GetState();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Song song;
        Texture2D texture, backgroundTexture;
        VisualizationData visData = new VisualizationData();
        Color backgroundColor = Color.White;
        Color tileColor = Color.BlueViolet;
        int barWidth;
        int barHeight = 7;
        int amount;
        int count;
        Boolean line = true;
        Layer myLayer = new Layer(new Point(32, 32), new Point(2, 2));

        public Song LoadSong(string FilePath)
        {
            FileInfo myFileInfo = new FileInfo(FilePath);
            return Song.FromUri(myFileInfo.Name.Replace(myFileInfo.Extension, ""), new Uri(FilePath, UriKind.Relative));
        }

        public int GetAverage(Point Between, VisualizationData VisData)
        {
            int average = 0;
            for (int i = Between.X; i < Between.Y; i++)
            {
                average += Convert.ToInt32(VisData.Frequencies[i]);
            }
            int diff = Between.Y - Between.X + 1;
            return average / diff;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
        }


        protected override void Initialize()
        {

            base.Initialize();
        }


        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);
            //creates a 1x1 texture
            texture = new Texture2D(GraphicsDevice,1,1);
            texture.SetData(new Color[] { Color.White });

            backgroundTexture = Content.Load<Texture2D>("bg");


            song = Content.Load<Song>("song2");
            barWidth = graphics.PreferredBackBufferWidth / 256;
            MediaPlayer.IsVisualizationEnabled = true;
            MediaPlayer.Play(song);
            myLayer.SetupTiles(graphics);
        }
        protected override void UnloadContent()
        {
        }


        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MediaPlayer.GetVisualizationData(visData);
            amount = GetAverage(new Point(0, 256), visData);
            KeyboardState keyState = Keyboard.GetState();
            
            if(keyState.IsKeyDown(Keys.W) && oldKeyState.IsKeyUp(Keys.W))
            {
                line = !line;
            }
            oldKeyState = keyState;
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightCoral);

            spriteBatch.Begin();
            

            if (MediaPlayer.State == MediaState.Playing && line)
            {
                spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), backgroundColor);
                for (int i = 0; i < 256; i++)
                {
                    backgroundColor = Color.FromNonPremultiplied(Convert.ToInt32(visData.Frequencies[amount] * 255), Convert.ToInt32(visData.Frequencies[amount] * 255),Convert.ToInt32(visData.Frequencies[amount] * 255), 255) ;
                    spriteBatch.Draw(texture, new Rectangle(i * barWidth, (graphics.PreferredBackBufferHeight / 2) + Convert.ToInt32(i * visData.Samples[i]), barWidth, barHeight), Color.FromNonPremultiplied(255, 0, i, 255));
                    spriteBatch.Draw(texture, new Rectangle(i * barWidth, (graphics.PreferredBackBufferHeight / 2) + Convert.ToInt32(i * visData.Samples[i]) + barHeight + 2, barWidth, barHeight), Color.FromNonPremultiplied(255, 0, i, 100));

                }
            }
            else if(MediaPlayer.State == MediaState.Playing)
            {
                count = 0;

                foreach(Rectangle myTile in myLayer.tiles)
                {
                    spriteBatch.Draw(texture, myTile, Color.FromNonPremultiplied(tileColor.R, tileColor.G, tileColor.B, (int) (visData.Frequencies[count] * count)));
                    if(count == 255) count = 0;
                    else count++;
                }
                    
            }
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
    public class Layer
    {
        public ArrayList tiles;
        public static int amount = 0;
        public Point tileSize, tilePadding;

        public Layer(Point TileSize, Point TilePadding)
        {
            tiles = new ArrayList();
            tileSize = TileSize;
            tilePadding = TilePadding;
        }

        public void AddTile(Rectangle Tile)
        {
            tiles.Add(Tile);
            amount++;
        }
        public void SetupTiles(GraphicsDeviceManager GDM)
        {
            for (int x = 0; x < GDM.PreferredBackBufferWidth; x += tileSize.X + tilePadding.X)
            {
                for(int y = 0; y < GDM.PreferredBackBufferHeight; y += tileSize.Y + tilePadding.Y)
                {
                    AddTile(new Rectangle(x, y, tileSize.X, tileSize.Y));
                }
            }
        }
    }
}
