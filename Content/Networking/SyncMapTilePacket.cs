using System;
using System.IO;
using Terraria;
using Terraria.Map;

namespace MultiplayerTweaks.Content.Networking;
internal class SyncMapTilePacket : Packet
{
    public int x;
    public int y;
    public byte light;

    public override void ReadData(BinaryReader reader)
    {
        x = reader.ReadInt32();
        y = reader.ReadInt32();
        light = reader.ReadByte();
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
        writer.Write(light);
    }

    public override void Recieve(int? fromWho)
    {
        if (Main.Map[x, y].Light < light)
            UpdateMapLighting(x, y, light);
    }

    private void UpdateMapLighting(int x, int y, byte light)
    {
        var other = Main.Map[x, y];
        if (light == 0 && other.Light == 0)
            return;

        var mapTile = MapHelper.CreateMapTile(x, y, Math.Max(other.Light, light));
        if (mapTile.Equals(ref other))
            return;

        Main.Map.SetTile(x, y, ref mapTile);
        return;
    }
}