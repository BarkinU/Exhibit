using UnityEngine;
using Random = System.Random;

public class Utils : MonoBehaviour
{
    private Random _random = new Random();

    public Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(_random.Next(-5, 5), -5.5f, _random.Next(-5, 5));
    }

    public static void SetRenderLayerInChildren(Transform transform, int layerNumber)
    {
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true))
        {
            //            trans.gameObject.layer = layerNumber;
        }
    }

    public static int GenerateRandomNumber()
    {
        Random random = new Random();
        return random.Next(1, 100);
    }
}