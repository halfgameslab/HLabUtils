using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mup.Misc.Generic;

public static class M_ObjectPoolGameObjectExtension
{
    public static void Store(this GameObject self)
    {
        M_ObjectPool.Instance.Store(self);
    }

    public static GameObject TryRestoreOrClone(this GameObject self)
    {
        return M_ObjectPool.Instance.Create(self);
    }

    public static GameObject TryRestoreOrClone(this GameObject self, Vector3 position, Quaternion rotation)
    {
        return M_ObjectPool.Instance.Create(self, position, rotation);
    }
}
