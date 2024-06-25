using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController
{
    public enum Playstyle { // could be aggression value
        Offensive,
        Defensive
    }

    private Team team;
    //private float aggression; // how much this AI favors offense over defense
    //private float persistence; // how much this AI commits to its strategy

    private static Dictionary<ResourcePile, ResourceData> resourceData;

    public AIController(Team team) {
        this.team = team;
        AnimationsManager.Instance.OnAnimationsEnd += ChooseMove;
    }

    public void PlanTurn() {
        // determine which resources to focus on
        resourceData = EvaluateResources();


        // when attacking, evaluate whether this should retreat or invest more

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
            List<int> moveOptions = monster.GetUsableMoveSlots();
            if(moveOptions.Count == 0) {
                continue;
            }

            int chosenMoveSlot = moveOptions[UnityEngine.Random.Range(0, moveOptions.Count)];

            // extra chance to choose an attack
            foreach(int moveSlot in moveOptions) {
                if(monster.Stats.Moves[moveSlot] is Attack && UnityEngine.Random.value < 0.3f) {
                    chosenMoveSlot = moveSlot;
                    break;
                }
            }

            Move chosenMove = monster.Stats.Moves[chosenMoveSlot];

            List<List<Vector2Int>> targetOptions = monster.GetMoveOptions(chosenMoveSlot);

            // when moving, bias towards the current objective
            if(chosenMove is MovementAbility) {
                //targetOptions.Sort((List<Vector2Int> tile1, List<Vector2Int> tile2) => { return Global.CalcTileDistance(tile1[0], targetPosition) - Global.CalcTileDistance(tile2[0], targetPosition); });
                //chosenTargets /= 4; // only choose from the better portion of options
                DebugHelp.Instance.ClearNumbers();
                List<Vector2Int> moveSpot = targetOptions.Max((List<Vector2Int> tileGroup) => { return DetermineTileForce(monster, tileGroup[0]); } );
                monster.UseMove(chosenMoveSlot, moveSpot);
                return;
            }

            int chosenTargets = UnityEngine.Random.Range(0, targetOptions.Count);
            monster.UseMove(chosenMoveSlot, targetOptions[chosenTargets]);
            return;
        }

        //AttemptCraft();

        team.EndTurn();
    }
    
    private Dictionary<ResourcePile, ResourceData> EvaluateResources() {
        Dictionary<ResourcePile, ResourceData> resourceData = new Dictionary<ResourcePile, ResourceData>();

        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            ResourceData data = new ResourceData();

            // determine how much influence each team has on this resource
            data.allyPaths = new Dictionary<Monster, List<Vector2Int>>();
            foreach(Monster ally in team.Teammates) {
                data.allyPaths[ally] = ally.FindPath(resource.Tile, false);
                data.controlValue += CalculateInfluence(ally, resource, data.allyPaths[ally]);
            }

            foreach(Team opponent in GameManager.Instance.AllTeams) {
                if(opponent == team) {
                    continue;
                }

                foreach(Monster enemy in opponent.Teammates) {
                    data.threatValue += CalculateInfluence(enemy, resource);
                }
            }

            // choose a force value for this resource
            data.forceValue = -1f;

            resourceData[resource] = data;
        }

        return resourceData;
    }

    private float CalculateInfluence(Monster monster, ResourcePile resource, List<Vector2Int> existingPath = null) {
        const int MAX_INFLUENCE_RANGE = 5;
        
        if(Global.CalcTileDistance(monster.Tile, resource.Tile) > MAX_INFLUENCE_RANGE + 2) { // influence range may be 2 greater than the actual distance
            return 0;
        }

        int distance = (existingPath == null ? monster.FindPath(resource.Tile, false) : existingPath).Count - 1; // distance to capture area
        if(monster.Tile.x != resource.Tile.x && monster.Tile.y != resource.Tile.y) {
            // make corners worth the same as orthogonally adjacent
            distance--;
        }

        if(distance > MAX_INFLUENCE_RANGE) {
            return 0f;
        }

        return (MAX_INFLUENCE_RANGE + 1 - distance) / (MAX_INFLUENCE_RANGE + 1f);
    }

    // determines how much this monster is encouraged to move to this tile
    private float DetermineTileForce(Monster mover, Vector2Int tile) {
        const float MAX_FORCE_RANGE = 15f;
        float force = 0f;
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            // add bonus if the tile is on the path to this resource
            if(resourceData[resource].forceValue >= 0) {
                continue; // don't care about paths to resources that are pushing away
            }

            int pathIndex = resourceData[resource].allyPaths[mover].IndexOf(tile);
            if(pathIndex >= 0) {
                int pathDistance = resourceData[resource].allyPaths[mover].Count - pathIndex;
                float distanceScale = 1f - pathDistance / MAX_FORCE_RANGE;
                if(distanceScale < 0f) {
                    continue;
                }
                force += -resourceData[resource].forceValue * distanceScale;
            }
        }
        DebugHelp.Instance.MarkTile(tile, force.ToString("F2"));
        return force;
    }

    // REMOVE THIS LATER
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
            if(team.CanAfford(monsterType)) {
                buyOptions.Add(monsterType);
            }
        }

        if(buyOptions.Count > 0) {
            team.BuyMonster(buyOptions[UnityEngine.Random.Range(0, buyOptions.Count)]);
        }
    }

    // finds the amount of each resource this team needs to win the game
    private Dictionary<Ingredient, int> DetermineNeededIngredients(Team team) {
        Dictionary<Ingredient, int> result = new Dictionary<Ingredient, int>();
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            result[ingredient] = 0;
        }

        Dictionary<MonsterName, bool> crafted = team.CraftedMonsters;
        foreach(MonsterName monster in Enum.GetValues(typeof(MonsterName))) {
            if(!crafted[monster]) {
                MonsterType data = MonstersData.Instance.GetMonsterData(monster);
                foreach(Ingredient ingredient in data.Recipe) {
                    result[ingredient]++;
                }
            }
        }
        return result;
    }

    private struct ResourceData {
        public Dictionary<Monster, List<Vector2Int>> allyPaths;
        public float threatValue; // opponent's influence
        public float controlValue; // controller's influence
        public float forceValue; // positive: push away, negative: pull in

        public float Intensity { get { return threatValue + controlValue; } } // amount of action at a control point
        public float Advantage { get { return controlValue - threatValue; } } // positive: winning, negative: losing
    }
}
