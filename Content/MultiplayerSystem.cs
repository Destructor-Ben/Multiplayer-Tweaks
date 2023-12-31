using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using MultiplayerTweaks.Content.Networking;
using MultiplayerTweaks.Content.UI;
using ReLogic.Graphics;
using Terraria.Map;
using Terraria.UI;

namespace MultiplayerTweaks.Content;
internal class MultiplayerSystem : ModSystem
{
    // TODO: add spectating during boss fights
    // TODO: refactor
    public static MultiplayerSystem Instance => ModContent.GetInstance<MultiplayerSystem>();

    public static bool MultiplayerChests => Config.Instance.MultiplayerChests;
    public static bool NoBossCheese => /*Util.IsMultiplayer && */Config.Instance.NoBossCheese && Main.CurrentFrameFlags.AnyActiveBossNPC;

    public static bool IsSpectating => Target != null;
    public static bool CanSpectate => /*Util.IsMultiplayer &&*/ Config.Instance.TeamSpectate switch// TODO: temporary
    {
        Config.TeamSpectateMode.Disabled => false,
        Config.TeamSpectateMode.BossFightDeath => Main.LocalPlayer.dead && Main.CurrentFrameFlags.AnyActiveBossNPC,
        Config.TeamSpectateMode.Death => Main.LocalPlayer.dead,
        Config.TeamSpectateMode.Always => true,
        _ => true
    };

    public static Entity Target
    {
        get => _target;
        set
        {
            _target = value;
            UISpectate.Instance.UpdateTargetInfo();
        }
    }
    private static Entity _target = null;

    public static ModKeybind StopSpectatingKey;
    public static ModKeybind NextTargetKey;
    public static ModKeybind PreviousTargetKey;

    // TODO: move animations to util
    private static float respawnMenuAnimationSpeed = 0.03f;
    private static float respawnMenuAnimationAmount = 0f;

    public override void Load()
    {
        // Hotkeys
        if (!Util.IsHeadless)
        {
            StopSpectatingKey = KeybindLoader.RegisterKeybind(Mod, "StopSpectating", Keys.RightAlt);
            NextTargetKey = KeybindLoader.RegisterKeybind(Mod, "NextTarget", Keys.OemPeriod);
            PreviousTargetKey = KeybindLoader.RegisterKeybind(Mod, "PreviousTarget", Keys.OemComma);
        }

        // Global info accessories
        IL_Player.RefreshInfoAccsFromTeamPlayers += delegate (ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                c.GotoNext(MoveType.After, i => i.MatchLdcI4(800));
                c.EmitDelegate(delegate (int originalDistance)
                {
                    return Util.IsMultiplayer && Config.Instance.GlobalTeamInfoAccs ? int.MaxValue : originalDistance;
                });
            }
            catch (Exception e)
            {
                throw new ILPatchFailureException(Mod, il, e);
            }
        };

        // Multiple players can use a chest
        On_Chest.UsingChest += delegate (On_Chest.orig_UsingChest orig, int i)
        {
            return MultiplayerChests ? -1 : orig(i);
        };

        On_Chest.IsPlayerInChest += delegate (On_Chest.orig_IsPlayerInChest orig, int i)
        {
            return !MultiplayerChests && orig(i);
        };

        // TODO: sync chests while they are open
        On_ChestUI.Draw += delegate (On_ChestUI.orig_Draw orig, SpriteBatch spritebatch)
        {
            orig(spritebatch);
            if (MultiplayerChests)
                NetMessage.SendData(MessageID.SyncPlayerChest, number: Main.LocalPlayer.chest);
        };

        // Replacing the respawn counter with a message when a boss is active
        // Changing the respawn UI height if the player is spectating
        // Fixing a bug with the respawn counter drawing
        IL_Main.DrawInterface_35_YouDied += delegate (ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // Custom respawn text
                c.GotoNext(MoveType.Before, i => i.MatchStloc(3));
                c.EmitDelegate(delegate (string originalValue)
                {
                    return NoBossCheese ? Util.GetTextValue("UI.RespawnText") : originalValue;
                });

                // Fixing the centering, since the wrong font is used to check the size (MouseText instead of DeathText)
                c.GotoNext(MoveType.After, i => i.MatchLdsfld(typeof(FontAssets).GetField("MouseText")));
                c.EmitDelegate(delegate (Asset<DynamicSpriteFont> originalValue)
                {
                    return NoBossCheese ? FontAssets.DeathText : originalValue;
                });

                // Changing the height of the menu
                c.Index = 0;
                c.GotoNext(MoveType.Before, i => i.MatchStloc(0));
                c.EmitDelegate(delegate (float originalValue)
                {
                    return GetCustomRespawnMenuHeight(originalValue);
                });

            }
            catch (Exception e)
            {
                throw new ILPatchFailureException(Mod, il, e);
            }
        };

        // Stopping map from being revealed if spectating
        // Syncing world map with other players
        // TODO: finish map syncing
        On_WorldMap.UpdateLighting += delegate (On_WorldMap.orig_UpdateLighting orig, WorldMap self, int x, int y, byte light)
        {
            if (IsSpectating)
                return false;

            bool changedMap = orig(self, x, y, light);
            if (changedMap && Util.IsMultiplayer)
            {
                new SyncMapTilePacket
                {
                    x = x,
                    y = y,
                    light = light,
                }.SendToAllClients();
            }

            return changedMap;
        };
    }

    public override void Unload()
    {
        StopSpectatingKey = null;
        NextTargetKey = null;
        PreviousTargetKey = null;
    }

    // Animations
    public override void PostUpdateEverything()
    {
        // Respawn menu
        int direction = IsSpectating && Main.LocalPlayer.dead ? -1 : 1;
        respawnMenuAnimationAmount += direction * respawnMenuAnimationSpeed;
        respawnMenuAnimationAmount = MathHelper.Clamp(respawnMenuAnimationAmount, 0f, 1f);

        // TODO: temporary
        // TODO fancy transition animation
        UISpectate.Instance.Visible = CanSpectate;
    }

    public static float GetCustomRespawnMenuHeight(float originalValue)
    {
        return MathHelper.Lerp(-Main.screenHeight / 2 + 100, originalValue, Util.Smootherstep(respawnMenuAnimationAmount));
    }

    // Team spectate
    public static void NextTarget()
    {
        ChangeTarget(1);
    }

    public static void PreviousTarget()
    {
        ChangeTarget(-1);
    }

    public static void ChangeTarget(int changeAmount)
    {
        bool dontChangeTarget = false;

        // If there are no targets, then reset it
        var targets = GetPossibleSpectatorTargets();
        if (targets.Count == 0)
            dontChangeTarget = true;

        // If the current target isn't in the list, reset it
        int index = targets.FindIndex(e => e == Target);
        if (index == -1 && Target != null)
            dontChangeTarget = true;

        // Resetting the target
        if (dontChangeTarget)
        {
            Target = null;
            return;
        }

        // Changing the target
        int newIndex = Math.Clamp(index + changeAmount, 0, targets.Count - 1);
        Target = targets[newIndex];
    }

    public static List<Entity> GetPossibleSpectatorTargets()
    {
        var targets = new List<Entity>();

        foreach (var player in Main.player)
        {
            if (player.active && !player.dead && player.whoAmI != Main.myPlayer)
            {
                if (player.team != Main.LocalPlayer.team && Config.Instance.SpectateOnlyTeamPlayers)
                    continue;

                targets.Add(player);
            }
        }

        if (Config.Instance.SpectateBosses)
        {
            foreach (var npc in Main.npc)
            {
                if (npc.active && npc.boss)
                    targets.Add(npc);
            }
        }

        return targets;
    }

    public override void ModifyScreenPosition()
    {
        if (!CanSpectate)
            Target = null;

        // Spectating hotkeys
        if (StopSpectatingKey.JustPressed)
            Target = null;

        if (NextTargetKey.JustPressed)
            NextTarget();

        if (PreviousTargetKey.JustPressed)
            PreviousTarget();

        // Checking if the target died or left
        if (!(Target?.active ?? false))
            Target = null;

        // Moving the screen
        // TODO fancy transtion animation
        if (Target != null)
            Main.screenPosition = Target.Center - Util.ScreenSize / 2;
    }
}