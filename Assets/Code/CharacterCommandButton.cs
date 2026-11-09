
using UnityEngine;

public class CharacterCommandButton : MonoBehaviour
{
    [Header("Command References")]
    [SerializeField] private CommandModeManager commandManager;

    [Header("Character")]
    [SerializeField] private Transform character;

    public void SelectThisCharacter()
    {
        if (commandManager == null || character == null)
            return;

        commandManager.SelectCharacter(character);
    }
}

