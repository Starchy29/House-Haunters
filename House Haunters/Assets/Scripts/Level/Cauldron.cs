using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : GridEntity
{
    [SerializeField] private GameObject cookIndicator;

    private MonsterName? cookingMonster;
    private bool cookingTurn;

    public bool Cooking { get { return cookingMonster.HasValue; } }

    protected override void Start() {
        base.Start();
        Controller.OnTurnStart += FinishCook;
        Controller.Spawnpoint = this;
    }

    public void StartCook(MonsterName monsterType) {
        cookingMonster = monsterType;
        cookIndicator.SetActive(true);
        cookIndicator.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.monsterToSprite[monsterType];
    }

    private void FinishCook() {
        if(cookingMonster.HasValue) {
            // find the spot to spawn on
            LevelGrid level = LevelGrid.Instance;
            Vector2Int levelMid = new Vector2Int(level.Width / 2, level.Height / 2);
            List<Vector2Int> options = level.GetTilesInRange(Tile, 1, true).Filter((Vector2Int tile) => { return level.GetEntity(tile) == null; });
            if(options.Count == 0) {
                return;
            }

            options.Sort((Vector2Int current, Vector2Int next) => { return Global.CalcTileDistance(current, levelMid) - Global.CalcTileDistance(next, levelMid); });
            Vector2Int spawnSpot = options[0];

            // spawn the monster
            GameManager.Instance.SpawnMonster(cookingMonster.Value, spawnSpot, Controller);
            cookingMonster = null;
            cookIndicator.SetActive(false);
        }
    }
}
