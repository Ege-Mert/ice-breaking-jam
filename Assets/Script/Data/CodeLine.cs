using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CodeLine
{
    public string text;
    public int difficulty; // 0: Easy, 1: Medium, 2: Hard
    public int pointValue;
}

[CreateAssetMenu(fileName = "CodeLineCollection", menuName = "Game/Code Line Collection")]
public class CodeLineCollection : ScriptableObject
{
    public List<CodeLine> easyLines = new List<CodeLine>();
    public List<CodeLine> mediumLines = new List<CodeLine>();
    public List<CodeLine> hardLines = new List<CodeLine>();
}