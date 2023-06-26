using Terraria.UI;

namespace MultiplayerTweaks.Content.UI;
internal class UISystem : ModSystem
{
    public static UISystem Instance => ModContent.GetInstance<UISystem>();

    public static UserInterface SpectateUI;
    public static UISpectate SpectateUIState;
    public static GameTime UIGameTime;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        SpectateUIState = new UISpectate();
        SpectateUIState.Activate();
        SpectateUI = new UserInterface();
        SpectateUI.SetState(SpectateUIState);
    }

    public override void Unload()
    {
        SpectateUI = null;
        SpectateUIState = null;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (index == -1)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer(
            "MultiplayerTweaks: SpectateUI",
            delegate
            {
                if (UIGameTime != null)
                    SpectateUI.Draw(Main.spriteBatch, UIGameTime);
                return true;
            },
            InterfaceScaleType.UI));
    }

    public override void UpdateUI(GameTime gameTime)
    {
        UIGameTime = gameTime;
        SpectateUI.Update(gameTime);
    }
}
