
using AddressableManage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneExample : MonoBehaviour
{
    [SerializeField] private string _sceneExample01Key;
    [SerializeField] private string _sceneExample02Key;
    
    [SerializeField] private Button _btnLoadScene01;
    [SerializeField] private Button _btnLoadScene02;
    [SerializeField] private Button _btnUnLoadScene01;
    [SerializeField] private Button _btnUnLoadScene02;

    private async void Start()
    {
        await ARM.ActivateAsync();

        _btnLoadScene01.onClick.AddListener(() =>
        {
            ARM.Scenes.LoadScene(_sceneExample01Key, LoadSceneMode.Additive);
        });
        
        _btnLoadScene02.onClick.AddListener(() =>
        {
            ARM.Scenes.LoadScene(_sceneExample02Key, LoadSceneMode.Additive);
        });
        
        _btnUnLoadScene01.onClick.AddListener(() =>
        {
            ARM.Scenes.UnloadScene(_sceneExample01Key);
        });
        
        _btnUnLoadScene02.onClick.AddListener(() =>
        {
            ARM.Scenes.UnloadScene(_sceneExample02Key);
        });
    }
}
