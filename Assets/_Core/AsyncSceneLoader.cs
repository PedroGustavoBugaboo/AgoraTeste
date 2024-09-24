using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AsyncAdditiveSceneLoader : MonoBehaviour
{
    // Nome da cena que você deseja carregar de forma aditiva
    public string sceneName;


    private void Start()
    {
        LoadSceneAsyncAdditive();
    }
    // Chame essa função para iniciar o carregamento assíncrono e aditivo da cena
    public void LoadSceneAsyncAdditive()
    {
        StartCoroutine(LoadSceneAdditiveCoroutine());
    }

    // Corrotina que faz o carregamento assíncrono da cena de maneira aditiva
    private IEnumerator LoadSceneAdditiveCoroutine()
    {
        // Inicia o carregamento da cena de forma assíncrona e aditiva
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Enquanto a cena não estiver completamente carregada, continue
        while (!asyncLoad.isDone)
        {
            // Aqui você pode colocar uma barra de carregamento ou feedback visual para o jogador
            Debug.Log("Progresso do carregamento: " + (asyncLoad.progress * 100) + "%");

            yield return null; // Espera até o próximo frame
        }

        // O carregamento está completo neste ponto
        Debug.Log("Cena carregada de forma aditiva!");
    }
}
