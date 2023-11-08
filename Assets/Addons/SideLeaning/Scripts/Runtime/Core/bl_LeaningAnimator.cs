using System;
using UnityEngine;

public class bl_LeaningAnimator : MonoBehaviour
{
    public Action<int> onAnimatorIK;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        onAnimatorIK?.Invoke(layerIndex);
    }
}