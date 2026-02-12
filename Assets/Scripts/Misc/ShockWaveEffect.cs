using UnityEngine;

public class ShockWaveEffect : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] [Min(0)] float maxSize = 30;
    float size;
    float alpha;
    void OnEnable()
    {
        alpha = 1;
        size = 0;
        transform.localScale = Vector3.one * size;
    }

    void Update()
    {
        size += maxSize * Time.deltaTime;
        transform.localScale = Vector3.one * size;

        alpha -= Time.deltaTime;
        meshRenderer.material.SetFloat("_Alpha", alpha);
        if(alpha <= 0)
        {
            Destroy(gameObject);
        }
       
    }
}
