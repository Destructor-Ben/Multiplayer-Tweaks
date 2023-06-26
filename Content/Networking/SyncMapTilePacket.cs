using Terraria.Map;
using TerraUtil.Networking;

namespace MultiplayerTweaks.Content.Networking;
internal class SyncMapTilePacket : Packet
{
    public int x;
    public int y;
    public byte light;

    public override void Handle(int? fromWho)
    {
        if (Main.Map[x, y].Light < light)
            UpdateMapLighting(x, y, light);
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
        writer.Write(light);
    }

    public override void Deserialize(BinaryReader reader)
    {
        x = reader.ReadInt32();
        y = reader.ReadInt32();
        light = reader.ReadByte();
    }

    private static void UpdateMapLighting(int x, int y, byte light)
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