using UnityEngine;

[System.Serializable]
public class WinCondition
{
    public enum ConType
    {
        KILLCOUNT
    }

    public ConType MyType;

    public bool Finished = false;

    public int NeededKills = 1;
    private int KillCount = 0;

    public bool Update(LevelPart part)
    {
        switch (MyType)
        {
            case ConType.KILLCOUNT:
                Finished = KillCount >= NeededKills;
                break;
            default:
                Finished = true;
                break;
        }
        return Finished;
    }
    public void Start()
    {
        KillCount = 0;

        Finished = false;
        Events.Instance.Register(this);
    }

    public void End()
    {
        KillCount = NeededKills;

        Finished = true;
        Events.Instance.Unregister(this);
    }


    public void OnMessage_EnemyDied()
    {
        KillCount++;
    }
}

