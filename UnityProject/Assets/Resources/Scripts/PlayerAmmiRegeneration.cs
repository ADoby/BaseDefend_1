using UnityEngine;
using System.Collections;

public class PlayerAmmiRegeneration : MonoBehaviour 
{

    public vp_PlayerInventory inventory;
    public vp_FPWeaponHandler weaponHandler;
    private vp_FPPlayerEventHandler m_Player = null;

    void Awake()
    {
        m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();
    }

    public float AmmoPerSecond = 0f;

    private float amount = 0f;

	// Update is called once per frame
	void Update () 
    {
        if (AmmoPerSecond > 0)
        {
            amount += Time.deltaTime * AmmoPerSecond;
            while (amount > 1)
            {
                AddAmmo();
                amount--;
            }
        }
	}

    private void AddAmmo()
    {
        vp_ItemIdentifier item = inventory.CurrentWeaponIdentifier;
        if (item == null)
            return;
        
        vp_UnitBankType type = (vp_UnitBankType)item.Type;
        if (m_Player.AddAmmo.Try(type.Unit))
        {
            inventory.TryGiveUnits(type.Unit, 1);
        }
    }
}
