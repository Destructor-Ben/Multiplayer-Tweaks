using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MultiplayerTweaks.Content.UI;
internal class UISpectate : UIState
{
    private UIPanel panel;
    private UIText targetName;
    private UIImage targetHead;

    private UIImageButton previousTargetButton;
    private UIImageButton nextTargetButton;
    private UIImageButton stopSpectatingButton;

    private Player playerTarget = null;

    public override void OnInitialize()
    {
        // Panel
        // TODO improve positioning of all elements
        panel = new UIPanel
        {
            Height = { Pixels = 125 },
            Width = { Pixels = 250 },
            Top = { Pixels = -80 },
            HAlign = 0.5f,
            VAlign = 1f
        };
        Append(panel);

        // Target info
        targetName = new UIText(Language.GetTextValue("Mods.MultiplayerTweaks.NotSpectating"))
        {
            HAlign = 0.5f,
            VAlign = 0f
        };
        panel.Append(targetName);

        targetHead = new UIImage(TextureAssets.NpcHead[0])
        {
            HAlign = 0.5f,
            VAlign = 0.75f,
            ImageScale = 1.5f
        };
        panel.Append(targetHead);

        // Previous target button
        previousTargetButton = new UIImageButton(ModContent.Request<Texture2D>("MultiplayerTweaks/Assets/PreviousButton", AssetRequestMode.ImmediateLoad))
        {
            HAlign = 0f,
            VAlign = 0.5f
        };
        previousTargetButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            MultiplayerSystem.PreviousTarget();
        };
        panel.Append(previousTargetButton);

        // Next target button
        nextTargetButton = new UIImageButton(ModContent.Request<Texture2D>("MultiplayerTweaks/Assets/NextButton", AssetRequestMode.ImmediateLoad))
        {
            HAlign = 1f,
            VAlign = 0.5f
        };
        nextTargetButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            MultiplayerSystem.NextTarget();
        };
        panel.Append(nextTargetButton);

        // Stop spectating button
        stopSpectatingButton = new UIImageButton(ModContent.Request<Texture2D>("MultiplayerTweaks/Assets/StopButton", AssetRequestMode.ImmediateLoad))
        {
            HAlign = 0.5f,
            VAlign = 1f
        };
        stopSpectatingButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            MultiplayerSystem.Target = null;
        };
        panel.Append(stopSpectatingButton);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Reset target ingo on world join
        if (targetName.Text is "" or "Mods.MultiplayerTweaks.NotSpectating")
            UpdateTargetInfo();

        // TODO fancy transition animation
        if (!MultiplayerSystem.CanSpectate())
            return;

        // Tooltips
        if (previousTargetButton.IsMouseHovering)
            Main.instance.MouseText(Language.GetTextValue("Mods.MultiplayerTweaks.PreviousTarget"));
        if (nextTargetButton.IsMouseHovering)
            Main.instance.MouseText(Language.GetTextValue("Mods.MultiplayerTweaks.NextTarget"));
        if (stopSpectatingButton.IsMouseHovering)
            Main.instance.MouseText(Language.GetTextValue("Mods.MultiplayerTweaks.StopSpectating"));

        base.Draw(spriteBatch);

        // TODO - improve player head drawing
        if (playerTarget != null)
            Main.PlayerRenderer.DrawPlayerHead(Main.Camera, playerTarget, targetHead.GetDimensions().Center(), scale: 1.5f);
    }

    public void UpdateTargetInfo()
    {
        // Initializing target name and head texture
        string name = "";
        Asset<Texture2D> headTexture = null;
        playerTarget = null;

        // Self
        if (MultiplayerSystem.Target == null)
        {
            name = Language.GetTextValue("Mods.MultiplayerTweaks.NotSpectating");
            headTexture = TextureAssets.NpcHead[0];
        }
        // Player
        else if (MultiplayerSystem.Target is Player player)
        {
            name = player.name;
            playerTarget = player;
        }
        // Boss
        else if (MultiplayerSystem.Target is NPC npc && npc.boss)
        {
            name = npc.FullName;

            int headIndex = npc.GetBossHeadTextureIndex();
            if (headIndex != -1)
                headTexture = TextureAssets.NpcHeadBoss[headIndex];
        }

        targetName.SetText(name);
        if (headTexture != null)
        {
            targetHead.Color = Color.White;
            targetHead.SetImage(headTexture);
        }
        else
        {
            targetHead.Color = Color.Transparent;
        }
    }
}
