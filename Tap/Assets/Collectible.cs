using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleType collectibleType;
 
}


public enum CollectibleType
{
    REED, GOLD, EMERALD
}