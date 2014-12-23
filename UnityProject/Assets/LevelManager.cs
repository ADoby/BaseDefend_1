using UnityEngine;
using System.Collections;

[System.Serializable]
public class LevelPartInfo
{
    public LevelPart LevelPart;
    public float Weight = 1f;
}

public class LevelManager : MonoBehaviour 
{
    public LevelPartInfo[] partInfos;

    public LevelPart SpawnPart;
    public LevelPart TutorialPart;

    public LevelPart LastPart = null;
    public LevelPart CurrentPart = null;
    public LevelPart NextPart = null;

    public float CurrentZPos = 0f;

    public Transform Player;
    public float MaxZPosToLoadNewPart = 0f;

    void Start()
    {
        AddPart(SpawnPart);
        LoadTutorial();
    }

    public Timer PlayerPositionCheck;
    void Update()
    {
        if (PlayerPositionCheck.Update())
        {
            PlayerPositionCheck.Reset();
            if (Player.position.z > MaxZPosToLoadNewPart)
            {
                CurrentPart.Start();
                AddRandomPart();
            }
        }
    }

    public void LoadTutorial()
    {
        AddPart(TutorialPart);
    }

    public float Weight(LevelPartInfo info)
    {
        return info.Weight;
    }

    public void AddRandomPart()
    {
        if (partInfos.Length < 3)
            return;
        LevelPart part = null;
        do
        {
            part = partInfos.RandomEntry(Weight).LevelPart;
        } while (part == CurrentPart || part == NextPart || part == null);
        AddPart(part);
    }

    public void AddPart(LevelPart part)
    {
        DespawnLastPart();

        LastPart = CurrentPart;
        CurrentPart = NextPart;
        NextPart = part;

        if (NextPart != null)
        {
            NextPart.Spawn(CurrentZPos);
            CurrentZPos += NextPart.Length;
        }
        
        if (CurrentPart != null)
            MaxZPosToLoadNewPart += CurrentPart.Length;
    }
    public void DespawnLastPart()
    {
        if (LastPart == null)
            return;

        LastPart.Despawn();
        LastPart = null;
    }

}
