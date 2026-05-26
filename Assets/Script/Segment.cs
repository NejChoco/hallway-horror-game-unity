using UnityEngine;

public class Segment : MonoBehaviour
{
    public Transform socketIn;
    public Transform socketOut;
    public bool isAnomaly = false;

    // THE FIX: A permanent ID number for the memory bank
    public int globalIndex = 0;
}