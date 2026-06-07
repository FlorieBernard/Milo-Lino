using UnityEngine;
using UnityEngine.UI;

public class ParticleSpawner : MonoBehaviour
{
    [Header("Particules")]
    public int count = 25;
    public float minSize = 40f;
    public float maxSize = 100f;
    public float minSpeed = 0.8f;
    public float maxSpeed = 2.2f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 0.7f;

    [Header("Couleurs")]
    public Color[] colors = new Color[]
    {
        new Color(0.75f, 0.51f, 0.99f),
        new Color(0.94f, 0.67f, 0.99f),
        new Color(0.50f, 0.55f, 0.98f),
        new Color(0.40f, 0.91f, 0.97f),
    };

    private RectTransform[] particles;
    private float[] speeds;
    private float[] offsets;
    private Vector2[] directions;
    private Image[] images;

    void Start()
    {
        particles  = new RectTransform[count];
        speeds     = new float[count];
        offsets    = new float[count];
        directions = new Vector2[count];
        images     = new Image[count];

        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float halfW = canvasRect.rect.width  / 2f;
        float halfH = canvasRect.rect.height / 2f;

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Particle_" + i, typeof(Image));
            go.transform.SetParent(transform, false);

            Image img = go.GetComponent<Image>();
            img.color = colors[Random.Range(0, colors.Length)];
            images[i] = img;

            RectTransform rt = go.GetComponent<RectTransform>();
            float size = Random.Range(minSize, maxSize);
            rt.sizeDelta = new Vector2(size, size);

            Vector2 startPos = new Vector2(
                Random.Range(-halfW, halfW),
                Random.Range(-halfH, halfH)
            );
            rt.anchoredPosition = startPos;
            rt.pivot = new Vector2(0.5f, 0.5f);

            go.AddComponent<RoundImage>();

            particles[i]  = rt;
            speeds[i]     = Random.Range(minSpeed, maxSpeed);
            offsets[i]    = Random.Range(0f, Mathf.PI * 2f);
            directions[i] = Random.insideUnitCircle.normalized;
        }
    }

    void Update()
    {
        float t = Time.time;

        for (int i = 0; i < count; i++)
        {
            if (particles[i] == null) continue;

            float s = Mathf.Sin(t * speeds[i] + offsets[i]);

            particles[i].anchoredPosition += directions[i] * s * 15f * Time.deltaTime;

            Color col = images[i].color;
            col.a = Mathf.Lerp(minAlpha, maxAlpha, (s + 1f) / 2f);
            images[i].color = col;
        }
    }
}

public class RoundImage : MonoBehaviour
{
    void Awake()
    {
        var img = GetComponent<Image>();
        if (img != null)
        {
            img.sprite = CreateCircleSprite();
            img.type   = Image.Type.Simple;
        }
    }

    Texture2D CreateCircleTexture(int size = 64)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
        }

        tex.Apply();
        return tex;
    }

    Sprite CreateCircleSprite()
    {
        Texture2D tex = CreateCircleTexture();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                             new Vector2(0.5f, 0.5f));
    }
}