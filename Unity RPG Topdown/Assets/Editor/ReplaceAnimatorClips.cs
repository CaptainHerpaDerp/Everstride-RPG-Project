using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class ReplaceAnimatorClips : MonoBehaviour
{
    [MenuItem("Tools/Replace Animator Clips")]
    public static void ReplaceClipsInAnimator()
    {
        // Get the selected Animator Controller
        AnimatorController animatorController = Selection.activeObject as AnimatorController;

        if (animatorController == null)
        {
            Debug.LogError("Please select an Animator Controller in the Project window.");
            return;
        }

        // Replace animations in all states of the Animator Controller
        int replacedCount = ReplaceClipsInStates(animatorController, "Torso", "Gloves");

        Debug.Log($"Replaced {replacedCount} animation clip(s) in Animator Controller '{animatorController.name}'.");
        AssetDatabase.SaveAssets(); // Save changes
    }

    private static int ReplaceClipsInStates(AnimatorController animatorController, string sourceKeyword, string targetKeyword)
    {
        int count = 0;

        foreach (var layer in animatorController.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                var clip = state.state.motion as AnimationClip;

                if (clip != null && clip.name.Contains(sourceKeyword))
                {
                    // Find the replacement clip by name
                    string newClipName = clip.name.Replace(sourceKeyword, targetKeyword);
                    string[] guids = AssetDatabase.FindAssets(newClipName + " t:AnimationClip");

                    if (guids.Length > 0)
                    {
                        string newClipPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        AnimationClip newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);

                        if (newClip != null)
                        {
                            state.state.motion = newClip; // Replace the clip
                            Debug.Log($"Replaced '{clip.name}' with '{newClip.name}' in state '{state.state.name}'.");
                            count++;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No matching clip found for '{newClipName}' to replace '{clip.name}'.");
                    }
                }
            }
        }

        return count;
    }
}
