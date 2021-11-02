using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MainGame
{
    public class PlayerNameInput : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private Button _continueButton;

        public static string DisplayName { get; private set; }
        private const string PlayerPrefsNameKey = "PlayerName";

        private void Start()
        {
            SetUpInputField();
        }

        private void SetUpInputField()
        {
            // reload the name from PlayerPrefs if there is one
            if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
                return;

            string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
            _nameInputField.text = defaultName;
            SetPlayerName(defaultName);
        }

        public void SetPlayerName(string name)
        {
            _continueButton.interactable = !string.IsNullOrEmpty(name);
        }

        public void SavePlayerName()
        {
            DisplayName = _nameInputField.text;
            PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
        }
    }
}
