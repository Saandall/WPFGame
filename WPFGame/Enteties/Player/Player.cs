using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core; // Подключаем нашу физику

namespace WPFGame.PlayerLogic
{
   public class Player : Entity
   {
      public bool IsClimbing { get; private set; }
      public bool FacingRight { get; private set; } = true;
      private int dropCooldown = 0;

      public Player(double startX, double startY)
      {
         X = startX;
         Y = startY;
         Width = 20;
         Height = 50;

         VisualShape = new Rectangle
         {
            Width = this.Width,
            Height = this.Height,
            Fill = Brushes.LimeGreen
         };
      }

      // Дирижер (GameTick) вызывает этот метод, передавая нажатые кнопки
      public void Update(UIElementCollection mapElements, double roomWidth)
      {
         // 1. ЧИТАЕМ КНОПКИ НАПРЯМУЮ ИЗ МЕНЕДЖЕРА
         bool goLeft = Inputmanager.GoLeft;
         bool goRight = Inputmanager.GoRight;
         bool goUp = Inputmanager.GoUp;
         bool goDown = Inputmanager.GoDown;
         bool jumping = Inputmanager.Jumping;

         // 2. ОБНОВЛЯЕМ НАПРАВЛЕНИЕ ВЗГЛЯДА
         if (goLeft) FacingRight = false;
         if (goRight) FacingRight = true;

         // 3. ОТРАБОТКА СПРЫГИВАНИЯ
         if (dropCooldown > 0) dropCooldown--;
         if (goDown && jumping) dropCooldown = 15;
         bool canStandOnPlatforms = (dropCooldown == 0) && !IsClimbing;

         // 4. БАЗОВАЯ ФИЗИКА КОЛЛИЗИЙ (Из Entity)
         base.UpdatePhysics(mapElements, 0, canStandOnPlatforms);

         // 5. ЛОГИКА ЛЕСТНИЦЫ ИГРОКА
         if (!TouchingLadder) IsClimbing = false;

         if (TouchingLadder && !IsClimbing && (goUp || goDown))
         {
            IsClimbing = true;
            VelocityY = 0;
         }

         // Сомнительная проверка. Проверить на необходимость. Идейно нужна при спуске и контакте с Ground
         if (IsClimbing && OnGround && (goLeft || goRight))
         {
            IsClimbing = false;
         }

         // 6. ДВИЖЕНИЕ И ФИЗИКА
         double currentGravity = 0.8;

         if (IsClimbing)
         {
            currentGravity = 0;
            VelocityY = 0;
            if (goUp) Y -= 5;
            if (goDown) Y += 5;
         }

         // Гравитация
         VelocityY += currentGravity;

         // Движение вбок
         if (goLeft) X -= 5;
         if (goRight) X += 5;

         // Прыжок
         if (jumping && OnGround && !IsClimbing && !goDown)
         {
            VelocityY = -15;
         }

         // Ограничение экрана
         double maxX = roomWidth - Width;
         if (X < 0) X = 0;
         if (X > maxX) X = maxX;
      }
   }
}