using System.Collections.Generic;
using UnityEngine;

public class IAPerseguidor : MonoBehaviour
{
    public Transform jogador;
    public GuiaLabirinto gridMapa;
    public float velocidade = 4f;

    No[,] malhaDeNos;
    List<No> caminhoAtual = new List<No>();

    void Start()
    {
        // Constrói a malha de Nós no início da execução
        int largura = gridMapa.mapa.GetLength(0);
        int altura = gridMapa.mapa.GetLength(1);
        malhaDeNos = new No[largura, altura];

        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                // Verifica na sua matriz base se o número é 0 (caminho livre)
                bool livre = (gridMapa.mapa[x, y] == 0);
                malhaDeNos[x, y] = new No(livre, x, y);
            }
        }
    }

    void Update()
    {
        EncontrarCaminho(transform.position, jogador.position);
        MoverPeloCaminho();
    }

    void MoverPeloCaminho()
    {
        // Se existe uma rota calculada, ande até o primeiro bloco dela
        if (caminhoAtual != null && caminhoAtual.Count > 0)
        {
            No proximoNo = caminhoAtual[0];
            Vector3 alvoFisico = new Vector3(proximoNo.gridX * gridMapa.tamanhoBloco, transform.position.y, proximoNo.gridY * gridMapa.tamanhoBloco);

            transform.position = Vector3.MoveTowards(transform.position, alvoFisico, velocidade * Time.deltaTime);
        }
    }

    void EncontrarCaminho(Vector3 inicio3D, Vector3 fim3D)
    {
        // Conversão das posições 3D reais para índices da matriz
        int startX = Mathf.RoundToInt(inicio3D.x / gridMapa.tamanhoBloco);
        int startY = Mathf.RoundToInt(inicio3D.z / gridMapa.tamanhoBloco);
        int alvoX = Mathf.RoundToInt(fim3D.x / gridMapa.tamanhoBloco);
        int alvoY = Mathf.RoundToInt(fim3D.z / gridMapa.tamanhoBloco);

        // Trava de segurança para o monstro não tentar calcular rotas fora dos limites do mapa
        if (startX < 0 || startX >= malhaDeNos.GetLength(0) || startY < 0 || startY >= malhaDeNos.GetLength(1)) return;
        if (alvoX < 0 || alvoX >= malhaDeNos.GetLength(0) || alvoY < 0 || alvoY >= malhaDeNos.GetLength(1)) return;

        No noInicial = malhaDeNos[startX, startY];
        No noAlvo = malhaDeNos[alvoX, alvoY];

        List<No> listaAberta = new List<No>();
        HashSet<No> listaFechada = new HashSet<No>();
        listaAberta.Add(noInicial);

        // Loop principal do Algoritmo A*
        while (listaAberta.Count > 0)
        {
            No noAtual = listaAberta[0];
            for (int i = 1; i < listaAberta.Count; i++)
            {
                if (listaAberta[i].fCost < noAtual.fCost || (listaAberta[i].fCost == noAtual.fCost && listaAberta[i].hCost < noAtual.hCost))
                {
                    noAtual = listaAberta[i];
                }
            }

            listaAberta.Remove(noAtual);
            listaFechada.Add(noAtual);

            // Condição de vitória: O alvo foi alcançado
            if (noAtual == noAlvo)
            {
                RetraçarCaminho(noInicial, noAlvo);
                return;
            }

            // Avaliação dos blocos adjacentes
            foreach (No vizinho in PegarVizinhos(noAtual))
            {
                if (!vizinho.caminhavel || listaFechada.Contains(vizinho)) continue;

                int novoCustoParaVizinho = noAtual.gCost + PegarDistancia(noAtual, vizinho);
                if (novoCustoParaVizinho < vizinho.gCost || !listaAberta.Contains(vizinho))
                {
                    vizinho.gCost = novoCustoParaVizinho;
                    vizinho.hCost = PegarDistancia(vizinho, noAlvo);
                    vizinho.pai = noAtual;

                    if (!listaAberta.Contains(vizinho)) listaAberta.Add(vizinho);
                }
            }
        }
    }

    void RetraçarCaminho(No inicio, No fim)
    {
        List<No> caminho = new List<No>();
        No noAtual = fim;

        while (noAtual != inicio)
        {
            caminho.Add(noAtual);
            noAtual = noAtual.pai; // Volta pelos blocos rastreando de onde vieram
        }
        caminho.Reverse(); // Inverte para a ordem correta de caminhada
        caminhoAtual = caminho;
    }

    int PegarDistancia(No a, No b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);
        return distX + distY;
    }

    List<No> PegarVizinhos(No no)
    {
        List<No> vizinhos = new List<No>();
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = no.gridX + dx[i];
            int checkY = no.gridY + dy[i];

            if (checkX >= 0 && checkX < malhaDeNos.GetLength(0) && checkY >= 0 && checkY < malhaDeNos.GetLength(1))
            {
                vizinhos.Add(malhaDeNos[checkX, checkY]);
            }
        }
        return vizinhos;
    }
}

public class No
{
    public bool caminhavel;
    public int gridX;
    public int gridY;
    public int gCost;
    public int hCost;
    public No pai;

    public int fCost { get { return gCost + hCost; } }

    public No(bool _caminhavel, int _x, int _y)
    {
        caminhavel = _caminhavel;
        gridX = _x;
        gridY = _y;
    }
}