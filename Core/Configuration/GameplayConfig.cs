using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace NoxusBoss.Core.Configuration;

public class GameplayConfig : ModConfig
{
    public static GameplayConfig Instance => ModContent.GetInstance<GameplayConfig>();

    public override ConfigScope Mode => ConfigScope.ServerSide;

    [BackgroundColor(94, 30, 64, 216)]
    [DefaultValue(false)]
    public bool AllowSeedOfWillAnywhere { get; set; }
}
