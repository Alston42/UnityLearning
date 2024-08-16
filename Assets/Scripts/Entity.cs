using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator anime;

    // Start is called before the first frame update
    protected virtual void Start() {
        rb = GetComponent<Rigidbody2D>();
        anime = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    protected virtual void Update() {
        
    }
}
