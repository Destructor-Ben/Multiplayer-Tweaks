using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MultiplayerTweaks.Content.Networking;
internal abstract class Packet : ModType
{
    public int Type { get; private set; }

    /// <summary>
    /// Called when the packet is recieved
    /// </summary>
    /// <param name="fromWho">The player ID of the sender.<br/>Null if the packet was from the server.</param>
    public abstract void Recieve(int? fromWho);

    /// <summary>
    /// Allows a packet to read data when recieved.
    /// </summary>
    /// <param name="reader"></param>
    public abstract void ReadData(BinaryReader reader);

    /// <summary>
    /// Allows a packet to write data before it is sent.
    /// </summary>
    /// <param name="writer"></param>
    public abstract void WriteData(BinaryWriter writer);

    protected sealed override void Register()
    {
        NetworkSystem.PacketTypes.Add(NetworkSystem.PacketTypeCount, GetType());
        NetworkSystem.PacketTypeCount++;
    }

    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }

    private void Send(int toWho = -1, int fromWho = -1, int ignoreWho = -1)
    {
        // Writing the basic packet info
        var packet = Mod.GetPacket();
        packet.Write(Type);
        packet.Write(toWho);
        packet.Write(fromWho);

        // Writing the data
        WriteData(packet);

        // Sending the packet
        packet.Send(toWho, ignoreWho);
    }

    // Sends to the player, works on server and client
    public void SendToPlayer(int playerID)
    {
        if (Main.netMode == NetmodeID.Server)
        {

        }
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {

        }
    }

    // Sends to the listed players, works on server and client
    public void SendToPlayers(params int[] playerIDs)
    {
        if (Main.netMode == NetmodeID.Server)
        {

        }
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {

        }
    }

    /// <summary>
    /// Sends the packet to all players.<br/>
    /// Works on the server and client.
    /// </summary>
    public void SendToAllPlayers()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1);
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            Send(-1, Main.myPlayer);
        }
    }

    /// <summary>
    /// Send the packet to the server.<br/>
    /// Only works on the client.
    /// </summary>
    public void SendToServer()
    {
        if (Main.netMode is NetmodeID.Server or NetmodeID.SinglePlayer)
            return;

        Send(-1, Main.myPlayer);
    }
}