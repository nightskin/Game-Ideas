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
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        transform.localScale += new Vector3(maxSize, maxSize * 0.1f, maxSize) * Time.deltaTime;

        alpha -= Time.deltaTime;
        meshRenderer.material.SetFloat("_Alpha", alpha);
        if(alpha <= 0)
        {
            gameObject.SetActive(false);
        }
       
    }
}
