using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : Destructible
{
    public override void Start() {
        base.Start();
        Controller.SpawnCauldron = this;
    }

    public override void TakeDamage(int amount, Monster source) {
        Health -= amount;
        if(Health <= 0) {
            Controller.Lose();
        }
    }
}
