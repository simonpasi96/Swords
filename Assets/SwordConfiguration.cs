using UnityEngine;

public class SwordConfiguration
{
    public SwordComponent bladeTemplate, guardTemplate, gripTemplate, pommelTemplate;
    public const string BladeName = "Blade", GuardName = "Guard", GripName = "Grip", PommelName = "Pommel";

    public SwordConfiguration(SwordComponent bladeTemplate, SwordComponent guardTemplate, SwordComponent gripTemplate, SwordComponent pommelTemplate)
    {
        this.bladeTemplate = bladeTemplate;
        this.guardTemplate = guardTemplate;
        this.gripTemplate = gripTemplate;
        this.pommelTemplate = pommelTemplate;
    }

    public SwordConfiguration(Transform parent)
    {
        bladeTemplate = new SwordComponent(parent.Find(BladeName));
        guardTemplate = new SwordComponent(parent.Find(GuardName));
        gripTemplate = new SwordComponent(parent.Find(GripName));
        pommelTemplate = new SwordComponent(parent.Find(PommelName));
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
        source.transform.forward = target.transform.forward + (target.transform.forward - source.end.forward);
        source.transform.position = target.start.position + (source.transform.position - source.end.position);
    }
    static void SnapComponentAfter(SwordComponent source, SwordComponent target)
    {
        source.transform.forward = target.transform.forward + (target.transform.forward - source.start.forward);
        source.transform.position = target.end.position + (source.transform.position - source.start.position);
    }
    #endregion

    static SwordComponent InstantiateTemplate(SwordComponent template, Transform parent)
    {
        SwordComponent component = new SwordComponent(Object.Instantiate(template.transform));
        SnapToParent(component.transform, parent);
        return component;
    }
}

public class SwordComponent
{
    public Transform transform;
    public Transform start, end;
    public bool IsValid => transform && start && end;
    public const string PartStartName = "Start", PartEndName = "End";

    public SwordComponent(Transform transform)
    {
        if (!transform)
            return;
        this.transform = transform;
        start = transform.Find(PartStartName);
        end = transform.Find(PartEndName);
    }
}
