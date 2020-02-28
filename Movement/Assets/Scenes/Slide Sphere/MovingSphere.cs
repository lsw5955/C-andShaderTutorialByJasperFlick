using UnityEngine;

public class MovingSphere : MonoBehaviour 
{
    [SerializeField, Range(0f, 1f)]
    float bounciness = 0.5f;

    [SerializeField]
	Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;

    Vector3 velocity;
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    void Update() {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        //playerInput.Normalize();
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        //Vector3 acceleration = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        //velocity += acceleration * Time.deltaTime;
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //处理x轴上的移动速度
        //if (velocity.x < desiredVelocity.x) {
        //    //velocity.x += maxSpeedChange;
        //    velocity.x = Mathf.Min(velocity.x + maxSpeedChange, desiredVelocity.x);
        //}
        //else if (velocity.x > desiredVelocity.x) {
        //    //velocity.x += maxSpeedChange;
        //    velocity.x = Mathf.Max(velocity.x - maxSpeedChange, desiredVelocity.x);
        //}
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        //transform.localPosition += displacement;
        Vector3 newPosition = transform.localPosition + displacement;

        //if (!allowedArea.Contains(newPosition)) {
        if (!allowedArea.Contains(new Vector2(newPosition.x, newPosition.z))) {
            //newPosition = transform.localPosition;
            //newPosition.x = Mathf.Clamp(newPosition.x, allowedArea.xMin, allowedArea.xMax);
            //newPosition.z = Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);

            if (newPosition.x < allowedArea.xMin) {
                newPosition.x = allowedArea.xMin;
                velocity.x = -velocity.x * bounciness;
            }
            else if (newPosition.x > allowedArea.xMax) {
                newPosition.x = allowedArea.xMax;
                velocity.x = -velocity.x * bounciness;
            }
            if (newPosition.z < allowedArea.yMin) {
                newPosition.z = allowedArea.yMin;
                velocity.z = -velocity.z * bounciness;
            }
            else if (newPosition.z > allowedArea.yMax) {
                newPosition.z = allowedArea.yMax;
                velocity.z = -velocity.z * bounciness;
            }
        }

        transform.localPosition = newPosition;
    }
}
