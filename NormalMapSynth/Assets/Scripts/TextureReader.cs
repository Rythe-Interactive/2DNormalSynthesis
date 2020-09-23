using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class TextureReader : MonoBehaviour
{
    public string TextureName = "NAME";
    public float Depth = 1000.0f;

    public bool DoBlur = true;
    public int GaussRadius = 3;
    public bool m_DrawDebugNormalRays = false;
    public bool m_UseAlphaForNormal = true;
    private Texture2D m_Texture2D;
    private Texture2D m_GrayScaleTexture;
    private Texture2D m_NormalMap;
    private Color[] m_originalColor;
    private Color32[] m_newColor;

    private int m_xDim;
    private int m_yDim;
    void Start()
    {
        m_Texture2D = GetComponent<SpriteRenderer>().sprite.texture;
        if (m_Texture2D == null)
        {
            Debug.LogError("Trying to Read NULL Texture!");
            return;
        }
        m_xDim = m_Texture2D.width;
        m_yDim = m_Texture2D.height;
        //init
        m_GrayScaleTexture = new Texture2D(m_xDim, m_yDim, m_Texture2D.format, false);
        m_NormalMap = new Texture2D(m_xDim, m_yDim, m_Texture2D.format, false);

        Graphics.CopyTexture(m_Texture2D, m_GrayScaleTexture);

        GenerateGrayScaleTexture();
        GenerateNormalMap();

        //create new sprite

        Sprite ogSprite = GetComponent<SpriteRenderer>().sprite;
        Sprite HeightSprite = Sprite.Create(m_GrayScaleTexture, ogSprite.rect, ogSprite.pivot);
        Sprite normalSprite = Sprite.Create(m_NormalMap, ogSprite.rect, ogSprite.pivot);
        CreateSpriteObject(HeightSprite, Vector3.zero, false);
        CreateSpriteObject(normalSprite, new Vector3(5, 0, 0), DoBlur);
        //  var gaus = this.gameObject.AddComponent<ParallelGaussianBlur>();
        //  gaus.Radial = GaussRadius;


        var bytes = m_NormalMap.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + TextureName + "_normal.png", bytes);

    }
    private void CreateSpriteObject(Sprite sprite, Vector3 offset, bool addBlur)
    {
        var gO = new GameObject();
        var sr = gO.AddComponent<SpriteRenderer>();

        sr.sprite = sprite;
        gO.transform.localScale = new Vector3(5, 5, 0);
        gO.transform.position = new Vector3(227.5f, 405, 0) + offset;
        //if (addBlur)
        //{
        //    var gaus = gO.AddComponent<ParallelGaussianBlur>();
        //    gaus.Radial = GaussRadius;
        //}
    }

    private void GenerateGrayScaleTexture()
    {
        //read color data
        Color32[] pixels = m_GrayScaleTexture.GetPixels32();
        Color32[] changedPixels = new Color32[m_GrayScaleTexture.width * m_GrayScaleTexture.height];

        for (int x = 0; x < m_xDim; x++)
        {
            for (int y = 0; y < m_yDim; y++)
            {
                //read sample value
                Color32 pixel = pixels[x + y * m_GrayScaleTexture.width];
                //calculate gray value from color value
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                //store new color
                Color c = new Color(l, l, l, pixels[x + y * m_GrayScaleTexture.width].a);
                changedPixels[x + y * m_GrayScaleTexture.width] = c;
            }
        }
        //set && apply GreyScale texture 
        m_GrayScaleTexture.SetPixels32(changedPixels);
        m_GrayScaleTexture.Apply(false);

    }


    private void GenerateNormalMap()
    {
        if (m_GrayScaleTexture == null)
        {
            Debug.LogError("trying to read NULL height map");
            return;
        }

        Color32[] NormalMapData = new Color32[m_xDim * m_yDim];
        Color[] pixels = m_GrayScaleTexture.GetPixels();
        for (int x = 0; x < m_xDim; x++)
        {
            for (int y = 0; y < m_yDim; y++)
            {
                //execute if alpha should be taken into account
                if (m_UseAlphaForNormal)
                {
                    //if alpha is 0 store no color and skip normal calculation
                    if (SampleHeightValue(x, y) == 0)
                    {
                        NormalMapData[x + m_xDim * y] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                }
                //Create vectors for sample point & adjacent points
                //p= sample point, U=Up, L = Left, R = RIght, D = Down
                float3 P = new float3(x, y, SampleHeightValue(x, y));
                float3 U = new float3(x, y + 1, SampleHeightValue(x, y + 1));
                float3 D = new float3(x, y - 1, SampleHeightValue(x, y - 1));
                float3 L = new float3(x - 1, y, SampleHeightValue(x - y, 1));
                float3 R = new float3(x + 1, y, SampleHeightValue(x + 1, y));

                //create direction vectors
                float3 pL = P - L;
                float3 pr = P - R;
                float3 pu = P - U;
                float3 pd = P - D;

                //cross the direction vectors
                float3 pu_x_pl = math.cross(pu, pL);
                float3 pr_x_pu = math.cross(pr, pu);
                float3 pl_x_pd = math.cross(pL, pd);
                float3 pd_X_pr = math.cross(pd, pr);

                //add up crossed vectors && normalize result
                float3 result = pu_x_pl + pr_x_pu + pl_x_pd + pd_X_pr;
                Debug.Log("Result value" + result);
                result = math.normalize(result);
                float3 temp = new float3((x / m_xDim) * 0.75f, 0, 0);
                result += temp;

                float dx = -SampleHeightValue(x - 1, y) + SampleHeightValue(x + 1, y);
                float dy = -SampleHeightValue(x, y - 1) + SampleHeightValue(x, y + 1);
                float3 n = new float3(-dx * (Depth / 1000.0f), dy * (Depth / 1000.0f), 1);

                //visualize normals
                if (m_DrawDebugNormalRays)
                    //   Debug.DrawRay(new Vector3(x, y, 0), result * 5, Color.red, 100.0f);
                    Debug.DrawRay(new Vector3(x, y, 0), n * 5, Color.red, 100.0f);

                //Color32 normal = new Color(result.x, result.y, result.z, 1);
                //     Vector3 normalColorSpace = TextureExtensions.NormalToColorSpace(result);
                //    Vector3 normalColorSpace = TextureExtensions.NormalToColorSpace(n);

                //Color32 normal = new Color(normalColorSpace.x, normalColorSpace.y, normalColorSpace.z, 1);

                // NormalMapData[x + m_xDim * y] = normal;
                // NormalMapData
            }
        }
        //store && set data
        m_NormalMap.SetPixels32(NormalMapData);
        m_NormalMap.Apply(false);
    }

    private float SampleHeightValue(int x, int y)
    {
        //return 0 if texture does not exist
        if (m_GrayScaleTexture == null) return 0;

        //return 0 if out of texture bounds
        if (x < 0 || x > m_xDim || y < 0 || y > m_yDim) return 0;

        //sample height
        Color sample = m_GrayScaleTexture.GetPixel(x, y);
        //Return 0 if the alpha is 0 
        if (sample.a == 0) return 0;
        //else return any color value which is the height value
        return sample.r;
    }
}
