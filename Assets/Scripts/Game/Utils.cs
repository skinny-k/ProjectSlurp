using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static bool LayerInMask(int layer, LayerMask mask)
    {
        return mask == (mask | 1 << layer);
    }

    public static bool Between(float a, float lower, float upper, bool lowerInclusive = true, bool upperInclusive = true)
    {
        bool inLower = lowerInclusive ? a >= lower : a > lower;
        bool inUpper = upperInclusive ? a <= upper : a < upper;

        return inLower && inUpper;
    }
}
