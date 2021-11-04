using System.Collections;
using UnityEngine;
using System.Linq;

namespace MainGame
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Weapon weaponToBePickedUp;
        public Transform weaponTransform;
        [SerializeField] private Weapon.Rarity rarity;
        public GameObject InteractCanvas;

        private bool isTriggered = false;
        private bool isLooted = false;
        private Transform triggerTransform;
        private Vector3 topPosition;
        private Vector3 bottomPosition;
        private float floatSpeed = 0.09f;
        private bool goingForward = true;

        // Start is called before the first frame update
        void Start()
        {
            weaponTransform.GetComponent<SpriteRenderer>().color = CommonClass.RarityColor.ElementAtOrDefault((int)rarity).Value;
            topPosition = weaponTransform.position + new Vector3(0, 0.07f, 0);
            bottomPosition = weaponTransform.position - new Vector3(0, 0.07f, 0);
        }

        // Update is called once per frame
        void Update()
        {
            // make the weapon transform floating
            if (topPosition != Vector3.zero && bottomPosition != Vector3.zero)
            {
                if (goingForward)
                    weaponTransform.position = Vector3.MoveTowards(weaponTransform.position, bottomPosition, floatSpeed * Time.deltaTime);
                else if (!goingForward)
                    weaponTransform.position = Vector3.MoveTowards(weaponTransform.position, topPosition, floatSpeed * Time.deltaTime);

                if (Vector3.Distance(weaponTransform.position, bottomPosition) <= 0.0001f && goingForward)
                    StartCoroutine(SwitchDirection());

                if (Vector3.Distance(weaponTransform.position, topPosition) <= 0.0001f && !goingForward)
                    StartCoroutine(SwitchDirection());
            }

            // a player in the trigger zone
            if (isTriggered && !isLooted)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    isLooted = true;
                    PickUpWeapon(triggerTransform);
                }
            }
        }

        IEnumerator SwitchDirection()
        {
            yield return new WaitForSeconds(0.15f);
            goingForward = !goingForward;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                InteractCanvas.SetActive(true);
                isTriggered = true;
                triggerTransform = collision.transform;
            }
        }

        private void PickUpWeapon(Transform collision)
        {
            PlayerWeapons playerWeapons = collision.GetComponent<PlayerWeapons>();
            if (playerWeapons)
            {
                Weapon newWeapon = Instantiate(weaponToBePickedUp);
                newWeapon.rarity = this.rarity;
                newWeapon.FillAmmo();
                playerWeapons.Pickup(newWeapon);
                Destroy(gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                InteractCanvas.SetActive(false);
                isTriggered = false;
                triggerTransform = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var offset = new Vector3(0, -0.3f, 0);
            Gizmos.color = CommonClass.RarityColor.ElementAtOrDefault((int)rarity).Value;
            Gizmos.DrawCube(transform.position + offset, new Vector3(0.4f, 0.25f, 0));
        }
#endif
    }
}