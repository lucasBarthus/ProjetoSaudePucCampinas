using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public List<NetworkObject> characterPrefabs; // Lista de personagens disponíveis para seleção

    // O índice do personagem atualmente selecionado
    private int currentCharacterIndex = 0;

    // O NetworkRunner, necessário para instanciar/despachar personagens na rede
    private NetworkRunner runner;

    // Referência ao objeto do personagem atual instanciado
    private NetworkObject currentCharacter;

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("NetworkRunner não encontrado na cena.");
            return;
        }

        // Não instanciamos nenhum personagem aqui, já que o personagem inicial já está criado.
        // No entanto, garantimos que `currentCharacter` esteja referenciando o personagem inicial.
        currentCharacter = FindObjectOfType<NetworkObject>(); // Supondo que o personagem inicial já está na cena
    }

    // Função chamada ao pressionar o botão "Next"
    public void NextCharacter()
    {
        // Aumenta o índice e volta ao início se ultrapassar o tamanho da lista
        currentCharacterIndex = (currentCharacterIndex + 1) % characterPrefabs.Count;
        SelectCharacter(currentCharacterIndex);
    }

    // Função chamada ao pressionar o botão "Previous"
    public void PreviousCharacter()
    {
        // Diminui o índice e volta ao final se passar do primeiro elemento
        currentCharacterIndex = (currentCharacterIndex - 1 + characterPrefabs.Count) % characterPrefabs.Count;
        SelectCharacter(currentCharacterIndex);
    }

    // Seleciona o personagem atual e destrói o anterior, se houver
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

    // Função para confirmar a seleção e marcar o personagem como jogável
    public void ConfirmSelection()
    {
        Debug.Log("Personagem confirmado: " + currentCharacter.name);
        // Aqui você pode configurar o personagem selecionado para ser controlado pelo jogador
    }
}