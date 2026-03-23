using Blazor.Extensions.Canvas.Canvas2D;
using CS3500.Networking;
using GUI.Client.Models;
using Microsoft.JSInterop;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
// Editted Benjamin Westlake Nov 22
namespace GUI.Client.Controllers
{
    /// <summary>
    /// This is the controller for the snake program it's responsible to 
    /// handle connection with the server 
    /// contain the game world and updates it when updated by server
    /// Written By Benjamin Westlake and Mark McLeod
    /// </summary>
    public class NetworkController : Drawable
    {
        

        NetworkConnection link; // holds the network connection object that sends and receives data

        private int clientID; // the ID of the client so its own snake can be identified

        World world = new World(); // the instance of the world that holds all the information to draw

        private bool controlReleased = false; // witholds the client from sending commands until the connection is ready

        JsonSerializerOptions jsonOptions; // makes sure objects are serialized properly

        public const string connectionString = "server=atr.eng.utah.edu;" + "database=u1412189;" + "uid=u1412189;" + "password=rankings";

        MySqlConnection sqlconn; // The connection to the High Score Database
        MySqlCommand sqlcommand; // The command object that sends queries to the Database
        private int gameID; // This is the current Game ID (from the database) of the active game.

        private Thread? listening;



        /// <summary>
        /// default constructor
        /// </summary>
        public NetworkController()
        {
            link = new NetworkConnection();
            jsonOptions = new JsonSerializerOptions // make sure the Models are serialized properly
            {
                IncludeFields = true,
            };
        }
        /// <summary>
        /// simply check if the server is connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return link.IsConnected;
        }
        /// <summary>
        /// Trys to connect to a server of given ip port and names player as given
        /// if it fails throw a exception (view should handle it)
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="serverPort"></param>
        /// <param name="playerName"></param>
        /// <exception cref="Exception"></exception>
        public void TryConnect(string serverName, string serverPort ,string playerName)
        {
            try
            {
                int portNum;
                if (!int.TryParse(serverPort, out portNum)) // check to make sure the user input is a valid port number
                {
                    throw new Exception("Port could not be found or is invalid.");
                }

                link.Connect(serverName, portNum); // player name must be <= 16 characters
                if (playerName.Length > 16) { throw new Exception("Player name is too long."); }

                link.Send(playerName); // send the server the player name.
                string playId = link.ReadLine();
                if (!int.TryParse(playId, out clientID)) // receive the player ID from the server.
                {
                    throw new Exception("Player ID not correctly received.");
                }

                int worldsize;
                if (!int.TryParse(link.ReadLine(), out worldsize)) // receive the world size from the server.
                {
                    throw new Exception("World size not correctly received.");
                }
                this.world = new World(worldsize); // set up a world object.

                SetupDB();

                DateTime rightnow = DateTime.Now;
                string startTime = rightnow.ToString("yyyy-MM-dd H:mm:ss"); // prepare to communicate with the High Score Database.
                sqlcommand.CommandText = "insert into Games (Start) values(\"" + startTime + "\");";
                sqlcommand.ExecuteNonQuery();

                sqlcommand.CommandText = "select ID from Games where Start = \"" + startTime + "\";";
                using (MySqlDataReader reader = sqlcommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gameID = (int)reader["ID"];
                    }
                    reader.Close();
                }
                sqlcommand.CommandText = "insert into Players (ID, Name, Encountered, AssociatedGameID) " +
                    "values(" + playId + ", \"" + playerName + "\", \"" + startTime + "\", " + gameID + ");";
                sqlcommand.ExecuteNonQuery();


                listening = new Thread(HandleReceivedData); // once world is set up, break a new thread off to listen for server updates.
                listening.Start();
                controlReleased = true;
            }
            catch (Exception e) // if any sort of exception is thrown, simple handle it by severing the connection.
            {
                Disconnect();
                throw e;
            }
        }

        /// <summary>
        /// This method sets up the connection between the client and the High Score Database.
        /// </summary>
        public void SetupDB()
        {
            // Connect to the DB
            sqlconn = new MySqlConnection(connectionString);
            try
            {
                // Open a connection
                sqlconn.Open();

                // Create a command object
                sqlcommand = sqlconn.CreateCommand();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }



        /// <summary>
        /// Disconnect from the server. This method is used in tandem with methods in the View to break the connection.
        /// </summary>
        public void Disconnect()
        {
            link = new NetworkConnection();
            controlReleased = false; // make sure user inputs are disregarded (since there's nowhere to send them)
            DateTime rightnow = DateTime.Now;
            string endTime = rightnow.ToString("yyyy-MM-dd H:mm:ss");

            MySqlConnection sqlconn2 = new MySqlConnection(connectionString);
            try
            {
                // Open a connection
                sqlconn2.Open();

                // Create a command object
                MySqlCommand sqlcommand2 = sqlconn2.CreateCommand();

                sqlcommand2.CommandText = "update Games set End = \"" + endTime + "\" where ID = " + gameID + ";";
                sqlcommand2.ExecuteNonQuery();
                sqlcommand2.CommandText = "update Players set Departed = \"" + endTime + "\" where Departed is null and AssociatedGameID = " + gameID + ";";
                sqlcommand2.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            sqlconn2.Dispose();
            sqlconn.Dispose();
            world = new World();
        }
        /// <summary>
        /// This method handles issuing control commands to the server. This method is called from the View when a key on the 
        /// keyboard is pressed. The string parameter must be either "left", "right", "up", or "down" for the server to
        /// recognize it as a valid control command. This parameter restriction is handled by the View.
        /// </summary>
        /// <param name="key"></param>
        public void KeyPressed(string key)
        {

            ControlCommand commandObj = new ControlCommand(key); // the key string must be wrapped in a ControlCommand object to serialize properly.
            string command = JsonSerializer.Serialize(commandObj, jsonOptions);
            if (controlReleased) // Not all key presses should be treated as control commands, such as typing in your name before connecting.
            {
                link.Send(command);
            }
        }

        /// <summary>
        /// This method is the handler for receiving and translating real-time data from the server. This method is called in a separate thread.
        /// Constantly look for new information, decide what kind of object the Json line represents, and deserialize it into an object.
        /// Then, it updates the world state with the new info. Since the world state and its contents are considered a race condition,
        /// the lock is handled in the Model classes.
        /// </summary>
        public void HandleReceivedData()
        {
            string currentLine;
            while (link.IsConnected) // The thread will listen for new data infinitely as long as the connection is intact.
            {
                currentLine= link.ReadLine();
                if (currentLine.Contains("snake")) // Filter Snake objects first. This avoids bugs arising from players inputing weird names.
                {
                    var newSnake = JsonSerializer.Deserialize<Snake>(currentLine, jsonOptions);
                    bool preexisting = this.world.UpdatePlayer(newSnake);
                    if (controlReleased) // prevents sql reader interference when disconnected
                    {
                        CheckDB(newSnake, preexisting);
                    }
                }
                else if (currentLine.Contains("wall")) // Filter for Wall objects
                {
                    var newWall = JsonSerializer.Deserialize<Wall>(currentLine, jsonOptions);
                    this.world.UpdateWall(newWall);
                }
                else if (currentLine.Contains("power")) // Last, filter for Powerup objects.
                {
                    var newPowerUp = JsonSerializer.Deserialize<Powerup>(currentLine, jsonOptions);
                    this.world.UpdatePowerUp(newPowerUp);
                }
            }
            sqlconn.Close();
            sqlconn.Dispose();
        }

        /// <summary>
        /// This method updates the High Score Database in real time as the game is played. This method is called
        /// whenever the client receives information about a snake.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="preexisting"></param>
        public void CheckDB(Snake player , bool preexisting)
        {
            if (player.joined) // if this snake has not been seen by the client before, make a new row in the DB
            {
                DateTime rightnow = DateTime.Now;
                string firstSeen = rightnow.ToString("yyyy-MM-dd H:mm:ss");
                sqlcommand.CommandText = "insert into Players (ID, Name, HiScore, Encountered, AssociatedGameID) " +
                    "values(" + player.snake + ", \"" + player.name + "\", " + player.hiscore + ", \"" + firstSeen + "\", " +
                    gameID + ");";
                sqlcommand.ExecuteNonQuery();
            }
            else if (player.dc | player.died)  // if this is the last time the client will see this snake, update hiscore and the last seen time
            {
                DateTime rightnow = DateTime.Now;
                string lastSeen = rightnow.ToString("yyyy-MM-dd H:mm:ss");
                sqlcommand.CommandText = "update Players set HiScore = " + player.hiscore + " where ID = " + player.snake + " and AssociatedGameID = " + gameID + ";";
                sqlcommand.ExecuteNonQuery();
                sqlcommand.CommandText = "update Players set Departed = \"" + lastSeen + "\" where ID = " + player.snake + " and AssociatedGameID = " + gameID + ";";
                sqlcommand.ExecuteNonQuery();
            }
            else if (!player.dc) // if this snake has already been logged, update the hiscore as the game progresses
            {
                sqlcommand.CommandText = "update Players set HiScore = " + player.hiscore + " where ID = " + player.snake + " and AssociatedGameID = " + gameID + ";";
                sqlcommand.ExecuteNonQuery();
            }
        }





        /// <summary>
        /// the x position of the players snakes head in world space
        /// </summary>
        /// <returns></returns>
        public int PlayerX()
        {
            int xlocation;
            Snake player = new Snake();
            if (!this.world.players.TryGetValue(clientID, out player))
            {
                return 0; // default value if search fails
            }
            else
            {
                xlocation = player.body[player.body.Count - 1].X;
                return xlocation;
            }
        }
        /// <summary>
        /// the y position of the players snakes head in world space
        /// </summary>
        /// <returns></returns>
        public int PlayerY()
        {
            int xlocation;
            Snake player = new Snake();
            if(!this.world.players.TryGetValue(clientID, out player))
            {
                return 0; // default value if search fails
            }
            else
            {
                xlocation = player.body[player.body.Count - 1].Y;
                return xlocation;
            }
        }
        /// <summary>
        /// Gives the view the worldsize 
        /// </summary>
        /// <returns></returns>
        public int WorldSize()
        {
            return this.world.size;
        }
        /// <summary>
        /// Should basically just call draw on the world
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task DrawAsync(Canvas2DContext context)
        {
            //Debug.WriteLine("Drawing");
            await context.SetFillStyleAsync("lightgreen");
            this.world.DrawAsync(context); //basically just call draw for the world
        }
    }
}
