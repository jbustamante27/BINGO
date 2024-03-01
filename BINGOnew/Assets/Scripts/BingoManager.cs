using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class BingoManager : NetworkComponent
{
    public GameObject[] bingoCards;
    public List<int> exclude = new() { };
    public bool stop;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while (MyCore.IsConnected)
        {
            if (IsServer)
            {
                if (GameObject.FindGameObjectWithTag("Client") != null)
                {
                    bingoCards = GameObject.FindGameObjectsWithTag("Client");

                    int r = 0;
                    for (int i = 0; i < bingoCards.Length; i++)
                    {
                        if (bingoCards[i].GetComponent<BingoCard>().isReady)
                        {
                            r++;
                        }
                        if (bingoCards[i].GetComponent<BingoCard>().stop)
                        {
                            stop = true;
                        }
                    }
                    if (r == bingoCards.Length && !stop)
                    {
                        int h = Random.Range(1, 75);
                        for (int i = 0; i < exclude.Count; i++)
                        {
                            if (h == exclude[i])
                            {
                                h = Random.Range(1, 75);
                                i = 0;
                            }
                        }
                        exclude.Add(h);
                        for (int i = 0; i < bingoCards.Length; i++)
                        {
                            bingoCards[i].GetComponent<BingoCard>().SendUpdate("Roll",h.ToString());
                            yield return new WaitForSeconds(MyId.UpdateFrequency);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(MyId.UpdateFrequency);
        }
    }
   
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
