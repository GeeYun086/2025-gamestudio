using System;
using System.Collections.Generic;
using System.Linq;
using GravityGame.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace GravityGame.Utils
{
    /// <summary>
    ///     Some useful debug utilities for highlighting points in the world like <see cref="DrawSphere" />
    ///     (which works analogous to Unity's <see cref="Debug.DrawLine(Vector3, Vector3)" />)
    ///     This class also provides some experimental utilities to draw on the UI (e.g. if you want to highlight a particular
    ///     pixel's location on the screen)
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DebugDraw : SingletonMonoBehavior<DebugDraw>
    {
        readonly List<Action<MeshGenerationContext>> _drawActionsNextFrame = new();

        static VisualElement DebugElement => GameUI.Instance.Elements.DebugElement; // Element to draw on

        void Update()
        {
            var toDraw = _drawActionsNextFrame.ToList();
            _drawActionsNextFrame.Clear();
            DebugElement.generateVisualContent = ctx => {
                foreach (var action in toDraw) {
                    action(ctx);
                }
                DebugElement.generateVisualContent = null;
            };
            DebugElement.MarkDirtyRepaint();
        }

        public static void DrawSphere(
            Vector3 center,
            float radius,
            Color? color = null,
            float duration = 0f,
            int segments = 16
        )
        {
            var sphereColor = color ?? Color.white;

            // Draw circles in the XZ plane (horizontal)
            DrawCircle(Vector3.up, Vector3.right);

            // Draw circles in the XY plane
            DrawCircle(Vector3.forward, Vector3.up);

            // Draw circles in the YZ plane
            DrawCircle(Vector3.right, Vector3.forward);
            return;

            void DrawCircle(Vector3 axis1, Vector3 axis2)
            {
                float angleStep = 360f / segments;
                for (int i = 0; i < segments; i++) {
                    float angleA = angleStep * i * Mathf.Deg2Rad;
                    float angleB = angleStep * (i + 1) * Mathf.Deg2Rad;

                    var pointA = center + (axis1 * Mathf.Cos(angleA) + axis2 * Mathf.Sin(angleA)) * radius;
                    var pointB = center + (axis1 * Mathf.Cos(angleB) + axis2 * Mathf.Sin(angleB)) * radius;

                    Debug.DrawLine(pointA, pointB, sphereColor, duration);
                }
            }
        }

        public static void DrawOnUI(Action<MeshGenerationContext> drawNextFrame) => Instance._drawActionsNextFrame.Add(drawNextFrame);

        public static void DrawUICircle(MeshGenerationContext ctx, Vector2 center, float radius, Color color)
        {
            var painter = ctx.painter2D;
            painter.strokeColor = color;
            painter.fillColor = color;
            painter.lineWidth = 0; // Set to 0 for a solid fill

            painter.BeginPath();
            painter.Arc(center, radius, 0, 360);
            painter.ClosePath();
            painter.Fill(); // Fill the circle
        }

        public static void DrawUICross(MeshGenerationContext ctx, Vector2 center, Color? lineColor = null)
        {
            const int lineWidth = 4;
            const int lineLength = 10;
            var painter = ctx.painter2D;
            painter.strokeColor = lineColor ?? Color.white;
            painter.lineWidth = lineWidth;

            // Calculate horizontal and vertical line endpoints
            var left = new Vector2(center.x - lineLength * 0.5f, center.y);
            var right = new Vector2(center.x + lineLength * 0.5f, center.y);
            var top = new Vector2(center.x, center.y - lineLength * 0.5f);
            var bottom = new Vector2(center.x, center.y + lineLength * 0.5f);

            // Draw horizontal line
            painter.BeginPath();
            painter.MoveTo(left);
            painter.LineTo(right);
            painter.Stroke();

            // Draw vertical line
            painter.BeginPath();
            painter.MoveTo(top);
            painter.LineTo(bottom);
            painter.Stroke();
        }

        public static void DrawUIArrow(
            MeshGenerationContext ctx,
            Color color,
            Vector2 startPosition,
            Vector2 targetPosition,
            float arrowheadSize = 10
        )
        {
            var painter = ctx.painter2D;
            painter.strokeColor = color;
            painter.lineWidth = 2f;

            // Draw main arrow line
            painter.BeginPath();
            painter.MoveTo(startPosition);
            painter.LineTo(targetPosition);
            painter.Stroke();

            // Calculate arrowhead
            var direction = (targetPosition - startPosition).normalized;
            var perpendicular = new Vector2(-direction.y, direction.x);

            var arrowHeadPoint1 = targetPosition - direction * arrowheadSize + perpendicular * (arrowheadSize * 0.5f);
            var arrowHeadPoint2 = targetPosition - direction * arrowheadSize - perpendicular * (arrowheadSize * 0.5f);

            // Draw arrowhead
            painter.BeginPath();
            painter.MoveTo(targetPosition);
            painter.LineTo(arrowHeadPoint1);
            painter.MoveTo(targetPosition);
            painter.LineTo(arrowHeadPoint2);
            painter.Stroke();
        }
    }
}