using System.Collections;
using System.Collections.Generic;
using RebornFrame.Core;
using RebornFrame.Utils;
using UnityEngine;

public class RandomUtils : Singleton<RandomUtils>
{
    public T WheightRandom<T>(List<T> results, IList<int> weights)
    {

        if (results.Count == 0 || results.Count != weights.Count)
        {
            Debug.LogError("随机权重与结果数量不匹配");
            return results[0];
        }
        
        var total = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            total += weights[i];
        }
#if REBORN_FS //采用帧同步
        var value = RebornMath.NextRandom(0,total);
#else
        var value = Random.Range(0, total);
#endif
        for (int j = 0; j < weights.Count; j++)
        {
            if (value <= weights[j])
                return results[j];
            value -= weights[j];
        }

        return results[0];
    }
    
    
    public int WheightRandomIndex(IList<int> weights)
    {

        var total = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            total += weights[i];
        }

#if REBORN_FS //采用帧同步
        var value = RebornMath.NextRandom(0,total);
#else
        var value = Random.Range(0, total);
#endif
        for (int j = 0; j < weights.Count; j++)
        {
            if (value <= weights[j])
                return j;
            value -= weights[j];
        }

        return -1;
    }

}
