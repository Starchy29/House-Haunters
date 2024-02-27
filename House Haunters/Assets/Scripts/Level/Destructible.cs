using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a grid item that can be the target of attacks
public abstract class Destructible : GridEntity
{
    public int Health { get; protected set; }

    public abstract void TakeDamage(int amount, Monster source);
}
