using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ColorBoard : MonoBehaviour
{
    [SerializeField] private GameObject AcceptanceArea;
    [SerializeField] private string barCollectibleTag = "BarCollectible";

    public List<GameObject> Collectibles;
    public TextMeshPro collectibleCountDisplay;

    public Transform StartPoint;

    public CollectibleType CollectibleType;

    public MagneticaAnchorPoint MagneticaAnchorPoint;
    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
        if(other.CompareTag(barCollectibleTag))
        {
            //check if item collected is acceptable to the board;
            if(CollectibleType == other.GetComponent<Collectible>().collectibleType)
            {
                Collectibles.Add(other.gameObject);
                other.transform.SetParent(AcceptanceArea.transform);
                MagneticaAnchorPoint.doAttract = true;
                RearrangeCollectibles();
                collectibleCountDisplay.text = Collectibles.Count.ToString();
            }
        }
    }

    void RearrangeCollectibles() {
        int count = 0;
        int limit = 5;
        for (int i = 0; i < Collectibles.Count; i++)
        {
            if(i == 0)
            {
                Collectibles[i].transform.position = StartPoint.position;
            }
            else
            {
                Vector3 oldPosition = Collectibles[i-1].transform.position;

                oldPosition.x += 0.8f;
                if (count / limit > 0)
                {
                    oldPosition = StartPoint.position;
                    oldPosition.z -= 1f;
                    count = 0;
                }

                Collectibles[i].transform.SetPositionAndRotation(oldPosition, Quaternion.Euler(0, 0,0));
            }
            count++;
        }
    }

}
