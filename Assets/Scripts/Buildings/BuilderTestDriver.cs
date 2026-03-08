using UnityEngine;

public class BuilderTestDriver : MonoBehaviour
{
    [SerializeField] private BuildSite buildSite;

    private Builder builder;

    void Start()
    {
        builder = GetComponent<Builder>();
    }

    void Update()
    {
        if (builder == null || buildSite == null) return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            builder.BeginBuild(buildSite);
            Debug.Log("Builder started building");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            builder.CancelBuild();
            Debug.Log("Builder cancelled building");
        }
    }
}