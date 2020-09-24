using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
    [Header("Objects to Instantiate")]
    public List<Mesh> meshes;
    public List<float> meshesProbabilities;

    [Header("Random Seed")]
    public int randomSeed;

    [Header("Instantiation")]
    public Vector3 areaOrigin;
    public bool centerOnOrigin = false;
    public Vector3 areaSize;
    public Vector3Int areaSubdivisions;
    [Space]
    public Vector3 meshPositionRandom;
    [Space]
    public Vector3 meshRotation;
    public Vector3 meshRotationRandom;
    [Space]
    public float meshScale;
    public Vector3 meshScaleRandom;
    public bool clampNegativeScale = true;

    [Header("Player Camera")]
    public Transform playerTransform;
    public float playerRadius;
    public AnimationCurve playerRadiusEffect;

    [Header("Material")]
    public Material material;
    public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode;

    [Header("Debug")]
    public bool reinitialize = false;
    public bool recomputeMeshProbabilities = false;

    private Matrix4x4[] _matrices;
    private int _instancesCount;

    private List<Matrix4x4>[] _subMatrices;

    private Vector3 _instancePosition;
    private Vector3 _instanceRotation;
    private Vector3 _instanceScale;

    private float[] _randoms;
    private const int _randomCount = 9;

    private int[] _meshIndices;

	private void Start() {

        Random.InitState(randomSeed);

        Initialize();
	}

	// Update is called once per frame
	void Update()
    {
        _instancesCount = areaSubdivisions.x * areaSubdivisions.y * areaSubdivisions.z;

		if (reinitialize) {
            Initialize();
            reinitialize = false;
		}

        if (recomputeMeshProbabilities) {
            ComputeRandomMeshIndices();
            recomputeMeshProbabilities = false;
        }

        UpdateMatrices();

        UpdateSubMatrices();

        //Draw
        for (int i = 0; i < _subMatrices.Length; i++) {
            if(meshes[i] != null)
                Graphics.DrawMeshInstanced(meshes[i], 0, material, _subMatrices[i].ToArray(), _subMatrices[i].Count, null, shadowCastingMode);
        }
    }

    void Initialize() {

        _instancesCount = areaSubdivisions.x * areaSubdivisions.y * areaSubdivisions.z;

        _matrices = new Matrix4x4[_instancesCount];

        _randoms = new float[_instancesCount * _randomCount];
        UpdateRandoms();

        _subMatrices = new List<Matrix4x4>[meshes.Count];
        for (int i = 0; i < meshes.Count; i++) {
            _subMatrices[i] = new List<Matrix4x4>();
        }

        ComputeRandomMeshIndices();
    }

    void ComputeRandomMeshIndices() {

        if (meshes.Count != meshesProbabilities.Count) {
            Debug.LogError(name + ": Meshes and Meshes Probabilities are not the same size.");
            return;
        }

        _meshIndices = new int[_instancesCount];

        float probabilitySum = 0;
        foreach (float proba in meshesProbabilities)
            probabilitySum += proba;

        if (probabilitySum <= 0) {
            Debug.LogError(name + ": Meshes probability should not sum to zero.");
            return;
        }

        float[]  _meshesNormalizedProbability = new float[meshes.Count];

        for(int i=0; i<_meshesNormalizedProbability.Length; i++) {
            _meshesNormalizedProbability[i] = meshesProbabilities[i] / probabilitySum;
		}

        for(int i=0; i<_meshIndices.Length; i++) {

            float rand = Random.Range(0.0f, 1.0f);

            int currentProbaIndex = 0;
            float currentProba = _meshesNormalizedProbability[0];

			while (true) {
                if (rand <= currentProba) {
                    _meshIndices[i] = currentProbaIndex;
                    break;
                } else {
                    currentProbaIndex++;
                    currentProba += _meshesNormalizedProbability[currentProbaIndex];
				}
			}
		}
	}

    void UpdateMatrices() {

        if (_matrices.Length != _instancesCount)
            _matrices = new Matrix4x4[_instancesCount];

        for (int i=0; i< areaSubdivisions.x; i++) {
            for(int j=0; j< areaSubdivisions.y; j++) {
                for(int k=0; k< areaSubdivisions.z; k++) {

                    _instancePosition.x = i * areaSize.x / areaSubdivisions.x + areaOrigin.x;
                    _instancePosition.y = j * areaSize.y / areaSubdivisions.y + areaOrigin.y;
                    _instancePosition.z = k * areaSize.z / areaSubdivisions.z + areaOrigin.z;

                    _instancePosition.x -= centerOnOrigin ? areaSize.x * 0.5f : 0;
                    _instancePosition.y -= centerOnOrigin ? areaSize.y * 0.5f : 0;
                    _instancePosition.z -= centerOnOrigin ? areaSize.z * 0.5f : 0;

                    float randomCoefficient = playerRadiusEffect.Evaluate(Vector3.Distance(_instancePosition, playerTransform.position) / playerRadius);

                    _instancePosition.x += _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y] * meshPositionRandom.x * randomCoefficient;
                    _instancePosition.y += _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 1] * meshPositionRandom.y * randomCoefficient;
                    _instancePosition.z += _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 2] * meshPositionRandom.z * randomCoefficient;

                    _instanceRotation.x = meshRotation.x + _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 3] * meshRotationRandom.x * randomCoefficient;
                    _instanceRotation.y = meshRotation.y + _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 4] * meshRotationRandom.y * randomCoefficient;
                    _instanceRotation.z = meshRotation.z + _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 5] * meshRotationRandom.z * randomCoefficient;

                    _instanceScale.x = meshScale + _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 6] * meshScaleRandom.x * randomCoefficient;
                    _instanceScale.y = meshScale + _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 7] * meshScaleRandom.y * randomCoefficient;
                    _instanceScale.z = meshScale + _randoms[i + j * areaSubdivisions.x + k * areaSubdivisions.x * areaSubdivisions.y + 8] * meshScaleRandom.z * randomCoefficient;

					if (clampNegativeScale)
                        _instanceScale = Vector3.Max(_instanceScale, Vector3.zero);

                    _matrices[i + j* areaSubdivisions.x + k* areaSubdivisions.x* areaSubdivisions.y] = Matrix4x4.TRS(_instancePosition, Quaternion.Euler(_instanceRotation), _instanceScale);

				}
			}
		}
	}

    void UpdateSubMatrices() {

        for(int i=0; i<meshes.Count; i++) {
            _subMatrices[i].Clear();
		}

        for (int i = 0; i < _matrices.Length; i++) {
            _subMatrices[_meshIndices[i]].Add(_matrices[i]);
        }
    }

	void UpdateRandoms() {

        if (_randoms.Length != _instancesCount * _randomCount)
            _randoms = new float[_instancesCount * _randomCount];

        for(int i=0; i<_randoms.Length; i++) {

            _randoms[i] = Random.Range(-1.0f, 1.0f);
		}
    }
}
