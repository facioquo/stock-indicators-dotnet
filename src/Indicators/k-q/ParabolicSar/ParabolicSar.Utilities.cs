using System.Globalization;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Parabolic SAR.
/// </summary>
public static partial class ParabolicSar
{
    private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Validates the parameters for the Parabolic SAR calculation.
    /// </summary>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation.</param>
    /// <param name="initialFactor">Initial acceleration factor for the SAR calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the acceleration step, maximum acceleration factor, or initial factor are out of range.
    /// </exception>
    internal static void Validate(
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
    {
        // check parameter arguments
        if (accelerationStep <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(accelerationStep), accelerationStep,
                "Acceleration Step must be greater than 0 for Parabolic SAR.");
        }

        if (maxAccelerationFactor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAccelerationFactor), maxAccelerationFactor,
                "Max Acceleration Factor must be greater than 0 for Parabolic SAR.");
        }

        if (accelerationStep > maxAccelerationFactor)
        {
            string message = string.Format(
                invariantCulture,
                "Acceleration Step cannot be larger than the Max Acceleration Factor ({0}) for Parabolic SAR.",
                maxAccelerationFactor);

            throw new ArgumentOutOfRangeException(nameof(accelerationStep), accelerationStep, message);
        }

        if (initialFactor <= 0 || initialFactor > maxAccelerationFactor)
        {
            throw new ArgumentOutOfRangeException(nameof(initialFactor), initialFactor,
                "Initial Factor must be greater than 0 and not larger than Max Acceleration Factor for Parabolic SAR.");
        }
    }
}
