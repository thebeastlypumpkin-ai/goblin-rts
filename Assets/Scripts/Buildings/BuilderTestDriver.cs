using UnityEngine;

public class BuilderTestDriver : MonoBehaviour
{
    private Builder builder;

    void Start()
    {
        builder = GetComponent<Builder>();
    }

    void Update()
    {
        if (builder == null) return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            BuildSite targetSite = FindFirstUnfinishedBuildSite();

            if (targetSite == null)
            {
                Debug.LogWarning("No unfinished BuildSite found.");
                return;
            }

            builder.BeginBuild(targetSite);
            Debug.Log("Builder started building: " + targetSite.name);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            builder.CancelBuild();
            Debug.Log("Builder cancelled building");
        }
    }

    private BuildSite FindFirstUnfinishedBuildSite()
    {
        BuildSite[] allSites = FindObjectsByType<BuildSite>(FindObjectsSortMode.None);

        foreach (BuildSite site in allSites)
        {
            if (site != null && !site.isComplete)
                return site;
        }

        return null;
    }
}