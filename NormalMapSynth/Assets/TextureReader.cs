using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class TextureReader : MonoBehaviour
{
    public Texture2D m_Texture2D;
    public Texture2D grayImg;

    private Color[] originalColor;
    private Color32[] newColor;
    void Start()
    {
        m_Texture2D = GetComponent<SpriteRenderer>().sprite.texture;
        if (m_Texture2D == null)
        {
            Debug.Log("TEXTURE IS NULL!");
            return;
        }

        grayImg = new Texture2D(m_Texture2D.width, m_Texture2D.height, m_Texture2D.format, false);
        Graphics.CopyTexture(m_Texture2D, grayImg);
        Color32[] pixels = grayImg.GetPixels32();
        Color32[] changedPixels = new Color32[grayImg.width * grayImg.height];

        for (int x = 0; x < grayImg.width; x++)
        {
            for (int y = 0; y < grayImg.height; y++)
            {
                Color32 pixel = pixels[x + y * grayImg.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l, l, l, pixels[x + y * grayImg.width].a);
                Debug.Log("temp a " + pixels[x + y * grayImg.width].a);
                changedPixels[x + y * grayImg.width] = c;
            }
        }
        grayImg.SetPixels32(changedPixels);
        grayImg.Apply(false);


        //GetComponent<SpriteRenderer>().sprite.texture.SetPixels(grayImg.GetPixels());
        //GetComponent<SpriteRenderer>().sprite.texture.Apply();

        Sprite ogSprite = GetComponent<SpriteRenderer>().sprite;
        Sprite newSprite = Sprite.Create(grayImg, ogSprite.rect, ogSprite.pivot);

        var gO = new GameObject();

        var sr = gO.AddComponent<SpriteRenderer>();

        sr.sprite = newSprite;
        gO.transform.localScale = new Vector3(5, 5, 0);
        gO.transform.position = new Vector3(230, 405, 0);
        //   spriteRenderer.spri
        //originalColor = m_Texture2D.GetPixels();



        //newColor = new Color32[originalColor.Length];

        //Color32[] tex = TextureExtensions.ConvertGrayScale(m_Texture2D);
        //m_Texture2D.SetPixels32(tex);
        //m_Texture2D.Apply(false);
        //m_Texture2D.Apply(false);
        //var bytes = m_Texture2D.EncodeToPNG();

        //System.IO.File.WriteAllBytes(Application.dataPath + "ImageSaveTest.png", bytes);

    }

    private void ReadColors()
    {

    }

}
public static class TextureExtensions
{
    public static Color32[] ConvertGrayScale(Texture2D tex)
    {
        Color32[] newColor = tex.GetPixels32();
        Color32 pixel;
        int index = 0;
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                pixel = newColor[index];
                Debug.Log("color " + pixel);
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                byte l = (byte)((0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f));
                Debug.Log(l);
                pixel = new Color32(l, l, l, pixel.a);
                newColor[index] = pixel;
            }
        }
        return newColor;

    }
}