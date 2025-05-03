using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
      // Called by the Play button’s OnClick()
      public void OnPlayPressed()
      {
        SceneManager.LoadScene("WebGLMergeSTT");
      }

      // And if you want a “Back” button in Chatbot:
      public void OnBackToMenu()
      {
        SceneManager.LoadScene("MainMenu");
      }
}
