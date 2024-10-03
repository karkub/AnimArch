using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Visualization.Animation
{
    public class LineFiller : UILineRenderer
    {
        Vector2 currentPosition;
        Vector2 nextPoint;
        Vector2[] targetPoints;
        List<Vector2> addedPoints;
        float step = 0.5f;
        int index = 1;

        private Color createLineFillerColor()
        {
            Color c = Animation.Instance.relationColor;
            float h, s, v;

            Color.RGBToHSV(c, out h, out s, out v);
            s *= 1.5f;
            if (s > 1f) {s = 1f;}
            v *= 1.5f;
            if (v > 1f) {v = 1f;}

            return Color.HSVToRGB(h, s, v);
        }

        public IEnumerator AnimateFlow(Vector2[] targetPoints, bool shouldFlip, Func<bool> highlightEdgeCallback, bool isObject)
        {
            float animSpeed = AnimationData.Instance.AnimSpeed / (5*targetPoints.Length);
            if (isObject)
            {
                animSpeed = (float)((float) animSpeed * 0.7);
            }
            this.targetPoints = targetPoints;
            if (targetPoints != null && targetPoints.Length >= 2)
            {
                Points = new Vector2[0];
                Vector3 tempVector = new Vector3(targetPoints[0].x, targetPoints[0].y, 0);
                if (shouldFlip)
                {
                    System.Array.Reverse(targetPoints);
                }
                currentPosition = targetPoints[0];
                nextPoint = targetPoints[1];
                addedPoints = new List<Vector2>();
                addedPoints.Add(currentPosition);
                //color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                color = Animation.Instance.methodColor;
            }
            while (addedPoints.Count < targetPoints.Length)
            {
                if (Mathf.Abs(nextPoint.x - currentPosition.x) > step / animSpeed)
                {
                    if (nextPoint.x > currentPosition.x)
                    {
                        currentPosition += new Vector2(step / animSpeed, 0);
                    }
                    else if (nextPoint.x < currentPosition.x)
                    {
                        currentPosition -= new Vector2(step / animSpeed, 0);
                    }
                }
                if (Mathf.Abs(nextPoint.y - currentPosition.y) > step / animSpeed)
                {
                    if (nextPoint.y > currentPosition.y)
                    {
                        currentPosition += new Vector2(0, step / animSpeed);
                    }
                    else if (nextPoint.y < currentPosition.y)
                    {
                        currentPosition -= new Vector2(0, step / animSpeed);
                    }
                }
                if (Mathf.Abs(nextPoint.y - currentPosition.y) < step / animSpeed && Mathf.Abs(nextPoint.x - currentPosition.x) < step / animSpeed)
                {
                    currentPosition = nextPoint;
                    addedPoints.Add(currentPosition);
                    Points = addedPoints.ToArray();
                    if (addedPoints.Count < targetPoints.Length)
                    {
                        index++;
                        nextPoint = targetPoints[index];
                    }
                    else
                    {
                        //Flip back
                        if (shouldFlip)
                        {
                            System.Array.Reverse(targetPoints);
                        }
                        break;
                    }
                }
                else
                {
                    List<Vector2> tempPoints = new List<Vector2>(addedPoints);
                    tempPoints.Add(currentPosition);
                    Points = tempPoints.ToArray();

                }

                if (Animation.Instance.AnimationIsRunning)
                {
                    yield return new WaitForFixedUpdate();
                }

            }

            if (highlightEdgeCallback != null)
            {
                highlightEdgeCallback();
            }
        }
    }
}
