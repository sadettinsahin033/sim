using UnityEngine;

public class CompoundTriggerRelay : MonoBehaviour
{
    public CompoundTriggerArea area;

    private void OnTriggerEnter(Collider other)
    {
        if (area != null)
            area.ChildEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (area != null)
            area.ChildStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (area != null)
            area.ChildExit(other);
    }
}