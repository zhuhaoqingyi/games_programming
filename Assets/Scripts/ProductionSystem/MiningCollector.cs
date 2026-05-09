using UnityEngine;
using System.Collections.Generic;

namespace ProductionSystem
{
    public class MiningCollector : MonoBehaviour
    {
        [Header("采集设置")]
        public float collectionRadius = 2f;

        private List<SpaceOre>