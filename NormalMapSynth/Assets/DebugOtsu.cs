using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOtsu : MonoBehaviour
{

    public Texture2D tex;
    // Start is called before the first frame update
    void Start()
    {
        if (tex)
        {
            GaussianBlur.Blur(ref tex);

        }
        Debug.Log(OtsuThreshold.GetThreshold(tex));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
