using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Sirenix.OdinInspector;

public class TEST : MonoBehaviour
{
    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Unity Functions

    public KUKAVARPROXY_SYS kukaSYS;
    public TcpClient tcpClient;
    public KUKAVARPROXY_SYS.MotionType motionType;
    public float delayMS;
    public float speed;

    public TMP_InputField inputField_IP;

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Unity Functions

    private void Start()
    {

    }

    private void Update()
    {

    }

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Custom Functions

    public void ConnectKUKA()
    {
        StartCoroutine(SendMessageToKUKA());
    }

    public IEnumerator SendMessageToKUKA()
    {
        tcpClient.targetAddress = inputField_IP.text;
        tcpClient.OnConnectedToServer();

        while (tcpClient.GetStatus() == TcpClient.StatusType.Connecting || tcpClient.GetStatus() == TcpClient.StatusType.Disconnected)
        {
            if (tcpClient.GetStatus() == TcpClient.StatusType.Disconnected)
            {
                yield break;
            }

            Debug.Log("Connecting...");

            yield return new WaitForSeconds(1);
        }

        while (true)
        {
            yield return new WaitForSeconds(delayMS / 1000f);

            string data = string.Format("{{ E6POS: X {0}, Y {1}, Z 0, A 0, B 0, C 0, E1 0.0, E2 0.0, E3 0.0, E4 0.0, E5 0.0, E6 0.0}}", UltimateJoystick.GetHorizontalAxis("DarkJoystick") * speed, UltimateJoystick.GetVerticalAxis("DarkJoystick") * speed);
            tcpClient.SendBytes(kukaSYS.WriteRequestMessage(motionType, data));
        }
    }

    [Button]
    public void TestTRead()
    {
        tcpClient.SendBytes(kukaSYS.ReadRequestMessage(motionType));
    }
}
