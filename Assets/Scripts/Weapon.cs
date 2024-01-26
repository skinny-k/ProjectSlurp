using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour, IThrowable, IReturnable
{
    [Header("Visual Settings")]
    [SerializeField] Vector3 _baseRotation = new Vector3(-70, 0, 0);
    [SerializeField] Vector3 _aimedRotation = new Vector3(-20, 0, 0);
    [SerializeField] Vector3 _holdPosition = new Vector3(1.4f, 0.6f, 0);

    [Header("Other")]
    [Tooltip("The time in seconds that it should take for the weapon to return to the owning player.")]
    [SerializeField] float _returnTime = 0.25f;

    private Rigidbody _rb;
    private Quaternion _throwRotation;
    private Vector3 _lastThrownFrom;
    private float _returnSpeed;

    public PlayerActions Owner;
    public Transform TravelNode { get; private set; }
    public float ThrowSpeed { get; private set; } = 4f;
    public float MaxThrowDistance { get; private set; } = 30f;
    public float ReturnTime => _returnTime;
    public bool IsAiming { get; private set; } = false;
    public bool IsHeld { get; private set; } = true;
    public bool IsThrown { get; private set; } = false;
    public bool IsReturning { get; private set; } = false;

    void Start()
    {
        // initialize weapon settings
        _rb = GetComponent<Rigidbody>();
        TravelNode = transform.Find("Travel Node");

        transform.localRotation = Quaternion.Euler(_baseRotation);
    }

    void Update()
    {
        // if the weapon is being held, not thrown...
        // NOTE: IsHeld and IsThrown should never be true at the same time, so checking one should be sufficient to
        //       know the other, but it can't hurt to check both just in case
        if (IsHeld && !IsThrown)
        {
            Vector3 targetRot = Vector3.zero;
            if (IsAiming)
            {
                targetRot = _aimedRotation;
            }
            else
            {
                targetRot = _baseRotation;
            }

            // rotate toward the target rotation as necessary
            if (transform.localRotation != Quaternion.Euler(targetRot))
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetRot), 0.75f);
            }
        }
        else if (!IsHeld && IsThrown)
        {
            // rotate toward the throw rotation
            transform.localRotation = Quaternion.Slerp(transform.localRotation, _throwRotation, 0.75f);

            // if the weapon has reached its maximum distance, boomerang back to the weapon's owner
            if (Vector3.Distance(transform.position, _lastThrownFrom) > MaxThrowDistance)
            {
                ReturnTo(Owner);
            }
        }
        
        if (IsReturning)
        {
            // return to the owner's hand, not the owner itself
            // NOTE: this must be repeated on update because the player could move after the weapon is recalled
            Vector3 returnPos = Owner.transform.position + _holdPosition;
            // point towards the return target
            Vector3 lookDir = (transform.position - returnPos).normalized;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(lookDir), 0.75f);
            // accelerate towards the return target
            _rb.velocity = (returnPos - transform.position).normalized * _returnSpeed;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsHeld)
        {
            // if the weapon was on its way out when it collided...
            if (IsThrown && !IsReturning)
            {
                // stop and parent the weapon to the hit object
                // TODO: can this work without parenting?
                _rb.velocity = Vector3.zero;
                transform.SetParent(collision.transform, true);
                transform.localScale = (new Vector3(1 / collision.transform.localScale.x, 1 / collision.transform.localScale.y, 1 / collision.transform.localScale.z));
                IsThrown = false;
            }
            else if (collision.gameObject.GetComponent<PlayerActions>() != null) // if the weapon touched the player...
            {
                Player player = collision.gameObject.GetComponent<Player>();
                player.GetComponent<PlayerActions>().CollectWeapon();

                GetComponent<Collider>().enabled = false;
                _rb.isKinematic = true;
                _rb.velocity = Vector3.zero;

                // parent the weapon back to the player
                transform.parent = player.transform;
                transform.localPosition = _holdPosition;
                transform.localRotation = Quaternion.Euler(_baseRotation);

                IsHeld = true;
                IsThrown = false;
                IsReturning = false;
            }
        }
    }
    
    public void Aim(bool state)
    {
        IsAiming = state;
    }
    
    // NOTE: speed is inherited from IThrowable, but is not used in this implementation
    public void Throw(Vector3 dir, float speed = 0)
    {
        // track where the weapon was thrown from
        // this is usefull for the boomerang effect that the weapon has
        _lastThrownFrom = transform.position;
        
        // enable collision
        GetComponent<Collider>().enabled = true;
        _rb.isKinematic = false;

        // rotate towards the direction of the throw and accelerate in that direction
        transform.parent = null;
        _throwRotation = Quaternion.LookRotation(dir, Vector3.Cross(dir, Vector3.right));
        _rb.velocity = dir * ThrowSpeed;
        IsHeld = false;
        IsThrown = true;
    }

    public void ReturnTo(PlayerActions player)
    {
        if (Owner != player)
        {
            Owner = player;
        }

        // return to the player
        // NOTE: speed is non-constant to ensure that the weapon always returns in a predictable amount of time
        IsReturning = true;
        _returnSpeed = Vector3.Distance(transform.position, player.transform.position) / _returnTime;
    }
}
