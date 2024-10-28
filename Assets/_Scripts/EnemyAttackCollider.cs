using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackCollider : PV_Object
{
    public bool amArmed = false;
    private Collider myCollider;

    [SerializeField] private Base_enemy myOwner;

    private void Awake()
    {
        myCollider = GetComponent<Collider>();
        myCollider.enabled = false;
        amArmed = false;

    }

    private void OnTriggerEnter( Collider other )
    {
        if (!amArmed)
            return;

        LogHistoric( $"Damage Collider made contact with: '{other.name}'" );

        if (other.tag == PV_GameManager.Tag_Player )
        {
            LogHistoric( $"Entered player damage collision if-check block..." );
            PV_GameManager.Instance.PlayerScript.TakeDmg( 
                myOwner.LoadedAttack.Damage, myOwner.LoadedAttack.Damage, transform.position, new RaycastHit() 
                );
            DisarmMe();
        }
    }

	public void ArmMe()
    {
        LogHistoric( $"ArmMe()", PV_Enums.PV_LogFormatting.UserMethod );

        myCollider.enabled = true;
        amArmed = true;

    }

    public void DisarmMe()
    {
        LogHistoric( $"DisarmMe()", PV_Enums.PV_LogFormatting.UserMethod );

        myCollider.enabled = false;
        amArmed = false;
    }

}
