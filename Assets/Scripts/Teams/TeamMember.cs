using UnityEngine;

public class TeamMember : MonoBehaviour
{
    [SerializeField] private Team team = Team.Team1;

    public Team Team => team;

    public bool IsEnemy(TeamMember other)
    {
        if (other == null)
            return false;

        if (team == Team.Neutral || other.team == Team.Neutral)
            return false;

        return team != other.team;
    }
}