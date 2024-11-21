using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public List<NetworkObject> characterPrefabs; // Lista de personagens dispon�veis para sele��o

    // O �ndice do personagem atualmente selecionado
    private int currentCharacterIndex = 0;

    // O NetworkRunner, necess�rio para instanciar/despachar personagens na rede
    private NetworkRunner runner;

    // Refer�ncia ao objeto do personagem atual instanciado
    private NetworkObject currentCharacter;

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("NetworkRunner n�o encontrado na cena.");
            return;
        }

        // N�o instanciamos nenhum personagem aqui, j� que o personagem inicial j� est� criado.
        // No entanto, garantimos que `currentCharacter` esteja referenciando o personagem inicial.
        currentCharacter = FindObjectOfType<NetworkObject>(); // Supondo que o personagem inicial j� est� na cena
    }

    // Fun��o chamada ao pressionar o bot�o "Next"
    public void NextCharacter()
    {
        // Aumenta o �ndice e volta ao in�cio se ultrapassar o tamanho da lista
        currentCharacterIndex = (currentCharacterIndex + 1) % characterPrefabs.Count;
        SelectCharacter(currentCharacterIndex);
    }

    // Fun��o chamada ao pressionar o bot�o "Previous"
    public void PreviousCharacter()
    {
        // Diminui o �ndice e volta ao final se passar do primeiro elemento
        currentCharacterIndex = (currentCharacterIndex - 1 + characterPrefabs.Count) % characterPrefabs.Count;
        SelectCharacter(currentCharacterIndex);
    }

    // Seleciona o personagem atual e destr�i o anterior, se houver
    private void SelectCharacter(int index)
    {
        if (currentCharacter != null)
        {
            // Despachar o personagem atual
            runner.Despawn(currentCharacter);
        }

        // Instanciar o novo personagem na rede
        runner.Spawn(characterPrefabs[index], Vector3.zero, Quaternion.identity, null, (runner, obj) =>
        {
            currentCharacter = obj;
            Debug.Log("Selecionou o personagem: " + currentCharacter.name);
        });
    }

    // Fun��o para confirmar a sele��o e marcar o personagem como jog�vel
    public void ConfirmSelection()
    {
        Debug.Log("Personagem confirmado: " + currentCharacter.name);
        // Aqui voc� pode configurar o personagem selecionado para ser controlado pelo jogador
    }
}