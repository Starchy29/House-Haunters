using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXAnimator : IMoveAnimator
{
    public bool Completed { get { return time > 1f; } }

    private CaptureVFX visual;
    private float time;

    public VFXAnimator(CaptureVFX visual, Color color) {
        this.visual = visual;
        visual.SetColor(color);
    }

    public void Start() {
        visual.SetRadius(0f);
    }

    public void Update(float deltaTime) {
        time += 1.0f * Time.deltaTime;
        float endT = time;
        if(time <= 0.5f) {
            // exponential while radius is expanding
            endT *= 2f; // become domain 0-1
            endT = endT - 1.0f;
            endT = 1f + endT * endT * endT * endT * endT; // must be an odd exponent
            endT /= 2f; // become range 0-0.5
        }
        visual.SetRadius(endT);
    }
}