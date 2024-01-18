using UnityEngine;

public abstract class MonoSingleton : MonoBehaviour
{
    public abstract void InitMonoSingleton();
}

public class MonoSingleton<T> : MonoSingleton where T : MonoSingleton
{
    protected static T _Instance;
    protected static bool searchedFailed = false;

    public static T Instance
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) searchedFailed = false;
#endif
            if (_Instance == null && !searchedFailed)
            {
#if UNITY_EDITOR
                string currStackTrace = StackTraceUtility.ExtractStackTrace();
                if (currStackTrace.Contains(":Awake()"))
                    Debug.LogErrorFormat("Ref to {0} MonoSingleton in Awake() This is NOT ALLOWED. \n {1}", typeof(T).ToString(), currStackTrace);
                if (currStackTrace.Contains(".ctor()"))
                    Debug.LogErrorFormat("Ref to {0} MonoSingleton in field initializer This is NOT ALLOWED. \n {1}", typeof(T).ToString(), currStackTrace);
#endif
                _Instance = FindObjectOfType<T>();
            }

            if (_Instance != null && _Instance as MonoBehaviour == null)
                _Instance = null;

            if (_Instance == null)
                searchedFailed = true;

            return _Instance;
        }
    }
    public virtual void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Debug.LogWarning(gameObject.name + " - Another instance of this MonoSingleton already exists (" + _Instance.name + ")", _Instance);

            Destroy(this.gameObject);
            return;
        }

        _Instance = this as T;
        _Instance.InitMonoSingleton();
    }
    public override void InitMonoSingleton()
    {
        //MonoSingletons without InitMonoSingleton() functions will default to this
    }
}