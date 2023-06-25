using MultiplayerTweaks.Content.Networking;

namespace MultiplayerTweaks;
internal class MultiplayerTweaks : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        NetworkSystem.HandlePacket(reader, whoAmI);
    }
}