using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MobileGyro : MonoBehaviour
{
    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Variables

    //
    public TcpClient tcpClient;
    public KUKAVARPROXY_SYS kukavarproxy;
    public string value;

    //
    public Rigidbody body;
    public Transform bodyCamera;
    private Gyroscope gyroscope;

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Unity Functions

    private void Start()
    {
        gyroscope = Input.gyro;
        gyroscope.enabled = true;

        StartCoroutine("SendingThePackageTimer");
    }

    private void FixedUpdate()
    {
        if (gyroscope.enabled)
        {
            /*
            body.AddForce(new Vector3(1, 0, 0) * Input.gyro.gravity.x * 10);
            body.AddForce(new Vector3(0, 1, 0) * Input.gyro.gravity.y * 10);
            body.AddForce(new Vector3(0, 0, 1) * Input.gyro.gravity.z * -10);
            */

            bodyCamera.localEulerAngles = new Vector3(-Input.gyro.attitude.eulerAngles.y + 90, Input.gyro.attitude.eulerAngles.x, Input.gyro.attitude.eulerAngles.z - 90);

            Debug.Log(Input.gyro.attitude.eulerAngles);
        }
    }

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Custom Functions

    public IEnumerator SendingThePackageTimer()
    {
        while (tcpClient.GetStatus() == TcpClient.StatusType.Connecting)
        {
            if(tcpClient.GetStatus() == TcpClient.StatusType.Disconnected)
            {
                yield break;
            }

            yield return null;
        }

        while (true)
        {
            tcpClient.SendBytes(kukavarproxy.WriteRequestMessage(KUKAVARPROXY_SYS.MotionType.userPTP_REL, value));

            yield return new WaitForSeconds(0.2f);
        }
    }

    [Button]
    public void SendMessage()
    {
        tcpClient.SendBytes(kukavarproxy.WriteRequestMessage(KUKAVARPROXY_SYS.MotionType.userPTP_REL, value));
    }
}
