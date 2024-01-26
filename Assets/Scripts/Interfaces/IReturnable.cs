using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface for an object that can be returned to a player
public interface IReturnable
{
    void ReturnTo(PlayerActions player);
}
