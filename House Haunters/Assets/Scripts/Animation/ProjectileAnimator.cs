﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAnimator : IMoveAnimator
{
    private float speed;
    private Vector3 direction;
    private Vector3 endPosition;
    private GameObject projectile;
    private bool finished;

    public bool Completed { get { return finished; } }

    public ProjectileAnimator(GameObject projectilePrefab, Vector3 startPosition, Vector3 endPosition, float speed, bool rotate = true) {
        this.speed = speed;
        this.endPosition = endPosition;
        direction = (endPosition - startPosition).normalized;

        projectile = GameObject.Instantiate(projectilePrefab);
        projectile.transform.position = startPosition;
        if(rotate) {
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x));
        }
        projectile.SetActive(false);
    }

    public void Start() {
        projectile.SetActive(true);
    }

    public void Update(float deltaTime) {
        projectile.transform.position += speed * deltaTime * direction.normalized;

        // check if passed the target
        if(Vector3.Dot(direction, endPosition - projectile.transform.position) <= 0) {
            finished = true;
            GameObject.Destroy(projectile);
        }
    }
}
