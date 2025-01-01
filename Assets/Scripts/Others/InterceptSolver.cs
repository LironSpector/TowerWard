using UnityEngine;

public static class InterceptSolver
{
    /// <summary>
    /// Finds the time 'tIntercept' for which distance(towerPos, balloonPosAtTime(tIntercept)) == projectileSpeed * tIntercept.
    /// We do a simple iterative approach until convergence or up to maxIter.
    /// </summary>
    public static float FindInterceptTime(
        Vector2 towerPos,
        BalloonMovement balloonMov,
        float balloonSpeed,  // or we can read from balloonMov.balloon.speed
        float projectileSpeed,
        int maxIterations = 10,
        float epsilon = 0.01f)
    {
        // 1) Start with a naive guess for t: 
        //    the distance ignoring balloon movement / projectile speed
        float dist = Vector2.Distance(towerPos, balloonMov.transform.position);
        float t = dist / projectileSpeed; // naive guess

        for (int i = 0; i < maxIterations; i++)
        {
            // 2) Predict where balloon will be after t
            Vector2 balloonFuturePos = balloonMov.PredictPositionInFuture(t);

            // 3) distance from tower to that future pos
            float newDist = Vector2.Distance(towerPos, balloonFuturePos);

            // 4) time the projectile requires for that distance
            float newT = newDist / projectileSpeed;

            if (Mathf.Abs(newT - t) < epsilon)
            {
                // converged
                return newT;
            }

            t = newT;
        }

        return t; // best guess after max iterations
    }
}
