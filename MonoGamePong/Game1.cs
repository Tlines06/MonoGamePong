using Microsoft.VisualBasic.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectWrite;
using System;
using System.Reflection;

namespace MonoGamePong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        int screenWidth = 960;
        int screenHeight = 540;
        Texture2D whiteTexture;
        Rectangle leftPaddle;
        Rectangle rightPaddle;
        Rectangle ball;
        int paddleWidth = 16;
        int paddleHeight = 110;
        int ballSize = 14;
        KeyboardState currentkeyboardState;
        float paddleSpeed = 420f;
        Vector2 ballVelocity = new Vector2(300, 300);
        double ballSpeed = 1;
        int leftScore = 0;
        int rightScore = 0;
        SpriteFont font;
        string outputText;
        Vector2 textSize;
        float cpuDeadZone = 10;
        const double maxBallSpeed = 5;
        const int shrink = 5;
        bool move = false;
        float timer = 3;
        float currentCooldown = 0;
        SoundEffect sound;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.PreferredBackBufferWidth = screenWidth;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            rightPaddle = new Rectangle(
                screenWidth - (40 + paddleWidth),
                (screenHeight / 2) - (paddleHeight / 2),
                paddleWidth,
                paddleHeight);
            leftPaddle = new Rectangle(
                40,
                (screenHeight / 2) - (paddleHeight / 2), 
                paddleWidth,
                paddleHeight);
            ball = new Rectangle(
                (screenWidth / 2) - (ballSize / 2),
                (screenHeight / 2) - (ballSize / 2),
                ballSize,
                ballSize);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            Color[] pixelColor = { Color.White };
            whiteTexture.SetData(pixelColor);
            font = Content.Load<SpriteFont>("File");
            sound = Content.Load<SoundEffect>("ball_hit");
            UpdateText();
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (move == false)
            {
                currentCooldown += deltaTime;
                if (currentCooldown >= timer)
                {
                    currentCooldown = 0;
                    move = true;
                }
            }
            else
            {
                currentkeyboardState = Keyboard.GetState();
                if (currentkeyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
                if (currentkeyboardState.IsKeyDown(Keys.W))
                {
                    leftPaddle.Y -= (int)(paddleSpeed * deltaTime);
                }
                else if (currentkeyboardState.IsKeyDown(Keys.S))
                {
                    leftPaddle.Y += (int)(paddleSpeed * deltaTime);
                }
                //if (currentkeyboardState.IsKeyDown(Keys.Up))
                //{
                //rightPaddle.Y -= (int)(paddleSpeed * deltaTime);
                //}
                //else if (currentkeyboardState.IsKeyDown(Keys.Down))
                //{
                //rightPaddle.Y += (int)(paddleSpeed * deltaTime);
                //}
                //rightPaddle.Y = ball.Y;
                if (ball.X >= screenWidth / 2)
                {
                    float paddleCenterY = rightPaddle.Y + rightPaddle.Height / 2;
                    float ballCenterY = ball.Y + ball.Height / 2;
                    float diff = (ballCenterY - paddleCenterY);
                    if (Math.Abs(diff) > cpuDeadZone)
                    {
                        if (diff > 0)
                        {
                            rightPaddle.Y += (int)(paddleSpeed * deltaTime);
                        }
                        else
                        {
                            rightPaddle.Y -= (int)(paddleSpeed * deltaTime);
                        }
                    }
                }
                leftPaddle.Y = MathHelper.Clamp(leftPaddle.Y, 0, screenHeight - leftPaddle.Height);
                rightPaddle.Y = MathHelper.Clamp(rightPaddle.Y, 0, screenHeight - rightPaddle.Height);
                if (move == true)
                {
                    ball.X += (int)((ballVelocity.X * deltaTime) * ballSpeed);
                    ball.Y += (int)((ballVelocity.Y * deltaTime) * ballSpeed);
                }

                if (ball.Top <= 0)
                {
                    ball.Y = 0;
                    ballVelocity.Y *= -1f;
                }
                else if (ball.Bottom >= screenHeight)
                {
                    ball.Y = screenHeight - ball.Height;
                    ballVelocity.Y *= -1f;
                }
                if (ball.Intersects(leftPaddle) && ballVelocity.X < 0f)
                {
                    ball.X = leftPaddle.Right;
                    ballVelocity.X *= -1f;
                    rightPaddle.Height -= shrink;
                    leftPaddle.Height -= shrink;
                    if (ballSpeed < maxBallSpeed)
                    {
                        ballSpeed += 0.1;
                    }
                }
                if (ball.Intersects(rightPaddle) && ballVelocity.X > 0f)
                {
                    ball.X = rightPaddle.Left - ball.Width;
                    ballVelocity.X *= -1f;
                    sound.Play();
                    rightPaddle.Height -= shrink;
                    leftPaddle.Height -= shrink;
                    if (ballSpeed < maxBallSpeed)
                    {
                        ballSpeed += 0.1;
                    }
                }
                if (ball.Right < 0 || ball.Left > screenWidth)
                {
                    if (ball.Right < 0)
                    {
                        rightScore++;
                    }
                    else
                    {
                        leftScore++;
                    }
                    UpdateText();
                    ResetBall();
                }

                base.Update(gameTime);
            }
                
        }
        private void ResetBall()
        {
            ball.X = (screenWidth / 2) - (ball.Width / 2);
            ball.Y = (screenHeight / 2) - (ball.Height / 2);
            ballVelocity.X = -ballVelocity.X;
            ballSpeed = 1;
            rightPaddle.Height = 110;
            leftPaddle.Height = 110;
            move = false;
        }
        private void UpdateText() 
        {
            outputText = $"P1: {leftScore} ----- P2: {rightScore}";
            textSize = font.MeasureString(outputText);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(whiteTexture, leftPaddle, Color.White);
            spriteBatch.Draw(whiteTexture, new Rectangle(screenWidth / 2 - 1, 0, 2, screenHeight), Color.White);
            spriteBatch.Draw(whiteTexture, rightPaddle, Color.White);
            spriteBatch.Draw(whiteTexture, ball, Color.White);
            spriteBatch.DrawString(
                font,
                outputText,
                new Vector2(screenWidth / 2 - textSize.X / 2, 5),
                Color.White);
            spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
