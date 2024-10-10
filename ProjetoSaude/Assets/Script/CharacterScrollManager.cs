using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScrollManager : MonoBehaviour
{
    private int selectedCharacterIndex = -1;
    public GameObject characterButtonPrefab; // Arraste o prefab do botão de personagem aqui
    public Transform content; // Arraste o objeto Content do Scroll View aqui
    public Image characterImage; // Arraste a imagem do personagem aqui
    public TextMeshProUGUI characterNameText; // Arraste o texto do nome do personagem aqui

    // Lista de nomes de personagens e suas imagens (substitua pelo seu array de dados)
    private CharacterData[] characterDataArray = {
        new CharacterData("Personagem 1", "Caminho/Para/Sprite1"),
        new CharacterData("Personagem 2", "Caminho/Para/Sprite2"),
        new CharacterData("Personagem 3", "Caminho/Para/Sprite3"),
        new CharacterData("Personagem 4", "Caminho/Para/Sprite4")
    };

    private void Start()
    {
        PopulateCharacterList();
    }

    private void PopulateCharacterList()
    {
        for (int i = 0; i < characterDataArray.Length; i++)
        {
            CharacterData data = characterDataArray[i];

            // Instancia um novo botão de personagem
            GameObject newCharacterButton = Instantiate(characterButtonPrefab, content);

            // Configura o texto do nome do personagem
            TextMeshProUGUI buttonText = newCharacterButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = data.characterName;

            // Configura a imagem do personagem no botão
            Image buttonImage = newCharacterButton.GetComponentInChildren<Image>();
            buttonImage.sprite = Resources.Load<Sprite>(data.spritePath); // Certifique-se de que o caminho esteja correto

            // Adiciona um listener para o botão
            Button button = newCharacterButton.GetComponent<Button>();
            int index = i; // Captura o índice atual
            button.onClick.AddListener(() => OnCharacterSelected(data, index)); // Passa o índice
        }
    }

    private void OnCharacterSelected(CharacterData selectedCharacter, int index)
    {
        // Salva o índice do personagem selecionado
        selectedCharacterIndex = index;

        // Atualiza o texto e a imagem do personagem
        characterNameText.text = selectedCharacter.characterName;
        characterImage.sprite = Resources.Load<Sprite>(selectedCharacter.spritePath); // Atualiza a imagem do personagem

        // Chama o método RPC para enviar a seleção para o Fusion
        FusionManager.Instance.RPCSelectCharacter(selectedCharacterIndex);
    }
}

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public string spritePath;

    public CharacterData(string name, string path)
    {
        characterName = name;
        spritePath = path;
    }
}