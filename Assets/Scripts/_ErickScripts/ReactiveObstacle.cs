using UnityEngine;
using System.Collections;

public class ReactiveObstacle : MonoBehaviour
{

    public float applyForce;
    BodyPhysicsController bpController;

    AudioSource audio;

    float jigglePositive1;
    float jigglePositive2;
    float jiggleNegative1;
    float jiggleNegative2;
    //public float jiggleTime;        //how long the jelly jiggles (blendshapes back at zero at end)
    SkinnedMeshRenderer JellySMR;
    bool isJiggling;
    int currentJiggleTarget;        //lerps towards the specified target.
    public float jiggleSpeed;       //lerp speed per second (%).
    float jiggleProgress;    //jiggle progress.
    float blndWeight;

    // Use this for initialization
    void Start()
    {
        JellySMR = GetComponent<SkinnedMeshRenderer>();
        isJiggling = false;
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(isJiggling) JiggleJelly();
    }

    void OnCollisionEnter(Collision coll)
    {
        Rigidbody hitRigidbody;
        GameObject hitGameObject;
        GameObject hitParent;

        hitParent = coll.gameObject.transform.root.gameObject;              // get the parent object's tag
        bpController = hitParent.GetComponent<BodyPhysicsController>();     // get the parent's BodyPhysicsController so we can set the currOffBalance value
        hitGameObject = coll.gameObject;                                    // reference the actual spawned player object so we can add force
        if (hitParent.tag == "Player" && (bpController.m_currOffBalance <= bpController.m_balance))  // add force if we hit a player character that's not already off balance
        {
            hitRigidbody = hitGameObject.GetComponent<Rigidbody>();
            bpController.m_currOffBalance = bpController.m_balance * 1.2f;
            hitRigidbody.AddForce(hitGameObject.transform.forward * -1 * applyForce);
            RandomJiggleTarget(100, 100);
        }
    }

    void RandomJiggleTarget(float positive1, float positive2)
    {
        audio.pitch = Random.Range(0.7f, 1.2f);
        audio.Play();

        if (!isJiggling)
        {
            jigglePositive1 = positive1;
            jigglePositive2 = positive2;
            currentJiggleTarget = 1;
            jiggleProgress = 0;
            isJiggling = true;
        }

    }

    void JiggleJelly()
    {
        switch (currentJiggleTarget)
        {
            case 1:
                if (jiggleProgress < 1)
                {
                    jiggleProgress += jiggleSpeed * Time.deltaTime;
                    blndWeight = Mathf.Lerp(0f, jigglePositive1, jiggleProgress);    //we start from zero
                    JellySMR.SetBlendShapeWeight(0, blndWeight);
                }
                else if (jiggleProgress >= 1)   //reset for next case
                {
                    currentJiggleTarget++;
                    jiggleProgress = 0;
                }
                break;
            case 2:
                if (jiggleProgress < 1)
                {
                    jiggleProgress += jiggleSpeed * Time.deltaTime;
                    blndWeight = Mathf.Lerp(jigglePositive1, 0, jiggleProgress);    //..to the first negative position
                    JellySMR.SetBlendShapeWeight(0, blndWeight);
                }
                else if (jiggleProgress >= 1)   //reset for next case
                {
                    currentJiggleTarget++;
                    jiggleProgress = 0;
                }
                break;
            case 3:
                if (jiggleProgress < 1)
                {
                    jiggleProgress += jiggleSpeed * Time.deltaTime;
                    blndWeight = Mathf.Lerp(0, jigglePositive2, jiggleProgress);    //..to the second positive
                    JellySMR.SetBlendShapeWeight(1, blndWeight);
                }
                else if (jiggleProgress >= 1)   //reset for next case
                {
                    currentJiggleTarget++;
                    jiggleProgress = 0;
                }
                break;
            case 4:
                if (jiggleProgress < 1)
                {
                    jiggleProgress += jiggleSpeed * Time.deltaTime;
                    blndWeight = Mathf.Lerp(jigglePositive2, 0, jiggleProgress);    //..to the second negative position
                }
                else if (jiggleProgress >= 1)   //reset for next case
                {
                    currentJiggleTarget++;
                    jiggleProgress = 0;
                }
                break;
            default:    //we know it's over
                currentJiggleTarget = 0;
                jiggleProgress = 0;
                isJiggling = false;
                break;
        }
    }
}
