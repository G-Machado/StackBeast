using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEffects : MonoBehaviour
{
    // Triggers punch explosion effect at player's script
    public void PunchEffect()
    {
        PlayerMovement.instance.ApplyPunchEffect();
    }
}
