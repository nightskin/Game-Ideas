using UnityEngine;

public abstract class EnemyBaseState
{
    public abstract void Start(EnemyStateMachine enemy);
    public abstract void Update(EnemyStateMachine enemy);
    public abstract void CollisionEnter(EnemyStateMachine enemy, ControllerColliderHit other);
}
