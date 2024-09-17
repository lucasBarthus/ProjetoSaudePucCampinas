using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;


public class NetworkObjectPoolDefault : NetworkObjectProviderDefault
{
    [Tooltip("Os objetos a serem agrupados, deixe vazio para agrupar todos os Objetos de Rede criados")]
    [SerializeField]
    private List<NetworkObject> _poolableObjects;

    private Dictionary<NetworkObjectTypeId, Stack<NetworkObject>> _free = new();

    public void ReleaseInstance(NetworkRunner runner, NetworkObject networkObject)
    {
        if (networkObject != null)
        {
            // Marca o objeto como inativo e o adiciona de volta à pool
            networkObject.gameObject.SetActive(false);
            if (!_free.TryGetValue(networkObject.NetworkTypeId, out var stack))
            {
                stack = new Stack<NetworkObject>();
                _free.Add(networkObject.NetworkTypeId, stack);
            }
            stack.Push(networkObject);
            Debug.Log($"Objeto retornado à pool: {networkObject.name}");
        }
    }

    protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
    {
        if (ShouldPool(runner, prefab))
        {
            var instance = GetObjectFromPool(prefab);
            instance.transform.position = Vector3.zero;
            return instance;
        }
        return Instantiate(prefab);
    }

    protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
    {
        if (_free.TryGetValue(prefabId, out var stack))
        {
            instance.gameObject.SetActive(false);
            stack.Push(instance);
            Debug.Log($"Objeto retornado à pool: {instance.name}");
        }
        else
        {
            Destroy(instance.gameObject);
            Debug.Log($"Objeto destruído: {instance.name}");
        }
    }

    public NetworkObject GetObjectFromPool(NetworkObject prefab)
    {
        NetworkObject instance = null;

        if (_free.TryGetValue(prefab.NetworkTypeId, out var stack))
        {
            while (stack.Count > 0 && instance == null)
            {
                instance = stack.Pop();
            }
        }

        if (instance == null)
        {
            instance = GetNewInstance(prefab);
        }

        instance.gameObject.SetActive(true);
        Debug.Log($"Objeto retirado da pool: {instance.name}");
        return instance;
    }

    private NetworkObject GetNewInstance(NetworkObject prefab)
    {
        NetworkObject instance = Instantiate(prefab);

        if (_free.TryGetValue(prefab.NetworkTypeId, out var stack) == false)
        {
            stack = new Stack<NetworkObject>();
            _free.Add(prefab.NetworkTypeId, stack);
        }

        Debug.Log($"Nova instância criada: {instance.name}");
        return instance;
    }

    private bool ShouldPool(NetworkRunner runner, NetworkObject prefab)
    {
        if (_poolableObjects.Count == 0)
        {
            return true;
        }

        return IsPoolableObject(prefab);
    }

    private bool IsPoolableObject(NetworkObject networkObject)
    {
        foreach (var poolableObject in _poolableObjects)
        {
            if (networkObject == poolableObject)
            {
                return true;
            }
        }
        return false;
    }
}