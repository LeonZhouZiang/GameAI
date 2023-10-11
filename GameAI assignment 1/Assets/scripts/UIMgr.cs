using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour
{

    public static UIMgr Instance;
    public Text text;
    public Text collision;
    private int cols = 0;
    void Awake()
    {
        Instance = this;
    }

    public void UpdateCollision()
    {
        cols++;
        collision.text = "Collisions: " + cols.ToString();
    }

    public void ChangeText(string content)
    {
        text.text = content;
    }
}
