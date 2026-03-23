using Blazor.Extensions.Canvas.Canvas2D;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
// Editted Benjamin Westlake Nov 22
namespace GUI.Client.Models
{
    /// <summary>
    /// Represents a Snake object that each player controls. The fields represent real-time information about
    /// each individual snake's state.
    /// </summary>
    public class Snake : Drawable
    {
        public int snake { get; set; } // unique snake ID
        public string name { get; set; } // The player's name controlling the snake
        public List<Point2D> body { get; set; } // list of points that represent snake segments. Last point = head.
        public Point2D dir { get; set; } // the direction the snake is facing. Will always be axis-aligned.
        public int score { get; set; } // how many powerups the snake has eaten.
        public bool died { get; set; } // will only be true on the exact frame the snake died, false otherwise.
        public bool alive { get; set; } // If the snake is dead or alive.
        public bool dc { get; set; } // true if the player disconnected on that exact frame, false otherwise.
        public bool joined { get; set; } // true if the player joined on that exact frame, false otherwise.

        [JsonIgnore]
        public int hiscore { get; set; } // High score for this player (used for the database, not for JSON)

        private string color;
        /// <summary>
        /// Json Serialization constructor for a Snake. Since the data within each Snake instance will be updated in real time,
        /// setting the fields within the constructor is not necessary.
        /// </summary>
        public Snake()
        {
            
        }
        /// <summary>
        /// Draw method for a Snake. This method is only called from the View when it decides to draw the object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DrawAsync(Canvas2DContext context)
        {
            if (color == null)
            {
                Random ran = new(snake);
                color = $"rgb( " + ran.Next(100,254) + ", " + ran.Next(100, 254)+ ", " + ran.Next(100, 254) +" )";
                Debug.WriteLine(color);
            }
            await context.SetFillStyleAsync(color);// will change for players based on Id
            Point2D? p1 = null;
            Point2D? p2 = p1;
            if (!alive && !dc)
                return;


            await context.BeginPathAsync();
            await context.ArcAsync(body[body.Count-1].X, body[body.Count - 1].Y, 6, 0, 2 * Math.PI);
            await context.StrokeAsync();

            System.Diagnostics.Debug.WriteLine("start snake transcript");
            foreach (Point2D p in body)
            {
                p2 = p;
                if (p1 != null)
                {
                    
                    await context.FillRectAsync(p1.X - 5, p1.Y - 5, p2.X - p1.X + 10, p2.Y - p1.Y + 10);
                    
                }
                p1 = p2;
            }
            await context.SetFillStyleAsync("black");
            var head = body[body.Count - 1];
            await context.FillTextAsync(name, head.X + 15, head.Y, 160);
            await context.SetFillStyleAsync("white");
            await context.FillTextAsync(name, head.X + 13, head.Y-2, 160);

        }
    }
}
