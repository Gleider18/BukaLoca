using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using BLINDED_AM_ME.Extensions;
using UnityEngine;
using Random = System.Random;

public class SliceWorker : MonoBehaviour
{
    [SerializeField] private ParticleSystem _cutParticleSystem;
    [SerializeField] private GameObject _topLeft;
    [SerializeField] private GameObject _topRight;
    [SerializeField] private GameObject _bottom;
    [SerializeField] private LineRenderer _lineRenderer;

    [SerializeField] private Color originColor;
    [SerializeField] private Color cuttableColor;
    [SerializeField] private Material lineMaterial;
    private List<GameObject> _objectsToCut = new List<GameObject>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_topRight.transform.position, 0.3f);
        Gizmos.DrawSphere(_topLeft.transform.position, 0.3f);
        Gizmos.DrawSphere(_bottom.transform.position, 0.3f);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool isTableFinded = false;
            Vector3 clickPosition;
            Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            foreach (var item in Physics.RaycastAll(rayToMouse, 50))
            {
                if (item.transform.gameObject.CompareTag("Table"))
                {
                    clickPosition = item.point;
                    _topLeft.transform.position = clickPosition;
                    _lineRenderer.SetPosition(1, clickPosition);
                    isTableFinded = true;
                }
            }
            _lineRenderer.enabled = isTableFinded;
        }

        if (Input.GetMouseButton(0))
        {
            bool isTableFinded = false;
            Vector3 clickPosition;
            Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            foreach (var item in Physics.RaycastAll(rayToMouse, 50))
            {
                if (item.transform.gameObject.CompareTag("Table"))
                {
                    clickPosition = item.point;
                    _topRight.transform.position = clickPosition;
                    _lineRenderer.SetPosition(0, clickPosition);
                    isTableFinded = true;
                }
            }

            if (isTableFinded)
            {
                _lineRenderer.enabled = true;
                _topLeft.transform.LookAt(_topRight.transform);
                var position = _topRight.transform.position;
                var position1 = _topLeft.transform.position;

                Vector3 P = position1 + 0.5f * (position - position1);
                
                P.y = 0;

                Vector3 rayPos = position1;
                rayPos.y = 1.3f;
                
                Ray findBread = new Ray(rayPos, _topLeft.transform.forward);
                
                RaycastHit[] hits = Physics.RaycastAll(findBread, Vector3.Distance(position, position1));

                _objectsToCut.Clear();
                if (hits.Length == 0)
                {
                    lineMaterial.color = originColor;
                }

                foreach (var hitBread in hits)
                {
                    if (hitBread.transform.gameObject.CompareTag("Sliceable"))
                    {
                        lineMaterial.color = cuttableColor;
                        _objectsToCut.Add(hitBread.transform.gameObject);
                    }
                    else
                    {
                        lineMaterial.color = originColor;
                        _objectsToCut.Clear();
                    }
                }

                _bottom.transform.position = P;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _lineRenderer.enabled = false;
            lineMaterial.color = originColor;
            if (_objectsToCut == null)
                return;
            Cut(_objectsToCut);
            _objectsToCut.Clear();
        }
    }

    private void Cut(List<GameObject> targets)
    {
        foreach (var target in targets)
        {


            try
            {
                // get the victims mesh
                _cutParticleSystem.transform.position = target.transform.position;
                var leftSide = target;
                var leftMeshFilter = leftSide.GetComponent<MeshFilter>();
                var leftMeshRenderer = leftSide.GetComponent<MeshRenderer>();
                Sliceable sliceable = target.GetComponent<Sliceable>();

                var materials = new List<Material>();
                leftMeshRenderer.GetSharedMaterials(materials);

                // the insides
                var capSubmeshIndex = 0;
                Material sliceableIsSolid;

                if (sliceable == null)
                    sliceableIsSolid = leftMeshRenderer.material;
                else
                    sliceableIsSolid = sliceable.MaterialToSet;


                _cutParticleSystem.GetComponent<ParticleSystemRenderer>().material = sliceableIsSolid;

                if (materials.Contains(sliceableIsSolid))
                    capSubmeshIndex = materials.IndexOf(sliceableIsSolid);
                else
                {
                    capSubmeshIndex = materials.Count;
                    materials.Add(sliceableIsSolid);
                }


                //Create a triangle between the tip and base so that we can get the normal
                Vector3 side1 = _topLeft.transform.position - _topRight.transform.position;
                Vector3 side2 = _topLeft.transform.position - _bottom.transform.position;

                //Get the point perpendicular to the triangle above which is the normal
                //https://docs.unity3d.com/Manual/ComputingNormalPerpendicularVector.html
                Vector3 normal = Vector3.Cross(side1, side2).normalized;

                //Transform the normal so that it is aligned with the object we are slicing's transform.
                Vector3 transformedNormal =
                    ((Vector3)(target.transform.localToWorldMatrix.transpose * normal)).normalized;

                //Get the enter position relative to the object we're cutting's local transform
                Vector3 transformedStartingPoint =
                    target.transform.InverseTransformPoint(_topRight.transform.position);

                Plane plane = new Plane();

                plane.SetNormalAndPosition(
                    transformedNormal,
                    transformedStartingPoint);

                var mesh = leftMeshFilter.sharedMesh;
                //var mesh = leftMeshFilter.mesh;

                // Cut
                var pieces = mesh.Cut(plane, capSubmeshIndex);

                leftSide.name = "LeftSide";
                leftMeshFilter.mesh = pieces.Item1;
                leftMeshRenderer.sharedMaterials = materials.ToArray();
                //leftMeshRenderer.materials = materials.ToArray();

                var rightSide = new GameObject("RightSide");
                var rightMeshFilter = rightSide.AddComponent<MeshFilter>();
                var rightMeshRenderer = rightSide.AddComponent<MeshRenderer>();

                rightSide.transform.SetPositionAndRotation(leftSide.transform.position, leftSide.transform.rotation);
                rightSide.transform.localScale = leftSide.transform.localScale;

                rightMeshFilter.mesh = pieces.Item2;
                rightMeshRenderer.sharedMaterials = materials.ToArray();
                //rightMeshRenderer.materials = materials.ToArray();

                // Physics 
                Destroy(leftSide.GetComponent<Collider>());

                // Replace
                var leftCollider = leftSide.AddComponent<MeshCollider>();
                leftCollider.convex = true;
                leftCollider.sharedMesh = pieces.Item1;

                var rightCollider = rightSide.AddComponent<MeshCollider>();
                rightCollider.convex = true;
                rightCollider.sharedMesh = pieces.Item2;

                // rigidbody
                if (!leftSide.GetComponent<Rigidbody>())
                    leftSide.AddComponent<Rigidbody>();

                if (!rightSide.GetComponent<Rigidbody>())
                    rightSide.AddComponent<Rigidbody>();

                Vector3 newNormal = Vector3.right * new Random().Next(5, 12);

                leftSide.GetComponent<Rigidbody>()
                    .AddForce(-newNormal, ForceMode.Impulse);
                rightSide.GetComponent<Rigidbody>()
                    .AddForce(newNormal, ForceMode.Impulse);
                rightSide.tag = "Sliceable";
                rightSide.AddComponent<Sliceable>().MaterialToSet = sliceableIsSolid;
                _cutParticleSystem.Play();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }

    // private void Cut()
    // {
    //     //Create a triangle between the tip and base so that we can get the normal
    //     var position = _topRight.transform.position;
    //     var position1 = _topLeft.transform.position;
    //     var position2 = _bottom.transform.position;
    //     Vector3 side1 = position1 - position;
    //     Vector3 side2 = position1 - position2;
    //
    //     //Get the point perpendicular to the triangle above which is the normal
    //     //https://docs.unity3d.com/Manual/ComputingNormalPerpendicularVector.html
    //     Vector3 normal = Vector3.Cross(side1, side2).normalized;
    //
    //     //Transform the normal so that it is aligned with the object we are slicing's transform.
    //     Vector3 transformedNormal =
    //         ((Vector3)(_objectToCut.transform.localToWorldMatrix.transpose * normal)).normalized;
    //
    //     //Get the enter position relative to the object we're cutting's local transform
    //     Vector3 transformedStartingPoint = _objectToCut.transform.InverseTransformPoint(position);
    //
    //     Plane plane = new Plane();
    //
    //     plane.SetNormalAndPosition(
    //         transformedNormal,
    //         transformedStartingPoint);
    //
    //
    //     var direction = Vector3.Dot(Vector3.up, transformedNormal);
    //
    //     //Flip the plane so that we always know which side the positive mesh is on
    //     if (direction < 0)
    //     {
    //         plane = plane.flipped;
    //     }
    //
    //     GameObject[] slices = Slicer.Slice(plane, _objectToCut);
    //     Destroy(_objectToCut);
    //
    //     Vector3 newNormal = Vector3.forward * 4;
    //     slices[0].GetComponent<Rigidbody>()
    //         .AddForce(-(newNormal * slices[0].transform.position.magnitude), ForceMode.Impulse);
    //     slices[1].GetComponent<Rigidbody>()
    //         .AddForce(newNormal * slices[1].transform.position.magnitude, ForceMode.Impulse);
    // }
}