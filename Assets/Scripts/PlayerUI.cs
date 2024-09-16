using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI followersText;
    private void OnEnable() {
        PlayerLength.ChangedLengthEvent += UpdateFollowers;
    }
    private void OnDisable() {
        PlayerLength.ChangedLengthEvent -= UpdateFollowers;
    }

    private void UpdateFollowers(ushort count) {
        followersText.text = count.ToString();
    }
}
