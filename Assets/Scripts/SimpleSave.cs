using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSave : MonoBehaviour
{
    public void Save(Unit[] activePartyUnits)
    {
        ES3.Save("activePartyUnits", activePartyUnits);
    }
}
