using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace GravityGame.UI
{
    /// <summary>
    ///     UI Element that draws the radial menu with an inner up-down circle and outer up-down-left-right circle
    ///     Used for 6-direction gravity selection
    /// </summary>
    [UxmlElement]
    public partial class GravityDirectionRadialMenu : VisualElement
    {
        [UxmlAttribute] float _deadZoneRadius = 45f;
        [UxmlAttribute] float _horizontalAngle = 58.82f;
        [UxmlAttribute] Color _outlineColor = Color.white;
        [UxmlAttribute] float _outlineWidth = 4f;

        [UxmlAttribute] float _upWidth = 100f;
        [UxmlAttribute] float _upLength = 350f;

        [UxmlAttribute] Color _xAxisColor = Color.red;
        [UxmlAttribute] Color _yAxisColor = Color.cyan;
        [UxmlAttribute] Color _zAxisColor = Color.green;

        public enum Zone { None, Left, Right, Up, Down, OuterUp, OuterDown }

        public Func<Zone, Color> ColorForZone;

        [UsedImplicitly] public GravityDirectionRadialMenu()
        {
            generateVisualContent += DrawRadialMenu;
            ColorForZone = zone => zone switch {
                Zone.None => Color.clear,
                Zone.Left or Zone.Right => _xAxisColor,
                Zone.Up or Zone.Down => _yAxisColor,
                Zone.OuterUp or Zone.OuterDown => _zAxisColor,
                _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, null)
            };
        }

        public Color CardinalDirectionToColor(Vector3 dir)
        {
            if (dir.x != 0) return _xAxisColor;
            if (dir.y != 0) return _yAxisColor;
            if (dir.z != 0) return _zAxisColor;
            return Color.clear;
        }

        public Zone GetDirection(Vector2 mouse)
        {
            mouse /= scaledPixelsPerPoint;
            float distance = mouse.magnitude;
            if (distance < _deadZoneRadius) return Zone.None;

            float angle = Vector3.Angle(mouse, Vector3.up);
            float alpha = _horizontalAngle * 0.5f;
            if (angle > 90 - alpha && angle < 90 + alpha) return mouse.x > 0 ? Zone.Right : Zone.Left;

            var right = LowerUpSection(alpha);
            var left = LowerUpSection(alpha + 180f);
            var up = new Vector2(0, _upLength);
            var down = new Vector2(0, -_upLength);
            if (PointInTriangle(mouse, right, left, up)) return Zone.Up;
            if (PointInTriangle(mouse, right, left, down)) return Zone.Down;

            return mouse.y > 0 ? Zone.OuterUp : Zone.OuterDown;

            static bool PointInTriangle(Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
            {
                float d1 = Sign(pt, a, b);
                float d2 = Sign(pt, b, c);
                float d3 = Sign(pt, c, a);
                return (d1 >= 0 && d2 >= 0 && d3 >= 0) || (d1 <= 0 && d2 <= 0 && d3 <= 0);

                static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
            }
        }

        void DrawRadialMenu(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            painter.lineWidth = _outlineWidth;

            painter.strokeColor = _outlineColor;

            float alpha = _horizontalAngle * 0.5f;
            (float upLeft, float upRight, float downRight, float downLeft) angles = (180f + alpha, 360f - alpha, alpha, 180f - alpha);

            float outer = Screen.width;
            float inner = LowerUpSection(alpha).magnitude;
            float dead = _deadZoneRadius;
            var upLeft = AngleToVector(angles.upLeft);
            var upRight = AngleToVector(angles.upRight);
            var downRight = AngleToVector(angles.downRight);
            var downLeft = AngleToVector(angles.downLeft);

            // left
            DrawShape(
                ColorForZone(Zone.Left), () => {
                    painter.MoveTo(upLeft * outer);
                    painter.LineTo(downLeft * outer);
                    painter.LineTo(downLeft * dead);
                    painter.Arc(Vector3.zero, dead, angles.downLeft, angles.upLeft);
                    painter.ClosePath();
                }
            );
            // right
            DrawShape(
                ColorForZone(Zone.Right), () => {
                    painter.MoveTo(upRight * outer);
                    painter.LineTo(downRight * outer);
                    painter.LineTo(downRight * dead);
                    painter.Arc(Vector3.zero, dead, angles.downRight, angles.upRight, ArcDirection.CounterClockwise);
                    painter.ClosePath();
                }
            );

            // spike up
            var spikeUp = new Vector2(0, -_upLength);
            DrawShape(
                ColorForZone(Zone.Up), () => {
                    painter.MoveTo(spikeUp);
                    painter.LineTo(upRight * inner);
                    painter.LineTo(upRight * dead);
                    painter.Arc(Vector3.zero, dead, angles.upRight, angles.upLeft, ArcDirection.CounterClockwise);
                    painter.LineTo(upLeft * inner);
                    painter.ClosePath();
                }
            );

            // spike Down
            var spikeDown = new Vector2(0, _upLength);
            DrawShape(
                ColorForZone(Zone.Down), () => {
                    painter.MoveTo(spikeDown);
                    painter.LineTo(downRight * inner);
                    painter.LineTo(downRight * dead);
                    painter.Arc(Vector3.zero, dead, angles.downRight, angles.downLeft);
                    painter.LineTo(downLeft * inner);
                    painter.ClosePath();
                }
            );

            // up
            DrawShape(
                ColorForZone(Zone.OuterUp), () => {
                    painter.MoveTo(upLeft * outer);
                    painter.LineTo(upRight * outer);
                    painter.LineTo(upRight * inner);
                    painter.LineTo(spikeUp);
                    painter.LineTo(upLeft * inner);
                    painter.ClosePath();
                }
            );
            // down
            DrawShape(
                ColorForZone(Zone.OuterDown), () => {
                    painter.MoveTo(downLeft * outer);
                    painter.LineTo(downRight * outer);
                    painter.LineTo(downRight * inner);
                    painter.LineTo(spikeDown);
                    painter.LineTo(downLeft * inner);
                    painter.ClosePath();
                }
            );
            return;

            void DrawShape(Color color, Action drawPath)
            {
                painter.fillColor = color;
                painter.BeginPath();
                drawPath();
                painter.Fill();
                painter.Stroke();
            }
        }

        Vector2 LowerUpSection(float angle)
        {
            var v = AngleToVector(angle);
            return v / Mathf.Abs(v.x) * _upWidth;
        }

        static Vector2 AngleToVector(float angleDegrees)
        {
            float rad = Mathf.Deg2Rad * angleDegrees;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}