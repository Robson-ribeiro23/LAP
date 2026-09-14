using UnityEngine;

public class GuiaLabirinto : MonoBehaviour
{
    public GameObject paredePrefab; // O espaço para receber o seu molde
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
        // Constrói as paredes físicas ao apertar Play
        for (int x = 0; x < mapa.GetLength(0); x++)
        {
            for (int y = 0; y < mapa.GetLength(1); y++)
            {
                if (mapa[x, y] == 1)
                {
                    // A altura (eixo Y) está como 2f, ajuste se sua parede ficar voando ou afundada
                    Vector3 posicao = new Vector3(x * tamanhoBloco, alturaParede, y * tamanhoBloco);
                    Instantiate(paredePrefab, posicao, Quaternion.identity);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Mantém as linhas amarelas na cena para referência visual
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