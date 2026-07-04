using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BubbleEffect : MonoBehaviour
{
    [SerializeField] private RectTransform bubblePrefab;
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private float spawnInterval = 0.3f;
    [SerializeField] private float floatSpeed = 50f;
    [SerializeField] private float flickerSpeed = 3f;
    [SerializeField] private Vector2 scaleRange = new(0.5f, 1.5f);

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnBubble();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBubble()
    {
        var bubble = Instantiate(bubblePrefab, spawnArea);
        var image = bubble.GetComponent<Image>();
        if (image == null) return;

        // 随机大小和水平位置
        var scale = Random.Range(scaleRange.x, scaleRange.y);
        bubble.localScale = new Vector3(scale, scale, 1f);
        var areaWidth = spawnArea.rect.width;
        bubble.anchoredPosition = new Vector2(Random.Range(-areaWidth / 2f, areaWidth / 2f), -spawnArea.rect.height / 2f);

        StartCoroutine(FloatAndFlicker(bubble, image));
    }

    private IEnumerator FloatAndFlicker(RectTransform bubble, Image image)
    {
        float elapsed = 0f;
        var startPos = bubble.anchoredPosition;
        var targetY = spawnArea.rect.height / 2f + 100f;

        while (bubble.anchoredPosition.y < targetY)
        {
            elapsed += Time.deltaTime;
            bubble.anchoredPosition = startPos + new Vector2(0f, elapsed * floatSpeed);
            var alpha = (Mathf.Sin(elapsed * flickerSpeed) + 1f) / 2f;
            var c = image.color;
            c.a = alpha;
            image.color = c;
            yield return null;
        }

        Destroy(bubble.gameObject);
    }
}
