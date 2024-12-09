using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public int currentObjectiveIndex = 0;

    //The index will be the objectiveID and the boolean will be whether or not that objective has been completed
    //If the currentObjectiveIndex index is 0, then all things related to objectives should be deactivated. objectiveCompleted[0] should always be false;
    public bool[] objectiveCompleted;
    public string[] objectiveNotifTexts;
    public string[] objectiveLocation;
    public GameObject[] possibleObjectives;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(currentObjectiveIndex == 0 && FindObjectOfType<MinimapCameraController>()) 
        { 
            FindObjectOfType<MinimapCameraController>().objectiveNotif.SetActive(false);
            var tempColor = FindObjectOfType<MinimapCameraController>().objectiveIcon.color;
            tempColor.a = 0f;
            FindObjectOfType<MinimapCameraController>().objectiveIcon.color = tempColor;
        }
        else if(currentObjectiveIndex > 0 && FindObjectOfType<MinimapCameraController>())
        {
            FindObjectOfType<MinimapCameraController>().objectiveNotif.SetActive(true);
            //Will need to do some tagging magic to find the appropriate game object to point to. Do something similar to reflectedID in battle dummy prefab
        }
        else
        {
            Debug.Log("Minimap not found!");
        }
    }

    public void FindObjectiveGameObject()
    {
        bool found = false;
        possibleObjectives = GameObject.FindGameObjectsWithTag("Objective");
        for(int i = 0; i < possibleObjectives.Length; i++)
        {
            if(possibleObjectives[i].GetComponent<ObjectiveID>().id == currentObjectiveIndex)
            {
                FindObjectOfType<MinimapCameraController>().objectivePos = possibleObjectives[i].transform;
                FindObjectOfType<MinimapCameraController>().objectiveNotifText.text = "Objective: " + objectiveNotifTexts[currentObjectiveIndex];
                found = true;
                break;
            }
        }
        if(!found && currentObjectiveIndex != 0) //THIS ACCOUNTS FOR IF WE ARE IN THE WRONG SCENE
        {
            FindObjectOfType<MinimapCameraController>().objectivePos = null;
            FindObjectOfType<MinimapCameraController>().objectiveNotifText.text = "Objective: " + objectiveNotifTexts[currentObjectiveIndex];
            FindObjectOfType<MinimapCameraController>().distanceText.text = "Objective located at " + objectiveLocation[currentObjectiveIndex];
            Debug.Log("What is going on?");
        }
        else
        {
            FindObjectOfType<MinimapCameraController>().distanceText.text = "";
        }
        Debug.Log(found + " | " + currentObjectiveIndex);
    }

    public void SwapObjective(int objectiveIndex)
    {
        if (objectiveIndex >= 0 && objectiveIndex < objectiveCompleted.Length && !objectiveCompleted[objectiveIndex])
        {
            currentObjectiveIndex = objectiveIndex;
            FindObjectiveGameObject();
        }
    }

    public void Reset()
    {
        Destroy(gameObject);
    }
}
