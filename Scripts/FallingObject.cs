using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FallingObject : MonoBehaviour
{
    public float fallSpeed = 2f; 
    public float yEnd = -10f;   

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        if (transform.position.y < yEnd)
        {
            Destroy(gameObject);  
        }
    }

    public void SetFallSpeed(float newFallSpeed)
    {
        fallSpeed = newFallSpeed;
    }
}
