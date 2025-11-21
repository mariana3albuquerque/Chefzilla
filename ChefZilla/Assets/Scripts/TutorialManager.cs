using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] TutorialEndPanel tutorialEndPanel;

    bool moved;
    bool pickedFood;
    bool placedFood;
    bool finished;

    void Update()
    {
        if (finished) return;

        // 1) Detectar se o jogador andou (alguma entrada de movimento)
        if (!moved)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h * h + v * v > 0.01f)
            {
                moved = true;
                CheckCompleted();
            }
        }
    }

    // 2) Chamar isso quando pegar comida no fog�o
    public void MarkPickedFood()
    {
        if (finished) return;
        pickedFood = true;
        CheckCompleted();
    }

    // 3) Chamar isso quando colocar comida na mesa
    public void MarkPlacedFood()
    {
        if (finished) return;
        placedFood = true;
        CheckCompleted();
    }

    void CheckCompleted()
    {
        if (finished) return;

        if (moved && pickedFood && placedFood)
        {
            finished = true;
            tutorialEndPanel.Show();   // aparece o banner "tutorial conclu�do"
        }
    }
}
