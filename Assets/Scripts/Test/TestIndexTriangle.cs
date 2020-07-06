// This script draws a debug line around mesh triangles
// as you move the mouse over them.
using UnityEngine;
using System.Collections;

public class TestIndexTriangle : MonoBehaviour
{
    GameObject rightHand;
    GameObject leftHand;

    void Start()
    {
        rightHand = GameObject.Find("RightHand");
        leftHand = GameObject.Find("LeftHand");
    }
    //TransformDirection(Vector3.forward)
    void Update()
    {
        RaycastHit hit_right;
        RaycastHit hit_left;
        if (!Physics.Raycast(rightHand.transform.position, rightHand.transform.TransformDirection(Vector3.forward), out hit_right) && !Physics.Raycast(leftHand.transform.position, leftHand.transform.TransformDirection(Vector3.forward), out hit_left))
            return;
        if (Physics.Raycast(rightHand.transform.position, rightHand.transform.TransformDirection(Vector3.forward), out hit_right))
        {
            MeshCollider meshCollider = hit_right.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return;

            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3 p0 = vertices[triangles[hit_right.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit_right.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit_right.triangleIndex * 3 + 2]];
            Transform hit_rightTransform = hit_right.collider.transform;
            p0 = hit_rightTransform.TransformPoint(p0);
            p1 = hit_rightTransform.TransformPoint(p1);
            p2 = hit_rightTransform.TransformPoint(p2);
            Debug.DrawLine(p0, p1, Color.red);
            Debug.DrawLine(p1, p2, Color.red);
            Debug.DrawLine(p2, p0, Color.red);


        }

        if (Physics.Raycast(leftHand.transform.position, leftHand.transform.TransformDirection(Vector3.forward), out hit_left))
        {
            MeshCollider meshCollider = hit_left.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return;

            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3 p0 = vertices[triangles[hit_left.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit_left.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit_left.triangleIndex * 3 + 2]];
            Transform hit_leftTransform = hit_left.collider.transform;
            p0 = hit_leftTransform.TransformPoint(p0);
            p1 = hit_leftTransform.TransformPoint(p1);
            p2 = hit_leftTransform.TransformPoint(p2);
            Debug.DrawLine(p0, p1);
            Debug.DrawLine(p1, p2);
            Debug.DrawLine(p2, p0);
        }
        
    }
}

