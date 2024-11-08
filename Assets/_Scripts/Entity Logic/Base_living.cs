using PV_Enums;
using UnityEngine;

/// <summary>Base class for all living entities to inherit from. </summary>
public class Base_living : PV_Object
{
    [Header("---------------[[ INHERITED (base_living) ]]-----------------")]
    protected int hp = 100;
    public int HP => hp;
    protected EntityState myEntityState = EntityState.Alive;
    public EntityState MyEntityState => myEntityState;
    protected EntityMovementMode myMovementMode;
    public EntityMovementMode MyMovementMode => myMovementMode;

    public Stats_Base_living MyBaseStats;

    protected float height, girth;
    public float MyHeight => height;

    protected Transform trans;
    protected Transform trans_perspective;
    protected Rigidbody rb;
    protected Animator anim;
    protected CapsuleCollider myCollider;

    /// <summary>
    /// Tells whether this entity can currently switch to any actions (other than taking damage). In other words, whether it's current 
    /// state be interupted by any player input.
    /// </summary>
	public bool AmTotallyPreoccupied => (flag_amInTotallyPreoccupyingAnimation || flag_amInTotallyPreoccupyingAlarm);
    /// <summary>Tells if entity is currently preoccupied by a moving alarm. This is one of the booleans that affects the AmPreoccupied property.</summary>
    protected bool flag_amInTotallyPreoccupyingAlarm;
    /// <summary>This boolean will restrict other actions if entity should not be able to perform them because it's being preoccupied by an animation.</summary>
    protected bool flag_amInTotallyPreoccupyingAnimation;


    /// <summary>Efficiency flag that tells this base class if it belongs to an enemy, which would mean it's extended by an enemy class. </summary>
    protected bool flag_amEnemy = false;
	/// <summary>Efficiency flag that tells this base class if it belongs to an enemy, which would mean it's extended by an enemy class. </summary>
	public bool Flag_amEnemy => flag_amEnemy;

	//[Header("---------------[[ OTHER ]]-----------------")]
	protected Vector3 v_startPosition_cached;

    [Tooltip("Amount of time after landing that we restrict entity's ability to jump again.")]
    protected float cd_JumpRecoverBuffer = 0f;
    [TextArea(1, 10), SerializeField] protected string DBG_Alarms;

    protected virtual void Awake()
    {
        trans = GetComponent<Transform>();
		trans_perspective = trans.Find("Perspective");
		rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
	}

	protected virtual void Start()
    {
        if( myCollider != null )
        {
		    height = myCollider.bounds.size.y;
		    girth = myCollider.radius;
        }

		v_startPosition_cached = trans.position;
		trans.position = v_startPosition_cached;
		//Note: I'm currently not calling CheckIfKosher() here, but rather in the derived classes after certain logic has completed.
	}

	public virtual void InitState()
    {
		myEntityState = EntityState.Alive;
        flag_amInTotallyPreoccupyingAlarm = false;
        flag_amInTotallyPreoccupyingAnimation = false;
	}

    protected virtual void MoveAlarms()
    {
        flag_amInTotallyPreoccupyingAlarm = false;

    }

	public virtual void TakeDmg( int amt, float dmgForce, Vector3 damageOriginPosition, RaycastHit rcHit )
    {
		Log_MethodStart($"{name}.takedmg");

        if( rcHit.transform == null )
        {
            print("hit had null rb");
        }
        else
        {
            print("rb was NOT null");
        }

		hp -= amt;

        if( dmgForce > 0f )
        {
            rb.AddForce( (trans.position - damageOriginPosition).normalized * dmgForce, ForceMode.Impulse );
        }
        //AudioSource.PlayClipAtPoint(mgrAudio.clipSwordDamage, G._transCamera.position);
        
    }

    public virtual void SetNotPreoccupied() //This is so that I can set this via animation events for an animation that I don't have an alarm for, such as the guard animation.
    {
        Log($"{name}.{nameof(SetNotPreoccupied)}()");

        flag_amInTotallyPreoccupyingAnimation = false;
    }

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

        if( MyBaseStats == null )
        {
            PV_Debug.LogError( $"{name}.{nameof(MyBaseStats)} reference was null." );
            amKosher = false;
        }

		if ( trans_perspective == null )
		{
			PV_Debug.LogError($"{name}.{nameof(trans_perspective)} reference was null.");
			amKosher = false;
		}

		if ( rb == null )
		{
			PV_Debug.LogError($"{name}.{nameof(rb)} reference was null.");
			amKosher = false;
		}

		if ( anim == null )
		{
			PV_Debug.LogError($"{name}.{nameof(anim)} reference was null.");
			amKosher = false;
		}

		return amKosher;       
	}
}
