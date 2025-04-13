using UnityEngine;

/// <summary>
/// Provides a method for computing the intercept time for a projectile to hit a moving target,
/// specifically a balloon. The algorithm iteratively finds a time <c>tIntercept</c> such that the distance
/// from the tower's position to the balloon's predicted position at that time matches the distance
/// a projectile would travel in the same period (i.e., <c>projectileSpeed * tIntercept</c>).
/// </summary>
public static class InterceptSolver
{
    /// <summary>
    /// Computes the intercept time (<c>tIntercept</c>) using an iterative approach.
    /// The algorithm determines the time for which the distance from <paramref name="towerPos"/>
    /// to the balloon's predicted position (using <paramref name="balloonMov"/>) equals
    /// <paramref name="projectileSpeed"/> multiplied by that time.
    /// </summary>
    /// <param name="towerPos">The position of the tower as a <see cref="Vector2"/>.</param>
    /// <param name="balloonMov">
    /// The <see cref="BalloonMovement"/> component used to predict the balloon's future position.
    /// </param>
    /// <param name="balloonSpeed">
    /// The speed of the balloon. (Alternatively, this could be read from <c>balloonMov.balloon.speed</c>.)
    /// </param>
    /// <param name="projectileSpeed">The speed of the projectile.</param>
    /// <param name="maxIterations">Maximum number of iterations to perform (default is 10).</param>
    /// <param name="epsilon">
    /// The tolerance used to determine convergence between successive time estimates (default is 0.01f).
    /// </param>
    /// <returns>
    /// The computed intercept time as a float. If convergence is not reached within <paramref name="maxIterations"/>,
    /// the best approximation is returned.
    /// </returns>
    public static float FindInterceptTime(
        Vector2 towerPos,
        BalloonMovement balloonMov,
        float balloonSpeed,
        float projectileSpeed,
        int maxIterations = 10,
        float epsilon = 0.01f)
    {
        // 1) Compute an initial guess for the intercept time.
        // The naive approach: Assume the projectile takes time t to cover the current distance.
        float dist = Vector2.Distance(towerPos, balloonMov.transform.position);
        float t = dist / projectileSpeed; // initial guess

        // Iterate to refine the intercept time.
        for (int i = 0; i < maxIterations; i++)
        {
            // 2) Predict the balloon's position after time t.
            Vector2 balloonFuturePos = balloonMov.PredictPositionInFuture(t);

            // 3) Calculate the distance from the tower to the predicted future position.
            float newDist = Vector2.Distance(towerPos, balloonFuturePos);

            // 4) Compute the time required for the projectile to traverse this new distance.
            float newT = newDist / projectileSpeed;

            // If the change between iterations is within the acceptable tolerance, convergence is reached.
            if (Mathf.Abs(newT - t) < epsilon)
            {
                return newT;
            }

            // Update the estimate and continue iterating.
            t = newT;
        }

        // Return the best estimated intercept time after max iterations.
        return t;
    }
}
