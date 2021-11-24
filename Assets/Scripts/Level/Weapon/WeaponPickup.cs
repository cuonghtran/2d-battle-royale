using System.Collections;
using UnityEngine;
using System.Linq;
using Mirror;
using System;

namespace MainGame
{
    public class WeaponPickup : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Weapon weaponToBePickedUp;
        public Transform weaponTransform;
        [SerializeField] private Weapon.Rarity rarity;
        public GameObject InteractCanvas;

        private bool isTriggered = false;
        private bool isLooted = false;
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

            //if (isTriggered && !isLooted)
            //{
            //    if (Input.GetKey(KeyCode.E))
            //    {
            //        isLooted = true;
            //        CmdPickUpWeapon();
            //    }
            //}
        }

        IEnumerator SwitchDirection()
        {
            yield return new WaitForSeconds(0.15f);
            goingForward = !goingForward;
        }

        [ServerCallback]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var conn = collision.gameObject.GetComponent<NetworkIdentity>().connectionToClient;
                TargetInteractUI(conn, collision.gameObject, true);
            }
        }
        [ServerCallback]
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var conn = collision.gameObject.GetComponent<NetworkIdentity>().connectionToClient;
                TargetInteractUI(conn, collision.gameObject, false);
            }
        }

        [TargetRpc]
        private void TargetInteractUI(NetworkConnection target, GameObject targetPlayer, bool isOn)
        {
            InteractCanvas.SetActive(isOn);
            isTriggered = isOn;
            if (isOn)
                targetPlayer.GetComponent<PlayerMovement>().OnPressedInteract += PlayerInteractionHandler;
            else targetPlayer.GetComponent<PlayerMovement>().OnPressedInteract -= PlayerInteractionHandler;
        }

        private void PlayerInteractionHandler(GameObject interactPlayer)
        {
            if (isTriggered && !isLooted)
            {
                PlayerWeapons playerWeapons = interactPlayer.GetComponent<PlayerWeapons>();
                isLooted = true;
                playerWeapons.weaponToBePickedUp = weaponToBePickedUp;
                CmdPickUpWeapon(playerWeapons, interactPlayer);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdPickUpWeapon(PlayerWeapons playerWeapons, GameObject interactPlayer)
        {
            isLooted = true;
            RpcPickUp(interactPlayer.transform, playerWeapons);
        }

        [ClientRpc]
        private void RpcPickUp(Transform targetTransform, PlayerWeapons playerWeapons)
        {
            Weapon newWeapon = Instantiate(weaponToBePickedUp, targetTransform);
            newWeapon.rarity = this.rarity;
            newWeapon.FillAmmo();
            playerWeapons.PostPickUpHandler(newWeapon);
            NetworkServer.Destroy(gameObject);
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