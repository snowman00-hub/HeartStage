using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> objects = new Queue<T>();
    private readonly T prefab;
    private readonly Transform parent;

    // ObjectPool쓰는 오브젝트는 IPoolable 인터페이스 구현하기 
    public ObjectPool(T prefab, int initialCount, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialCount; i++)
        {
            T obj = GameObject.Instantiate(this.prefab, this.parent);
            obj.gameObject.SetActive(false);
            objects.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (objects.Count == 0)
        {
            T newObj = GameObject.Instantiate(prefab, parent);
            newObj.gameObject.SetActive(false);
            objects.Enqueue(newObj);
        }

        T obj = objects.Dequeue();
        obj.gameObject.SetActive(true);

        if (obj is IPoolable poolable)
            poolable.OnSpawnFromPool();

        return obj;
    }

    public void Release(T obj)
    {
        if (obj is IPoolable poolable)
            poolable.OnReturnToPool();

        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }
}
