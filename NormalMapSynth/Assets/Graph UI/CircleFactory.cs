using UnityEngine;



public static class CircleFactory
{
    public static UICircle CreateCircle(float x, float y, float size, Color c)
    {
        GameObject parent = GameObject.FindWithTag("DataParent");
        if (parent == null)
        {
            Debug.LogWarning("please set up a parent Ui object for the graph  data with tag 'DataParent'");

            return null;
        }



        GameObject go = new GameObject("point");
        go.transform.parent = parent.transform;
        go.tag = "Circle";
        UICircle circle = go.AddComponent<UICircle>();
        circle.Init(c, size, x, y);
        // circle.color = c;
        return circle;
    }

}
