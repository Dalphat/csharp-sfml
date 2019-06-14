using System;

using SFML.Graphics;
using SFML.Window;
using SFML.System;
using System.Collections.Generic;

/*
    This application demonstrates a simple binding of keys, managing states of binded keys, and time relative game loop.
    But more imporatantly, I demonstrates time controlled animation. We will only be alternating color in our example, but
        you can just apply this to alternating the sub rects of your texture or outright changing your textures if you're 
        using multiple textures.
*/

namespace CSharpSFML
{
    //Key binded values: actual Keyboard.Key's would be bounded to these input alias'. 
    //Example: 'Keyboard.Key.W' could be bound to our 'UP' enum in Game's KeyBind dictionary.
    enum Key
    {
        Up,
        Down,
        Left,
        Right,
        Run,
    };
    class AnimatedRectangleShape: RectangleShape
    {
        private float AnimationDelayInMilliseconds;
        private float DeltaAccumulator;
        public List<Color> FillColors;
        private int Index;
        public float Velocity;
        public float MinVelocity;
        public float MaxVelocity;
        public float Acceleration;
        public AnimatedRectangleShape(Vector2f size, float animateHowOftenPerSecond) : base(size)
        {
            AnimationDelayInMilliseconds = 1000 / animateHowOftenPerSecond;
            DeltaAccumulator = 0;
            FillColors = new List<Color>();
            Index = 0;
            Velocity = 1;
            MinVelocity = 1;
            MaxVelocity = 1;
            Acceleration = 1;
        }
        public void Animate(float delta)
        {
            DeltaAccumulator += delta;
            if(DeltaAccumulator > AnimationDelayInMilliseconds)
            {
                if(++Index >= FillColors.Count)
                    Index = 0;
                if (Index < FillColors.Count && FillColor != FillColors[Index])
                    FillColor = FillColors[Index];
                DeltaAccumulator -= AnimationDelayInMilliseconds;
            }
        }
    }
    class Game//Simple Game Class to demonstrate the game.
    {
        //Info on readonly: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/readonly
        public readonly RenderWindow Window;//The Window.
        public readonly Dictionary<Key, bool> KeyState;
        public readonly Dictionary<Keyboard.Key, Key> KeyBind;
        public readonly List<Shape> Shapes;
        private readonly Clock Clock;//The clock to acquire delta time.

        public Action<float> UpdateAction;
        public bool Running;//Running bool variable.
        public Action<string> Output;//Output action function.

        public Game(string title, uint width, uint height)
        {
            Window = new RenderWindow(new VideoMode(width, height), title);
            KeyState = new Dictionary<Key, bool>();
            KeyBind = new Dictionary<Keyboard.Key, Key>();
            Shapes = new List<Shape>();
            Clock = new Clock();

            KeyState.Add(Key.Up, false);
            KeyState.Add(Key.Down, false);
            KeyState.Add(Key.Left, false);
            KeyState.Add(Key.Right, false);

            //Handles closed button pressed.
            Window.Closed += new EventHandler((sender, e) => {
                Running = false;
                ((Window)sender).Close();
            });
        }
        public void Run()
        {
            //Some simple variables for managing how long to sleep, how often to update, how often to draw, and how often to print.
            int sleepPerSecond = 1000 / 240 + 1;//Occurs about 240 times per second. This is how many milliseconds the game loop sleeps each cycle.

            int updatePerSecond = 1000 / 120 + 1;//Occurs about 120 times per second. This is how many milliseconds before we do the update procedure.
            int drawPerSecond = 1000 / 60 + 1;//Occurs about 60 times per second. This is how many milliseconds before we do the draw procedure.
            int oncePerSecond = 1000;//Occurs about 1 times per second. This is how many milliseconds before we do the print procedure.

            int updateAccumulator = 0;//Accumulator for update. Units are in milliseconds.
            int drawAccumulator = 0;//Accumulator for draw. Units are in milliseconds.
            int printAccumulator = 0;//Accumulator for print. Units are in milliseconds.

            int fpsAccumulator = 0;//Accumulator for Frames per Second.

            Running = true;//Assign our running condition to true.
            Clock.Restart();//Restart the clock to give us a fresh clock timer before the game loop.
            while (Running)
            {
                int tick = Clock.ElapsedTime.AsMilliseconds();//Acquire clock timer since last Restart in milliseconds.
                Clock.Restart();//Restart the clock timer; set ElapsedTime to zero.

                //Handles our handlers: Rate == how fast this game loop cycles == relative to how long 
                Window.DispatchEvents();

                updateAccumulator += tick;
                if(updateAccumulator > updatePerSecond)
                {
                    UpdateAction?.Invoke(updatePerSecond);//IF not null, invoke this function
                    updateAccumulator -= updatePerSecond;
                }

                drawAccumulator += tick;
                if(drawAccumulator > drawPerSecond)
                {
                    Window.Clear();
                    foreach (Shape shape in Shapes)
                        Window.Draw(shape);
                    Window.Display();
                    drawAccumulator -= drawPerSecond;
                    ++fpsAccumulator;
                }

                printAccumulator += tick;
                if(printAccumulator > oncePerSecond)
                {
                    Debug("FPS: "+fpsAccumulator.ToString());
                    printAccumulator -= oncePerSecond;
                    fpsAccumulator = 0;
                }

                //Sleep to conserve processing prower and limit game loop ticks.
                System.Threading.Thread.Sleep(sleepPerSecond);//Force game loop to sleep X milliseconds.
            }
        }
        //Our debug function:
        private void Debug(string str)
        {
            Output?.Invoke(str);//IF not null, invoke this function
        }
    }
    class Program
    {
        static void Main(string[] args)//Test example for this game.
        {
            Game game = new Game("Example for Poke", 800, 600)
            {
                //Output function on initialization
                //Useful in case you want output to print to a file or over the net, you would just modify the Lamda function.
                //Comment out to remove console printout.
                Output = (str) => { Console.WriteLine(str); }//Sets debug output to print data to console.
            };
            //Adding a shape to index 0 and seting it's color:
            AnimatedRectangleShape aniShape = new AnimatedRectangleShape(new Vector2f(50, 50), 1);
            game.Shapes.Add(aniShape);
            aniShape.FillColor = Color.Red;
            aniShape.FillColors.Add(Color.Red);
            aniShape.FillColors.Add(Color.Blue);
            aniShape.FillColors.Add(Color.Green);
            aniShape.Velocity = 10;//How fast this shape will be moving.
            aniShape.MinVelocity = 10;
            aniShape.MaxVelocity = 30;
            aniShape.Acceleration = 0.2f;

            //Binding actual SFML Key's to our alias keys:
            game.KeyBind.Add(Keyboard.Key.Up, Key.Up);//Bind Up arrow to Up
            game.KeyBind.Add(Keyboard.Key.Down, Key.Down);//Bind Down arrow to Down
            game.KeyBind.Add(Keyboard.Key.Left, Key.Left);//Bind Left arrow to Left
            game.KeyBind.Add(Keyboard.Key.Right, Key.Right);//Bind Right arrow to Right
            game.KeyBind.Add(Keyboard.Key.Z, Key.Run);//Bind Z to Run

            //Add a key PRESSED handler to our game.
            //If the actual key is binded to our alias, update it's state to true: It was PRESSED!
            game.Window.KeyPressed += (sender, e) => {
                foreach (KeyValuePair<Keyboard.Key, Key> pair in game.KeyBind)//Evaluates against key binds.
                    if (e.Code == pair.Key)
                        game.KeyState[pair.Value] = true;//Applies changes to key states.
            };
            //Add a key RELEASED handler to our game.
            //If the actual key is binded to our alias, update it's state to true: It was RELEASED!
            game.Window.KeyReleased += (sender, e) => {
                foreach (KeyValuePair<Keyboard.Key, Key> pair in game.KeyBind)//Evaluates against key binds.
                    if (e.Code == pair.Key)
                        game.KeyState[pair.Value] = false;//Applies changes to key states.
            };

            //Assigns our KeyUpdate function to the below LAMBDA.
            //The LAMBDA will iterate through the dictionary of KeyState and evaluate if a key was pressed.
            //If key was pressed, we use the appropriate procedure.
            //Note:     If you wanted you could also have a dictionary of aliased Keys to procedures and avoid the switch statement
            //          This would also allow you to change individual procedures rather than this whole entire "KeyUpdate" function.
            //          I didn't want to confuse you by adding more dictionaries.
            game.UpdateAction = (delta) => {
                float moveDelta = delta / 120;//Moves 1 pixel every 1/120th of a second time objects Velocity.
                Vector2f Velocity = new Vector2f();
                foreach (KeyValuePair<Key, bool> pair in game.KeyState)
                {
                    if (pair.Value)
                    {
                        //Hard coded veol
                        switch (pair.Key)
                        {
                            //      You could better improve this by having a player reference to the shape instead.
                            case Key.Up:
                                Velocity.Y -= moveDelta;
                                break;
                            case Key.Down:
                                Velocity.Y += moveDelta;
                                break;
                            case Key.Left:
                                Velocity.X -= moveDelta;
                                break;
                            case Key.Right:
                                Velocity.X += moveDelta;
                                break;
                            case Key.Run:
                                aniShape.Velocity += aniShape.Velocity <= aniShape.MaxVelocity ? aniShape.Acceleration : 0;
                                break;
                        }
                    }
                    else
                    {
                        switch (pair.Key)
                        {
                            case Key.Run:
                                aniShape.Velocity += aniShape.Velocity >= aniShape.MinVelocity ? -aniShape.Acceleration : 0;
                                break;
                        }
                    }
                }
                if (Velocity.X != 0 || Velocity.Y != 0)
                    aniShape.Position += Velocity * aniShape.Velocity;
                foreach (AnimatedRectangleShape shape in game.Shapes)
                {
                    shape.Animate(delta);
                }
            };
            game.Run();//Run the game.
        }
    }
}
