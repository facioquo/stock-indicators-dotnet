namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Why a <see cref="BarHub"/> refused a bar timestamped before its oldest cached bar.
/// </summary>
public enum BarRejectionReason
{
    /// <summary>
    /// The bar falls inside history the hub already pruned. Admitting it would
    /// place it next to a bar it never preceded.
    /// </summary>
    PrunedHistory = 0,

    /// <summary>
    /// The cache is full, and the bar is older than everything retained.
    /// Raise the hub's maximum cache size to keep such bars.
    /// </summary>
    CacheFull = 1
}
