using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Settings")]
    // Якщо в пулі вже є 10 вимкнених об'єктів одного типу, наступні будуть знищуватися.
    // Ти казав про 5, я поставив 10 для "запасу", це мізер для пам'яті.
    public int maxPoolSize = 10;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Робимо його глобальним, щоб не ламався при переході між сценами
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        // Якщо черги для цього ключа ще немає - створюємо
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        // Спробуємо дістати об'єкт з пулу
        GameObject objectToSpawn;

        if (poolDictionary[key].Count > 0)
        {
            objectToSpawn = poolDictionary[key].Dequeue();

            // Страховка: якщо об'єкт був знищений зовнішнім скриптом (наприклад, при зміні сцени)
            if (objectToSpawn == null)
            {
                return CreateNewObject(prefab, position, rotation);
            }
        }
        else
        {
            // Якщо пул пустий - створюємо новий
            objectToSpawn = CreateNewObject(prefab, position, rotation);
        }

        // Активація
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Виклик інтерфейсу (скидання статів кулі і т.д.)
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        return objectToSpawn;
    }

    private GameObject CreateNewObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject newObj = Instantiate(prefab, position, rotation);
        // Важливо: прибираємо "(Clone)", щоб ім'я співпадало з ключем при поверненні
        newObj.name = prefab.name;
        return newObj;
    }

    public void ReturnToPool(GameObject obj)
    {
        string key = obj.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        // --- ГОЛОВНА ЗМІНА: КОНТРОЛЬ ЛІМІТУ ---
        // Якщо в черзі вже достатньо об'єктів, просто знищуємо цей
        if (poolDictionary[key].Count >= maxPoolSize)
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }
}