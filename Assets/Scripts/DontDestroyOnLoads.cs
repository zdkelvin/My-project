using UnityEngine;

public class DontDestroyOnLoads : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
