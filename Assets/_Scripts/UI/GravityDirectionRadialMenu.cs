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
        [UxmlAttribute] float _deadZoneRadius = 30f;
        [UxmlAttribute] float _innerRadius = 220;
        [UxmlAttribute] float _outerRadius = 1400;
        [UxmlAttribute] Color _outlineColor = Color.white;
        [UxmlAttribute] float _outlineWidth = 4f;

        public float DeadZoneRadius => _deadZoneRadius * scaledPixelsPerPoint;
        public float InnerRadius => _innerRadius * scaledPixelsPerPoint;

        public GravityDirectionRadialMenu()
        {
            generateVisualContent += DrawRadialMenu;
        }

        void DrawRadialMenu(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            painter.lineWidth = _outlineWidth;

            painter.strokeColor = _outlineColor;

            // Draw Inner Radial Menu (Up / Down)
            DrawRadialSection(painter, Vector2.zero, _innerRadius, _outerRadius, 0, 180);
            DrawRadialSection(painter, Vector2.zero, _innerRadius, _outerRadius, 180, 360);

        }

        static void DrawRadialSection(
            Painter2D painter,
            Vector2 center,
            float innerRadius,
            float outerRadius,
            float startAngle,
            float endAngle
        )
        {
            painter.BeginPath();

            // Move to the starting position on the inner circle
            painter.MoveTo(center + AngleToVector(startAngle) * innerRadius);

            // Draw arc at the outer radius
            for (float angle = startAngle; angle < endAngle; angle += 5) {
                painter.LineTo(center + AngleToVector(angle) * outerRadius);
            }
            painter.LineTo(center + AngleToVector(endAngle) * outerRadius);

            // Draw arc back at the inner radius
            for (float angle = endAngle; angle > startAngle; angle -= 5) {
                painter.LineTo(center + AngleToVector(angle) * innerRadius);
            }

            painter.ClosePath();
            painter.Stroke();
        }

        static Vector2 AngleToVector(float angleDegrees)
        {
            float rad = Mathf.Deg2Rad * angleDegrees;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}