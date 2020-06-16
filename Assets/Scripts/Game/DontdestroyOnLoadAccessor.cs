using UnityEngine;

// use as "spy" to get all the roots from DontdestroyOnLoad from the "inside" :)
public class DontdestroyOnLoadAccessor : MonoBehaviour
{
    private static DontdestroyOnLoadAccessor _instance;
    public static DontdestroyOnLoadAccessor Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null) Destroy(this);
        this.gameObject.name = this.GetType().ToString();
        _instance = this;
        DontDestroyOnLoad(this);
    }

    public GameObject[] GetAllRootsOfDontDestroyOnLoad()
    {
        return this.gameObject.scene.GetRootGameObjects();
    }
}

// Example to access the dontdestroy-objects from anywhere
public class FindDontDestroyOnLoad : MonoBehaviour
{
    public GameObject[] rootsFromDontDestroyOnLoad;
    void Start()
    {
        rootsFromDontDestroyOnLoad = DontdestroyOnLoadAccessor.Instance.GetAllRootsOfDontDestroyOnLoad();
    }
}