using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WPFGame.Core
{
   public abstract class Entity
   {
      public double X { get; set; }
      public double Y { get; set; }

      public double Width { get; protected set; }
      public double Height { get; protected set; }

      public double VelocityY { get; set; }

      public bool OnGround { get; protected set; }
      public bool TouchingLadder { get; protected set; }

      public double ActiveLadderTop { get; protected set; }
      public double ActiveLadderBottom { get; protected set; }

      public Rectangle VisualShape { get; protected set; }

      public Rect HitBox =>
          new Rect(
              X,
              Y,
              Width,
              Height);

      // Применяет старую вертикальную физику и дополнительно разрешает
      // столкновения со всеми сторонами Ground
      public virtual void UpdatePhysics(
          UIElementCollection mapElements,
          double gravity,
          bool canStandOnPlatforms,
          double? previousX = null,
          double? previousY = null)
      {
         double startX =
             previousX ?? X;

         double startY =
             previousY ?? Y;

         // Горизонтальное движение уже выполнено наследником.
         // Здесь только корректируется пересечение с твёрдой стеной.
         ResolveHorizontalGroundCollision(
             mapElements,
             startX);

         VelocityY +=
             gravity;

         Y +=
             VelocityY;

         OnGround =
             false;

         TouchingLadder =
             false;

         double highestFloorY =
             double.MaxValue;

         bool foundFloor =
             false;

         double lowestCeilingY =
             double.MinValue;

         bool foundCeiling =
             false;

         double previousFeetY =
             startY +
             Height;

         double feetY =
             Y +
             Height;

         foreach (var element in
                  mapElements.OfType<Rectangle>())
         {
            Rect elementHitBox =
                GetElementHitBox(
                    element);

            string? tag =
                element.Tag as string;

            if (tag == "Ground")
            {
               // Верхняя сторона Ground работает как обычный пол.
               if (VelocityY >= 0 &&
                   HorizontallyOverlaps(
                       HitBox,
                       elementHitBox) &&
                   previousFeetY <=
                       elementHitBox.Top + 0.1 &&
                   feetY >=
                       elementHitBox.Top)
               {
                  highestFloorY =
                      Math.Min(
                          highestFloorY,
                          elementHitBox.Top);

                  foundFloor =
                      true;
               }

               // Нижняя сторона того же Ground работает как потолок.
               if (VelocityY < 0 &&
                   HorizontallyOverlaps(
                       HitBox,
                       elementHitBox) &&
                   startY >=
                       elementHitBox.Bottom - 0.1 &&
                   Y <=
                       elementHitBox.Bottom)
               {
                  lowestCeilingY =
                      Math.Max(
                          lowestCeilingY,
                          elementHitBox.Bottom);

                  foundCeiling =
                      true;
               }

               continue;
            }

            if (!HitBox.IntersectsWith(
                    elementHitBox))
            {
               continue;
            }

            if (tag == "Platform")
            {
               double platformTop =
                   elementHitBox.Top;

               // Платформа остаётся односторонней.
               if (canStandOnPlatforms &&
                   VelocityY >= 0 &&
                   previousFeetY <=
                       platformTop + 0.1 &&
                   feetY >=
                       platformTop &&
                   feetY <=
                       platformTop + 15)
               {
                  highestFloorY =
                      Math.Min(
                          highestFloorY,
                          platformTop);

                  foundFloor =
                      true;
               }
            }
            else if (tag == "Ladder")
            {
               TouchingLadder =
                   true;

               // Если лестница состоит из нескольких сегментов,
               // сохраняется общий вертикальный диапазон.
               if (ActiveLadderTop == 0 &&
                   ActiveLadderBottom == 0)
               {
                  ActiveLadderTop =
                      elementHitBox.Top;

                  ActiveLadderBottom =
                      elementHitBox.Bottom;
               }
               else
               {
                  ActiveLadderTop =
                      Math.Min(
                          ActiveLadderTop,
                          elementHitBox.Top);

                  ActiveLadderBottom =
                      Math.Max(
                          ActiveLadderBottom,
                          elementHitBox.Bottom);
               }
            }
            else if (tag ==
                     "SlopeUpRight")
            {
               double slopeLeft =
                   elementHitBox.Left;

               double slopeWidth =
                   element.Width;

               double slopeHeight =
                   element.Height;

               double slopeBottom =
                   elementHitBox.Bottom;

               double targetX =
                   X +
                   Width;

               double progress =
                   (targetX -
                    slopeLeft) /
                   slopeWidth;

               progress =
                   Math.Clamp(
                       progress,
                       0,
                       1);

               double currentFloorY =
                   slopeBottom -
                   progress *
                   slopeHeight;

               if (canStandOnPlatforms &&
                   VelocityY >= 0 &&
                   feetY >=
                       currentFloorY - 15 &&
                   feetY <=
                       currentFloorY + 20)
               {
                  highestFloorY =
                      Math.Min(
                          highestFloorY,
                          currentFloorY);

                  foundFloor =
                      true;
               }
            }
            else if (tag ==
                     "SlopeUpLeft")
            {
               double slopeLeft =
                   elementHitBox.Left;

               double slopeTop =
                   elementHitBox.Top;

               double slopeWidth =
                   element.Width;

               double slopeHeight =
                   element.Height;

               double targetX =
                   X;

               double progress =
                   (targetX -
                    slopeLeft) /
                   slopeWidth;

               progress =
                   Math.Clamp(
                       progress,
                       0,
                       1);

               double currentFloorY =
                   slopeTop +
                   progress *
                   slopeHeight;

               if (canStandOnPlatforms &&
                   VelocityY >= 0 &&
                   feetY >=
                       currentFloorY - 15 &&
                   feetY <=
                       currentFloorY + 20)
               {
                  highestFloorY =
                      Math.Min(
                          highestFloorY,
                          currentFloorY);

                  foundFloor =
                      true;
               }
            }
         }

         // ==========================================
         // ИСПРАВЛЕННЫЙ БЛОК: ПОТОЛКИ И ПОЛЫ
         // ==========================================
         if (foundCeiling)
         {
            Y = lowestCeilingY;
            VelocityY = 0;
         }
         else if (foundFloor)
         {
            Y = highestFloorY - Height;
            VelocityY = 0;
            OnGround = true;
         }

         if (!TouchingLadder)
         {
            ActiveLadderTop = 0;
            ActiveLadderBottom = 0;
         }
      }

      // Блокирует пересечение вертикальной стороны Ground,
      // не считая обычный пол стеной
      private void ResolveHorizontalGroundCollision(
          UIElementCollection mapElements,
          double previousX)
      {
         double movementX =
             X -
             previousX;

         if (Math.Abs(
                 movementX) <
             0.001)
         {
            return;
         }

         Rect currentHitBox =
             HitBox;

         double correctedX =
             X;

         bool foundWall =
             false;

         foreach (var element in
                  mapElements.OfType<Rectangle>())
         {
            if (element.Tag as string !=
                "Ground")
            {
               continue;
            }

            Rect tileHitBox =
                GetElementHitBox(
                    element);

            // Важный момент:
            // когда игрок просто стоит НА полу, диапазоны по Y
            // только касаются границей и пол не считается стеной.
            if (!VerticallyOverlaps(
                    currentHitBox,
                    tileHitBox))
            {
               continue;
            }

            if (movementX > 0)
            {
               double previousRight =
                   previousX +
                   Width;

               double currentRight =
                   X +
                   Width;

               if (previousRight <=
                       tileHitBox.Left + 0.1 &&
                   currentRight >=
                       tileHitBox.Left)
               {
                  double candidateX =
                      tileHitBox.Left -
                      Width;

                  correctedX =
                      foundWall
                          ? Math.Min(
                              correctedX,
                              candidateX)
                          : candidateX;

                  foundWall =
                      true;
               }
            }
            else
            {
               double previousLeft =
                   previousX;

               double currentLeft =
                   X;

               if (previousLeft >=
                       tileHitBox.Right - 0.1 &&
                   currentLeft <=
                       tileHitBox.Right)
               {
                  double candidateX =
                      tileHitBox.Right;

                  correctedX =
                      foundWall
                          ? Math.Max(
                              correctedX,
                              candidateX)
                          : candidateX;

                  foundWall =
                      true;
               }
            }
         }

         if (foundWall)
         {
            X =
                correctedX;
         }
      }

      // Возвращает мировую геометрию WPF-тайла
      private static Rect GetElementHitBox(
          Rectangle element)
      {
         double left =
             Canvas.GetLeft(
                 element);

         double top =
             Canvas.GetTop(
                 element);

         if (double.IsNaN(
                 left))
         {
            left =
                0;
         }

         if (double.IsNaN(
                 top))
         {
            top =
                0;
         }

         return new Rect(
             left,
             top,
             element.Width,
             element.Height);
      }

      // Проверяет пересечение по горизонтальной оси с ненулевой шириной
      private static bool HorizontallyOverlaps(
          Rect first,
          Rect second)
      {
         return first.Right >
                    second.Left + 0.1 &&
                first.Left <
                    second.Right - 0.1;
      }

      // Проверяет пересечение по вертикальной оси с ненулевой высотой
      private static bool VerticallyOverlaps(
          Rect first,
          Rect second)
      {
         return first.Bottom >
                    second.Top + 0.1 &&
                first.Top <
                    second.Bottom - 0.1;
      }

      // Синхронизирует мировые координаты сущности с WPF-объектом
      public void Draw()
      {
         if (VisualShape is null)
         {
            return;
         }

         Canvas.SetLeft(
             VisualShape,
             X);

         Canvas.SetTop(
             VisualShape,
             Y);
      }
   }
}