using System.Collections.Generic;
using UnityEngine;

public class IAPerseguidor : MonoBehaviour
{
    public Transform jogador;
    public GuiaLabirinto gridMapa;
    public float velocidade = 4f;

    public float tempoDeAtualizacao = 0.2f;
    private float tempoDecorrido = 0f;
    public GameObject telaGameOver;

    private Vector3 ultimaPosicaoConhecida;
    private bool vendoJogador = false; // <-- Nova memória para saber se está te vendo agora

    No[,] malhaDeNos;
    List<No> caminhoAtual = new List<No>();

    void Start()
    {
        ultimaPosicaoConhecida = jogador.position; // Faro inicial

        int largura = gridMapa.mapa.GetLength(0);
        int altura = gridMapa.mapa.GetLength(1);
        malhaDeNos = new No[largura, altura];

        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                bool livre = (gridMapa.mapa[x, y] == 0);
                malhaDeNos[x, y] = new No(livre, x, y);
            }
        }
    }

    void Update()
    {
        tempoDecorrido += Time.deltaTime;
        vendoJogador = false; // Começa o frame achando que não está vendo

        // --- SISTEMA DE VISÃO (RAYCAST) ---
        Vector3 origemOlhos = transform.position + Vector3.up;
        Vector3 alvoPeito = jogador.position + Vector3.up;
        Vector3 direcaoVisao = (alvoPeito - origemOlhos).normalized;
        float distanciaAteJogador = Vector3.Distance(origemOlhos, alvoPeito);

        if (Physics.Raycast(origemOlhos, direcaoVisao, out RaycastHit hit, distanciaAteJogador + 1f))
        {
            if (hit.transform == jogador || hit.transform.IsChildOf(jogador))
            {
                ultimaPosicaoConhecida = jogador.position;
                vendoJogador = true; // Confirma visualmente o alvo
                Debug.DrawRay(origemOlhos, direcaoVisao * distanciaAteJogador, Color.green);
            }
            else
            {
                Debug.DrawRay(origemOlhos, direcaoVisao * distanciaAteJogador, Color.red);
            }
        }

        // --- SISTEMA DE VARREDURA (PATRULHA) ---
        // Se perdeu você de vista, ele vai até onde te viu por último
        if (!vendoJogador)
        {
            Vector3 alvoPlano = new Vector3(ultimaPosicaoConhecida.x, transform.position.y, ultimaPosicaoConhecida.z);
            // Se chegou lá e não te achou...
            if (Vector3.Distance(transform.position, alvoPlano) < 0.5f)
            {
                IniciarVarredura(); // Escolhe um corredor aleatório para investigar!
            }
        }

        // --- CONDIÇÃO DE GAME OVER ---
        if (Vector3.Distance(transform.position, jogador.position) < 3.0f)
        {
            telaGameOver.SetActive(true);
            Time.timeScale = 0f;
            this.enabled = false;
        }

        // --- MATEMÁTICA DA PERSEGUIÇÃO ---
        if (tempoDecorrido >= tempoDeAtualizacao)
        {
            EncontrarCaminho(transform.position, ultimaPosicaoConhecida);
            tempoDecorrido = 0f;
        }

        MoverPeloCaminho();
    }

    void MoverPeloCaminho()
    {
        if (caminhoAtual != null && caminhoAtual.Count > 0)
        {
            // O monstro segue os trilhos da matriz (A*)
            No proximoNo = caminhoAtual[0];
            Vector3 alvoFisico = new Vector3(proximoNo.gridX * gridMapa.tamanhoBloco, transform.position.y, proximoNo.gridY * gridMapa.tamanhoBloco);
            transform.position = Vector3.MoveTowards(transform.position, alvoFisico, velocidade * Time.deltaTime);
        }
        else
        {
            // O caminho acabou (ele chegou no seu bloco). Ele abandona o grid e dá o bote direto na sua coordenada!
            Vector3 alvoExato = new Vector3(ultimaPosicaoConhecida.x, transform.position.y, ultimaPosicaoConhecida.z);
            transform.position = Vector3.MoveTowards(transform.position, alvoExato, velocidade * Time.deltaTime);
        }
    }

    void IniciarVarredura()
    {
        int atualX = Mathf.RoundToInt(transform.position.x / gridMapa.tamanhoBloco);
        int atualY = Mathf.RoundToInt(transform.position.z / gridMapa.tamanhoBloco);

        // Proteção contra quebras
        if (atualX < 0 || atualX >= malhaDeNos.GetLength(0) || atualY < 0 || atualY >= malhaDeNos.GetLength(1)) return;

        No noAtual = malhaDeNos[atualX, atualY];
        List<No> vizinhos = PegarVizinhos(noAtual);
        List<No> vizinhosLivres = new List<No>();

        // Separa só os caminhos onde não tem parede
        foreach (No vizinho in vizinhos)
        {
            if (vizinho.caminhavel) vizinhosLivres.Add(vizinho);
        }

        // Se houver opções, escolhe um bloco vizinho aleatoriamente e o define como o novo "objetivo"
        if (vizinhosLivres.Count > 0)
        {
            No escolhido = vizinhosLivres[Random.Range(0, vizinhosLivres.Count)];
            ultimaPosicaoConhecida = new Vector3(escolhido.gridX * gridMapa.tamanhoBloco, transform.position.y, escolhido.gridY * gridMapa.tamanhoBloco);
        }
    }

    // --- CÓDIGO DO A* ABAIXO PERMANECE INTACTO ---

    void EncontrarCaminho(Vector3 inicio3D, Vector3 fim3D)
    {
        int startX = Mathf.RoundToInt(inicio3D.x / gridMapa.tamanhoBloco);
        int startY = Mathf.RoundToInt(inicio3D.z / gridMapa.tamanhoBloco);
        int alvoX = Mathf.RoundToInt(fim3D.x / gridMapa.tamanhoBloco);
        int alvoY = Mathf.RoundToInt(fim3D.z / gridMapa.tamanhoBloco);

        if (startX < 0 || startX >= malhaDeNos.GetLength(0) || startY < 0 || startY >= malhaDeNos.GetLength(1)) return;
        if (alvoX < 0 || alvoX >= malhaDeNos.GetLength(0) || alvoY < 0 || alvoY >= malhaDeNos.GetLength(1)) return;

        No noInicial = malhaDeNos[startX, startY];
        No noAlvo = malhaDeNos[alvoX, alvoY];

        List<No> listaAberta = new List<No>();
        HashSet<No> listaFechada = new HashSet<No>();
        listaAberta.Add(noInicial);

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

            if (noAtual == noAlvo)
            {
                RetracarCaminho(noInicial, noAlvo);
                return;
            }

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

    void RetracarCaminho(No inicio, No fim)
    {
        List<No> caminho = new List<No>();
        No noAtual = fim;

        while (noAtual != inicio)
        {
            caminho.Add(noAtual);
            noAtual = noAtual.pai;
        }
        caminho.Reverse();
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