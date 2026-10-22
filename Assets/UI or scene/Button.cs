using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // 씬 이름으로 로드하는 버전
    public void StartGame()
    {
        // 빌드 세팅에 등록한 게임 씬 이름으로 바꿔줘
        SceneManager.LoadScene("Map");
    }

    public void ExitGame()
    {
        // 에디터에서 테스트할 때도 종료처럼 보이게
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
