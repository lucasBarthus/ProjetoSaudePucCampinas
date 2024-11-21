using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerUp
{
    void OnPickup(NetworkObject player);
}
