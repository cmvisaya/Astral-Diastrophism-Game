using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCamController : MonoBehaviour
{
    Cinemachine.CinemachineDollyCart cart;
    Cinemachine.CinemachineVirtualCamera cam;

    public Cinemachine.CinemachineSmoothPath[] paths;

    public Transform lookTarget;

    int currentPath = 0;

    private void Awake()
    {
        cart = GetComponent<Cinemachine.CinemachineDollyCart>();
        cam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        Reset();
    }

    public void Reset(Transform overShoulderTarget, bool lookAtEnemyParty, Unit unit)
    {
        StopAllCoroutines();
        currentPath = 0;
        cart.m_Path = null;
        cam.m_LookAt = null;
        cart.m_Position = 0;

        Vector3 targetPos;
        Quaternion targetRot;
        switch (lookAtEnemyParty)
        {
            case true:
                targetPos = overShoulderTarget.position + new Vector3(0.45f, 1.15f + unit.sizeOffset - 1, 0.75f);
                targetRot = Quaternion.Euler(0, 245, 0); break;
            case false:
                targetPos = overShoulderTarget.position + new Vector3(-0.45f, 1.15f + unit.sizeOffset - 1, -0.75f); //FROM ACTUAL: +1, +0, +0
                targetRot = Quaternion.Euler(0, 65, 0); break;
        }
        transform.position = targetPos;
        transform.rotation = targetRot;

        StartCoroutine(ChangeTrack());
    }

    public void ChangeLookTarget(Transform target)
    {
        StopAllCoroutines();
        currentPath = 0;
        cart.m_Path = null;
        cam.m_LookAt = target;
    }

    public void ChangePos(Vector3 newPos, float xRot, float yRot, float zRot)
    {
        StopAllCoroutines();
        currentPath = 0;
        cart.m_Path = null;
        transform.position = newPos;
        transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
    }

    private void Reset()
    {
        StopAllCoroutines();
        currentPath = 0;
        cart.m_Path = null;
        cam.m_LookAt = null;
        cart.m_Position = 0;
        StartCoroutine(ChangeTrack());
    }

    IEnumerator ChangeTrack()
    {
        yield return new WaitForSeconds(12);
        cam.m_LookAt = lookTarget;
        if (currentPath >= paths.Length) { currentPath = 0; }
        cart.m_Path = paths[currentPath];
        cart.m_Position = 0;
        currentPath++;
        StartCoroutine(ChangeTrack());
    }
}
