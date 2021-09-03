using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;

namespace KaizerWaldCode
{
    [CreateAssetMenu(menuName = "MapSettings")]
    public class MapSettings : ScriptableObject
    {
        [SerializeField] private int chunkSize;
        [SerializeField] private int numChunk;
        [Range(2, 10)]
        [SerializeField] private int pointPerMeter;

        private int _mapSize;
        private float _pointSpacing;
        private int _chunkPointPerAxis;
        private int _mapPointPerAxis;

        private void OnValidate()
        {
            chunkSize = max(1, chunkSize);
            numChunk = max(1, numChunk);
            _mapSize = mul(chunkSize, numChunk);
            pointPerMeter = max(2, pointPerMeter);
        }

        private void Awake()
        {
            chunkSize = max(1, chunkSize);
            numChunk = max(1, numChunk);
            pointPerMeter = max(2, pointPerMeter);

            _mapSize = chunkSize * numChunk;
            _pointSpacing = 1f / (pointPerMeter - 1f);
            _chunkPointPerAxis = (chunkSize * pointPerMeter) - (chunkSize - 1);
            _mapPointPerAxis = (numChunk * chunkSize) * pointPerMeter - (numChunk * chunkSize - 1);
        }
    }
}
