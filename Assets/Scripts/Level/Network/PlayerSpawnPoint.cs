using UnityEngine;

namespace MainGame
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        private void Awake()
        {
            PlayerSpawnSystem.AddSpawnPoint(transform);
        }

        private void OnDestroy()
        {
            PlayerSpawnSystem.RemoveSpawnPoint(transform);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.3f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
        }
    }
}
