using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallPlacementTool : EditorWindow
{
    private enum DrawState
    {
        Drawing,
        Erasing,
        None
    }

    private DrawState drawState = DrawState.None;

    private int sizeX = 3;
    private int sizeY = 3;

    private Tilemap overlayTileMap;
    private Tilemap centerTileMap;
    private Tilemap underlayTileMap;

    [SerializeField] private WallData wallData;

    private bool isDrawing = false;
    private Tool previousTool;

    [MenuItem("Tools/Wall Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<WallPlacementTool>("Wall Placement Tool");
    }

    private void OnGUI()
    {
        // Create a button to add a new tilemap
        if (GUILayout.Button("Create New Wall"))
        {
            CreateWall();   
        }

        GUILayout.Label("Rectangle Settings", EditorStyles.boldLabel);

        sizeX = EditorGUILayout.IntField("Width", sizeX);
        sizeY = EditorGUILayout.IntField("Height", sizeY);

        wallData = (WallData)EditorGUILayout.ObjectField("Wall Data", wallData, typeof(WallData), false);

        GUILayout.Space(20);

        if (GUILayout.Button(isDrawing ? "Stop Drawing" : "Start Drawing"))
        {
            ToggleDrawing();
            SceneView.RepaintAll();
        }
    }

    private void CreateWall()
    {
        // Set the position the the center of the scene view
        Vector3 scenePos = SceneView.lastActiveSceneView.camera.transform.position;

        // Create the base object that will parent the 3 tilemaps
        GameObject tilemapObject = new GameObject("Wall");

        //Set the position of the tilemap object to the center of the scene view
        tilemapObject.transform.position = new Vector3(scenePos.x, scenePos.y, 0);

        // Child the tilemap parent to the main grid
        tilemapObject.transform.SetParent(FindObjectOfType<Grid>().transform.GetChild(0));

        GameObject underlayObject = CreateTileLayer(name: "ActorUnderlay", sortingLayer: "Actor", sortingOrder: -1);
        underlayTileMap = underlayObject.GetComponent<Tilemap>();
        underlayObject.transform.SetParent(tilemapObject.transform);

        GameObject centerObject = CreateTileLayer(name: "ActorCenter", sortingLayer: "Actor", sortingOrder: 0);
        centerTileMap = centerObject.GetComponent<Tilemap>();
        centerObject.transform.SetParent(tilemapObject.transform);

        GameObject overlayObject = CreateTileLayer(name: "ActorOverlay", sortingLayer: "Actor", sortingOrder: 8);
        overlayTileMap = overlayObject.GetComponent<Tilemap>();
        overlayObject.transform.SetParent(tilemapObject.transform);
    }

    private GameObject CreateTileLayer(string name, string sortingLayer, int sortingOrder)
    {
        GameObject tilemapObject = new GameObject(name);
        
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();

        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = sortingOrder;

        return tilemapObject;
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (isDrawing)
        {
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                drawState = DrawState.Drawing;

                Vector3 mouseWorldPosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
                Vector3Int cellPosition = underlayTileMap.WorldToCell(mouseWorldPosition);

                DrawRectangle(cellPosition);
                e.Use(); // Consume the event to prevent other actions (like selecting objects)
            }
            else if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 1)
            {
                drawState = DrawState.Erasing;

                Vector3 mouseWorldPosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
                Vector3Int cellPosition = underlayTileMap.WorldToCell(mouseWorldPosition);

                EraseRectangle(cellPosition);
                e.Use(); // Consume the event to prevent other actions (like selecting objects)
            }
            else
            {
                drawState = DrawState.None;
            }

            DrawHighlightedArea(sceneView, Color.white);
        }

        // Check for pressing the "C" key to clear to toggle the drawing
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.C && e.keyCode == KeyCode.LeftControl)
        {
            ToggleDrawing();
            SceneView.RepaintAll();

            // repaint the inspector window
            Repaint();
        }
    }

    private void DrawRectangle(Vector3Int cellPos)
    { 
        // Set the underlay tile (bottom of the wall)
        underlayTileMap.SetTile(cellPos, wallData.UnderlayTile);

        // Set the overlay tile (top of the wall)
        Vector3Int topPos = new(cellPos.x, cellPos.y + sizeY , 0);
        overlayTileMap.SetTile(topPos, wallData.OverlayTile);

        // Set all the tiles in between the underlay and overlay tiles
        int tiles = sizeY - 2; // Subtract 2 to account for the underlay and overlay tiles

        for (int y = 0; y <= tiles; y++)
        {
            Vector3Int tilePos = new Vector3Int(cellPos.x, 1 + cellPos.y + y, 0);
            centerTileMap.SetTile(tilePos, wallData.CenterTile);
        }

        // Request repaint to update the scene view
        SceneView.RepaintAll();
    }

    private void EraseRectangle(Vector3Int cellPos)
    {

        underlayTileMap.SetTile(cellPos, null); // Set the tile to null to erase

        // Set the overlay tile (top of the wall)
        Vector3Int topPos = new(cellPos.x, cellPos.y + sizeY, 0);
        overlayTileMap.SetTile(topPos, null);

        int tiles = sizeY - 2; // Subtract 2 to account for the underlay and overlay tiles

        for (int y = 0; y < sizeY; y++)
        {
            Vector3Int tilePos = new Vector3Int(cellPos.x, cellPos.y, 0);
            centerTileMap.SetTile(tilePos, null); // Set the tile to null to erase
        } 

        // Request repaint to update the scene view
        SceneView.RepaintAll();
    }

    private void ToggleDrawing()
    {
        isDrawing = !isDrawing;

        // Restore previous tool when drawing is stopped
        if (!isDrawing)
        {
            Tools.current = Tool.Custom;
        }
    }

    private void DrawHighlightedArea(SceneView sceneView, Color color)
    {
        // Get mouse position
        Event e = Event.current;
        Vector3 mouseWorldPosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        Vector3Int cellPosition = underlayTileMap.WorldToCell(mouseWorldPosition);

        // Calculate the starting position for the rectangle
        int startX = cellPosition.x - sizeX / 2;
        int startY = cellPosition.y - sizeY / 2;

        // Draw the highlighted area
        Vector3 topLeft = underlayTileMap.CellToWorld(new Vector3Int(startX, startY, 0));
        Vector3 topRight = underlayTileMap.CellToWorld(new Vector3Int(startX + sizeX, startY, 0));
        Vector3 bottomLeft = underlayTileMap.CellToWorld(new Vector3Int(startX, startY + sizeY, 0));
        Vector3 bottomRight = underlayTileMap.CellToWorld(new Vector3Int(startX + sizeX, startY + sizeY, 0));

        Handles.DrawSolidRectangleWithOutline(
            new Vector3[] { topLeft, topRight, bottomRight, bottomLeft },

            Color.clear, // Fill color
            color // Outline color
        );
    }

}
