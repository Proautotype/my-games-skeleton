using UnityEngine;
using UnityEngine.Events;

public class DiceManager : MonoBehaviour
{
    private Rigidbody rb;
    public float followMouse;

    public float followSpeed = 2f;
    public float maxDistance = 3f;
    Vector3 targetPosition;

    private bool Tossing = true;

    [HideInInspector] public UnityEvent rollEvent;
    [SerializeField] private AudioManager audioManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        GameManager.gmInstance.processOnGoing += processOnGoing;
    }

    private void OnDestroy()
    {
        GameManager.gmInstance.processOnGoing -= processOnGoing;
    }

    //being used here to disable and enable dice rolling event
    private void processOnGoing(bool tossState)
    {
        Tossing = tossState;
    }

    private void Update()
    {
        if(followMouse > 0f)
        {
             //Get the position of the mouse cursor in screen space
            Vector3 mousePosition = Input.mousePosition;
            //mousePosition.z = 0.5f;
            //mousePosition.y = 0.5f;

            // Convert the mouse position to world space
            targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            targetPosition.y = 5f;
            // Move the game object towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            //decrease spin
            followMouse -= 4.5f;
        }
    }

    // Update is called once per frame
    private void OnMouseDown()
    {
        if(rb.velocity == Vector3.zero && !Tossing)
        {
            rb.useGravity = true;
            Vector3 postion = rb.transform.position;
            //lift the dice
            rb.transform.position = new Vector3(postion.x, postion.y * Random.Range(7, 15), postion.z);
            //initiate mouse follow
            followMouse = 30f;
            //apply some rotation
            rb.transform.rotation = Quaternion.Euler(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360));
            //adding torge
            rb.AddTorque(Random.Range(-250, 250), Random.Range(-250, 250), Random.Range(-250, 250));
            StartCoroutine(audioManager.PlayAudio(1, 0));
            //tell the board to get ready to read the next Value
            rollEvent?.Invoke();
        }        
    }
}
