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
    public int KillCount = 0;

    public bool CheckFinished()
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

    public void Stop()
    {
        KillCount = NeededKills;

        Finished = true;
        Events.Instance.Unregister(this);
    }


    public void OnMessage_EnemyDied()
    {
        KillCount++;
        if (MyType == ConType.KILLCOUNT)
            CheckFinished();
    }

    public string GetText()
    {
        switch (MyType)
        {
            case ConType.KILLCOUNT:
                return string.Format("Kill Enemies <color=#ff0bb>{0}</color>/<color=#00ffaa>{1}</color>", KillCount, NeededKills);
            default:
                return "WinCondition Template";
        }
    }
}

