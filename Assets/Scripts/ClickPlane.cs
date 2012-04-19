using UnityEngine;
using System.Collections;

public class ClickPlane : MonoBehaviour {
	private Vector3 targetPosition;
	
	// Use this for initialization
	void Start () 
	{
		targetPosition = new Vector3(0,0,0);	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		RaycastHit hit;
		if (Input.GetMouseButtonDown (0))
        {
			//Debug.Log("Click");
		
			if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 10000, 1 << 8))
			{
				return;
			}
			
      		//our target is where we clicked
			targetPosition = hit.point;
			targetPosition.Set(targetPosition.x, 3.0f, targetPosition.z);
			
			Debug.Log("Target point: " + targetPosition);
		
			GameObject[] blocksArray = GameObject.FindGameObjectsWithTag("Block");
			
			foreach (GameObject block in blocksArray)
			{
				
				if (block.GetComponent<BoxCollider>().bounds.Contains(targetPosition))
				{
					Debug.Log("original " + targetPosition.ToString());
					var extents = block.GetComponent<BoxCollider>().bounds.extents;
					if(targetPosition.x > block.transform.position.x)
						targetPosition += new Vector3(extents.x, 0, 0);
					else
						targetPosition -= new Vector3(extents.x, 0, 0);
					
					if(targetPosition.z > block.transform.position.z)
						targetPosition += new Vector3(0, 0, extents.z);
					else
						targetPosition -= new Vector3(0, 0, extents.z);
					
					Debug.Log("modified " + targetPosition.ToString());
				}
				
				(block.GetComponent(typeof(Rigidbody)) as Rigidbody).AddExplosionForce(100.0f, targetPosition, 100.0f, 0.0f, ForceMode.Impulse);
				
			
				//Debug.Log("Some block pos: " + block.transform.position.ToString());
			}
		}
	}
}
