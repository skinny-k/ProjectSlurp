using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] public Weapon PlayerWeapon;
    // [SerializeField] float _throwSpeed = 50f;
    // [SerializeField] float _maxThrowDistance = 20f;

    private Player _player;
    private PlayerMovement _movement;
    
    public bool IsBlocking { get; private set; } = false;
    public bool IsAiming { get; private set; } = false;
    public bool HasWeapon { get; private set; } = true;

    void OnValidate()
    {
        if (PlayerWeapon != null)
        {
            PlayerWeapon.Owner = this;
        }
    }

    void Start()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<PlayerMovement>();
    }
    
    public void Attack()
    {
        // if the player has their weapon and can attack...
        if (PlayerWeapon.IsHeld && !_movement.IsInPrivilegedMove)
        {
            Debug.Log("Attack");
        }
        // NOTE: the player can recall their weapon while traveling, but not while dashing
        else if (!_movement.IsDashing)
        {
            // recall weapon and try again after a short delay
            ForceWeaponReturn();
            StartCoroutine(RetryAttack());
        }
    }
    
    public void Block()
    {
        // if the player is not dashing, traveling, aiming, high jumping or slow falling...
        if (!_movement.IsInPrivilegedMove && !_movement.IsInActiveAerial && !IsAiming)
        {
            IsBlocking = !IsBlocking;
            string msg = "Start ";
            if (!IsBlocking)
                msg = "End ";
            Debug.Log(msg + "Block");
        }
    }

    public void Aim()
    {
        // if the player is not dashing, traveling or high jumping...
        if (!_movement.IsInPrivilegedMove && !_movement.IsHighJumping)
        {
            IsAiming = !IsAiming;

            // snap player rotation to camera direction
            Vector3 targetRot = Quaternion.LookRotation(_player.Camera.transform.forward).eulerAngles;
            targetRot.x = transform.rotation.x;
            targetRot.z = transform.rotation.z;
            GetComponent<PlayerMovement>().RotateTo(targetRot);

            // activate weapon aim as necessary
            if (!IsAiming || HasWeapon)
                PlayerWeapon.Aim(IsAiming);

            string msg = "Start ";
            // reduce speed while aiming
            if (IsAiming)
            {
                _movement.AddSpeedModifier(_movement.AimSpeedModifier);
            }
            else
            {
                _movement.RemoveSpeedModifier(_movement.AimSpeedModifier);
                msg = "End ";
            }
            Debug.Log(msg + "Aim");
        }
    }

    public void Throw()
    {
        // if the player has their weapon and is not dashing or traveling...
        if (PlayerWeapon.IsHeld && !_movement.IsInPrivilegedMove)
        {
            // find the weapon's hit point if it is in range
            // if not, find the maximum distance of the weapon instead
            Vector3 target = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, PlayerWeapon.MaxThrowDistance));
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, PlayerWeapon.MaxThrowDistance))
            {
                target = hit.point;
            }

            // throw the weapon towards the found target
            PlayerWeapon.Throw(target - PlayerWeapon.transform.position);
            HasWeapon = false;
            Debug.Log("Throw");
        }
        // NOTE: the player can recall their weapon while traveling, but not while dashing
        else if (!_movement.IsDashing)
        {
            PlayerWeapon.ReturnTo(this);
            StartCoroutine(RetryThrow());
        }
    }

    public void ForceWeaponReturn()
    {
        // force the player's weapon to begin traveling back to this player
        PlayerWeapon.ReturnTo(this);
        if (_movement.IsTraveling)
        {
            _movement.EndTravel();
        }
    }

    public void CollectWeapon()
    {
        HasWeapon = true;
    }

    private IEnumerator RetryAttack()
    {
        yield return new WaitForSeconds(PlayerWeapon.ReturnTime);
        
        if (!IsAiming)
        {
            Attack();
        }
    }

    private IEnumerator RetryThrow()
    {
        yield return new WaitForSeconds(PlayerWeapon.ReturnTime);

        if (IsAiming)
        {
            Throw();
        }
    }
}
