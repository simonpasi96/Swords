using System.Collections.Generic;
using UnityEngine;

public class SwordGenerator : MonoBehaviour
{
    public Transform[] bladeTemplates;
    public Transform[] guardTemplates;
    public Transform[] gripTemplates;
    public Transform[] pommelTemplates;
    const string TemplateStartName = "Start";
    const string TemplateEndName = "End";
    public Transform sword;

    public class SwordComponent
    {
        public Transform transform;
        public Transform start, end;

        public SwordComponent(Transform transform)
        {
            this.transform = transform;
            start = transform.Find(TemplateStartName);
            end = transform.Find(TemplateEndName);
        }
    }
    public class SwordConfiguration
    {
        public SwordComponent bladeTemplate, guardTemplate, gripTemplate, pommelTemplate;

        public SwordConfiguration(SwordComponent bladeTemplate, SwordComponent guardTemplate, SwordComponent gripTemplate, SwordComponent pommelTemplate)
        {
            this.bladeTemplate = bladeTemplate;
            this.guardTemplate = guardTemplate;
            this.gripTemplate = gripTemplate;
            this.pommelTemplate = pommelTemplate;
        }

        public void BuildOnto(Transform parent)
        {
            // Create each component.
            SwordComponent grip = InstantiateTemplate(gripTemplate, parent);
            SwordComponent guard = InstantiateTemplate(guardTemplate, parent);
            SwordComponent blade = InstantiateTemplate(bladeTemplate, parent);
            SwordComponent pommel = InstantiateTemplate(pommelTemplate, parent);

            // Snap each component.
            SnapComponentAfter(guard, grip);
            SnapComponentAfter(blade, guard);
            SnapComponentBefore(pommel, grip);
        }

        static void SnapToParent(Transform transform, Transform parent)
        {
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        #region Snap components together.
        static void SnapComponentBefore(SwordComponent source, SwordComponent target)
        {
            source.transform.position = target.start.position + target.transform.forward * -source.end.localPosition.z;
            source.transform.forward = target.transform.forward;
        }
        static void SnapComponentAfter(SwordComponent source, SwordComponent target)
        {
            source.transform.position = target.end.position + target.transform.forward * -source.start.localPosition.z;
            source.transform.forward = target.transform.forward;
        }
        #endregion

        static SwordComponent InstantiateTemplate(SwordComponent template, Transform parent)
        {
            SwordComponent component = new SwordComponent(Instantiate(template.transform));
            SnapToParent(component.transform, parent);
            return component;
        }
    }

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

    static T GetRandomItem<T>(T[] array) => array.Length == 0 ? default : array[Random.Range(0, array.Length)];

    static SwordComponent[] GetSwordComponents(Transform[] transforms)
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
            if (child.name == TemplateStartName)
                hasStart = true;
            if (child.name == TemplateEndName)
                hasEnd = true;
            if (hasStart && hasEnd)
                return true;
        }
        return false;
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
}
#endif
