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

    private Rigidbody _rb;
    private Quaternion _throwRotation;

    public Transform TravelNode { get; private set; }
    public bool IsAiming { get; private set; } = false;
    public bool IsHeld { get; private set; } = true;
    public bool IsInTransit { get; private set; } = false;
    
    public void Aim(bool state)
    {
        IsAiming = state;
    }
    
    public void Throw(Vector3 dir, float speed)
    {
        GetComponent<Collider>().enabled = true;
        _rb.isKinematic = false;

        transform.parent = null;
        _throwRotation = Quaternion.LookRotation(dir, Vector3.Cross(dir, Vector3.right));
        _rb.velocity = dir * speed;
        IsHeld = false;
        IsInTransit = true;
    }

    public void Return(Player player)
    {
        GetComponent<Collider>().enabled = false;
        _rb.isKinematic = true;

        transform.parent = player.transform;
        _rb.velocity = Vector3.zero;
        transform.localPosition = _holdPosition;
        transform.localRotation = Quaternion.Euler(_baseRotation);
        IsHeld = true;
        IsInTransit = false;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        TravelNode = transform.Find("Travel Node");

        transform.localRotation = Quaternion.Euler(_baseRotation);
    }

    void Update()
    {
        if (IsHeld && !IsInTransit)
        {
            if (IsAiming)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_aimedRotation), 0.75f);
            }
            else
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_baseRotation), 0.75f);
            }
        }
        else if (!IsHeld && IsInTransit)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, _throwRotation, 0.75f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsHeld)
        {
            if (IsInTransit)
            {
                _rb.velocity = Vector3.zero;
                transform.SetParent(collision.transform, true);
                transform.localScale = (new Vector3(1 / collision.transform.localScale.x, 1 / collision.transform.localScale.y, 1 / collision.transform.localScale.z));
                IsInTransit = false;
            }
            else if (collision.gameObject.GetComponent<PlayerActions>() != null)
            {
                collision.gameObject.GetComponent<PlayerActions>().ReturnWeapon();
            }
        }
    }
}
