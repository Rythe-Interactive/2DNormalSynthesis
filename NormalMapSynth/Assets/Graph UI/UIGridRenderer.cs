using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
public class UIGridRenderer : Graphic
{
    //public config variables
    [Range(0.5f, 25f)]
    public float LineWidth = 5.0f;
    public Color GridColor;
    public Vector2Int GridSize = new Vector2Int(5, 5);

    //privat variables
    private float m_CellWidth;
    private float m_CellHeight;
    private UIVertex m_Vertex;
    private float m_Xoffset;
    private float m_Yoffset;

    //init grid
    public void Init(Vector2Int newGridSize, float newLineWidth, Color newColor, Vector2 gridScale)
    {
        //update values
        this.rectTransform.localPosition = new Vector2(0, 0);
        this.rectTransform.sizeDelta = gridScale;

        GridColor = newColor;
        GridSize = newGridSize;
        LineWidth = newLineWidth;
        //update graph
        UpdateGeometry();
        //VertexHelper vertexHelper = new VertexHelper();
        //OnPopulateMesh(vertexHelper);
    }
    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        //init vertex
        m_Vertex = UIVertex.simpleVert;
        m_Vertex.color = GridColor;
        vertexHelper.Clear();
        //Get rect && width / heigth
        RectTransform rectT = this.rectTransform;

        float height = rectT.rect.height;
        float width = rectT.rect.width;

        m_CellWidth = width / (float)GridSize.x;
        m_CellHeight = height / (float)GridSize.y;

        m_Xoffset = width * -0.5f;
        m_Yoffset = height * -0.5f;
        //Draw cells
        int count = 0;
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                DrawGrid(vertexHelper, x, y, count);
                count++;
            }
        }
    }
    private void DrawGrid(VertexHelper vertexHelper, int x, int y, int index)
    {
        float xPos = m_CellWidth * x + m_Xoffset;
        float yPos = m_CellHeight * y + m_Yoffset;

        //setup vertex positions
        m_Vertex.position = new Vector3(xPos, yPos);
        vertexHelper.AddVert(m_Vertex);

        m_Vertex.position = new Vector3(xPos, yPos + m_CellHeight);
        vertexHelper.AddVert(m_Vertex);

        m_Vertex.position = new Vector3(m_CellWidth + xPos, yPos + m_CellHeight);
        vertexHelper.AddVert(m_Vertex);

        m_Vertex.position = new Vector3(xPos + m_CellWidth, yPos);
        vertexHelper.AddVert(m_Vertex);


        float widthSquare = LineWidth * LineWidth;
        float distSquare = widthSquare / 2;
        float dist = math.sqrt(distSquare);

        //create new vertecies
        m_Vertex.position = new Vector3(xPos + dist, yPos + dist);
        vertexHelper.AddVert(m_Vertex);

        m_Vertex.position = new Vector3(xPos + dist, yPos + m_CellHeight - dist);
        vertexHelper.AddVert(m_Vertex);

        m_Vertex.position = new Vector3(xPos + m_CellWidth - dist, yPos + m_CellHeight - dist);
        vertexHelper.AddVert(m_Vertex);

        m_Vertex.position = new Vector3(xPos + m_CellWidth - dist, yPos + dist);
        vertexHelper.AddVert(m_Vertex);

        int offset = index * 8;

        //create triangles
        //left edge
        vertexHelper.AddTriangle(offset + 0, offset + 1, offset + 5);
        vertexHelper.AddTriangle(offset + 5, offset + 4, offset + 0);

        //top edge 
        vertexHelper.AddTriangle(offset + 1, offset + 2, offset + 6);
        vertexHelper.AddTriangle(offset + 6, offset + 5, offset + 1);

        //right edge 
        vertexHelper.AddTriangle(offset + 2, offset + 3, offset + 7);
        vertexHelper.AddTriangle(offset + 7, offset + 6, offset + 2);

        //bot edge
        vertexHelper.AddTriangle(offset + 3, offset + 0, offset + 4);
        vertexHelper.AddTriangle(offset + 4, offset + 7, offset + 3);
    }
}
