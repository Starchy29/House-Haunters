using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Monster tracked;
    [SerializeField] private GameObject VisualBar;
    private SpriteRenderer barColor;
    public bool IsAccurate { get { return currentWidth == DetermineWidth(tracked.Health); } }
    public int RepresentedHealth { get; private set; }

    private float maxWidth;
    private float localLeft;
    private float currentWidth;

    private static Color fullHue = Color.green;
    private static Color midHue = Color.yellow;
    private static Color lowHue = Color.red;

    void Awake() {
        maxWidth = VisualBar.transform.localScale.x;
        currentWidth = maxWidth;
        localLeft = VisualBar.transform.localPosition.x - maxWidth / 2f;
        barColor = VisualBar.GetComponent<SpriteRenderer>();
        RepresentedHealth = -1;
    }

    public void UpdateDisplay(float deltaTime, int targetHealth) {
        float targetWidth = DetermineWidth(targetHealth);
        float difference = targetWidth - currentWidth;
        if(difference == 0) {
            RepresentedHealth = targetHealth;
            return;
        }

        float change = deltaTime * (Mathf.Sign(difference) * 0.3f + 5f * difference / maxWidth);
        if(Mathf.Abs(change) > Mathf.Abs(difference)) {
            currentWidth = targetWidth;
            RepresentedHealth = targetHealth;
        } else {
            currentWidth += change;
        }

        // update visual bar
        Vector3 scale = VisualBar.transform.localScale;
        scale.x = currentWidth;
        VisualBar.transform.localScale = scale;
        Vector3 position = VisualBar.transform.localPosition;
        position.x = localLeft + currentWidth / 2f;
        VisualBar.transform.localPosition = position;

        Color newColor = fullHue;
        float percentLeft = currentWidth / maxWidth;
        if(percentLeft < 0.66 && percentLeft > 0.33f) {
            float percentFull = (percentLeft - 0.33f) * 3f;
            newColor = percentFull * fullHue + (1f - percentFull) * midHue;
        }
        else if(percentLeft < 0.33f) {
            float pecrentMid = percentLeft * 3f;
            newColor = pecrentMid * midHue + (1f - pecrentMid) * lowHue;
        }
        barColor.color = newColor;
    }

    private float DetermineWidth(int health) {
        return maxWidth * health / tracked.Stats.Health;
    }
}
