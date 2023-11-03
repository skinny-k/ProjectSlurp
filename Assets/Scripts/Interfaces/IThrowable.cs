using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThrowable : IAimable
{
    void Throw(Vector3 dir, float speed);
}
