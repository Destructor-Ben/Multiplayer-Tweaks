namespace MultiplayerTweaks.Content;
internal class MultiplayerPlayer : ModPlayer
{
    public bool IsMyPlayer => Player.whoAmI == Main.myPlayer;

    public override void OnEnterWorld()
    {
        // Auto team
        if (Util.IsMultiplayer && Config.Instance.AutoTeam != Config.AutoTeamMode.None && IsMyPlayer && Player.team == 0)
        {
            Player.team = (int)Config.Instance.AutoTeam;
            NetMessage.SendData(MessageID.PlayerTeam, number: Main.myPlayer);
        }
    }

    public override void UpdateDead()
    {
        // Don't respawn if a boss is active
        if (MultiplayerSystem.NoBossCheese)
            Player.respawnTimer = 2;
    }

    public override void PostUpdate()
    {
        // Syncing the world while spectating
        if (IsMyPlayer && Util.IsMultiplayer && Main.GameUpdateCount % 10 == 0)
        {
            // TODO: sync the world
        }
    }
}