using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Game game;

    private void Update()
    {
        if (!game.gameOver)
        {
            if (Input.GetMouseButtonDown(1))
            {
                game.Flag();
            }
            if (Input.GetMouseButtonDown(0))
            {
                game.Reveal();
            }
        }
    }
}
