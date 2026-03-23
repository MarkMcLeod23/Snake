// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// Editted Benjamin Westlake Nov 24
// </copyright>

using CS3500.Networking;
using System.Text;

namespace CS3500.Chatting;

/// <summary>
///   A Driver class that runs and keeps open a BetterChatServer
/// </summary>
public partial class ChatServer
{
    List<NetworkConnection> connections = new List<NetworkConnection>();
    List<string> channels = new List<string>();
    /// <summary>
    ///   The main program.Basically just a driver for BetterChatServer.start
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    private static void Main( string[] args )
    {
        new BetterChatServer().Start();
        Console.Read(); // don't stop the program.
    }
}
/// <summary>
/// This class replaces what was Chat Server it basically does the same thing but doesn't have to have 
/// methods limited to being static since they aren't run from main so they can actually access 
/// have the handle connection threads send information outside of it's method
/// </summary>
public class BetterChatServer()
{
    /// <summary>
    /// A list of all the Network connections that have connected
    /// </summary>
    List<NetworkConnection> connections = new List<NetworkConnection>();
    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    public void Start()
    {
        Server.StartServer(HandleConnect, 11_001);
    }
    /// <summary>
    /// Broadcasts the message to all connections that have connected
    /// </summary>
    /// <param name="message"></param>
    private void BroadcastMessage(string message)
    {
        lock (this)
        {
            foreach (NetworkConnection connection in connections)
            {
                connection.Send(message);
            }
        }
    }
    /// <summary>
    ///   <pre>
    ///     When a new connection is established, enter a loop that receives from and
    ///     replies to a client. once 
    ///   </pre>
    /// </summary>
    ///
    private void HandleConnect(NetworkConnection connection)
    {
        // handle all messages until disconnect.
        try
        {
            connection.Send("123");
            var name = connection.ReadLine();
            lock (this)
            {
                connections.Add(connection);
            }
            //BroadcastMessage(name + " Connected to Chat");
            while (true)
            {
                var message = connection.ReadLine();
                //BroadcastMessage(name +": "+message );
                BroadcastMessage(message);
            }
        }
        catch (Exception)
        {
            lock(this)
            {
                connections.Remove(connection);
            }
        }
    }
}