using System.Collections.Generic;
using UnityEngine;

public class SwordGenerator : MonoBehaviour
{
    public List<Transform> bladeTemplates = new List<Transform>();
    public List<Transform> guardTemplates = new List<Transform>();
    public List<Transform> gripTemplates = new List<Transform>();
    public List<Transform> pommelTemplates = new List<Transform>();
    public Transform sword;

    [ContextMenu("Randomize")]
    public void Randomize()
    {
        // Recreate the sword.
        ClearSword();
        sword = new GameObject("Sword").transform;
        sword.SetParent(transform, false);

        // Get a sword configration with random templates.
        SwordConfiguration configuration = new SwordConfiguration(
            GetRandomItem(GetSwordComponents(bladeTemplates)),
            GetRandomItem(GetSwordComponents(guardTemplates)),
            GetRandomItem(GetSwordComponents(gripTemplates)),
            GetRandomItem(GetSwordComponents(pommelTemplates)));
        configuration.BuildOnto(sword);
    }

    [ContextMenu("Clear Sword")]
    public void ClearSword()
    {
        if (!sword)
            return;
        if (Application.isPlaying)
            Destroy(sword.gameObject);
        else
            DestroyImmediate(sword.gameObject);
    }

    public void ImportSwordTemplate(Transform parent)
    {
        SwordConfiguration importedConfig = new SwordConfiguration(parent);
        AddComponentToTemplates(importedConfig.bladeTemplate, bladeTemplates);
        AddComponentToTemplates(importedConfig.guardTemplate, guardTemplates);
        AddComponentToTemplates(importedConfig.gripTemplate, gripTemplates);
        AddComponentToTemplates(importedConfig.pommelTemplate, pommelTemplates);
    }

    static T GetRandomItem<T>(T[] array) => array.Length == 0 ? default : array[Random.Range(0, array.Length)];

    static SwordComponent[] GetSwordComponents(IEnumerable<Transform> transforms)
    {
        List<SwordComponent> components = new List<SwordComponent>();
        foreach (var transform in transforms)
        {
            SwordComponent component = new SwordComponent(transform);
            if (!component.start || !component.end)
                Debug.Log(transform + " does not have a Start and End transform!");
            else
                components.Add(component);
        }
        return components.ToArray();
    }

    static bool CheckTemplateStartEnd(Transform template)
    {
        if (template.childCount == 0)
            return false;
        bool hasStart = false;
        bool hasEnd = false;
        foreach (Transform child in template)
        {
            if (child.name == SwordComponent.PartStartName)
                hasStart = true;
            if (child.name == SwordComponent.PartEndName)
                hasEnd = true;
            if (hasStart && hasEnd)
                return true;
        }
        return false;
    }

    static bool AddComponentToTemplates(SwordComponent component, List<Transform> templates)
    {
        if (!component.IsValid)
            return false;
        if (templates.Contains(component.transform))
            return false;
        templates.Add(component.transform);
        return true;
    }
}


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SwordGenerator)), UnityEditor.CanEditMultipleObjects]
class SwordGenaratorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(5);

        SwordGenerator script = (SwordGenerator)target;

        DrawDropArea<GameObject>("Drop sword template here", (go) =>
        {
            script.ImportSwordTemplate(go.transform);
            MarkSceneDirty(script);
        });

        if (GUILayout.Button("Randomize"))
        {
            script.Randomize();
            MarkSceneDirty(script);
        }

        if (GUILayout.Button("Clear Sword"))
        {
            script.ClearSword();
            MarkSceneDirty(script);
        }
    }

    static void MarkSceneDirty(Component script)
    {
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.EditorUtility.SetDirty(script);
        }
    }

    Rect DrawDropArea<T>(string boxText, System.Action<T> ObjectAction)
    {
        Event evt = Event.current;
        Rect boxRect = GUILayoutUtility.GetRect(50, 30, GUILayout.ExpandWidth(true));
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.MiddleCenter;
        if (UnityEditor.EditorGUIUtility.isProSkin)
            boxStyle.normal.textColor = Color.white;
        GUI.Box(boxRect, boxText, boxStyle);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!boxRect.Contains(evt.mousePosition))
                    break;

                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    UnityEditor.DragAndDrop.AcceptDrag();

                    foreach (Object droppedObject in UnityEditor.DragAndDrop.objectReferences)
                        if (droppedObject is T target)
                            ObjectAction(target);
                }
                break;
        }

        return boxRect;
    }
}
#endif
