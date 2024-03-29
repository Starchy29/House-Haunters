using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMove : Move
{
    public Shield AppliedShield { get; private set; }

    public delegate void BonusEffect(Monster user, Monster shielded);
    private BonusEffect OnUse;
    
    public ShieldMove(string name, int cooldown, ISelector selection, Shield effect, AnimationQueuer effectAnimation, string description = "", BonusEffect bonusEffect = null) 
        : base(name, cooldown, MoveType.Shield, Targets.Allies, selection, effectAnimation, description) 
    {
        AppliedShield = effect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        Monster selectedMonster = LevelGrid.Instance.GetMonster(tile);
        selectedMonster.ApplyShield(AppliedShield);
        
        if(OnUse != null) {
            OnUse(user, selectedMonster);
        }
    }
}
