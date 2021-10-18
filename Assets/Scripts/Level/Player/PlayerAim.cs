using UnityEngine;

namespace MainGame
{
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Transform aimTransform;

        private Vector3 mousePosition = Vector3.zero;
        public float aimAngle { get; private set; }

        // Update is called once per frame
        void Update()
        {
            HandleAiming();
            HandleShooting();
        }

        void HandleAiming()
        {
            mousePosition = UtilsClass.GetMouseWorldPosition(LevelManager.MainCamera);
            Vector3 aimDirection = (mousePosition - transform.position).normalized;
            aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            aimTransform.eulerAngles = new Vector3(0, 0, aimAngle);

            // vertically flip the aim
            Vector3 aimLocalScale = Vector3.one;
            if (aimAngle > 90 || aimAngle < -90)
                aimLocalScale.y = -1f;
            else
                aimLocalScale.y = 1f;
            aimTransform.localScale = aimLocalScale;
        }

        void HandleShooting()
        {
            //if (Input.GetMouseButtonDown(0))
            //{
            //    aimAnimator.SetBool("Shoot", true);
            //}
            //else aimAnimator.SetBool("Shoot", false);
        }
    }
}