using UnityEngine;

public class SetPosToShader : MonoBehaviour
{
    public float z = 10.0f;
    public Material mat;
    public GameObject target;
    Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        if (target == null) return;

        mat = target.GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {

        if (target == null || mat == null) return;
        else Debug.Log("updating!");

        // Vector3 delta = this.transform.position - target.transform.position;
        //  delta = delta.normalized;
        mat.SetVector("POS", this.transform.position);

    }
}
