using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    // 씬 이름으로 로드하는 버전
    public void StartGame()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
