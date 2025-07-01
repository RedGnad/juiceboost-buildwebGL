using UnityEngine;
using System.Runtime.InteropServices;

public class WalletScoreManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void TestWalletCollection();
    
    public void RunTestWalletCollection()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        TestWalletCollection();
        #else
        Debug.Log("TestWalletCollection disponible uniquement en WebGL");
        #endif
    }
    
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Test WalletScores"))
        {
            RunTestWalletCollection();
        }
    }
}
