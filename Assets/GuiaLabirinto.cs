using UnityEngine;

public class GuiaLabirinto : MonoBehaviour
{
    public GameObject paredePrefab; 
    public float tamanhoBloco = 4f;
    public float alturaParede = 0f;

    public int[,] mapa = {
        {1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,1,0,0,0,0,0,1},
        {1,0,1,0,1,0,1,1,1,0,1},
        {1,0,1,0,0,0,0,0,1,0,1},
        {1,0,1,1,1,1,1,0,1,0,1},
        {1,0,0,0,0,0,1,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1}
    };

    void Start()
    {
        // Constrói as paredes
        for (int x = 0; x < mapa.GetLength(0); x++)
        {
            for (int y = 0; y < mapa.GetLength(1); y++)
            {
                if (mapa[x, y] == 1)
                {
                    Vector3 posicao = new Vector3(x * tamanhoBloco, alturaParede, y * tamanhoBloco);
                    Instantiate(paredePrefab, posicao, Quaternion.identity);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
       
        for (int x = 0; x < mapa.GetLength(0); x++)
        {
            for (int y = 0; y < mapa.GetLength(1); y++)
            {
                if (mapa[x, y] == 1)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(new Vector3(x * tamanhoBloco, 0.1f, y * tamanhoBloco), new Vector3(tamanhoBloco, 0, tamanhoBloco));
                }
            }
        }
    }
}