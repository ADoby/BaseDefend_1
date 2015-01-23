using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AttributeSystem;

public class AttributeManager : MonoBehaviour 
{

    #region Singleton
    private static AttributeManager instance;
    public static AttributeManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<AttributeManager>();
            return instance;
        }
    }
    protected void Awake()
    {
        foreach (var attribute in Attributes)
        {
            AttributeDictionary.Add(attribute.Name, attribute);
        }
        instance = this;
        Events.Instance.Register(this);
    }
    #endregion

    [SerializeField]
    public List<Attribute> Attributes = new List<Attribute>();

    [SerializeField]
    public List<string> AttributeNames = new List<string>();

    private Dictionary<string, Attribute> AttributeDictionary = new Dictionary<string, Attribute>();

    #region UI
    public bool Attributes_Debug = false;
    public bool Attributes_FoldOut = false;
    public bool NameList_FoldOut = false;
    #endregion

    public virtual void Start()
    {
        Reset();
    }

    public virtual void Reset()
    {
        ResetAttributes();
    }

    public virtual void ResetAttributes()
    {
        foreach (var item in Attributes)
        {
            item.Reset();
        }
    }

    public void AddAttribute(Attribute attribute)
    {
        if (Application.isPlaying || attribute == null || Attributes.Contains(attribute))
            return;
        if (AttributeNames.Contains(attribute.Name))
            return;
        Attributes.Add(attribute);
        AttributeNames.Add(attribute.Name);
    }
    public void RemoveAttribute(Attribute attribute)
    {
        if (Application.isPlaying || attribute == null || !Attributes.Contains(attribute))
            return;
        Attributes.Remove(attribute);
        if (AttributeNames.Contains(attribute.Name))
            AttributeNames.Remove(attribute.Name);
    }
    public void RemoveAttributeByName(string name)
    {
        RemoveAttribute(GetAttributeByName(name));
    }
    public void UpdateAttributeNamesList()
    {
        AttributeNames.Clear();
        foreach (var item in Attributes)
        {
            AttributeNames.Add(item.Name);
        }
    }

    public Attribute GetAttributeByName(string name)
    {
        foreach (var item in Attributes)
        {
            if(string.Equals(item.Name, name, System.StringComparison.OrdinalIgnoreCase))
                return item;
        }
        return null;
    }
}
