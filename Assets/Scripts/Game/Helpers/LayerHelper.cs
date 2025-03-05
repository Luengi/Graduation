using UnityEngine;

public static class LayerHelper 
{
    /// <summary>
    /// Checks if a specified layer is included in the given LayerMask.
    /// </summary>
    /// <param name="layer">The layer index to check (0-31).</param>
    /// <param name="layerMask">The LayerMask to compare against.</param>
    /// <returns>True if the layer is in the LayerMask; otherwise, false.</returns>
    public static bool IsLayerInMask (int layer, LayerMask layerMask)
    {
        return (layerMask & (1 << layer)) != 0;
    }
}
