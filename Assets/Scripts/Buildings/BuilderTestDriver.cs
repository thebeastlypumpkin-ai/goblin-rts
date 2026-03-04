using UnityEngine;

public class BuilderTestDriver : MonoBehaviour
{
    [SerializeField] private Transform buildSite;

    private Builder builder;

    void Start()
    {
        builder = GetComponent<Builder>();
    }

    void Update()
    {
        if (builder == null || buildSite == null) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            builder.BeginBuild(buildSite.position);
            Debug.Log("Builder started building");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            builder.CancelBuild();
            Debug.Log("Builder cancelled building");
        }
    }
}