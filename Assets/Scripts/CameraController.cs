using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	private float speedArrow = 5;
	private float speedMouse = 5;
	private float speedWheel = 10;
    [SerializeField] float minXVal = -20f;
    [SerializeField] float maxXVal = 35f;
    [SerializeField] float minZVal = -25f;
    [SerializeField] float maxZVal = 10f;


	
	// Update is called once per frame
	void Update ()
	{
		// camera movement with arrows(Axis)
		Vector3 x = Input.GetAxis ("Horizontal") * transform.right * speedArrow * Time.deltaTime;
		Vector3 z = Input.GetAxis ("Vertical") * transform.forward * speedArrow * Time.deltaTime;
		Vector3 combo = x + z;
		transform.Translate (new Vector3 (combo.x, 0, combo.z), Space.World);
		
		// Horizontal camera movement
		if (Input.mousePosition.x < 10) {
			combo = new Vector3 (transform.right.x, 0, transform.right.z);
			
			transform.Translate (-combo * speedMouse * Time.deltaTime, Space.World);
			
		}
		
		if (Input.mousePosition.x > Screen.width - 10) {
			combo = new Vector3 (transform.right.x, 0, transform.right.z);
			transform.Translate (combo * speedMouse * Time.deltaTime, Space.World);
			
		}
		
		// Vertical camera movement
		if (Input.mousePosition.y < 10) {
			combo = new Vector3 (transform.forward.x, 0, transform.forward.z);
			transform.Translate (-combo * speedMouse * Time.deltaTime, Space.World);
			
		}
		
		if (Input.mousePosition.y > Screen.height - 10) {
			combo = new Vector3 (transform.forward.x, 0, transform.forward.z);
			transform.Translate (combo * speedMouse * Time.deltaTime, Space.World);

		}

		if (Input.GetAxis ("Mouse ScrollWheel") > 0) { // forward
			transform.Translate (transform.forward * speedWheel * Time.deltaTime);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) { // back
			transform.Translate (-transform.forward * speedWheel * Time.deltaTime);
		}
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minXVal, maxXVal), 
            transform.position.y,
            Mathf.Clamp(transform.position.z, minZVal, maxZVal));
	}
}
