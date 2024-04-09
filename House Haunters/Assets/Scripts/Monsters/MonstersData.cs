using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MonsterName {
    LostSoul,
    Demon,
    ThornBush,
    Flytrap,
    Fungus,
    Jackolantern
    //Temporary,
}

public class MonstersData
{
    private static MonstersData instance;
    public static MonstersData Instance { get {
        if(instance == null) {
            instance = new MonstersData();
        }
        return instance;
    } }

    private MonsterType[] monsterTypes; // index is name enum cast to an int

    // define the stats and abilities of all monster types
    private MonstersData() {
        PrefabContainer prefabs = PrefabContainer.Instance;

        monsterTypes = new MonsterType[Enum.GetValues(typeof(MonsterName)).Length];

        //monsterTypes[(int)MonsterName.Temporary] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
        //    10, 3,
        //    new List<Move>() {
        //        new Attack("Attack", 0, 1, new RangeSelector(4, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 8.0f)),
        //        new ZoneMove("Poison Zone", 5, new ZoneSelector(2, 3), new TileEffect(StatusEffect.Poison, 0, 3, prefabs.ExampleZone, null), null, ""),
        //        new ShieldMove("Block", 2, new SelfSelector(), new Shield(Shield.Strength.Medium, 1, false, false, prefabs.ExampleShield), null)
        //    }
        //);

        int spookDuration = 3;
        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            18, 4,
            new List<Move>() {
                new UniqueMove("Revitalize", 1, MoveType.Support, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(4); }, null),
                new StatusMove("Haunt", spookDuration, StatusEffect.Haunted, 3, true, new RangeSelector(1, false, false), AnimateStatus(prefabs.spookHaunt, spookDuration)),
                new Attack("Soul Drain", 1, 3, new RangeSelector(2, false, false), null, "Steals the target's health.", StealHealth)
            }
        );

        int sacDuration = 3;
        int ritualDuration = 2;
        monsterTypes[(int)MonsterName.Demon] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            20, 4,
            new List<Move>() {
                new StatusMove("Sacrifice", 5, StatusEffect.Strength, sacDuration, false, new SelfSelector(), AnimateStatus(prefabs.demonStrength, sacDuration), "Pay 5 life to gain strength.", (user, tile) => { user.TakeDamage(5, null); }),
                new StatusMove("Ritual", 2, StatusEffect.Cursed, ritualDuration, true, new ZoneSelector(2, 2), AnimateStatus(prefabs.demonCurse, ritualDuration)),
                new Attack("Fireball", 1, 6, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 10f), "Deals 4 damage to enemies adjacent to the target.", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 4); })
            }
        );

        monsterTypes[(int)MonsterName.ThornBush] = new MonsterType(Ingredient.Flora, Ingredient.Flora, Ingredient.Flora,
            22, 4,
            new List<Move>() {
                new ShieldMove("Thorn Guard", 1, new SelfSelector(), new Shield(Shield.Strength.Weak, 1, false, false, prefabs.thornShieldPrefab, DamageMeleeAttacker), null, "Deals 6 damage to enemies that attack this within melee range."),
                new ZoneMove("Spike Trap", 0, new RangeSelector(3, false, true), new TileEffect(null, 0, 4, prefabs.thornTrapPrefab, (lander) => { lander.TakeDamage(5, null); }, true), null, "Places a trap that deals 5 damage to an enemy that lands on it."),
                new Attack("Barb Bullet", 0, 6, new DirectionSelector(6, true), null, "Pierces through enemies.")
            }
        );

        int nectarDuraion = 3;
        int tangleDuration = 2;
        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(Ingredient.Flora, Ingredient.Flora, Ingredient.Flora,
            24, 3,
            new List<Move>() {
                new StatusMove("Sweet Nectar", 4, StatusEffect.Regeneration, nectarDuraion, false, new RangeSelector(2, false, true), AnimateStatus(prefabs.nectarRegen, nectarDuraion)),
                new StatusMove("Entangle", 1, StatusEffect.Slowness, tangleDuration, true, new RangeSelector(2, false, true), AnimateStatus(prefabs.tangleVines, tangleDuration)),
                new Attack("Chomp", 0, 8, new RangeSelector(1, false, false), null)
            }
        );

        int sleepyDuration = 2;
        int psychicDuration = 1;
        monsterTypes[(int)MonsterName.Fungus] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Flora,
            20, 3,
            new List<Move>() {
                new StatusMove("Psychedelic Spores", 1, StatusEffect.Fear, psychicDuration, true, new ZoneSelector(2, 2), AnimateStatus(prefabs.fearSpores, psychicDuration)),
                new StatusMove("Sleepy Spores", 2, StatusEffect.Drowsiness, sleepyDuration, true, new RangeSelector(1, false, false), AnimateStatus(prefabs.drowsySpores, sleepyDuration)),
                new UniqueMove("Infect", 0, MoveType.Disrupt, Move.Targets.Enemies, new RangeSelector(2, false, true), LeechStatus.Infect, AnimateStatus(prefabs.leechSeed, LeechStatus.DURATION))
            }
        );

        monsterTypes[(int)MonsterName.Jackolantern] = new MonsterType(Ingredient.Decay, Ingredient.Flora, Ingredient.Flora,
            20, 4,
            new List<Move>() {
                new ZoneMove("Will 'o 'Wisps", 4, new ZoneSelector(2, 2), new TileEffect(StatusEffect.Haunted, 0, 3, prefabs.ExampleZone, null), null),
                new Attack("Scrape", 1, 7, new RangeSelector(1, false, false), null),
                new Attack("Hex", 1, 5, new RangeSelector(4, false, true), AnimateStatus(prefabs.demonCurse, psychicDuration), "Curses the target for one turn.", ApplyStatusOnHit(StatusEffect.Cursed, 1))
            }
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }

    #region Animation Helpers
    // creates the function that queues the animation of a projectile
    private static AnimationQueuer AnimateProjectile(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed) {
        return (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)tiles[0]);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, destroyParticlePrefab, start, end, speed));
        };
    }

    private static AnimationQueuer AnimateStatus(GameObject effectParticlePrefab, int duration) {
        LevelGrid level = LevelGrid.Instance;
        return (Monster user, List<Vector2Int> tiles) => {
            foreach(Vector2Int tile in tiles) {
                Monster target = level.GetMonster(tile);
                if(target != null) {
                    AnimationsManager.Instance.QueueAnimation(new StatusApplicationAnimator(target, effectParticlePrefab, duration));
                }
            }
        };
    }
    #endregion

    #region bonus effects
    private static void StealHealth(Monster user, Monster target, int healthLost) {
        user.Heal(healthLost);
    }

    private static void DealSplashDamage(Monster attacker, Vector2Int center, int damage) {
        List<Monster> targets = LevelGrid.Instance.GetTilesInRange(center, 1, true)
            .Filter((Vector2Int tile) => { return Move.IsEnemyOn(attacker, tile); })
            .Map((Vector2Int tile) => { return LevelGrid.Instance.GetMonster(tile); });

        foreach(Monster target in targets) {
            if(target.Tile != center) {
                target.TakeDamage(damage, attacker);
            }
        }
    }

    private static void DamageMeleeAttacker(Monster attacker, Monster defender) {
        if(Global.IsAdjacent(attacker.Tile, defender.Tile)) {
            attacker.TakeDamage(6, null);
        }
    }

    private static Attack.HitTrigger ApplyStatusOnHit(StatusEffect status, int duration) {
        return (Monster user, Monster target, int healthLost) => {
            target.ApplyStatus(status, duration);
        };
    }
    #endregion
}
