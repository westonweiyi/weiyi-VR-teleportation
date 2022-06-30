using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionAsset;
    [SerializeField] private XRRayInteractor xRRayInteractor;
    private InputAction tumbstick;
    [SerializeField] private TeleportationProvider provider;
    [SerializeField] private GameObject reticle;
    [SerializeField] private GameObject followTarget;
    bool isActived = false;
    RaycastHit hit;
    float tumbstickRange = 0.9f;//摇杆阈值
    float tumbstickMagnitude;//摇杆的距离
 // Start is called before the first frame update
    void Start()
    {
        xRRayInteractor.enabled = false;
        var activate = actionAsset.FindActionMap("XRI LeftHand Locomotion").FindAction("Teleport Mode Activate");
        activate.Enable();
        activate.performed += OnTeleportActive;
        var cancel = actionAsset.FindActionMap("XRI LeftHand Locomotion").FindAction("Teleport Mode Cancel");
        cancel.Enable();
        cancel.performed += OnTeleportCancel;
        tumbstick = actionAsset.FindActionMap("XRI LeftHand Locomotion").FindAction("Move");
        tumbstick.Enable();
    }

    void OnTeleportActive(InputAction.CallbackContext context){
        xRRayInteractor.enabled = true;
        isActived = true;
        // reticle.gameObject.SetActive(true);
        tumbstickMagnitude = 0f;
    }

    void OnTeleportCancel(InputAction.CallbackContext context){
        xRRayInteractor.enabled = false;
        isActived = false;
        reticle.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isActived){
            return;
        }
        if(tumbstick.triggered){
            var rotationVector2 = tumbstick.ReadValue<Vector2>();
            float angle = Vector2.SignedAngle(rotationVector2,Vector2.up);
            tumbstickMagnitude = rotationVector2.magnitude;

            
            if(tumbstickMagnitude>tumbstickRange){
                if (!reticle.activeSelf)
                {
                    reticle.SetActive(true);
                }
                // reticle.transform.localRotation = Quaternion.AngleAxis(angle,Vector3.up);
                reticle.transform.localRotation = Quaternion.LookRotation(new Vector3(rotationVector2.x,0,rotationVector2.y));
                
                // Debug.Log("test"+rotationVector2+":"+angle+"dist:"+tumbstickMagnitude);
            }
            else{
                if(reticle.activeSelf){
                    reticle.SetActive(false);
                }
            }

            if (followTarget.activeSelf)
            {
                reticle.transform.position = followTarget.transform.position;
            }
            else
            {
                reticle.SetActive(false);
            }
            

            

            return;
        }

        if(!xRRayInteractor.TryGetCurrent3DRaycastHit(out hit)){
            isActived = false;
            xRRayInteractor.enabled = false;
            return;
        }

        TeleportRequest quest = new TeleportRequest(){
            destinationPosition = hit.point,
            destinationRotation = reticle.transform.rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward,
        };

        provider.QueueTeleportRequest(quest);
        isActived = false;
        xRRayInteractor.enabled = false;
        reticle.gameObject.SetActive(false);
    }
}
