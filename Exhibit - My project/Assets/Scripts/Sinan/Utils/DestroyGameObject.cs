using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    public float liteTime = 1.5f;

    // Update is called once per frame
    IEnumerator Start()
    {
        yield return new WaitForSeconds(liteTime);

        Destroy(gameObject);
    }
}
