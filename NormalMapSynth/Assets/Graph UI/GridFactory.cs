using UnityEngine;
public static class GridFactory
{

    public static UIGridRenderer CreateGraphDrawer(float lineWidth, Color gridColor, Vector2Int gridSize, Vector2 GridScale)
    {
        GameObject parent = GameObject.FindWithTag("GraphParent");
        if (parent == null)
        {
            Debug.LogWarning("please set up a parent Ui object for the graph with the tag 'GraphParent'");
            return null;
        }
        //init grid
        GameObject go = new GameObject("Grid");
        go.transform.parent = parent.transform;
        UIGridRenderer Grid = go.AddComponent<UIGridRenderer>();
        Grid.Init(gridSize, lineWidth, gridColor, GridScale);

        //init child container for points
        GameObject dataParent = new GameObject("Data parent");
        dataParent.tag = "DataParent";
        RectTransform trans = dataParent.AddComponent<RectTransform>();
        dataParent.transform.parent = go.transform;
        //set anchors to bot left
        trans.anchorMin = (new Vector2(0, 0));
        trans.anchorMax = (new Vector2(0, 0));
        //set position
        trans.anchoredPosition = new Vector2(0, 0);

        return Grid;
    }
}
