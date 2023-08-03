#pragma warning disable CS0649
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MultiplayerTweaks.Configuration;
public class Config : ModConfig
{
    public static Config Instance => ModContent.GetInstance<Config>();
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(AutoTeamMode.Red)]
    [DrawTicks]
    public AutoTeamMode AutoTeam;

    [DefaultValue(SharedMapMode.Team)]
    [DrawTicks]
    public SharedMapMode SharedMap;

    [DefaultValue(TeamSpectateMode.BossFightDeath)]
    [DrawTicks]
    public TeamSpectateMode TeamSpectate;

    [DefaultValue(true)]
    public bool SpectateOnlyTeamPlayers;

    [DefaultValue(true)]
    public bool SpectateBosses;

    [DefaultValue(true)]
    public bool NoBossCheese;

    [DefaultValue(true)]
    public bool GlobalTeamInfoAccs;

    [DefaultValue(true)]
    public bool MultiplayerChests;

    [DefaultValue(true)]
    public bool TeamPersonalStorages;

    // TODO: Voice chat

    public enum AutoTeamMode
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
        Pink
    }

    public enum SharedMapMode
    {
        Disabled,
        Team,
        Global,
    }

    public enum TeamSpectateMode
    {
        Disabled,
        BossFightDeath,
        Death,
        Always,
    }
}
