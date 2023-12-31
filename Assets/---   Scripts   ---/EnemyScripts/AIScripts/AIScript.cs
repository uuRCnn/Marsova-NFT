using System.Collections;
using ______Scripts______.Canvas.Enemy;
using ______Scripts______.EnemyScripts.Enemy.Enemy;
using ______Scripts______.EnemyScripts.Enemy.EnemyAnimations;
using ______Scripts______.EnemyScripts.Enemy.EnemyAnimationsScripts;
using ______Scripts______.EnemyScripts.Enemy.EnemyAttack;
using ______Scripts______.EnemyScripts.Enemy.EnemySkills.EnemyAttackAnimation;
using ______Scripts______.PlayerScripts;
using ______Scripts______.UIScripts.Canvas.Enemy;
using EnemyScripts.Enemy;
using EnemyScripts.OwnScript;
using PlayerScripts;
using UnityEngine;

namespace EnemyScripts.AIScripts
{
    public class AIScript : MonoBehaviour
    {
        public bool isKnockBackNotActive = true;
        public bool isEnemySeePlayer;
        public bool isWaitingInTheBase;
        public bool isRight = true;
        public bool isEnemyAttackToPlayer = false;

        private int JustOneTimeWork;

        public float moveSpeed;
        private float distance; // Player ile Enemy arasoındaki mesafeyi ölçer 

        private Transform Player; // Hedef
        private GameObject Enemy;
        private Rigidbody2D RB2;
        private GameObject GameManager;

        private EnemyScript __EnemyScript;
        private AISkillsScript __AISkillsScript;
        private DmgColliderScript __DmgColliderScript;
        private AnimationsController _animationsController;
        private NearEnemyAttackScript _nearEnemyAttackScript; //Todo: Enemy objesi yakından vuruyorsa __RangeEnemyAttackScript'ini kaldır.
        private RangeEnemyAttackScript __RangeEnemyAttackScript; //Todo: Enemy objesi uzaktan vuruyorsa _nearEnemyAttackScript'ini kaldır.
        private HealtBarBugFixed _healtBarBugFixed;


        private Vector2 direction; // bu Enemy'e karşılık Player hangi yönde onu bulur.   -1 ise Enemy Player'ın sağında
        public Vector2 startingPosition; // Başlangıç pozisyonu
        public Vector2 basePosition; // Base pozisyonu

        [SerializeField] float baseRange; // Serbest Yürüyüş alanının uzunlugu

        private void Awake()
        {
            Player = GameObject.Find("Player").transform;
            Enemy = this.gameObject;
            RB2 = this.gameObject.GetComponent<Rigidbody2D>();
            GameManager = GameObject.Find("GameManager");

            __EnemyScript = Enemy.GetComponent<EnemyScript>();
            __AISkillsScript = Enemy.GetComponent<AISkillsScript>();
            _animationsController = GameManager.GetComponent<AnimationsController>();
            _nearEnemyAttackScript = Enemy.GetComponent<NearEnemyAttackScript>();
            __RangeEnemyAttackScript = Enemy.GetComponent<RangeEnemyAttackScript>();
            _healtBarBugFixed = Enemy.GetComponentInChildren<HealtBarBugFixed>();

            isKnockBackNotActive = true;
        }

        private void Start()
        {
            moveSpeed = Enemy.GetComponent<EnemyScript>().speed;
            startingPosition.x = transform.position.x;
            basePosition = startingPosition + new Vector2(baseRange, 0); // Base'in genişligini ayarlıyorum.
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void MYFixedUpdate() // bunu GameManagerda çagırıyorum.
        {
            direction = (Player.position - transform.position).normalized; // Player Enemy'nin hangi tarafında onu hesaplar
            distance = Vector2.Distance(Player.position, transform.position); // Player ile Enemy arasoındaki mesafeyi ölçer

            SpecialFunctions1();

            if (distance < 1.5f && !__EnemyScript.isAttackinRange) // Enemy Player'ın dibine geldiginde, Enemy dursun. ve vursun
                NearAttackToPlayer();
            else if (distance < 10) // 10 metre içinde görüyorsa ve Enemy vuruş hareketi yapmıyor ise hareket edicektir
            {
                if (distance < 7 && __EnemyScript.isAttackinRange) // Enemy menzilli ise vursun.
                    RangeAttackToPlayer();
                else
                    GoPlayerPosition(); //eger Enemy uzaktan vurmuyor ise Player'ın dibine git. // todo: burda GoPlayerPossition'ı dmg alsa bile gitcek şekilde yap

                SpecialFunctions2(); // slime - giant vb. Scriptlerin kullanıldıgı yer
                EnemyLookingToPlayer();

                isWaitingInTheBase = false; // hatayı önledigi için buraya yazıyorum.
            }
            else if (distance >= 10)
            {
                EnemyDontLookingToPlayer();
                if (isWaitingInTheBase == false) // Base'in içinde beklemiyor ise hareket etsin
                {
                    __AISkillsScript.MoveOwnBase(distance, moveSpeed, startingPosition, basePosition, isRight, baseRange);
                    JustOneTimeWork = 1;
                }
                else if (JustOneTimeWork == 1) // JustOneTimeWork bir kez çalışmasını saglıyor
                {
                    // ilerde bunun gibi farklı Bekleme şeyleride yaparsın.
                    JustOneTimeWork = 0;
                    StartCoroutine(__AISkillsScript.StopMoveAndLookAround(distance)); // durur ve etrafına bakar.
                }
            }
        }

        void GoPlayerPosition() // Player'a doğru hareket etme
        {
            if (isKnockBackNotActive && !__EnemyScript.isItFly)
                RB2.velocity = new Vector2(direction.x * moveSpeed, RB2.velocity.y); // direction.x 1 veya -1 dir
            else if (isKnockBackNotActive)
                RB2.velocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed); // direction.x 1 veya -1 dir
        }

        void EnemyLookingToPlayer()
        {
            if (direction.x < 0) // Enemy Player'ın sağında ise çalışır     // direction.x < 0  :  Enemy Player'ın sağında olunca -1 olur
            {
                Enemy.transform.rotation = new Quaternion(0, 1, 0, 0); // baktıgı yöne çevirir
                _healtBarBugFixed.TurnLeft();
            }
            else if (direction.x > 0) // Enemy Player'ın solunda ise çalışır
            {
                Enemy.transform.rotation = new Quaternion(0, 0, 0, 0); // baktıgı yöne çevirir
                _healtBarBugFixed.TurnRight();
            }

            isEnemySeePlayer = true;
        }

        void EnemyDontLookingToPlayer()
        {
            if (direction.x < 0 && isEnemySeePlayer) // Enemy Player'ın sağında ise çalışır 1 kez çalışır ve yönünü Player'ın zıttına döndürür.
            {
                Enemy.transform.rotation = new Quaternion(0, 0, 0, 0); // baktıgı yöne çevirir
                _healtBarBugFixed.TurnRight();
            }
            else if (direction.x > 0 && isEnemySeePlayer) // Enemy Player'ın solunda ise çalışır  1 kez çalışır ve yönünü Player'ın zıttına döndürür.
            {
                Enemy.transform.rotation = new Quaternion(0, 1, 0, 0); // baktıgı yöne çevirir
                _healtBarBugFixed.TurnLeft();
            }

            isEnemySeePlayer = false;
        }

        void SpecialFunctions2()
        {
            if (Enemy.CompareTag("Smale")) // eger Dogs ise zıplasın
                Enemy.GetComponent<SlimeScript>().Jump();

            if (Enemy.CompareTag("Salyangoz")) // salyangoz ise dursun.
                Enemy.GetComponent<EnemyScript>().speed = 0;
        }

        void SpecialFunctions1()
        {
            _animationsController.SalyangozTurtleActive(Enemy);
            _animationsController.AnimationSpeedUp(Enemy, isEnemySeePlayer);
            _animationsController.EnemyAnimations(Enemy);
            BugFixed(Enemy);
        }

        void NearAttackToPlayer()
        {
            if (!isEnemyAttackToPlayer) // bu if koşulunun nedeni hep çalışıp döngüye girmesin diye.
                _nearEnemyAttackScript.StopAndAttack(this.gameObject);
        }

        void RangeAttackToPlayer()
        {
            __RangeEnemyAttackScript.SuportEnemyStop(Enemy);

            if (!isEnemyAttackToPlayer)
                __RangeEnemyAttackScript.StopAndAttack(Enemy);
        }

        void BugFixed(GameObject _gameObject) // en sol ve en saga gidince blockalrın içinden geçiyor, onun için yazdım
        {
            if (_gameObject.CompareTag("Giant") || _gameObject.CompareTag("Salyangoz"))
            {
                if (gameObject.transform.position.x <= -42)
                    gameObject.transform.position = new Vector3(-32f, -2.82f);
                else if (gameObject.transform.position.x >= 47)
                    gameObject.transform.position = new Vector3(32f, -2.82f);
            }
        }
    }
}