using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EmptyPlayer : NetworkBehaviour {

    [SerializeField] private Text errorText = null; 

    private void Start()
    {
        if (isLocalPlayer)
        {
            // - Change to fixed camera (spectate?)
            errorText.text = "Character selection on your side has failed";
        }
        else
        {
            errorText.text = "One of the players got character selection error";
        }
    }
}
