using UnityEngine;

public class BuildSite : MonoBehaviour
{
    public BuildingDefinition definition;
    public int teamId;

    public void Init(BuildingDefinition def, int team)
    {
        definition = def;
        teamId = team;
        name = $"BuildSite_{def.name}_T{team}";
    }
}