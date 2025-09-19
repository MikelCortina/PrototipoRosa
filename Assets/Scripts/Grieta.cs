using UnityEngine;

public class Grieta : MonoBehaviour
{
    public bool ocupada = false;
    private Collider col;

    private void Start()
    {
        col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    public void Ocupada()
    {
        ocupada = true;
        if (col != null)
            col.enabled = false;
    }
}
