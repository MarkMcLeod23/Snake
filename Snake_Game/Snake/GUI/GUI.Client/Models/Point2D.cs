namespace GUI.Client.Models
{
    /// <summary>
    /// Simple Point class that represents a location in 2d  World space.
    /// </summary>
    public class Point2D
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Default constructor for a Point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Json Serialization Constructor for a Point.
        /// </summary>
        public Point2D()
        {

        }
    }
}
