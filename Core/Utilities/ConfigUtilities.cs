using System.Reflection;
using NoxusBoss.Core.CrossCompatibility.Inbound;

namespace NoxusBoss.Core.Utilities;

public static partial class Utilities
{
    /// <summary>
    /// Useful way of acquiring information from Calamity's config without a strong reference.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName">The name of the config value. Should correspond to the property name in Calamity's source code. If it doesn't exist for some reason the default value will be used.</param>
    /// <param name="defaultValue">The default value to use if the config data could not be accessed. Normally is the default for whatever data type is requested (such as zero for integers), but can be manually specified.</param>
    public static T GetFromCalamityConfig<T>(string propertyName, T defaultValue = default) where T : struct
    {
        // Immediately return the default value if Calamity is not enabled.
        if (ModReferences.Calamity is null)
            return defaultValue;

        // Calamity 2.x split config into client vs server; legacy CalamityConfig may still exist on older builds.
        ReadOnlySpan<string> typeNames =
        [
            "CalamityMod.CalamityConfig",
            "CalamityMod.CalamityClientConfig",
            "CalamityMod.CalamityServerConfig",
        ];

        foreach (string fullName in typeNames)
        {
            Type? calConfigType = ModReferences.Calamity.Code.GetType(fullName);
            if (calConfigType is null)
                continue;

            FieldInfo? instanceField = calConfigType.GetField("Instance");
            object? calConfig = instanceField?.GetValue(null);
            if (calConfig is null)
                continue;

            PropertyInfo? property = calConfig.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object? raw = property?.GetValue(calConfig);
            if (raw is null)
                continue;

            if (raw is T typed)
                return typed;

            try
            {
                return (T)Convert.ChangeType(raw, typeof(T));
            }
            catch
            {
                // Property exists but is not convertible to T; try next config type.
            }
        }

        return defaultValue;
    }
}
