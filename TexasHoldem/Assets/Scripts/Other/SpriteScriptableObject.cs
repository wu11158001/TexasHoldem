using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpriteList", menuName = "MyMenu/Create SpriteList", order = 1)]
public class SpriteScriptableObject : ScriptableObject
{
    public Sprites sprites;
}

[System.Serializable]
public class Sprites
{
    public Sprite[] spritesList;
}
