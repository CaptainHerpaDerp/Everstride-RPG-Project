using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(NPCEquipmentRandomizer))]
public class NPCEquipmentRandomizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
#endif  