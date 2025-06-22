using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WallData", menuName = "WallPlacement/WallData", order = 1)]
public class WallData : ScriptableObject
{
    [ShowInInspector] public TileBase OverlayTile { get; private set; }
    [ShowInInspector] public TileBase CenterTile { get; private set; }
    [ShowInInspector] public TileBase UnderlayTile { get; private set; }
}
