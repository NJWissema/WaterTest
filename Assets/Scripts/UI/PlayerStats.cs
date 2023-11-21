using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public List<int> maxStats; // 0 is oxygen, 1 is health
    List<int> currentStats;

    // Coroutines for stats
    Coroutine oxygenCo;

    bool swimCheck;

    [Header("UI")]
    public List<Slider> statBars;
    public List<TextMeshProUGUI> statNums;


    // Start is called before the first frame update
    void Start()
    {
        currentStats = new List<int>(maxStats);


        // initialise the statBars
        for (int i=0; i<maxStats.Count; i++)
        {
            statBars[i].maxValue = maxStats[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerController.isSwimming && swimCheck == true)
        {
            swimCheck = false;
            StopCoroutine(oxygenCo);
            ChangeStat(0, maxStats[0]);
        }
        if (PlayerController.isSwimming && swimCheck == false)
        {
            swimCheck = true;
            oxygenCo = StartCoroutine(DecreaseStats(0, 3, 3));
        }


        for(int i=0; i<maxStats.Count; i++)
        {
            statBars[i].value = currentStats[i];
            statNums[i].text = currentStats[i].ToString();
        }
    }

    IEnumerator DecreaseStats(int stat, int interval, int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if(currentStats[stat] > 0)
            {
                currentStats[stat] = Mathf.Max(currentStats[stat] - amount, 0);
            }
        }
    }

    public void ChangeStat(int stat, int refreshAmount)
    {
        if(refreshAmount > 0)
        {
            currentStats[stat] = Mathf.Min(currentStats[stat] + refreshAmount, maxStats[stat]);
        }
        else
        {
            currentStats[stat] = Mathf.Max(currentStats[stat] + refreshAmount, 0);
        }
    }
}
