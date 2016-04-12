using UnityEngine;
using System.Collections;

public class CameraLogic : MonoBehaviour {

  public float Boundary = 50;
  public float Speed = 5;

  float ScreenWidth;
  float ScreenHeight;

  void Start() 
  {
    ScreenWidth = Screen.width;
    ScreenHeight = Screen.height;
  }
  void Update() 
  {
    if (Input.mousePosition.x > ScreenWidth - Boundary)
    {
      transform.position += Vector3.right * Speed * Time.deltaTime;
    }
    if (Input.mousePosition.x < 0 + Boundary)
    {
      transform.position -= Vector3.right * Speed * Time.deltaTime;
    }
    if (Input.mousePosition.y > ScreenHeight - Boundary)
    {
      transform.position += Vector3.forward * Speed * Time.deltaTime;
    }
    if (Input.mousePosition.y < 0 + Boundary)
    {
      transform.position -= Vector3.forward * Speed * Time.deltaTime;
    }
  }   
}
