using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    public enum Playstyle {
        Offensive,
        Defensive
    }

    private Team team;
    private int difficulty;


    public AIController(Team team) {
        this.team = team;
        team.OnTurnStart += PlanTurn;
        AnimationsManager.Instance.OnAnimationsEnd += ChooseMove;
    }

    public void PlanTurn() {
        // determine priorities

        // order monsters

        // assign tasks
    }

    // chooses 1 move at a time
    public void ChooseMove(Team currentTurn) {
        if(currentTurn != team) {
            return;
        }

        Vector2Int targetPosition = FindTargetPosition();
        foreach(Monster monster in team.Teammates) {
            List<int> moveOptions = GetUsableMoveSlots(monster);
            if(moveOptions.Count == 0) {
                continue;
            }

            int chosenMoveSlot = moveOptions[Random.Range(0, moveOptions.Count)];

            // extra chance to choose an attack
            foreach(int moveSlot in moveOptions) {
                if(monster.Stats.Moves[moveSlot] is Attack && Random.value < 0.3f) {
                    chosenMoveSlot = moveSlot;
                    break;
                }
            }

            Move chosenMove = monster.Stats.Moves[chosenMoveSlot];

            List<List<Vector2Int>> targetOptions = monster.GetMoveOptions(chosenMoveSlot);

            int chosenTargets = Random.Range(0, targetOptions.Count);

            // when moving, bias towards the current objective
            if(chosenMove is MovementAbility) {
                targetOptions.Sort((List<Vector2Int> tile1, List<Vector2Int> tile2) => { return Global.CalcTileDistance(tile1[0], targetPosition) - Global.CalcTileDistance(tile2[0], targetPosition); });
                chosenTargets /= 4; // only choose from the better portion of options
            }

            monster.UseMove(chosenMoveSlot, targetOptions[chosenTargets]);
            return;
        }

        AttemptCraft();

        team.EndTurn();
    }

    private List<int> GetUsableMoveSlots(Monster monster) {
        List<int> moveOptions = new List<int>();
        for(int i = 0; i < monster.Stats.Moves.Length; i++) {
            if(monster.CanUse(i)) {
                moveOptions.Add(i);
            }
        }
        return moveOptions;
    }

    private void CheckResources() {
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            if(resource.Controller == team) {
                // check if an enemy is close to capturing this
            }
            else if(resource.Controller != null) {
                // check if attacking
            }
        }
    }

    private Vector2Int FindTargetPosition() {
        ResourcePile closestUnclaimed = null;
        int closestDistance = 0;
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            int distance = Global.CalcTileDistance(resource.Tile, team.Spawnpoint.Tile);
            if(resource.Controller != team && (closestUnclaimed == null || distance < closestDistance)) {
                closestUnclaimed = resource;
                closestDistance = distance;
            }
        }

        return closestUnclaimed == null ? Vector2Int.zero : closestUnclaimed.Tile;
    }

    private void AttemptCraft() {
        if(team.Spawnpoint.CookState != Cauldron.State.Ready) {
            return;
        }

        List<MonsterName> buyOptions = new List<MonsterName>();
        foreach(MonsterName monsterType in System.Enum.GetValues(typeof(MonsterName))) {
            if(team.CanBuy(monsterType)) {
                buyOptions.Add(monsterType);
            }
        }

        if(buyOptions.Count > 0) {
            team.BuyMonster(buyOptions[Random.Range(0, buyOptions.Count)]);
        }
    }
}
