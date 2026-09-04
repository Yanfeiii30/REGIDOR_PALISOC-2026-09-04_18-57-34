using System.Collections;
using TMPro;
using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    public static Color CurrentColor { get; private set; } = Color.red;
    public static string CurrentColorName { get; private set; } = "RED";

    [Header("Color Notification")]
    public TextMeshProUGUI colorNoticeText;
    public float noticeDuration = 1.5f;

    private Coroutine noticeCoroutine;

    void Start()
    {
        if (colorNoticeText != null)
        {
            colorNoticeText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectRed();
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectBlue();
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectGreen();
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectYellow();
    }

    public void SelectRed() => SetColor(Color.red, "RED");
    public void SelectBlue() => SetColor(Color.blue, "BLUE");
    public void SelectGreen() => SetColor(Color.green, "GREEN");
    public void SelectYellow() => SetColor(Color.yellow, "YELLOW");

    void SetColor(Color newColor, string colorName)
    {
        CurrentColor = newColor;
        CurrentColorName = colorName;

        ShowColorNotice();
    }

    void ShowColorNotice()
    {
        if (colorNoticeText == null) return;

        if (noticeCoroutine != null)
        {
            StopCoroutine(noticeCoroutine);
        }

        noticeCoroutine = StartCoroutine(ShowNoticeRoutine());
    }

    IEnumerator ShowNoticeRoutine()
    {
        colorNoticeText.text = "BULLET COLOR: " + CurrentColorName;

        Color noticeColor = CurrentColor;
        noticeColor.a = 1f;
        colorNoticeText.color = noticeColor;

        colorNoticeText.gameObject.SetActive(true);

        yield return new WaitForSeconds(noticeDuration);

        colorNoticeText.gameObject.SetActive(false);
        noticeCoroutine = null;
    }
}