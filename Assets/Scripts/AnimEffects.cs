using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEffects : MonoBehaviour
{
    public void PunchEffect()
    {
        PlayerMovement player = transform.parent.GetComponent<PlayerMovement>();
        if(!player) { Debug.LogError("PlayerMovement component not found."); return; }

        player.ApplyPunchEffect();
    }
}
