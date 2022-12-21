using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject sandPlaceholder;

    [SerializeField]
    GameObject waterPlaceholder;

    [SerializeField]
    Sprite sandSprite;

    [SerializeField]
    Sprite waterSprite;

    [SerializeField]
    [Range(1, 10)]
    float lacunarity = 1.0f;

    float lastLacunarity;

    [SerializeField]
    [Range(0, 0.5f)]
    float persistence = 1.0f;

    float lastPersistence;

    [SerializeField]
    float fullScale = 100.0f;

    float lastScale;

    [SerializeField]
    int textureWidth = 200;

    [SerializeField]
    int textureHeight = 200;

    [SerializeField]
    SpriteRenderer renderSprite;

    [SerializeField]
    [Range(0, 1)]
    float seaLevel = 0.5f;

    [SerializeField]
    int seed = 1000;

    Texture2D perlinNoise;

    Texture2D[] octaves = new Texture2D[3];

    float[] finalValues;

    float randomOffset;
    float lastSeaLevel;

    void Start()
    {
        Regenerate();
        lastSeaLevel = seaLevel;
        lastPersistence = persistence;
        lastLacunarity = lacunarity;
        lastScale = fullScale;
    }

    void Update()
    {
        if (lastSeaLevel != seaLevel || lastPersistence != persistence || lastLacunarity != lacunarity || lastScale != fullScale)
        {
            Regenerate();
            lastSeaLevel = seaLevel;
            lastPersistence = persistence;
            lastLacunarity = lacunarity;
            lastScale = fullScale;
        }

    }
    
    public void Regenerate()
    {
        perlinNoise = new Texture2D(textureWidth, textureHeight);


        if (transform.childCount != textureWidth*textureHeight)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);


            }

            for (int y = 0; y < perlinNoise.height; y++)
            {
                for (int x = 0; x < perlinNoise.width; x++)
                {
                    Instantiate(sandPlaceholder, new Vector2(x, y), Quaternion.identity).transform.SetParent(transform);
                }
            }
        }

        GenerateIslands();

        GameObject spawnedObject;

        for (int y = 0; y < perlinNoise.height; y++)
        {
            for (int x = 0; x < perlinNoise.width; x++)
            {

                spawnedObject = transform.GetChild(y*perlinNoise.width+x).gameObject;
                SpriteRenderer sr = spawnedObject.GetComponent<SpriteRenderer>();
                float s = perlinNoise.GetPixel(x, y).r;

                if (s > seaLevel *1.25f)
                {
                    sr.sprite = sandSprite;
                    sr.color = Color.green;
                }
                else if (s > seaLevel)
                {
                    sr.sprite = sandSprite;
                    sr.color = Color.white;
                }
                else
                {
                    sr.sprite = waterSprite;
                }


                sr.color += perlinNoise.GetPixel(x, y);
                sr.color /= 2.0f;

                spawnedObject.transform.SetParent(transform);
            }
        }
    }

    void GenerateIslands()
    {
        Random.seed = seed;

        for (int i = 0; i < octaves.Length; i++)
        {
            octaves[i] = new Texture2D(textureWidth, textureHeight);
            octaves[i].SetPixels(GenerateNoise(fullScale / (Mathf.Pow(lacunarity, i))));
        }

        finalValues = new float[textureWidth * textureHeight];

        for (int i = 0; i < octaves.Length; i++)
        {
            float influence = Mathf.Pow(persistence, i);
            Debug.Log(influence);
            for (int y = 0; y < perlinNoise.height; y++)
            {
                for (int x = 0; x < perlinNoise.width; x++)
                {
                    int index = y * perlinNoise.width + x;
                    finalValues[index] += octaves[i].GetPixel(x, y).r * influence;
                }
            }
        }


        Color[] finalColors = new Color[textureWidth * textureHeight];
        for (int i = 0; i < finalValues.Length; i++)
        {
            finalColors[i].r = finalValues[i];
            finalColors[i].g = finalValues[i];
            finalColors[i].b = finalValues[i];
            finalColors[i].a = 1;
        }

        //perlinNoise.SetPixels(GenerateNoise(fullScale));
        

        perlinNoise.SetPixels(finalColors);

    }

    Color[] GenerateNoise(float scale)
    {
        float offset = Random.Range(0, 1000);
        Color[] colors = new Color[textureWidth * textureHeight];

        for (int y = 0; y < perlinNoise.height; y++)
        {
            for (int x = 0; x < perlinNoise.width; x++)
            {
                float c = Mathf.PerlinNoise((float)x / scale + offset, (float)y / scale + offset);
                colors[y * perlinNoise.width + x].r = c;
                colors[y * perlinNoise.width + x].g = c;
                colors[y * perlinNoise.width + x].b = c;
                colors[y * perlinNoise.width + x].a = 1;
            }
        }

        return colors;
    }
}
