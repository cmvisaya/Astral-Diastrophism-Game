using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseEventTrigger : MonoBehaviour
{
    public int cutsceneID;
    public int petId = -1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            bool[] replayArray = FindObjectOfType<DialogueManager>().unreplayablePETPlayed;
            //replay array stores if the unreplayable pause event trigger with the id equal to the index has been played already or not
            if((petId >= 0 && petId < replayArray.Length && !replayArray[petId]))
            {
                replayArray[petId] = true;
                HandleImpassable();
                FindObjectOfType<DialogueManager>().PlayCutscene(cutsceneID);
            }
            else if(petId == -1)
            {
                HandleImpassable();
                FindObjectOfType<DialogueManager>().PlayCutscene(cutsceneID);
            }
            else
            {
                Debug.Log("for the love of pp weiner guy at least let me not collide with this stupid metal ball");
            }
        }
    }

    private void HandleImpassable()
    {
        if(GetComponent<ImpassablePET>())
        {
            if(petId >= 0)
            {
                FindObjectOfType<DialogueManager>().unreplayablePETPlayed[petId] = false;
            }
            GetComponent<ImpassablePET>().ResetPlayerPos();
        }
    }
}
