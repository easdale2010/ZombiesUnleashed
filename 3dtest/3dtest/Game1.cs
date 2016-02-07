using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Devices.Sensors;
using gamelib2d;
using gamelib3d;
using accelerometer1;
using System.IO.IsolatedStorage;
using System.IO;
using ShapeRenderingSample;

namespace _ZombiesUnleashed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Resolution
        int displaywidth;
        int displayheight;

        Vector3 acc;

        SpriteFont mainfont;        // Font for drawing text on the screen

        Boolean gameover = false;   // Is the game over TRUE or FALSE?    

        float gameruntime = 0;      // Time since game started
        int score = 0;
        int health = 6;
        graphic2d background, background2, backgroundGO;       // Background image
        Random randomiser = new Random();       // Variable to generate random numbers

        int gamestate = -1;         // Current game state

        GamePadState[] pad = new GamePadState[1];       // Array to hold gamepad states
        KeyboardState keys;                             // Variable to hold keyboard state

        const int numberofoptions = 4;                    // Number of main menu options
        sprite2d[] menuoptions = new sprite2d[numberofoptions]; // Array of sprites to hold the menu options
        int optionselected = 0;                         // Current menu option selected

        const int numberofhighscores = 10;                              // Number of high scores to store
        int[] highscores = new int[numberofhighscores];                 // Array of high scores

        // Main 3D Game Camera
        camera gamecamera;

        const int numberofgrounds = 5;
        staticmesh[] ground = new staticmesh[numberofgrounds];  // 3D graphic for the ground in-game
        
        const int numberofwalls = 5;
        staticmesh[] leftwall = new staticmesh[numberofwalls];  // 3D graphic for walls
        staticmesh[] rightwall = new staticmesh[numberofwalls];  // 3D graphic for walls

        model3d playerchar;     // Robot model for user control

        const int numberofbullets = 2;
        staticmesh[] playerbullet = new staticmesh[numberofbullets];
        float bulletcount;

        // Create an array of trees
        const int numberofzombs = 25;
        staticmesh[] zombies = new staticmesh[numberofzombs];

        // Buttons for movement
        sprite2d up, down, left, right, firebut, controlshowto, menuback;

        // In-game sounds
        SoundEffect soundtrack, playergun, playerhit, zombiehit;
        SoundEffectInstance music;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
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
            displaywidth = graphics.GraphicsDevice.Viewport.Width;
            displayheight = graphics.GraphicsDevice.Viewport.Height;
            graphics.ToggleFullScreen();

            accelerometer1.Accelerometer.Initialize();
            gamecamera = new camera(new Vector3(0, 0, 0), new Vector3(0, 0, 0), displaywidth, displayheight, 45, Vector3.Up, 1000, 20000);

            // Initialises Debug shape renderer for drawing bounding boxes and spheres
            DebugShapeRenderer.Initialize(GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a music track
            soundtrack = Content.Load<SoundEffect>("Behind-every-Idea");
            music = soundtrack.CreateInstance();

            playergun = Content.Load<SoundEffect>("starting_pistol");
            zombiehit = Content.Load<SoundEffect>("Zombie Attacked");
            playerhit = Content.Load<SoundEffect>("Bite");

            //Make the track looped and if its stop then play it
            music.IsLooped = true;
            music.Volume = 0.75f;
            if (music.State == SoundState.Playing) music.Stop();



            // TODO: use this.Content to load your game content here
            mainfont = Content.Load<SpriteFont>("quartz4");  // Load the quartz4 font

            background = new graphic2d(Content, "ZombieBG", displaywidth, displayheight);
            background2 = new graphic2d(Content, "skyline", displaywidth, displayheight);
            backgroundGO = new graphic2d(Content, "gameoverbg", displaywidth, displayheight);

            up = new sprite2d(Content, "up", 115, displayheight - 150, 0.25f, Color.White, true);
            down = new sprite2d(Content, "down",115, displayheight - 50, 0.25f, Color.White, true);
            left = new sprite2d(Content, "left", 55, displayheight - 100, 0.25f, Color.White, true);
            right = new sprite2d(Content, "right", 175, displayheight - 100, 0.25f, Color.White, true);

            controlshowto = new sprite2d(Content, "Controls", 425, displayheight - 225, 0.65f, Color.White, true);

            firebut = new sprite2d(Content, "fire", 700, displayheight - 100, 0.50f, Color.White, true);
            menuback = new sprite2d(Content, "right", displaywidth - 50, 50, 0.25f, Color.White, true);
            
            menuoptions[0] = new sprite2d(Content, "buttonstart", displaywidth / 2, 150, 0.50f, Color.White, true);
            menuoptions[1] = new sprite2d(Content, "buttonhowtoplay", displaywidth / 2, 220, 0.50f, Color.White, true);
            menuoptions[2] = new sprite2d(Content, "buttonhighscore", displaywidth / 2, 290, 0.50f, Color.White, true);
            menuoptions[3] = new sprite2d(Content, "buttonexit", displaywidth / 2, 360, 0.50f, Color.White, true);

            // Initialise robot1 object
            playerchar = new model3d(Content, "player", 2f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.002f, 0.06f, 10);
            playerchar.bboxsize = new Vector3(25, 115, 25);


            // Load High Scores in
            using (IsolatedStorageFile savegamestorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (savegamestorage.FileExists("highscores.txt"))
                {
                    using (IsolatedStorageFileStream fs = savegamestorage.OpenFile("highscores.txt", System.IO.FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            string line;
                            for (int i = 0; i < numberofhighscores; i++)
                            {
                                line = sr.ReadLine();
                                highscores[i] = Convert.ToInt32(line);
                            }

                            sr.Close();
                        }
                    }
                }
            }
            // Sort high scores
            Array.Sort(highscores);
            Array.Reverse(highscores);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            pad[0] = GamePad.GetState(PlayerIndex.One);     // Reads gamepad 1
            keys = Keyboard.GetState();                     // Read keyboard

            float timebetweenupdates = (float)gameTime.ElapsedGameTime.TotalMilliseconds; // Time between updates
            gameruntime += timebetweenupdates;  // Count how long the game has been running for

            bulletcount -= timebetweenupdates;
            // TODO: Add your update logic here
            switch (gamestate)
            {
                case -1:
                    // Game is on the main menu
                    updatemenu();
                    break;
                case 0:
                    // Game is being played
                    updategame(timebetweenupdates);
                    break;
                case 1:
                    // Options menu
                    updateoptions();
                    break;
                case 2:
                    // High Score table
                    updatehighscore();
                    break;
                default:
                    // Do something if none of the above are selected

                    // save high scores
                    using (IsolatedStorageFile savegamestorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream("highscores.txt", System.IO.FileMode.Create, savegamestorage))
                        {
                            using (StreamWriter writer = new StreamWriter(fs))
                            {
                                for (int i = 0; i < numberofhighscores; i++)
                                {
                                    writer.WriteLine(highscores[i].ToString());
                                }
                                writer.Flush();
                                writer.Close();
                            }
                        }
                    }

                    this.Exit();    // Quit Game
                    break;
            }

            base.Update(gameTime);
        }

        void playerfire()
        {
            for (int i = 0; i < numberofbullets; i++)
            {
                if (!playerbullet[i].visible && bulletcount <= 0)
                {
                    playergun.Play();
                    bulletcount = 500;
                    playerbullet[i].visible = true;
                    playerbullet[i].position.X = playerchar.position.X;
                    playerbullet[i].position.Y = playerchar.position.Y;
                    playerbullet[i].position.Z = playerchar.position.Z;
                    playerbullet[i].velocity.Z = 0;
                    playerbullet[i].updateobject();
                }
            }

        }


        void reset()
        {
            gameover = false;

            gameruntime = 0;
            score = 0;
            health = 6;

            playerchar.position = new Vector3(0, 0, 0);
            playerchar.rotation = new Vector3(0, 0, 0);



            // Load the 3D models for the static objects in the game from the ContentManager
            for (int i = 0; i < numberofgrounds; i++)
            {
                ground[i] = new staticmesh(Content, "groundaxa", 10f, new Vector3(0, -40, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                if (i > 0)
                    ground[i].position.Z += ground[i - 1].position.Z + 800 * ground[i - 1].size;
            }

            for (int i = 0; i < numberofbullets; i++)
            {
                playerbullet[i] = new staticmesh(Content, "bullet", 1f, new Vector3(playerchar.position.X, playerchar.position.Y, playerchar.position.Z),
                    new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                playerbullet[i].radius = 30;
                playerbullet[i].visible = false;

            }


            for (int i = 0; i < numberofwalls; i++)
            {
                leftwall[i] = new staticmesh(Content, "wallaxa", 10f, new Vector3(-1000, -100, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                rightwall[i] = new staticmesh(Content, "wallaxa", 10f, new Vector3(1000, -100, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                if (i > 0)
                {
                    leftwall[i].position.Z += leftwall[i - 1].position.Z + 800 * leftwall[i - 1].size;
                    rightwall[i].position.Z = leftwall[i].position.Z;
                }
            }

            for (int i = 0; i < numberofzombs; i++)
            {
                spawnzombie(i, 1000);
                zombies[i].bboxsize = new Vector3(50, 300, 50);
                zombies[i].updateobject();
            }


        }


        void spawnzombie(int zombienumber, float zposition)
        {
            zombies[zombienumber] = new staticmesh(Content, "zombie", 3f,
                new Vector3(randomiser.Next(1600) - 800, 0, randomiser.Next(10000, 50000) + zposition),
                    new Vector3(0, 0, 0), new Vector3(0, 0, -(randomiser.Next(10, 50))));
            
            zombies[zombienumber].bboxsize = new Vector3(50, 300, 50);
            zombies[zombienumber].updateobject();
        }

        public void updatemenu()
        {
            optionselected = -1;

            // Check for touch over a menu option
            TouchCollection tcoll = TouchPanel.GetState();
            Boolean pressed = false;
            BoundingSphere touchsphere = new BoundingSphere(new Vector3(0, 0, 0), 0);
            foreach (TouchLocation t1 in tcoll)
            {
                if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                {
                    pressed = true;
                    touchsphere = new BoundingSphere(new Vector3(t1.Position.X, t1.Position.Y, 0), 1);
                }
            }

            for (int i = 0; i < numberofoptions; i++)
            {
                if (pressed && touchsphere.Intersects(menuoptions[i].bbox))
                {
                    optionselected = i;
                    gamestate = optionselected;
                    if (gamestate == 0) reset(); // If play game has been selected reset positions etc
                }

            }
        }

      

        public void drawmenu()
        {
            spriteBatch.Begin();
            // Draw menu options
            for (int i = 0; i < numberofoptions; i++)
            {
                    menuoptions[i].drawme(ref spriteBatch);
            }

            spriteBatch.End();
        }

        public void updategame(float gtime)
        {
            music.Play();

            // Main game code
            if (!gameover)
            {

                if (bulletcount <= 0)
                    bulletcount = 0;


                if (health <= 0)
                {
                    gameover = true;
                }

                for (int i = 0; i < numberofbullets; i++)
                {
                    if (playerbullet[i].visible)
                    {
                        playerbullet[i].velocity.Z += playerchar.velocity.Z * 2;
                        playerbullet[i].updateobject();

                        if (playerbullet[i].position.Z > playerchar.position.Z + 10000)
                        {
                            playerbullet[i].visible = false;
                        }

                        // Check for bullet hitting zombies
                        for (int j = 0; j < numberofzombs; j++)
                        {
                            if (playerbullet[i].bsphere.Intersects(zombies[j].bbox))
                            {
                                zombiehit.Play();
                                score += 10;
                                playerbullet[i].visible = false;
                                spawnzombie(j, playerchar.position.Z);
                            }

                        }
                    }
                }
                for (int i = 0; i < numberofzombs; i++)
                {
                    if (zombies[i].position.Z <= playerchar.position.Z - 1000)
                    {
                        spawnzombie(i, playerchar.position.Z);
                    }
                }



              
                // Game is being played
                if (pad[0].Buttons.Back == ButtonState.Pressed) gameover = true; // Allow user to quit game

                // Check for touch presses on arrow keys
                Vector2 dirtomove = new Vector2(0, 0);
                float turnamount = MathHelper.ToRadians(0);
                TouchCollection tcoll = TouchPanel.GetState();
                BoundingSphere touchsphere = new BoundingSphere(new Vector3(0, 0, 0), 1);
                foreach (TouchLocation t1 in tcoll)
                {
                    if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                    {
                        touchsphere = new BoundingSphere(new Vector3(t1.Position.X, t1.Position.Y, 0), 1);

                        if (touchsphere.Intersects(up.bbox))
                            dirtomove.Y = 1;
                        if (touchsphere.Intersects(down.bbox))
                            dirtomove.Y = -1;
                        if (touchsphere.Intersects(left.bbox))
                            dirtomove.X = 1;
                        if (touchsphere.Intersects(right.bbox))
                            dirtomove.X = -1;

                        if (touchsphere.Intersects(firebut.bbox) && bulletcount <= 0)
                            playerfire();

                    }
                }

                // Move Robot based on touch control input
                playerchar.moveme(dirtomove, turnamount, gtime, 70);


                // Check for collisions between player and zombies
                for (int i = 0; i < numberofzombs; i++)
                {
                    zombies[i].updateobject();
                    if (playerchar.bbox.Intersects(zombies[i].bbox) && zombies[i].visible)
                    {
                        playerhit.Play();
                        zombies[i].visible = false;
                        health--;
                        playerchar.velocity.Z = -playerchar.velocity.Z;
                        spawnzombie(i, playerchar.position.Z);      
                    }
                   
                }

                // Move Robot based on accelerometer
                dirtomove = new Vector2(0, 0);

                AccelerometerState accelsen = accelerometer1.Accelerometer.GetState();
                if (accelsen.IsActive)
                {
                    acc = accelsen.Acceleration;
                    dirtomove.X = accelsen.Acceleration.Y * 2;
                    if (TouchPanel.DisplayOrientation == DisplayOrientation.LandscapeRight)
                        dirtomove.X = -dirtomove.X;
                }
                // Move Robot based on accelerometer input
                playerchar.moveme(dirtomove, turnamount, gtime, 70);

                if (playerchar.velocity.Z < 5) playerchar.velocity.Z = 5; // Set a minimum speed for the robot

                // Set limits for the robots movements
                int wall_limits = 822;
                if (playerchar.position.X < -wall_limits) playerchar.position.X = -wall_limits;
                if (playerchar.position.X > wall_limits) playerchar.position.X = wall_limits;

                // Read flick gestures
                TouchPanel.EnabledGestures = GestureType.Flick;
                while (TouchPanel.IsGestureAvailable)
                {
                    GestureSample gs = TouchPanel.ReadGesture();
                    if (gs.GestureType == GestureType.Flick)
                    {
                        playerchar.jump(Math.Abs(gs.Delta.Y / 175));   // Jump robot based on the Y flick gesture amount
                    }
                }

                // Move ground & wall panels forward once you pass them
                for (int i = 0; i < numberofgrounds; i++)
                {
                    if (playerchar.position.Z > ground[i].position.Z + (500 * ground[i].size))
                    {
                        ground[i].position.Z += (800 * ground[i].size * (numberofgrounds - 1));
                        leftwall[i].position.Z = ground[i].position.Z;
                        rightwall[i].position.Z = ground[i].position.Z;
                    }
                }

                // Set side on camera view
                //gamecamera.setsideon(uservehicle.position, uservehicle.rotation, 1000, 100, 400);
                // Set the camera to first person
                //gamecamera.setFPor3P(uservehicle.position, uservehicle.direction, new Vector3(0, 0, 0), 100, 300, 60, 45);
                // Set overhead camera view
                //gamecamera.setoverhead(playerchar.position, 2000);
                // Set the camera to third person
                gamecamera.setFPor3P(playerchar.position, playerchar.direction, playerchar.velocity, 100, 100, 150, 100);
                // Allow the camera to look up and down
                //cameraposition.Y += (pad[0].ThumbSticks.Right.Y * 140);

            }
            else
            {
                //Stop music if its playing
                if (music.State == SoundState.Playing) music.Stop();

                // Game is over, allow game to return to the main menu
                if (pad[0].Buttons.Back == ButtonState.Pressed)
                {
                    if (score > highscores[9] && score > 0)
                        highscores[9] = score;

                    // SORT HIGHSCORE TABLE
                    Array.Sort(highscores);
                    Array.Reverse(highscores);

                    gamestate = -1; // Allow user to quit game
                }
            }
        }

        public void drawgame(GameTime gameTime)
        {

                // Draw the in-game graphics
                sfunctions3d.resetgraphics(GraphicsDevice);

                // Draw the ground & walls
                for (int i = 0; i < numberofgrounds; i++)
                    ground[i].drawme(gamecamera, false);

                for (int i = 0; i < numberofbullets; i++)
                    playerbullet[i].drawme(gamecamera, true);

                for (int i = 0; i < numberofwalls; i++)
                {
                    leftwall[i].drawme(gamecamera, false);
                    rightwall[i].drawme(gamecamera, false);
                }

                // Draw the robot
                playerchar.drawme(gamecamera, true);

                // Draw the Zombies
                for (int i = 0; i < numberofzombs; i++)
                {
                    if (Math.Abs(playerchar.position.Z - zombies[i].position.Z) < 10000)
                        zombies[i].drawme(gamecamera, true);
                }

                

                DebugShapeRenderer.Draw(gameTime, gamecamera.getview(), gamecamera.getproject());


                spriteBatch.Begin();
                //Draw the arrows for controls
                up.drawme(ref spriteBatch);
                down.drawme(ref spriteBatch);
                left.drawme(ref spriteBatch);
                right.drawme(ref spriteBatch);
            
            if (bulletcount <= 0)
                firebut.drawme(ref spriteBatch);


                //spriteBatch.DrawString(mainfont, "Res " + displaywidth.ToString() + " " + displayheight.ToString() + " Pos X:" + uservehicle.position.X.ToString("00000") + " Y:" + uservehicle.position.Y.ToString("00000") + " Z:" + uservehicle.position.Z.ToString("00000"), 
                //    new Vector2(20, 60), Color.Yellow, MathHelper.ToRadians(0), new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

                //spriteBatch.DrawString(mainfont, "Accelerometer X:" + acc.X.ToString("0.00") + " Y:" + acc.Y.ToString("0.00") + " Z:" + acc.Z.ToString("0.00") + " Game Time:" + (gameruntime / 1000).ToString("0"), 
                //    new Vector2(20, 80), Color.Yellow, MathHelper.ToRadians(0), new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

                spriteBatch.DrawString(mainfont, "Score: " + score.ToString(), new Vector2(625, 10),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 0.75f, SpriteEffects.None, 0);

                spriteBatch.DrawString(mainfont, "Health: " + health.ToString(), new Vector2(10, 10),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 0.75f, SpriteEffects.None, 0);


                spriteBatch.DrawString(mainfont, "Survive for as long as possible!", new Vector2((displaywidth / 2) - 200, displayheight - 35), 
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 0.75f, SpriteEffects.None, 0);

                if (gameover)
                {
                    backgroundGO.drawme(ref spriteBatch);
                }

            spriteBatch.End();
        }

        public void updateoptions()
        {
            // Update code for the options screen
            // Check for touch over a menu option
            TouchCollection tcoll = TouchPanel.GetState();
            //Boolean pressed = false;
            BoundingSphere touchsphere = new BoundingSphere(new Vector3(0, 0, 0), 0);
            foreach (TouchLocation t1 in tcoll)
            {
                if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                {
                    //pressed = true;
                    touchsphere = new BoundingSphere(new Vector3(t1.Position.X, t1.Position.Y, 0), 1);

                    if (touchsphere.Intersects(menuback.bbox))
                        gamestate = -1;
                }
            }
   

            // Allow game to return to the main menu
            if (pad[0].Buttons.Back == ButtonState.Pressed) gamestate = -1;
        }

        public void drawoptions()
        {
            // Draw graphics for OPTIONS screen
            spriteBatch.Begin();

            spriteBatch.DrawString(mainfont, "Controls and How To Play", new Vector2((displaywidth / 2) - 250, 50),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);

            menuback.drawme(ref spriteBatch);
            controlshowto.drawme(ref spriteBatch);

            spriteBatch.DrawString(mainfont, "How to Play / Objective:", new Vector2((displaywidth / 2) - 200, displayheight - 75),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 0.75f, SpriteEffects.None, 0);

            spriteBatch.DrawString(mainfont, "Survive for as long as possible!", new Vector2((displaywidth / 2) - 150, displayheight - 35),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 0.75f, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        public void updatehighscore()
        {
            // Update code for the high score screen
            // Check for touch over a menu option
            TouchCollection tcoll = TouchPanel.GetState();
            //Boolean pressed = false;
            BoundingSphere touchsphere = new BoundingSphere(new Vector3(0, 0, 0), 0);
            foreach (TouchLocation t1 in tcoll)
            {
                if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                {
                   // pressed = true;
                    touchsphere = new BoundingSphere(new Vector3(t1.Position.X, t1.Position.Y, 0), 1);

                    if (touchsphere.Intersects(menuback.bbox))
                        gamestate = -1;
                }
            }

            // Allow game to return to the main menu
            if (pad[0].Buttons.Back == ButtonState.Pressed) gamestate = -1;
        }

        public void drawhighscore()
        {
            // Draw graphics for High Score table
            spriteBatch.Begin();

            menuback.drawme(ref spriteBatch);

            spriteBatch.DrawString(mainfont, "High-Score Leaderboard", new Vector2((displaywidth / 2) - 200, 50),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);

            // Draw top ten high scores
            for (int i = 0; i < numberofhighscores; i++)
            {
                spriteBatch.DrawString(mainfont, (i + 1).ToString("0") + ". " + highscores[i].ToString("0"), new Vector2(displaywidth / 2 - 50, 100 + (i * 35)),
                    Color.LightGreen, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (gamestate == 0)
            {
                background2.drawme(ref spriteBatch);
            }
            else
            {
                background.drawme(ref spriteBatch);
            }

            spriteBatch.End();

            // Draw stuff depending on the game state
            switch (gamestate)
            {
                case -1:
                    // Game is on the main menu
                    drawmenu();
                    break;
                case 0:
                    // Game is being played
                    drawgame(gameTime);
                    break;
                case 1:
                    // Options menu
                    drawoptions();
                    break;
                case 2:
                    // High Score table
                    drawhighscore();
                    break;
                default:
                    break;
            }

            
            base.Draw(gameTime);
        }
    }
}
