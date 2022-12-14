/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Whisperer
{
    public static class Utilities
    {
            /// <summary>
            ///     Casts overlap spheres of increasing radius over distance to simulate a conical cast.<br></br>
            ///     If hit(s) detected, out colliders will provide all overlapped colliders at first collided sphere.
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="direction"></param>
            /// <param name="distance"></param>
            /// <param name="minRadius"></param>
            /// <param name="maxRadius"></param>
            /// <param name="colliders"></param>
            /// <param name="layerMask"></param>
            /// <param name="queryTriggerInteraction"></param>
            /// <returns></returns>
            public static bool ConeCast(Vector3 origin, Vector3 direction, float distance, float minRadius, float maxRadius,
            out List<Collider> colliders, LayerMask layerMask,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            colliders = new List<Collider>();
            minRadius = Mathf.Max(minRadius, 0.01f);
            var sumDist = minRadius + 0.001f;
            Vector3 point;
            float radius;

            while (sumDist <= distance)
            {
                point = origin + direction * sumDist;
                radius = Mathf.Lerp(minRadius, maxRadius, (sumDist - minRadius) / distance);
                sumDist += radius;

                colliders.AddRange(Physics.OverlapSphere(point, radius, layerMask, queryTriggerInteraction));
            }

            return colliders.Count > 0;
        }

            /// <summary>
            ///     Casts overlap spheres of increasing radius over distance to simulate a conical cast.<br></br>
            ///     If hit(s) detected, out collider will be the center-most object at first collided sphere.
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="direction"></param>
            /// <param name="distance"></param>
            /// <param name="minRadius"></param>
            /// <param name="maxRadius"></param>
            /// <param name="collider"></param>
            /// <param name="layerMask"></param>
            /// <param name="queryTriggerInteraction"></param>
            /// <returns></returns>
            public static bool ConeCast(Vector3 origin, Vector3 direction, float distance, float minRadius, float maxRadius,
            out Collider collider, LayerMask layerMask,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var hit = false;
            collider = null;
            minRadius = Mathf.Max(minRadius, 0.01f);
            var sumDist = minRadius + 0.001f;
            Vector3 point;
            float radius;

            while (sumDist <= distance && !hit)
            {
                point = origin + direction * sumDist;
                radius = Mathf.Lerp(minRadius, maxRadius, (sumDist - minRadius) / distance);
                sumDist += radius;

                var colliders = Physics.OverlapSphere(point, radius, layerMask, queryTriggerInteraction);

                if (colliders.Length > 0)
                {
                    hit = true;
                    float hiDot = 0;
                    var closest = 0;
                    for (var i = 0; i < colliders.Length; i++)
                    {
                        var dot = Vector3.Dot(direction, (colliders[i].ClosestPoint(point) - origin).normalized);
                        if (dot > hiDot)
                        {
                            hiDot = dot;
                            closest = i;
                        }
                    }

                    collider = colliders[closest];
                }
            }

            return hit;
        }

            /// <summary>
            ///     Returns a randomized list
            /// </summary>
            /// <param name="randList"></param>
            /// <returns></returns>
            public static List<string> RandomizeList(List<string> randList)
        {
            for (var i = 0; i < randList.Count; i++)
            {
                var temp = randList[i];
                var rand = Random.Range(i, randList.Count);
                randList[i] = randList[rand];
                randList[rand] = temp;
            }

            return randList;
        }
    }
}
