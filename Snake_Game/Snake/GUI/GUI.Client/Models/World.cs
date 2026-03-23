using Blazor.Extensions.Canvas.Canvas2D;
using System.Runtime.CompilerServices;

namespace GUI.Client.Models
{
    /// <summary>
    /// Represents everything in the game world
    /// </summary>
    public class World : Drawable
    {
        public int size;
        public Dictionary<int, Snake> players; // A list of all the snake objects in the world.
        public Dictionary<int, Wall> walls; // A list of all the walls in the world.
        public Dictionary<int, Powerup> powerups; // A list of all the powerups in the world.

        /// <summary>
        /// 0-arg constructor for a Wall object.
        /// </summary>
        public World()
        {
            this.size = 0;
            walls = new Dictionary<int, Wall>();
            powerups = new Dictionary<int, Powerup>();
            players = new Dictionary<int, Snake>();
        }
        /// <summary>
        /// Default Constructor for a Wall object. This is the constructor that the Controller uses.
        /// </summary>
        /// <param name="size"></param>
        public World(int size)
        {
            this.size = size;
            walls = new Dictionary<int, Wall>();
            powerups = new Dictionary<int, Powerup>();
            players = new Dictionary<int, Snake>();
        }
        /// <summary>
        /// Use this function to update world state with new objects (limited to wall snake and powerup)
        /// </summary>
        /// <param name="obj"></param>
        public void UpdateGameObject(Object obj)
        {
            if (obj as Wall != null)
                UpdateWall((Wall)obj);
            if (obj as Snake != null)
                UpdatePlayer((Snake)obj);
            if (obj as Powerup != null)
                UpdatePowerUp((Powerup)obj);
        }
        /// <summary>
        /// Adds or replaces a Wall object in the world. A Wall will be replaced if the new Wall
        /// has the same ID.
        /// </summary>
        /// <param name="wall"></param>
        public void UpdateWall(Wall wall)
        {
            if (!walls.ContainsKey(wall.wall)) // This is where the race condition is handled between the main thread and the Controller thread.
                lock (walls)
                {
                    walls.Add(wall.wall, wall);
                }
            else
                lock (walls)
                {
                    walls[wall.wall] = wall;
                }
        }
        /// <summary>
        /// Adds or replaces a Snake object in the world. A Snake will be replaced if the new Snake
        /// has the same ID. Returns true if a preexisting object was updated, or false if the object was newly added.
        /// </summary>
        /// <param name="snek"></param>
        public bool UpdatePlayer(Snake snek)
        {
            if (!players.ContainsKey(snek.snake)) // This is where the race condition is handled between the main thread and the Controller thread.
                lock (players)
                {
                    players.Add(snek.snake, snek);
                    snek.hiscore = snek.score;
                    return false;
                }
            else
                lock (players)
                {
                    if (players[snek.snake].hiscore > snek.score)
                    {
                        snek.hiscore = players[snek.snake].hiscore;
                    }
                    else { snek.hiscore = snek.score; }
                    players[snek.snake] = snek;
                    return true;
                }
        }
        /// <summary>
        /// Adds or replaces a Powerup object in the world. A Powerup will be replaced if the new Powerup
        /// has the same ID.
        /// </summary>
        /// <param name="pow"></param>
        public void UpdatePowerUp(Powerup pow)
        {
            if (!powerups.ContainsKey(pow.power)) // This is where the race condition is handled between the main thread and the Controller thread.
                lock (powerups)
                {
                    powerups.Add(pow.power, pow);
                }
            else
                lock (powerups)
                {
                    powerups[pow.power] = pow;
                }
        }
        /// <summary>
        /// Draw method for the World. This will in turn call Draw on all the objects within the World.
        /// This method is only called from the View when it decides to draw the World.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task DrawAsync(Canvas2DContext context)
        {

            await context.SetFillStyleAsync("purple");
            List<Drawable> drawables = new List<Drawable>();
            // Get the objects and put them into a list so we can draw
            lock (walls)
            {
                foreach (Wall wal in walls.Values)
                {
                    drawables.Add((Drawable)wal);
                }
            }
            lock (powerups)
            {
                foreach (Powerup pow in powerups.Values)
                {
                    drawables.Add((Drawable)pow);
                }
            }
            lock (players)
            {
                foreach (Snake snek in players.Values)
                {
                    drawables.Add((Drawable)snek);
                }
            }
            foreach (var toDraw in drawables)
            {
                await toDraw.DrawAsync(context);
            }
        }
    }
}