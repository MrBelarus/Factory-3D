using UnityEngine;

/// <summary>
/// Script for clock in the main menu
/// </summary>
public class ClockShowTime : MonoBehaviour
{
    [SerializeField] private Transform secondsArrow;
    [SerializeField] private Transform minutesArrow;
    [SerializeField] private Transform hourArrow;

    [SerializeField] private float updateArrowsDelaySeconds = 1f;
    private float timer = 0f;

    private const int minuteDeltaAngle = 6;
    private const int secondsDeltaAngle = 6;
    private const int hourDeltaAngle = 30;

    private void Awake()
    {
        timer = updateArrowsDelaySeconds;
    }

    private void FixedUpdate()
    {
        timer += Time.unscaledDeltaTime;

        if (timer > updateArrowsDelaySeconds)
        {
            System.DateTime currentTime = System.DateTime.Now;

            float minutesAngle = currentTime.Minute * minuteDeltaAngle;
            float hourAngle = currentTime.Hour * hourDeltaAngle;
            float secondsAngle = currentTime.Second * secondsDeltaAngle;

            secondsArrow.rotation = Quaternion.Euler(new Vector3(0f, secondsAngle, 0f));
            minutesArrow.rotation = Quaternion.Euler(new Vector3(0f, minutesAngle, 0f));
            hourArrow.rotation = Quaternion.Euler(new Vector3(0f, hourAngle, 0f));
        }
    }
}
