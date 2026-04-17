using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Configuration;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar;

public static class AvatarDimensionVariants
{
    /// <summary>
    /// The Avatar's dark dimension. Creates a pitch black background, with no extra elements.
    /// </summary>
    public static readonly AvatarDimensionVariant DarkDimension = new AvatarDimensionVariant(DrawPitchBlackBackground, null, false);

    /// <summary>
    /// The Avatar's universal annihilation dimension. Similar to the dark dimension, except with expanding stars in the background.
    /// </summary>
    public static readonly AvatarDimensionVariant UniversalAnnihilationDimension = new AvatarDimensionVariant(DrawUniversalAnnihilationBackground, null, false);

    /// <summary>
    /// The Avatar's fog dimension. Has a near-identical look to the fog used during the Rift Eclipse.
    /// </summary>
    public static readonly AvatarDimensionVariant FogDimension = new AvatarDimensionVariant(DrawFogBlackBackground, null, false);

    /// <summary>
    /// The Avatar's antishadow dimension. Turns the background red and everything else black, for a high contrast visual.
    /// </summary>
    public static readonly AvatarDimensionVariant AntishadowDimension = new AvatarDimensionVariant(DrawAntishadowBackground, null, true);

    /// <summary>
    /// The Avatar's cryonic dimension. Turns the background into a swirling, windy zone with cold colors.
    /// </summary>
    public static readonly AvatarDimensionVariant CryonicDimension = new AvatarDimensionVariant(DrawCryonicBackground, GennedAssets.Sounds.Avatar.CryostasisAmbientLoop, false);

    /// <summary>
    /// The Avatar's visceral dimension. Turns the background into a bloody whirlpool with a fountain in the center.
    /// </summary>
    public static readonly AvatarDimensionVariant VisceralDimension = new AvatarDimensionVariant(DrawVisceralBackground, GennedAssets.Sounds.Avatar.BloodVortexAmbientLoop, false);

    private static void DrawPitchBlackBackground()
    {
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea * 2f;
        Vector2 drawPosition = screenArea * 0.5f;
        Color color = Color.Black;

        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, color, 0f, WhitePixel.Size() * 0.5f, textureArea / WhitePixel.Size(), 0, 0f);
    }

    private static void DrawUniversalAnnihilationBackground()
    {
        if (AvatarOfEmptiness.Myself is null)
            return;

        if (CosmicBackgroundSystem.KalisetFractal is null)
            return;

        float maxDimension = MathF.Max(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
        Vector2 screenArea = Vector2.One * maxDimension;
        Vector2 textureArea = screenArea * 1.1f;
        Vector2 drawPosition = screenArea * 0.5f;
        Vector3[] starPalette = AvatarOfEmptinessSky.Palettes["Stars"];

        float expandInterpolant = InverseLerp(0f, 154f, AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().AITimer - AvatarOfEmptiness.UniversalAnnihilation_StarChargeUpTime - AvatarOfEmptiness.UniversalAnnihilation_StarExplodeDelay);
        ManagedShader cosmicShader = ShaderManager.GetShader("NoxusBoss.UniversalAnnihilationStarFieldShader");
        cosmicShader.TrySetParameter("zoom", 0.133f);
        cosmicShader.TrySetParameter("brightness", 1f);
        cosmicShader.TrySetParameter("parallaxOffset", Main.screenPosition * 0.000013f);
        cosmicShader.TrySetParameter("detailIterations", (int)Lerp(11f, 18f, WoTGConfig.Instance.VisualOverlayIntensity));
        cosmicShader.TrySetParameter("starGradient", starPalette);
        cosmicShader.TrySetParameter("starGradientCount", starPalette.Length);
        cosmicShader.TrySetParameter("expandInterpolant", expandInterpolant);
        cosmicShader.TrySetParameter("avatarPosition", AvatarOfEmptiness.Myself.Center - Main.screenPosition);
        cosmicShader.SetTexture(CosmicBackgroundSystem.KalisetFractal, 1, SamplerState.AnisotropicWrap);
        cosmicShader.SetTexture(WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        cosmicShader.Apply();

        float opacity = Pow(AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().UniversalAnnihilation_BackgroundBrightnessInterpolant, 0.67f);
        if (AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState == AvatarOfEmptiness.AvatarAIType.Teleport)
            opacity = 0f;

        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, Color.White * opacity, 0f, WhitePixel.Size() * 0.5f, textureArea / WhitePixel.Size(), 0, 0f);
    }

    private static void DrawFogBlackBackground()
    {
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea * 2f;
        Vector2 drawPosition = screenArea * 0.5f;
        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, Color.Black, 0f, WhitePixel.Size() * 0.5f, textureArea / WhitePixel.Size(), 0, 0f);
    }

    private static void DrawAntishadowBackground()
    {
        // Pure-green pixels are replaced with red, while everything else is replaced with black, in the screen shader.
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea * 2f;
        Vector2 drawPosition = screenArea * 0.5f;
        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, new Color(0f, 1f, 0f), 0f, WhitePixel.Size() * 0.5f, textureArea / WhitePixel.Size(), 0, 0f);

        ManagedScreenFilter antishadowShader = ShaderManager.GetFilter("NoxusBoss.AntishadowSilhouetteShader");
        antishadowShader.TrySetParameter("silhouetteColor", Color.Black);
        antishadowShader.TrySetParameter("foregroundColor", AvatarOfEmptiness.AntishadowBackgroundColor);
        antishadowShader.Activate();
    }

    private static void DrawCryonicBackground()
    {
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea;

        float brightness = 1f;
        float vignetteBrightness = 1f;
        float vignetteAppearanceBoost = InverseLerp(1400f, 600f, Main.LocalPlayer.Distance(AvatarOfEmptiness.Myself?.Center ?? Main.LocalPlayer.Center)) * 0.15f;
        if (AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState == AvatarOfEmptiness.AvatarAIType.AbsoluteZeroOutburst)
        {
            brightness = InverseLerp(AvatarOfEmptiness.AbsoluteZeroOutburst_BackgroundDisappearEndTime, AvatarOfEmptiness.AbsoluteZeroOutburst_BackgroundDisappearStartTime, AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().AITimer);
            float bumpInterpolant = InverseLerp(
                AvatarOfEmptiness.AbsoluteZeroOutburst_LilyFreezeTime,
                AvatarOfEmptiness.AbsoluteZeroOutburst_LilyFreezeTime + AvatarOfEmptiness.AbsoluteZeroOutburst_ExplodeDelay + 120,
                AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().AITimer);
            vignetteBrightness *= InverseLerp(1f, 0.4f, bumpInterpolant);
        }
        if (AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState == AvatarOfEmptiness.AvatarAIType.AbsoluteZeroOutburstPunishment)
        {
            brightness = 0f;
            vignetteBrightness = 0f;
        }
        if (AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().PreviousState == AvatarOfEmptiness.AvatarAIType.AbsoluteZeroOutburstPunishment)
        {
            brightness = 0f;
            vignetteBrightness = 0f;
        }
        if (AvatarOfEmptiness.Myself is not null && AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState == AvatarOfEmptiness.AvatarAIType.SendPlayerToMyUniverse)
        {
            brightness = 0f;
            vignetteBrightness = 0f;
        }

        // Draw the background with a special shader.
        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.CryostasisBackgroundShader");
        backgroundShader.TrySetParameter("playerUV", Vector2.One * 0.5f);
        backgroundShader.TrySetParameter("textureSize", textureArea);
        backgroundShader.TrySetParameter("windColorA", new Vector3(1f, 0.67f, 1f) * brightness);
        backgroundShader.TrySetParameter("windColorB", new Vector3(0.03f, 0.51f, 1.1f) * brightness);
        backgroundShader.TrySetParameter("frostVignetteColorA", new Vector3(0.5f, 0.5f, 0.75f) * vignetteBrightness);
        backgroundShader.TrySetParameter("frostVignetteColorB", new Vector3(0.7f, 0.9f, 1.2f) * vignetteBrightness);
        backgroundShader.TrySetParameter("vignetteAppearanceBoost", vignetteAppearanceBoost);
        backgroundShader.TrySetParameter("vignetteCurvature", 0.56f);
        backgroundShader.TrySetParameter("time", Main.gameMenu || AvatarOfEmptiness.Myself is null ? Main.GlobalTimeWrappedHourly : AvatarOfEmptinessSky.WindTimer);
        backgroundShader.TrySetParameter("vignetteSwirlTime", Main.gameMenu || AvatarOfEmptiness.Myself is null ? Main.GlobalTimeWrappedHourly : AvatarOfEmptinessSky.CryonicWindSwirlTimer);
        backgroundShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);
        backgroundShader.SetTexture(BurnNoise, 2, SamplerState.LinearWrap);
        backgroundShader.SetTexture(HarshNoise, 3, SamplerState.LinearWrap);
        backgroundShader.Apply();

        Main.spriteBatch.Draw(MulticoloredNoise, screenArea * 0.5f, null, Color.White, 0f, MulticoloredNoise.Size() * 0.5f, textureArea / MulticoloredNoise.Size(), 0, 0f);
    }

    private static void DrawVisceralBackground()
    {
        Texture2D noise = WavyBlotchNoise.Value;
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea * 5f;
        if (textureArea.Length() > 14800f)
            textureArea = textureArea.SafeNormalize(Vector2.UnitY) * 14800f;

        Vector2 parallaxOffset = (Main.screenPosition - new Vector2(Main.maxTilesX, Main.maxTilesY) * 8f) / (new Vector2(Main.maxTilesX, Main.maxTilesY) * 16f);
        parallaxOffset *= -new Vector2(1200f, 400f) * AvatarOfEmptinessSky.SkyTarget.DownscaleFactor;

        if (Main.gameMenu || AvatarOfEmptiness.Myself is null)
            parallaxOffset = Vector2.Zero;

        Vector3[] palette = AvatarOfEmptinessSky.Palettes["Visceral"];

        // Draw the background with a special shader.
        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.VisceralBackgroundShader");
        backgroundShader.TrySetParameter("textureSize", textureArea);
        backgroundShader.TrySetParameter("center", new Vector2(0.5f, Main.gameMenu ? 0.504f : 0.52f));
        backgroundShader.TrySetParameter("time", Main.gameMenu || AvatarOfEmptiness.Myself is null ? Main.GlobalTimeWrappedHourly : AvatarOfEmptinessSky.WindTimer);
        backgroundShader.TrySetParameter("gradientCount", palette.Length);
        backgroundShader.TrySetParameter("gradient", palette);
        backgroundShader.TrySetParameter("darkening", 0.85f);
        backgroundShader.SetTexture(CrackedNoiseA, 1, SamplerState.LinearWrap);
        backgroundShader.SetTexture(HarshNoise, 2, SamplerState.LinearWrap);
        backgroundShader.Apply();

        Main.spriteBatch.Draw(noise, screenArea * 0.5f + parallaxOffset, null, Color.White, 0f, noise.Size() * 0.5f, textureArea / noise.Size(), 0, 0f);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        VisceraDimensionParticleSystem.Update();
        VisceraDimensionParticleSystem.Render();
    }

    public static void DrawVortexBackground()
    {
        if (AvatarOfEmptiness.Myself is null || AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState != AvatarOfEmptiness.AvatarAIType.TravelThroughVortex)
            return;

        Texture2D noise = WavyBlotchNoise.Value;
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea * 2f;

        Vector2 vortexCenter = screenArea * 0.5f + AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().TravelThroughVortex_VortexPositionOffset * AvatarOfEmptinessSky.SkyTarget.DownscaleFactor;
        vortexCenter /= screenArea;

        // Draw the background with a special shader.
        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.VortexBackgroundShader");
        backgroundShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.94f);
        backgroundShader.TrySetParameter("innerGlowColor", new Vector4(-0.2f, -0.6f, -0.6f, 0f));
        backgroundShader.TrySetParameter("textureSize", textureArea);
        backgroundShader.TrySetParameter("vortexCenter", vortexCenter);
        backgroundShader.TrySetParameter("holeGrowInterpolant", AvatarOfEmptiness.Myself?.As<AvatarOfEmptiness>()?.TravelThroughVortex_HoleGrowInterpolant ?? 0f);
        backgroundShader.TrySetParameter("revealDimensionInterpolant", AvatarOfEmptiness.Myself?.As<AvatarOfEmptiness>()?.TravelThroughVortex_RevealDimensionInterpolant ?? 0f);
        backgroundShader.SetTexture(DendriticNoise, 1, SamplerState.LinearWrap);
        backgroundShader.Apply();

        Main.instance.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        Main.spriteBatch.Draw(noise, screenArea * 0.5f, null, new Color(123, 10, 8), 0f, noise.Size() * 0.5f, textureArea / noise.Size(), 0, 0f);
    }

    public static void DrawVortexBackHomeBackground()
    {
        if (AvatarOfEmptiness.Myself is null || AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState != AvatarOfEmptiness.AvatarAIType.ParadiseReclaimed_NamelessReturnsEveryoneToOverworld)
            return;

        Texture2D noise = WavyBlotchNoise.Value;
        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea * 2f;
        Vector3[] palette = AvatarOfEmptinessSky.Palettes["NamelessVortex"];

        Vector2 vortexCenter = Vector2.One * 0.5f;

        // Draw the background with a special shader.
        ManagedShader backgroundShader = ShaderManager.GetShader("NoxusBoss.NamelessVortexBackgroundShader");
        backgroundShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 1.6f);
        backgroundShader.TrySetParameter("gradient", palette);
        backgroundShader.TrySetParameter("gradientCount", palette.Length);
        backgroundShader.TrySetParameter("textureSize", textureArea);
        backgroundShader.TrySetParameter("vortexCenter", vortexCenter);
        backgroundShader.TrySetParameter("streakUndermineFactor", 0.51f);
        backgroundShader.TrySetParameter("backgroundColor", Color.White.ToVector4());
        backgroundShader.TrySetParameter("subtractive", true);
        backgroundShader.TrySetParameter("holeGrowInterpolant", AvatarOfEmptiness.Myself?.As<AvatarOfEmptiness>()?.TravelThroughVortex_HoleGrowInterpolant ?? 0f);
        backgroundShader.TrySetParameter("revealDimensionInterpolant", AvatarOfEmptiness.Myself?.As<AvatarOfEmptiness>()?.TravelThroughVortex_RevealDimensionInterpolant ?? 0f);
        backgroundShader.Apply();

        Main.instance.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        Main.spriteBatch.Draw(noise, screenArea * 0.5f, null, Color.White, 0f, noise.Size() * 0.5f, textureArea / noise.Size(), 0, 0f);
    }
}
