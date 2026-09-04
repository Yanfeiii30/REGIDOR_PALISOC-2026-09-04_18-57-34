using UnityEngine;

public enum TargetColor
{
    Red,
    Blue,
    Green,
    Yellow
}

public class ColorTarget : MonoBehaviour
{
    [Header("Required Color")]
    public TargetColor requiredColor;

    [HideInInspector]
    public bool isCompleted;

    public bool CheckColor(string shotColorName)
    {
        if (isCompleted)
        {
            return false;
        }

        bool isCorrect = shotColorName == requiredColor.ToString().ToUpper();

        if (isCorrect)
        {
            isCompleted = true;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RegisterCorrect();
            }

            Debug.Log(gameObject.name + " colored correctly!");
        }
        else
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RegisterMistake();
            }

            Debug.Log(
                gameObject.name + " colored incorrectly. Required: " + requiredColor
            );
        }

        return isCorrect;
    }
}