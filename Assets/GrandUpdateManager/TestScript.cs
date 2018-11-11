using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : OverridableMonoBehaviour, IUpdatable {

    private float randomRange = 1;

    public override void UpdateMe()
    {
        transform.position = new Vector3(Random.Range(-randomRange, randomRange), Random.Range(-randomRange, randomRange), Random.Range(-randomRange, randomRange));
    }
}
