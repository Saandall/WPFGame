//using System.Windows.Controls;
//<<<<<<< HEAD
//using System.Windows.Media;
//using System.Windows.Shapes;
//using WPFGame.Core;

//namespace WPFGame.PlayerLogic
//{
//    public class Player : Entity
//    {
//        public bool IsClimbing { get; private set; }
//        public bool FacingRight { get; private set; } = true;

//        // Р—Р°РїСЂРµС‰Р°РµС‚ Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРёР№ РїСЂС‹Р¶РѕРє РїРѕСЃР»Рµ РІС‹С…РѕРґР° СЃ Р»РµСЃС‚РЅРёС†С‹
//        private bool preventAutoJump;

//        // РќР° РЅРµСЃРєРѕР»СЊРєРѕ РєР°РґСЂРѕРІ РѕС‚РєР»СЋС‡Р°РµС‚ СЃС‚РѕР»РєРЅРѕРІРµРЅРёРµ СЃ РїР»Р°С‚С„РѕСЂРјР°РјРё
//        private int dropCooldown;

//        public Player(
//            double startX,
//            double startY)
//        {
//            X = startX;
//            Y = startY;

//            Width = 20;
//            Height = 50;

//            VisualShape =
//                new Rectangle
//                {
//                    Width =
//                        Width,

//                    Height =
//                        Height,

//                    Fill =
//                        Brushes.LimeGreen
//                };
//        }

//        // РћР±РЅРѕРІР»СЏРµС‚ СѓРїСЂР°РІР»РµРЅРёРµ Рё С„РёР·РёРєСѓ, РЅРµ РѕРіСЂР°РЅРёС‡РёРІР°СЏ РјРёСЂРѕРІС‹Рµ РєРѕРѕСЂРґРёРЅР°С‚С‹
//        public void Update(
//            UIElementCollection mapElements)
//        {
//            // РЎРѕС…СЂР°РЅСЏСЋС‚СЃСЏ РєРѕРѕСЂРґРёРЅР°С‚С‹ РґРѕ РѕР±С‹С‡РЅРѕРіРѕ РґРІРёР¶РµРЅРёСЏ РёРіСЂРѕРєР°.
//            // Entity РёСЃРїРѕР»СЊР·СѓРµС‚ РёС… С‚РѕР»СЊРєРѕ РґР»СЏ РѕРїСЂРµРґРµР»РµРЅРёСЏ СЃС‚РѕСЂРѕРЅС‹ СЃС‚РѕР»РєРЅРѕРІРµРЅРёСЏ.
//            double previousX =
//                X;

//            double previousY =
//                Y;

//            bool goLeft =
//                Inputmanager.GoLeft;

//            bool goRight =
//                Inputmanager.GoRight;

//            bool goUp =
//                Inputmanager.GoUp;

//            bool goDown =
//                Inputmanager.GoDown;

//            bool jumping =
//                Inputmanager.Jumping;

//            if (!jumping)
//            {
//                preventAutoJump =
//                    false;
//            }

//            if (goLeft)
//            {
//                FacingRight =
//                    false;
//            }

//            if (goRight)
//            {
//                FacingRight =
//                    true;
//            }

//            if (dropCooldown > 0)
//            {
//                dropCooldown--;
//            }

//            if (goDown &&
//                jumping &&
//                OnGround)
//            {
//                dropCooldown =
//                    10;
//            }

//            bool canStandOnPlatforms =
//                dropCooldown == 0 &&
//                !IsClimbing;

//            double currentGravity =
//                0.8;

//            if (IsClimbing)
//            {
//                currentGravity =
//                    0;

//                VelocityY =
//                    0;

//                if (goUp)
//                {
//                    Y -=
//                        5;
//                }

//                if (goDown)
//                {
//                    Y +=
//                        5;
//                }
//            }

//            // Р“РѕСЂРёР·РѕРЅС‚Р°Р»СЊРЅРѕРµ РґРІРёР¶РµРЅРёРµ РѕСЃС‚Р°С‘С‚СЃСЏ С‚Р°РєРёРј Р¶Рµ,
//            // РєР°Рє РІ СЃС‚Р°Р±РёР»СЊРЅРѕР№ РІРµСЂСЃРёРё РґРѕ РґРѕР±Р°РІР»РµРЅРёСЏ СЃС‚РµРЅ.
//            if (goLeft)
//            {
//                X -=
//                    15;
//            }

//            if (goRight)
//            {
//                X +=
//                    15;
//            }

//            base.UpdatePhysics(
//                mapElements,
//                currentGravity,
//                canStandOnPlatforms,
//                previousX,
//                previousY);

//            if (!TouchingLadder)
//            {
//                if (IsClimbing)
//                {
//                    preventAutoJump =
//                        true;
//                }

//                IsClimbing =
//                    false;
//            }

//            if (TouchingLadder &&
//                !IsClimbing)
//            {
//                double feetY =
//                    Y +
//                    Height;

//                if (feetY <=
//                    ActiveLadderTop + 10)
//                {
//                    if (goDown)
//                    {
//                        IsClimbing =
//                            true;

//                        VelocityY =
//                            0;
//                    }
//                }
//                else if (goUp ||
//                         goDown)
//                {
//                    IsClimbing =
//                        true;

//                    VelocityY =
//                        0;
//                }
//            }

//            if (IsClimbing &&
//                OnGround &&
//                (goLeft ||
//                 goRight) &&
//                !goUp &&
//                !goDown)
//            {
//                IsClimbing =
//                    false;
//            }

//            if (jumping &&
//                OnGround &&
//                !IsClimbing &&
//                !goDown &&
//                !preventAutoJump)
//            {
//                VelocityY =
//                    -15;
//            }
//        }
//    }
//}
//=======
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Shapes;
//using WPFGame.Core; // Подключаем нашу физику

//namespace WPFGame.PlayerLogic
//{
//   public class Player : Entity
//   {
//      public bool IsClimbing { get; private set; }
//      public bool FacingRight { get; private set; } = true;

//      // Флаг нужен для того чтобы после поднятия на вершину лестницы герой 
//      // не начал сразу же прыгать
//      private bool preventAutoJump = false;
//      private int speed = 5;
//      private int dropCooldown = 0;

//      public Player(double startX, double startY)
//      {
//         X = startX;
//         Y = startY;
//         Width = 20;
//         Height = 50;

//         VisualShape = new Rectangle
//         {
//            Width = this.Width,
//            Height = this.Height,
//            Fill = Brushes.LimeGreen
//         };
//      }

//      // Дирижер (GameTick) вызывает этот метод, передавая нажатые кнопки
//      public void Update(UIElementCollection mapElements, double roomWidth)
//      {
//         // 1. ЧИТАЕМ КНОПКИ НАПРЯМУЮ ИЗ МЕНЕДЖЕРА
//         bool goLeft = Inputmanager.GoLeft;
//         bool goRight = Inputmanager.GoRight;
//         bool goUp = Inputmanager.GoUp;
//         bool goDown = Inputmanager.GoDown;
//         bool jumping = Inputmanager.Jumping;

//         if (!jumping)
//         {
//            preventAutoJump = false; // Палец отпустили, можно снова прыгать
//         }

//         // 2. ОБНОВЛЯЕМ НАПРАВЛЕНИЕ ВЗГЛЯДА
//         if (goLeft) FacingRight = false;
//         if (goRight) FacingRight = true;

//         // 3. ОТРАБОТКА СПРЫГИВАНИЯ
//         if (dropCooldown > 0) dropCooldown--;
//         if (goDown && jumping && OnGround) dropCooldown = 10; // 10 - количество кадров, которые герой является "призраком" при спрыгивании.
//                                                               // Измменить при надобности ради левел-дизайна
//                                                               // Без OnGround проверка будет выполняться каждый кадр и герой будет безостановочно падать,
//                                                               // пока кнопки не будут отжаты
//         bool canStandOnPlatforms = (dropCooldown == 0) && !IsClimbing;


//         // 6. ДВИЖЕНИЕ И ФИЗИКА
//         double currentGravity = 0.8;

//         if (IsClimbing)
//         {
//            currentGravity = 0;
//            VelocityY = 0;
//            if (goUp) Y -= speed;
//            if (goDown) Y += speed;
//         }

//         // Движение вбок
//         if (goLeft) X -= speed;
//         if (goRight) X += speed;

//         // 4. БАЗОВАЯ ФИЗИКА КОЛЛИЗИЙ (Из Entity)
//         base.UpdatePhysics(mapElements, currentGravity, canStandOnPlatforms);

//         // 5. ЛОГИКА ЛЕСТНИЦЫ ИГРОКА
//         if (!TouchingLadder)
//         {
//            if (IsClimbing)
//            {
//               preventAutoJump = true;
//            }
//            IsClimbing = false;
//         }
//         if (TouchingLadder && !IsClimbing)
//         {
//            double feetY = Y + Height;

//            // Если наши ноги находятся на самом верху лестницы (в пределах 10 пикселей от её верхушки)
//            if (feetY <= ActiveLadderTop + 10)
//            {
//               // Нам разрешено хвататься за неё ТОЛЬКО если мы жмем "ВНИЗ" (хотим спуститься)
//               if (goDown)
//               {
//                  IsClimbing = true;
//                  VelocityY = 0;
//               }
//            }
//            // Если мы находимся ниже верхушки лестницы (на самой лестнице)
//            else
//            {
//               // Хватаемся стандартно по нажатию Вверх или Вниз
//               if (goUp || goDown)
//               {
//                  IsClimbing = true;
//                  VelocityY = 0;
//               }
//            }
//         }

//         if (IsClimbing && OnGround && (goLeft || goRight) && !goUp && !goDown)
//         {
//            IsClimbing = false;
//         }

//         // Прыжок
//         if (jumping && OnGround && !IsClimbing && !goDown && !preventAutoJump)
//         {
//            VelocityY = -15;
//         }

//         // Ограничение экрана
//         double maxX = roomWidth - Width;
//         if (X < 0) X = 0;
//         if (X > maxX) X = maxX;
//      }
//   }
//}
//>>>>>>> opezdol


using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;

namespace WPFGame.PlayerLogic
{
   public class Player : Entity
   {
      public bool IsClimbing { get; private set; }
      public bool FacingRight { get; private set; } = true;

      // Запрещает автоматический прыжок после выхода с лестницы
      private bool preventAutoJump = false;

      private int speed = 5;
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

      // Обновляет управление и физику. Принимает ширину комнаты от камеры!
      public void Update(UIElementCollection mapElements, double roomWidth)
      {
         // СОХРАНЯЕМ СТАРЫЕ КООРДИНАТЫ (Нововведение напарника для столкновения со стенами)
         double previousX = X;
         double previousY = Y;

         // 1. ЧИТАЕМ КНОПКИ НАПРЯМУЮ ИЗ МЕНЕДЖЕРА
         bool goLeft = Inputmanager.GoLeft;
         bool goRight = Inputmanager.GoRight;
         bool goUp = Inputmanager.GoUp;
         bool goDown = Inputmanager.GoDown;
         bool jumping = Inputmanager.Jumping;

         if (!jumping)
         {
            preventAutoJump = false; // Палец отпустили, можно снова прыгать
         }

         // 2. ОБНОВЛЯЕМ НАПРАВЛЕНИЕ ВЗГЛЯДА
         if (goLeft) FacingRight = false;
         if (goRight) FacingRight = true;

         // 3. ОТРАБОТКА СПРЫГИВАНИЯ С ПЛАТФОРМ
         if (dropCooldown > 0) dropCooldown--;
         if (goDown && jumping && OnGround) dropCooldown = 10;

         bool canStandOnPlatforms = (dropCooldown == 0) && !IsClimbing;

         // 4. ДВИЖЕНИЕ И ФИЗИКА (ГРАВИТАЦИЯ)
         double currentGravity = 0.8;

         if (IsClimbing)
         {
            currentGravity = 0;
            VelocityY = 0;
            if (goUp) Y -= speed;
            if (goDown) Y += speed;
         }

         // Горизонтальное движение
         if (goLeft) X -= speed;
         if (goRight) X += speed;

         // 5. ВЫЗЫВАЕМ БАЗОВУЮ ФИЗИКУ (Передаем previousX и previousY для стен!)
         base.UpdatePhysics(mapElements, currentGravity, canStandOnPlatforms, previousX, previousY);

         // 6. ЛОГИКА ЛЕСТНИЦЫ ИГРОКА
         if (!TouchingLadder)
         {
            if (IsClimbing)
            {
               preventAutoJump = true;
            }
            IsClimbing = false;
         }

         if (TouchingLadder && !IsClimbing)
         {
            double feetY = Y + Height;

            // Если наши ноги находятся на самом верху лестницы (в пределах 10 пикселей от её верхушки)
            if (feetY <= ActiveLadderTop + 10)
            {
               // Нам разрешено хвататься за неё ТОЛЬКО если мы жмем "ВНИЗ" (хотим спуститься)
               if (goDown)
               {
                  IsClimbing = true;
                  VelocityY = 0;
               }
            }
            // Если мы находимся ниже верхушки лестницы (на самой лестнице)
            else
            {
               if (goUp || goDown)
               {
                  IsClimbing = true;
                  VelocityY = 0;
               }
            }
         }

         // Сход с лестницы вбок на полу
         if (IsClimbing && OnGround && (goLeft || goRight) && !goUp && !goDown)
         {
            IsClimbing = false;
         }

         // Прыжок
         if (jumping && OnGround && !IsClimbing && !goDown && !preventAutoJump)
         {
            VelocityY = -15;
         }

         // 7. ОГРАНИЧЕНИЕ ЭКРАНА КОМНАТЫ
         double maxX = roomWidth - Width;
         if (X < 0) X = 0;
         if (X > maxX) X = maxX;
      }
   }
}