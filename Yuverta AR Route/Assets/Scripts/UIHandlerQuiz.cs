using UnityEngine;
using UnityEngine.UI;

public class UIHandlerQuiz : MonoBehaviour
{
    public int index;
    private Button button;
    private QuizManager quizManager;

    void Start()
    {
        button = GetComponent<Button>();
        quizManager = FindObjectOfType<QuizManager>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        quizManager.OnOptionSelected(index);
    }
}