using UnityEngine;

namespace PlayerScripts.PlayerLaserAbout
{
    public class StartShipAttack : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform; // Takip edilecek karakterin Transform bile�eni

        private GameObject Ship;
        private Rigidbody2D RB2;

        private float distance;
        private Vector2 direction;

        private void Awake()
        {
            Ship = this.gameObject;
            RB2 = Ship.GetComponent<Rigidbody2D>();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void MYUpdate()
        {
            LookingTheMousePosition();
            WalkingTheSky();
        }

        void LookingTheMousePosition()
        {
            Vector2 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; 

            if (difference.x > 0)
                this.GetComponent<SpriteRenderer>().flipY = false;
            else
                this.GetComponent<SpriteRenderer>().flipY = true;

            float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

            // this.gameObject.transform.position = new Vector2(playerTransform.position.x + 2, playerTransform.position.y + 3);
        }

        void Calculation()
        {
            // RB2.velocity = new Vector2(playerTransform.position.x, playerTransform.position.y + 40);
        }

        void WalkingTheSky()
        {
            direction = (playerTransform.position - Ship.transform.position).normalized; // Player Enemy'nin hangi tarafında onu hesaplar
            distance = Vector2.Distance(playerTransform.position, transform.position); // Player ile Drone arasoındaki mesafeyi ölçer

            RB2.AddForce(new Vector2(direction.x, RB2.velocity.y), ForceMode2D.Impulse); // direction.x 1 veya -1 dir
        }
    }
}