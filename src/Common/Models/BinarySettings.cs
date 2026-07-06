namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Binary on/off switches for high performance access
/// to behaviors and characteristics.
/// </summary>
/// <remarks>
/// <para>
/// Initializes a new instance of the <see cref="BinarySettings"/> struct.
/// The Mask parameter is optional and defaults to 0b11111111 where all bits
/// pass through to "combinor" sets.
/// </para>
///
/// Example of accessing a specific bit:
/// <code>
/// BinarySettings settings = new(0b00000001); // bit 0 is set to 1
/// bool isBit0Set = settings[0]; // true
/// bool isBit1Set = settings[1]; // false
/// </code>
///
/// Example of re/setting a specific bit:
/// <code>
/// BinarySettings settings = new(0);
/// settings = settings with { [0] = true  }; // set bit 0 to true
/// settings = settings with { [1] = false }; // set bit 1 to false
/// </code>
///
/// As a reminder, bit positions are zero-based:
/// <code>
/// 0b00000000 8-bit binary literal (byte)
///   ↑↑↑↑↑↑↑↑
///   76543210 bit positions
/// </code>
/// </remarks>
/// <param name="settings">
/// Binary settings.
/// Default is 0b00000000 (binary literal of 0).
/// </param>
/// <param name="mask">
/// Mask for settings inheritance.
/// Default is 0b11111111 (binary literal of 255).
/// </param>
[Serializable]
public readonly struct BinarySettings(
    byte settings,
    byte mask = 0b11111111) : IEquatable<BinarySettings>
{
    /// <summary>
    /// Gets the binary settings.
    /// </summary>
    public byte Settings { get; } = settings;

    /// <summary>
    /// Gets the mask for settings inheritance.
    /// </summary>
    public byte Mask { get; } = mask;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinarySettings"/> struct
    /// with default settings (0b00000000) and mask (0b11111111).
    /// </summary>
    public BinarySettings() : this(settings: 0b00000000) { }

    /// <summary>
    /// Gets the value of the bit at the specified index.
    /// </summary>
    /// <param name="index">Index of the bit to get.</param>
    /// <returns>True if the bit is set; otherwise, false.</returns>
    public bool this[short index]
        => (Settings & (1 << index)) != 0;

    /// <summary>
    /// Combines the current settings with another <see cref="BinarySettings"/> instance
    /// using a bitwise OR operation, excluding the bits masked by the parent settings.
    /// </summary>
    /// <param name="parentSettings">Parent <see cref="BinarySettings"/> instance to combine with.</param>
    /// <returns>
    /// A new <see cref="BinarySettings"/> instance with combined settings.
    /// Notably, it does not modify the current read-only instance.
    /// </returns>
    ///
    /// <remarks>
    /// The mask is used to determine which bits from the parent settings should be excluded
    /// during the combination. By default, the mask is set to 0b11111111, meaning all bits
    /// are included. If a different mask is provided, the corresponding bits in the parent
    /// settings will be excluded based on the mask.
    /// <para>
    /// In other words, the mask you provide on instantiation will determine which bits are
    /// genetic material passed on to the "combinor" child settings. The child settings will inherit the
    /// bits from the parent settings that the parent decides to pass along.
    /// </para>
    ///
    /// Usage example (default mask):
    /// <code>
    /// BinarySettings srcSettings = new(0b01101001);
    /// BinarySettings defSettings = new(0b00000010);
    /// BinarySettings newSettings = defSettings.Combine(srcSettings);  // result: 0b01101011
    /// </code>
    /// Using a custom mask:
    /// <code>
    /// BinarySettings customMaskSettings = new(0b01101001, 0b11111110);  // do not pass 0th bit value
    /// BinarySettings newSettingsWithCustomMask = defSettings.Combine(customMaskSettings);  // result: 0b01101010
    /// </code>
    /// </remarks>
    public BinarySettings Combine(BinarySettings parentSettings)
    {
        // add parent bits according to their mask template
        byte maskedParentSettings = (byte)(parentSettings.Settings & parentSettings.Mask);

        // combine the settings
        return new BinarySettings((byte)(Settings | maskedParentSettings), parentSettings.Mask);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is BinarySettings other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(BinarySettings other)
        => Settings == other.Settings && Mask == other.Mask;

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Settings, Mask);

    /// <inheritdoc/>
    public static bool operator ==(BinarySettings left, BinarySettings right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(BinarySettings left, BinarySettings right)
        => !(left == right);
}
