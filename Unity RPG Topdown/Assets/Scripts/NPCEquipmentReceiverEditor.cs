using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(NPCEquipmentReceiver))]
public class NPCEquipmentReceiverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NPCEquipmentReceiver equipmentReceiver = (NPCEquipmentReceiver)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Randomize Equipment"))
        {
            equipmentReceiver.EquipRandomItems();
        }
    }
}
#endif
