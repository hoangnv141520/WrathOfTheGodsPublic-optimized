using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Configuration;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using NoxusBoss.Core.World.Subworlds;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.SummonItems;

public class SeedOfWill : ModItem
{
    public override string Texture => GetAssetPath("Content/Items/SummonItems", Name);

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = 10;
        Item.height = 10;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = null;
        Item.value = 0;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
    }

    public override bool CanUseItem(Player player)
    {
        if (NPC.AnyNPCs(ModContent.NPCType<NamelessDeityBoss>()))
            return false;

        if (GameplayConfig.Instance.AllowSeedOfWillAnywhere)
            return true;

        return EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            // If the player is not in multiplayer, spawn Nameless.
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X - 400, (int)player.Center.Y, ModContent.NPCType<NamelessDeityBoss>(), 1);

            // If the player is in multiplayer, request a boss spawn.
            else
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: ModContent.NPCType<NamelessDeityBoss>());

            TestOfResolveSystem.IsActive = true;
            PacketManager.SendPacket<TestOfResolvePacket>();
        }

        return true;
    }
}
