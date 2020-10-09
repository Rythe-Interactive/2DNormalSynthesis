using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Graph : MonoBehaviour
{
    //config grid
    public Vector2Int GridSize = new Vector2Int(10, 10);
    public Vector2 GraphScale = new Vector2(800, 800);
    [Range(1, 10)]
    public float GridLineWidth = 2.0f;
    public Color GraphColor = Color.black;
    public Color BackgroundColor = Color.white;

    //config points
    public Color PointColor = Color.black;
    public float circleSize = 25;


    //Data 
    public float MaxYValue = 100.0f;
    public float MinYValue = 0.0f;
    public float MaxXValue = 15.0f;
    public float minXValue = 0.0f;
    private Vector2[] data = new Vector2[4] { new Vector2(0, 10), new Vector2(1, 20), new Vector2(2, 40), new Vector2(3, 30) };
    private UICircle[] circles;


    private Image backgroundImage;
    private UIGridRenderer Grid;

    private float cellStepX;
    private float cellStepY;
    private float cellWidth;
    private float cellHeight;
    private void OnValidate()
    {
        UpdateValues();
        UpdateGraph();
    }
    private void UpdateValues()
    {
        cellWidth = GraphScale.x / (float)GridSize.x;
        cellHeight = GraphScale.y / (float)GridSize.y;
        cellStepY = (float)GridSize.y / MaxYValue;
        cellStepX = (float)GridSize.x / MaxXValue;
    }
    public void DrawGraph()
    {
        UpdateGraph();
        UpdateCircles();
    }
    private void UpdateGraph()
    {
        UpdateBackground();

        UpdateGrid();

        //UpdateCircles();
    }
    private void UpdateBackground()
    {
        //update backgroundColor
        if (backgroundImage == null)
            FindBackground();
        if (backgroundImage != null)
            backgroundImage.color = BackgroundColor;

    }

    private void UpdateGrid()
    {
        //update graph grid
        //get grid if null
        if (Grid == null)
        {
            //try searching scene for existing grid
            if (GameObject.FindGameObjectWithTag("DataParent"))
            {
                Grid = GameObject.FindGameObjectWithTag("DataParent").GetComponent<UIGridRenderer>();
            }
            else
            {
                //create new grid if non could be found
                Grid = GridFactory.CreateGraphDrawer(GridLineWidth, GraphColor, GridSize, GraphScale);
                return;
            }
        }
        //update grid
        Grid.Init(GridSize, GridLineWidth, GraphColor, GraphScale);
    }

    private void UpdateCircles()
    {
        GameObject[] destroyObject;
        destroyObject = GameObject.FindGameObjectsWithTag("Circle");
        foreach (GameObject oneObject in destroyObject)
            DestroyImmediate(oneObject);

        Debug.Log(cellStepY);
        //Create new circle
        foreach (Vector2 dataValue in data)
        {
            CircleFactory.CreateCircle(dataValue.x * cellStepX * cellWidth, dataValue.y * cellStepY * cellHeight, circleSize, PointColor);
        }
    }


    private void FindBackground()
    {
        Image t = GetComponentInChildren<Image>();
        backgroundImage = t;
    }

}
