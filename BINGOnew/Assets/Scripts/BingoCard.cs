using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;
using TMPro;
using System;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Button = UnityEngine.UI.Button;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using System.Linq;
using System.Reflection;


public class BingoCard : NetworkComponent
{
    private const int gridSize = 5;
    private const int rowSize = 5;
    private const int colSize = 5;

    private bool[] checker = new bool[gridSize * gridSize];
    private int[] bingo = new int[gridSize * gridSize];
    private int roll = 0;
    public bool isReady;
    private int b = 0;
    public bool stop = false;
    private string receivedValue;

    public override void HandleMessage(string flag, string value)
    {
        receivedValue = value;
        if (flag == "Roll")
        {
            if (IsServer)
            {
                roll = int.Parse(value);
                CheckAndUpdate();
            }

            if (IsClient && value != string.Empty)
            {
                isReady = false;
                roll = int.Parse(value);
                StartCoroutine(Check());
                SendCommand("Roll", roll.ToString());
                isReady = true;
            }
        }

        if (flag == "Create")
        {
            if (IsServer)
            {
                int i = int.Parse(value);
                if (bingo[i] == 0)
                {
                    InitializeBingoCell(i);
                }
                SendUpdate("Create", bingo[i].ToString());
            }

            if (IsClient)
            {
                UpdateBingoCell();
            }
        }

        if (flag == "R")
        {
            if (IsServer)
            {
                isReady = true;
                SendUpdate("R", isReady.ToString());
            }
            if (IsClient)
            {
                this.transform.GetChild(25).gameObject.GetComponent<Button>().interactable = false;
                this.transform.GetChild(25).gameObject.GetComponent<Image>().color = Color.green;
            }
        }
    }

    private void InitializeBingoCell(int index)
    {
        int col = index % colSize;
        int row = index / colSize;
        //if cell is middle cell, print nothing
        /*if ((row == colSize / 2) && (col == colSize / 2))
        {
            return;
        }*/

        int[] columnNumbers = new int[rowSize];
        for (int i = 0; i < rowSize; i++)
        {
            columnNumbers[i] = bingo[i * colSize + col];
        }
        int generatedNumber;
        do
        {
            generatedNumber = GenerateNumberForColumn(col);
        } while (columnNumbers.Contains(generatedNumber));

        bingo[index] = generatedNumber;
    }

    private int GenerateNumberForColumn(int col)
    {
        if (col == 0)
            return Random.Range(1, 15);
        else if (col == 1)
            return Random.Range(16, 30);
        else if (col == 2)
            return Random.Range(31, 45);
        else if (col == 3)
            return Random.Range(46, 60);
        else // if (col == 4)
            return Random.Range(61, 75);
    }

        
  

    // Inside the UpdateBingoCell method
    private void UpdateBingoCell()
    {
        this.transform.GetChild(b).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = receivedValue;
        bingo[b] = int.Parse(receivedValue);
        b++;
        if (b < bingo.Length)
        {
            SendCommand("Create", b.ToString());
        }
        else
        {
            SendCommand("R", string.Empty);
        }
    }


    private void CheckAndUpdate()
    {
        for (int i = 0; i < bingo.Length; i++)
        {
            if (roll == bingo[i])
            {
                checker[i] = true;
                this.transform.GetChild(i).GetComponent<Image>().color = Color.green;
            }
        }

        if (CheckForWin())
        {
            SendUpdate("Win", string.Empty);
            stop = true;
        }
    }

    private bool CheckForWin()
    {
        for (int i = 0; i < rowSize; i++)
        {
            if (CheckRow(i) || CheckColumn(i))
                return true;
        }

        if (CheckDiagonals())
            return true;

        return false;
    }

    private bool CheckRow(int row)
    {
        int startIndex = row * colSize;
        for (int i = 0; i < colSize; i++)
        {
            if (!checker[startIndex + i])
                return false;
        }
        return true;
    }

    private bool CheckColumn(int col)
    {
        for (int i = 0; i < rowSize; i++)
        {
            if (!checker[i * colSize + col])
                return false;
        }
        return true;
    }

    private bool CheckDiagonals()
    {
        return (CheckLeftDiagonal() || CheckRightDiagonal());
    }

    private bool CheckLeftDiagonal()
    {
        for (int i = 0; i < rowSize; i++)
        {
            if (!checker[i * colSize + i])
                return false;
        }
        return true;
    }

    private bool CheckRightDiagonal()
    {
        for (int i = 0; i < rowSize; i++)
        {
            if (!checker[i * colSize + (rowSize - 1 - i)])
                return false;
        }
        return true;
    }

    // The following methods remain unchanged:

    public override void NetworkedStart()
    {
        if (IsLocalPlayer)
        {
            this.gameObject.GetComponent<Image>().color = Color.green;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (MyCore.IsConnected)
        {
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().enabled = MyCore.IsClient;
            if (IsServer)
            {
                if (IsDirty)
                {
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(MyId.UpdateFrequency);
        }
    }

    public IEnumerator Exit()
    {
        yield return new WaitForSeconds(10);
        StartCoroutine(MyCore.DisconnectServer());
    }

    public void Ready()
    {
        if (IsClient)
        {
            SendCommand("Create", 0.ToString());
        }
    }

    public IEnumerator Check()
    {
        for (int i = 0; i < bingo.Length; i++)
        {
            if (roll == bingo[i])
            {
                this.transform.GetChild(i).GetComponent<Image>().color = Color.red;
            }
        }
        yield return new WaitForSeconds(2);
    }

    void Start()
    {
        GameObject temp = GameObject.FindGameObjectWithTag("Player");
        this.transform.SetParent(temp.transform);
        this.transform.localScale = Vector3.one;
    }
}
