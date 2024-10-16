using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfSelector : ISelector
{
    public int Range { get { return 0; } }

    public static SelfSelector Instance { get; private set; } = new SelfSelector();

    private SelfSelector() { }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        return new List<List<Vector2Int>>() { new List<Vector2Int>() { user.Tile } };
    }
}
