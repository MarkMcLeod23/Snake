using Blazor.Extensions.Canvas.Canvas2D;
// Editted Benjamin Westlake Nov 22
namespace GUI.Client.Models
{
    /// <summary>
    /// This is the interface that all the Drawable Game objects will use
    /// 
    /// </summary>
    public interface Drawable
    {
        /// <summary>
        /// The Draw Method to implement
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task DrawAsync(Canvas2DContext context);
    }
}
