using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectSceneControll : MonoBehaviour
{
    public void ScenLoad(string name)
    {
        SceneManager.LoadScene(name);
    }
}
