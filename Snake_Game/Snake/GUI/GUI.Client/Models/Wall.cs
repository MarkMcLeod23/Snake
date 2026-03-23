using Blazor.Extensions.Canvas.Canvas2D;
using System.Text.Json.Serialization;

namespace GUI.Client.Models
{
    public class Wall : Drawable
    {
        public int wall; // the wall's unique ID
        public Point2D p1;// represents the wall's 2 endpoints. Both endpoints must be axis-aligned with each other.
        public Point2D p2;// the order of the endpoints does not matter but wall length must be muliples of 50.

        /// <summary>
        /// Default constructor for a Wall. For a wall to be a valid actor within the game, it must have a unique ID,
        /// and the distance between points must be a multiple of 50.
        /// NOTE: this method is redundant if the server handles the creation of all the walls.
        /// </summary>
        /// <param name="ID"></param>
        public Wall(int ID, Point2D first, Point2D second)
        {
            wall = ID;

            if ( !(first.X == second.X || first.Y == second.Y) ) // checks if the wall is axis-aligned.
            {
                throw new Exception("Creation of Wall failed: Invalid orientation");
            }else if( (first.X - second.X) % 50 != 0) // if the wall length is not a multiple of 50, it will correct the length.
            {
                double variance = (first.X - second.X) / 50;
                int correctedLength = (int)variance * 50;
                this.p1 = first;
                second.X = first.X + correctedLength;
                this.p2 = second;
            }else if( (first.Y - second.Y) % 50 != 0)
            {
                double variance = (first.Y - second.Y) / 50;
                int correctedLength = (int)variance * 50;
                this.p1 = first;
                second.X = first.Y + correctedLength;
                this.p2 = second;

            }
        }

        /// <summary>
        /// Json Serialization constructor for a Wall.
        /// </summary>
        public Wall()
        {

        }
        /// <summary>
        /// Draw method for a Wall. This method is only called from the View when it decides to draw the object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DrawAsync(Canvas2DContext context)
        {
            await context.SetFillStyleAsync("brown");
            await context.FillRectAsync(p1.X-25, p1.Y-25, p2.X- p1.X+50, p2.Y - p1.Y + 50);
        }

    }
}
