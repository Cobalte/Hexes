using System.Collections.Generic;
using UnityEngine;

// Reuses inactive copies of prefabs (blocks, celebration effects) instead of instantiating and destroying
// them every turn.
public static class PrefabPool {

    private static readonly Dictionary<GameObject, Stack<GameObject>> available =
        new Dictionary<GameObject, Stack<GameObject>>();
    private static readonly Dictionary<GameObject, GameObject> prefabOfInstance =
        new Dictionary<GameObject, GameObject>();
    private static readonly HashSet<GameObject> pooledInstances = new HashSet<GameObject>();
    private static Transform poolRoot;

    //--------------------------------------------------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        // keeps the pool clean when entering play mode with domain reload turned off
        available.Clear();
        prefabOfInstance.Clear();
        pooledInstances.Clear();
        poolRoot = null;
    }

    //--------------------------------------------------------------------------------------------------------
    // With a rotation, the copy gets that world rotation (like Instantiate(prefab, position, rotation, parent)).
    // Without one, it keeps the prefab's own local rotation (like Instantiate(prefab, parent, false)).
    public static GameObject Get(GameObject prefab, Transform parent, Vector3 position, Quaternion? rotation = null) {
        GameObject instance = null;

        if (available.TryGetValue(prefab, out Stack<GameObject> stack)) {
            // skip anything that got destroyed while it was sitting in the pool
            while (instance == null && stack.Count > 0) {
                instance = stack.Pop();
            }
        }

        if (instance == null) {
            if (rotation.HasValue) {
                instance = Object.Instantiate(prefab, position, rotation.Value, parent);
            }
            else {
                instance = Object.Instantiate(prefab, parent, false);
                instance.transform.position = position;
            }

            prefabOfInstance[instance] = prefab;
            return instance;
        }

        pooledInstances.Remove(instance);
        Transform t = instance.transform;
        t.SetParent(parent, false);
        t.localScale = prefab.transform.localScale;

        if (rotation.HasValue) {
            t.SetPositionAndRotation(position, rotation.Value);
        }
        else {
            t.localRotation = prefab.transform.localRotation;
            t.position = position;
        }

        instance.SetActive(true);
        return instance;
    }

    //--------------------------------------------------------------------------------------------------------
    public static void Release(GameObject instance) {
        if (!prefabOfInstance.TryGetValue(instance, out GameObject prefab)) {
            // not ours (e.g. part of the results screen's copy of the board) - just get rid of it
            Object.Destroy(instance);
            return;
        }

        if (!pooledInstances.Add(instance)) {
            // already released
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(PoolRoot, false);

        if (!available.TryGetValue(prefab, out Stack<GameObject> stack)) {
            stack = new Stack<GameObject>();
            available[prefab] = stack;
        }
        stack.Push(instance);
    }

    //--------------------------------------------------------------------------------------------------------
    private static Transform PoolRoot {
        get {
            if (poolRoot == null) {
                poolRoot = new GameObject("Prefab Pool").transform;
            }
            return poolRoot;
        }
    }
}
