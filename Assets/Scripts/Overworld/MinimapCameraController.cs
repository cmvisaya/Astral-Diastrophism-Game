using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapCameraController : MonoBehaviour
{
    public static MinimapCameraController Instance;

    public Transform player;
    public Transform pivot;
    public Image playerIcon;
    public Image objectiveIcon;

    public Transform objectivePos;
    public TextMeshProUGUI objectiveNotifText;
    public TextMeshProUGUI distanceText;
    public GameObject objectiveNotif;

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

    void Update()  //AHHHH TRIGONOMETRY FUCK ME
    {
        if(FindObjectOfType<MinimapUI>())
        {
            if (player == null && GameObject.Find("Player") != null)
            {
                Debug.Log("It's found 1");
                player = GameObject.Find("Player").transform;
            }

            if (objectivePos != null && player != null)
            {
                float xDist = objectivePos.position.x - player.position.x;
                float yDist = objectivePos.position.y - player.position.y;
                float zDist = objectivePos.position.z - player.position.z;
                float distanceToObjective = CalculateDistance(xDist, yDist, zDist);
                float angleToObjective = CalculateAngle(zDist, xDist);
                bool higher = yDist > 0f;
                float objIconX = Mathf.Cos(angleToObjective) * 20f;
                float objIconY = Mathf.Sin(angleToObjective) * 20f;
                Vector3 newIconPos = new Vector3(objIconX, objIconY, 0f);

                objectiveIcon.rectTransform.anchoredPosition = newIconPos;
                objectiveIcon.rectTransform.rotation = Quaternion.Euler(0f, 0f, angleToObjective * Mathf.Rad2Deg);

                float colorAlphaScalar = 1f;
                if (distanceToObjective < 5f)
                {
                    colorAlphaScalar = distanceToObjective / 5f - 0.25f;
                    if (colorAlphaScalar < 0f)
                    {
                        colorAlphaScalar = 0f;
                    }
                }

                var tempColor = objectiveIcon.color;
                tempColor.a = colorAlphaScalar;
                objectiveIcon.color = tempColor;

                string higherText = "Lower";
                if (higher) { higherText = "Higher"; }
                distanceToObjective = (int)distanceToObjective;
                distanceText.text = distanceToObjective + "m (" + higherText + ")";
            }
            else
            {
                //objectiveNotif.SetActive(false);

                var tempColor = objectiveIcon.color;
                tempColor.a = 0f;
                objectiveIcon.color = tempColor;
            }
        }
    }

    void LateUpdate()
    {
        if(FindObjectOfType<MinimapUI>())
        {
            if (pivot == null && GameObject.Find("Pivot") != null)
            {
                pivot = GameObject.Find("Pivot").transform;
                Debug.Log("It's found 2");
            }
            if (player != null && pivot != null)
            {
                Vector3 newPosition = player.position;
                newPosition.y = player.position.y + 20f;
                transform.position = newPosition;

                //transform.rotation = Quaternion.Euler(90f, pivot.eulerAngles.y, 0f);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                playerIcon.rectTransform.rotation = Quaternion.Euler(0f, 0f, -pivot.eulerAngles.y + 90f);
            }
        }
    }

    float CalculateDistance(float dx, float dy, float dz)
    {
        return Mathf.Sqrt(Mathf.Pow(dx, 2f) + Mathf.Pow(dy, 2f) + Mathf.Pow(dz, 2f));
    }

    float CalculateAngle(float dx, float dz)
    {
        float angle = 90f - Mathf.Atan2(dz, dx) * 180f / Mathf.PI;
        if (angle > 180f) { angle -= 360f; }
        return angle * Mathf.Deg2Rad;
    }
}
