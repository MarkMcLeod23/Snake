namespace GUI.Client.Models
{
    public class ControlCommand
    {
        public string moving { get; set; }

        public ControlCommand()
        {

        }
        public ControlCommand(string direction)
        {
            moving = direction;
        }
    }
}
