using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColliderCombiner : MonoBehaviour
{
    private List<(BoxCollider, Transform)> boxes = new List<(BoxCollider, Transform)>();

    public void FindLeafNodes(Transform node)
    {
        if (node.childCount > 0)
        {
            foreach (Transform child in node)
            {
                FindLeafNodes(child);
            }
        }
        else
        {
            BoxCollider box = node.GetComponent<BoxCollider>();
            if (box != null)
            {
                boxes.Add((box, node));
            }
        }
    }

    public void CombineColliders()
    {
        if (boxes.Count == 0)
        {
            Debug.LogWarning("No box colliders found to combine!");
            return;
        }

        Dictionary<Quaternion, List<(BoxCollider, Transform)>> rotationGroups = new Dictionary<Quaternion, List<(BoxCollider, Transform)>>();

        // Group colliders by world-space rotation
        foreach (var entry in boxes)
        {
            Quaternion worldRotation = entry.Item2.rotation;
            if (!rotationGroups.ContainsKey(worldRotation))
            {
                rotationGroups[worldRotation] = new List<(BoxCollider, Transform)>();
            }
            rotationGroups[worldRotation].Add(entry);
        }

        // Process each rotation group separately
        foreach (var group in rotationGroups.Values)
        {
            MergeCollidersInGroup(group);
        }
    }

    private void MergeCollidersInGroup(List<(BoxCollider, Transform)> group)
    {
        List<(BoxCollider, Transform)> remaining = new List<(BoxCollider, Transform)>(group);
        List<List<(BoxCollider, Transform)>> mergedGroups = new List<List<(BoxCollider, Transform)>>();

        while (remaining.Count > 0)
        {
            List<(BoxCollider, Transform)> newGroup = new List<(BoxCollider, Transform)> { remaining[0] };
            remaining.RemoveAt(0);
            
            bool merged;
            do
            {
                merged = false;
                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    foreach (var entry in newGroup)
                    {
                        if (CanMerge(entry, remaining[i]))
                        {
                            newGroup.Add(remaining[i]);
                            remaining.RemoveAt(i);
                            merged = true;
                            break;
                        }
                    }
                }
            } while (merged);

            mergedGroups.Add(newGroup);
        }

        foreach (var finalGroup in mergedGroups)
        {
            MergeGroup(finalGroup);
        }
    }

    private bool CanMerge((BoxCollider box, Transform obj) a, (BoxCollider box, Transform obj) b)
    {
        if (a.obj.rotation != b.obj.rotation)
            return false;

        Bounds aBounds = a.box.bounds;
        Bounds bBounds = b.box.bounds;

        bool xAligned = Mathf.Approximately(aBounds.min.y, bBounds.min.y) &&
                        Mathf.Approximately(aBounds.max.y, bBounds.max.y) &&
                        Mathf.Approximately(aBounds.min.z, bBounds.min.z) &&
                        Mathf.Approximately(aBounds.max.z, bBounds.max.z) &&
                        (Mathf.Approximately(aBounds.max.x, bBounds.min.x) || Mathf.Approximately(bBounds.max.x, aBounds.min.x));

        bool yAligned = Mathf.Approximately(aBounds.min.x, bBounds.min.x) &&
                        Mathf.Approximately(aBounds.max.x, bBounds.max.x) &&
                        Mathf.Approximately(aBounds.min.z, bBounds.min.z) &&
                        Mathf.Approximately(aBounds.max.z, bBounds.max.z) &&
                        (Mathf.Approximately(aBounds.max.y, bBounds.min.y) || Mathf.Approximately(bBounds.max.y, aBounds.min.y));

        bool zAligned = Mathf.Approximately(aBounds.min.x, bBounds.min.x) &&
                        Mathf.Approximately(aBounds.max.x, bBounds.max.x) &&
                        Mathf.Approximately(aBounds.min.y, bBounds.min.y) &&
                        Mathf.Approximately(aBounds.max.y, bBounds.max.y) &&
                        (Mathf.Approximately(aBounds.max.z, bBounds.min.z) || Mathf.Approximately(bBounds.max.z, aBounds.min.z));

        return xAligned || yAligned || zAligned;
    }

    private void MergeGroup(List<(BoxCollider, Transform)> group)
    {
        if (group.Count == 1) return;

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (var (box, obj) in group)
        {
            Bounds bounds = box.bounds;
            minBounds = Vector3.Min(minBounds, bounds.min);
            maxBounds = Vector3.Max(maxBounds, bounds.max);
        }

        Vector3 combinedCenter = (minBounds + maxBounds) / 2;
        Vector3 combinedSizeWorld = maxBounds - minBounds;

        Transform attachTo = group[group.Count / 2].Item2;
        Quaternion worldRotation = attachTo.rotation;

        // Convert world-space size into local space of the attaching object
        Vector3 combinedSizeLocal = attachTo.InverseTransformDirection(combinedSizeWorld);

        foreach (var (box, _) in group)
        {
            DestroyImmediate(box);
        }

        BoxCollider newCollider = attachTo.gameObject.AddComponent<BoxCollider>();
        newCollider.transform.rotation = worldRotation;
        newCollider.center = attachTo.InverseTransformPoint(combinedCenter);
        newCollider.size = combinedSizeLocal;

        Debug.Log($"Merged {group.Count} colliders into one at {combinedCenter} with size {combinedSizeLocal} and rotation {worldRotation.eulerAngles}");
    }


    public void RunCombiner()
    {
        boxes.Clear();
        FindLeafNodes(transform);
        CombineColliders();
    }
}