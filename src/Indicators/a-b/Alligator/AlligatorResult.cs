namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Alligator indicator calculation.
/// </summary>
/// <param name="Timestamp">Date and time of the record.</param>
/// <param name="Jaw">Value of the Jaw line of the Alligator indicator.</param>
/// <param name="Teeth">Value of the Teeth line of the Alligator indicator.</param>
/// <param name="Lips">Value of the Lips line of the Alligator indicator.</param>
[Serializable]
public record AlligatorResult
(
    DateTime Timestamp,
    double? Jaw,
    double? Teeth,
    double? Lips
) : ISeries;
