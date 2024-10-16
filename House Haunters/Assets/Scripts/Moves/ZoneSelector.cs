using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// selects a square area within a larger square area
public class ZoneSelector : ISelector
{
    public static ZoneSelector AdjacentSelector { get; private set; } = new ZoneSelector(1, 3);
    public static ZoneSelector AOESelector { get; private set; } = new ZoneSelector(2, 5);

    public int Range { get; private set; }
    public int Width { get; private set; }
    public bool Circular { get; private set; }

    private LevelGrid level;

    public ZoneSelector(int reachRadius, int width, bool circular = false) {
        Range = reachRadius;
        Width = width;
        Circular = circular;
    }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        level = LevelGrid.Instance;
        List<List<Vector2Int>> groups = new List<List<Vector2Int>>();

        for(int x = -Range; x <= Range - Width + 1; x++) {
            for(int y = -Range; y <= Range - Width + 1; y++) {
                Vector2Int bottomLeft = user.Tile + new Vector2Int(x, y);
                List<Vector2Int> group = GenerateSquare(bottomLeft);
                if(Circular && group.FindAll((Vector2Int tile) => Global.CalcTileDistance(tile, user.Tile) > Range).Count >= group.Count / 2) {
                    continue;
                }

                if(group.Count > 0) {
                    groups.Add(group);
                }
            }
        }

        return groups;
    }

    private List<Vector2Int> GenerateSquare(Vector2Int bottomLeft) {
        List<Vector2Int> result = new List<Vector2Int>();
        for(int x = 0; x < Width; x++) {
            for(int y = 0; y < Width; y++) {
                Vector2Int testTile = bottomLeft + new Vector2Int(x, y);
                if(level.IsInGrid(testTile)) {
                    result.Add(testTile);
                }
            }
        }
        return result;
    }
}
