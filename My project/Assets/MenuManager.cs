using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
      private string surveyURL = "https://forms.office.com/Pages/ResponsePage.aspx?id=fcKXmj64lEazU1S9vxirWzmPEXmfCotKhQujNVqK70VURUZYNjhQNTVEOTdGRlVKWUVNMUlFUE5HQy4u";
      
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
      
      public void OpenLink()
      {
          Application.OpenURL(surveyURL);
      }
}
