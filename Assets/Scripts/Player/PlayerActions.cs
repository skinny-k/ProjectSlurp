using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] public PlayerWeapon Weapon;
    // [SerializeField] float _throwSpeed = 50f;
    // [SerializeField] float _maxThrowDistance = 20f;

    private Player _player;
    private PlayerMovement _movement;

    private string _aimSModKey;
    
    public bool IsBlocking { get; private set; } = false;
    public bool IsAiming { get; private set; } = false;
    public bool HasWeapon { get; private set; } = true;

    void OnValidate()
    {
        if (Weapon != null)
        {
            Weapon.Owner = this;
        }
    }

    void Start()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<PlayerMovement>();

        _aimSModKey = GetInstanceID() + "_aim";
    }
    
    public void Attack()
    {
        // if the player has their weapon and can attack...
        if (Weapon.IsHeld && !_movement.IsInPrivilegedMove)
        {
            Debug.Log("Attack");
            // Play haptics if a hit occurs
            // HapticsManager.TimedRumble(_player.HapticsSettings.A_strength, _player.HapticsSettings.A_duration);
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
                Weapon.Aim(IsAiming);

            string msg = "Start ";
            // reduce speed while aiming
            if (IsAiming)
            {
                _movement.AddSpeedModifier(_aimSModKey, _movement.AimSpeedModifier);
            }
            else
            {
                _movement.RemoveSpeedModifier(_aimSModKey);
                msg = "End ";
            }
            Debug.Log(msg + "Aim");
        }
    }

    public void Throw()
    {
        // if the player has their weapon and is not dashing or traveling...
        if (Weapon.IsHeld && !_movement.IsInPrivilegedMove)
        {
            // find the weapon's hit point if it is in range
            // if not, find the maximum distance of the weapon instead
            Vector3 target = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Weapon.MaxThrowDistance));
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, Weapon.MaxThrowDistance))
            {
                target = hit.point;
            }

            // throw the weapon towards the found target
            Weapon.Throw(target - Weapon.transform.position);
            HasWeapon = false;
            Debug.Log("Throw");
            HapticsManager.TimedRumble(_player.HapticsSettings.C_strength, _player.HapticsSettings.C_duration);
        }
        // NOTE: the player can recall their weapon while traveling, but not while dashing
        else if (!_movement.IsDashing)
        {
            Weapon.ReturnTo(this);
            StartCoroutine(RetryThrow());
        }
    }

    public void ForceWeaponReturn()
    {
        // force the player's weapon to begin traveling back to this player
        Weapon.ReturnTo(this);
        if (_movement.IsTraveling)
        {
            _movement.EndTravel();
        }
    }

    public void CollectWeapon()
    {
        HasWeapon = true;
        _movement.EndTravel();
        HapticsManager.TimedRumble(_player.HapticsSettings.J_strength, _player.HapticsSettings.J_duration);
    }

    private IEnumerator RetryAttack()
    {
        yield return new WaitForSeconds(Weapon.ReturnTime);
        
        if (!IsAiming)
        {
            Attack();
        }
    }

    private IEnumerator RetryThrow()
    {
        yield return new WaitForSeconds(Weapon.ReturnTime);

        if (IsAiming)
        {
            Throw();
        }
    }
}
