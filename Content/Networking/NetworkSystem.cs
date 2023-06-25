using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace MultiplayerTweaks.Content.Networking;
internal class NetworkSystem : ModSystem
{
    public static NetworkSystem Instance => ModContent.GetInstance<NetworkSystem>();

    public static Dictionary<int, Type> PacketTypes
    {
        get
        {
            _packetTypes ??= new Dictionary<int, Type>();
            return _packetTypes;
        }
    }
    private static Dictionary<int, Type> _packetTypes;
    public static int PacketTypeCount = 0;

    public static void HandlePacket(BinaryReader reader, int senderID)
    {
        // Reading data
        int type = reader.ReadInt32();
        int toWho = reader.ReadInt32();
        int fromWho = reader.ReadInt32();

        // Handling packet
        if (PacketTypes.TryGetValue(type, out var packetType))
        {
            var packet = Activator.CreateInstance(packetType) as Packet;
            packet.ReadData(reader);

            if (toWho == Main.myPlayer)
                packet.Recieve(fromWho);
        }
        else
        {
            Instance.Mod.Logger.Warn("Unknown Packet Type: " + type);
        }
    }
}
