using Blazor.Extensions.Canvas.Canvas2D;
// Editted Benjamin Westlake Nov 22

namespace GUI.Client.Models
{
    /// <summary>
    /// Class represent a Powerup object within the world.
    /// </summary>
    public class Powerup : Drawable
    {
        public int power { get; set; } // the powerup's unique ID.
        public Point2D loc { get; set; } // the powerup's location.
        public bool died { get; set; } // true if the powerup got eaten on that exact frame, false otherwise.

        /// <summary>
        /// Default constructor for a Powerup. Every Powerup must have a unique ID and a valid location.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="location"></param>
        public Powerup( int ID, Point2D location)
        {
            this.power = ID;
            this.loc = location;
            died = false;
        }

        /// <summary>
        /// Json Serialization contstructor for a Powerup.
        /// </summary>
        public Powerup()
        {
        }
        /// <summary>
        /// Draw method for a Powerup. This method is only called from the View when it decides to draw the object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DrawAsync(Canvas2DContext context)
        {
            
            await context.SetFillStyleAsync("purple");
            await context.SetLineWidthAsync(1);
            if (!died) 
                await context.FillRectAsync(loc.X-8, loc.Y-8,  16,  16);
        }
    }
}
