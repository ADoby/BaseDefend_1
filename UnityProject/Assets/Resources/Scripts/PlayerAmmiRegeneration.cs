using UnityEngine;
using System.Collections;

public class PlayerAmmiRegeneration : MonoBehaviour 
{

    public vp_PlayerInventory inventory;
    public vp_FPWeaponHandler weaponHandler;
    public vp_FPPlayerEventHandler m_Player = null;



    void Awake()
    {
        if(!m_Player) m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();
    }

    public float AmmoPerSecond = 0f;

    private float amount = 0f;

    public bool currentClip = false;

	// Update is called once per frame
	void Update () 
    {
        if (!enabled)
            return;
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

        if (currentClip)
        {
            vp_UnitBankInstance bank = (inventory.GetItem(item.Type, item.ID) as vp_UnitBankInstance);
            bank.Count += 1;
            bank.ClampToCapacity();
        }else 
        {
            vp_UnitBankType type = (vp_UnitBankType)item.Type;
            if (m_Player.AddAmmo.Try(type.Unit))
            {
                inventory.TryGiveUnits(type.Unit, 1);
            }
        }

        
    }
}
