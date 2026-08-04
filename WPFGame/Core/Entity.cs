using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WPFGame.Core
{
   public abstract class Entity
   {
      // Базовые физические свойства, общие для всех
      public double X { get; set; }
      public double Y { get; set; }
      public double Width { get; protected set; }
      public double Height { get; protected set; }
      public double VelocityY { get; set; }

      // Состояния, которые физика отдает наружу (наследникам)
      public bool OnGround { get; protected set; }
      public bool TouchingLadder { get; protected set; }

      // Для понимание верха лестницы (иначе герой будет на ней прыгать
      public double ActiveLadderTop { get; protected set; }
      public double ActiveLadderBottom { get; protected set; }

      // Визуал (прямоугольник), который WPF рисует на экране
      public Rectangle VisualShape { get; protected set; }

      // Хитбокс генерируется на лету
      public Rect HitBox => new Rect(X, Y, Width, Height);

      // =========================================================
      // ТОТ САМЫЙ ОБЩИЙ ЦИКЛ КОЛЛИЗИЙ И ГРАВИТАЦИИ
      // =========================================================
      public virtual void UpdatePhysics(UIElementCollection mapElements, double gravity, bool canStandOnPlatforms)
      {
         OnGround = false;
         TouchingLadder = false;
         double highestFloorY = double.MaxValue;
         bool foundFloor = false;
         double feetY = Y + Height;

         // 1. ПРОВЕРКА КОЛЛИЗИЙ
         foreach (var element in mapElements.OfType<Rectangle>())
         {
            Rect elementHitBox = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element) == double.NaN ? 0 : Canvas.GetTop(element), element.Width, element.Height);

            if (this.HitBox.IntersectsWith(elementHitBox))
            {
               string tag = (string)element.Tag;

               if (tag == "Ground")
               {
                  double floorY = Canvas.GetTop(element);
                  if (VelocityY >= 0 && feetY >= floorY && Y < floorY)
                  {
                     if (floorY < highestFloorY) highestFloorY = floorY;
                     foundFloor = true;
                  }
               }
               else if (tag == "Platform")
               {
                  double platformTop = Canvas.GetTop(element);
                  if (canStandOnPlatforms && VelocityY >= 0 && feetY >= platformTop && feetY <= platformTop + 15)
                  {
                     if (platformTop < highestFloorY) highestFloorY = platformTop;
                     foundFloor = true;
                  }
               }
               else if (tag == "Ladder")
               {
                  TouchingLadder = true;
                  // Запоминаем верх и низ лестницы, которой коснулись
                  ActiveLadderTop = Canvas.GetTop(element);
                  ActiveLadderBottom = ActiveLadderTop + element.Height;
               }
               // Ступенчатая лестница /
               else if ((string)element.Tag == "SlopeUpRight")
               {
                  double slopeLeft = Canvas.GetLeft(element);
                  double slopeWidth = element.Width;
                  double slopeHeight = element.Height;
                  double slopeBottom = Canvas.GetTop(element) + slopeHeight;

                  // Считаем правый край игрока по X
                  double targetX = X + Width;
                  double progress = (targetX - slopeLeft) / slopeWidth;

                  progress = Math.Max(0, Math.Min(1, progress)); // Защита от выхода за рамки

                  double currentFloorY = slopeBottom - (progress * slopeHeight);

                  // Проверяем: падаем ли мы, и находятся ли наши ноги рядом с диагональю (допуск 20 пикселей)
                  if (canStandOnPlatforms && VelocityY >= 0 && feetY >= currentFloorY - 15 && feetY <= currentFloorY + 20)
                  {
                     if (currentFloorY < highestFloorY) highestFloorY = currentFloorY;
                     foundFloor = true;
                  }
               }
               // Ступенчатая лестница \
               else if ((string)element.Tag == "SlopeUpLeft")
               {
                  double slopeLeft = Canvas.GetLeft(element);
                  double slopeTop = Canvas.GetTop(element);
                  double slopeWidth = element.Width;
                  double slopeHeight = element.Height;
                  double slopeBottom = slopeTop + slopeHeight;

                  double targetX = X;
                  double progress = (targetX - slopeLeft) / slopeWidth;
                  progress = Math.Max(0, Math.Min(1, progress));

                  // Математика ИНАЯ: пол начинается вверху (slopeTop) и спускается к низу (slopeBottom)
                  double currentFloorY = slopeTop + (progress * slopeHeight);

                  if (canStandOnPlatforms && VelocityY >= 0 && feetY >= currentFloorY - 15 && feetY <= currentFloorY + 20)
                  {
                     if (currentFloorY < highestFloorY) highestFloorY = currentFloorY;
                     foundFloor = true;
                  }
               }
            }

         }
         // 2. ПРИМЕНЕНИЕ ПОЛА
         if (foundFloor)
         {
            Y = highestFloorY - Height;
            VelocityY = 0;
            OnGround = true;
         }

         // 3. ПРИМЕНЕНИЕ ГРАВИТАЦИИ
         // Если кто-то (например, игрок) отключит гравитацию снаружи (для лестницы), 
         // он просто передаст gravity = 0
         VelocityY += gravity;
         Y += VelocityY;
      }

      // Синхронизация математики с визуалом
      public void Draw()
      {
         if (VisualShape != null)
         {
            Canvas.SetLeft(VisualShape, X);
            Canvas.SetTop(VisualShape, Y);
         }
      }
   }
}