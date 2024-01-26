using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface for an object that can be thrown
public interface IThrowable : IAimable
{
    void Throw(Vector3 dir, float speed);
}
