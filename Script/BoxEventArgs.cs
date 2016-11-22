using UnityEngine;
using System.Collections;
using System;

public class PlayerMoveDistance : EventArgs
{

    private Vector3 distance;
    private GameObject Player;
    public PlayerMoveDistance(Vector3 distance, GameObject player)
    {
        this.distance = distance;
        this.Player = player;
    }
    public Vector3 d {get{return distance;}}
    public GameObject player { get { return Player; } }
    
}
public delegate void PushBoxEvent(object sender, PlayerMoveDistance e);
