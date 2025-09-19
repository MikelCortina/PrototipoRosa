using UnityEngine;

public class TipDetector : MonoBehaviour
{
    public RamaOrg�nica rama;


    private void OnTriggerEnter(Collider other)
    {
        Grieta g = other.GetComponent<Grieta>();
        if (g != null && !g.ocupada)
        {
            rama.AnclarEnGrieta(g);
        }
    }

}
