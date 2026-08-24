using System.Windows.Controls;

namespace WPFGame.Weapons
{
   public abstract class Weapon
   {
      public string Name { get; set; }
      public int Damage { get; set; }

      // Свойства для патронов (protected set значит, что менять их могут только наследники, например Pistol)
      public int Ammo { get; protected set; }
      public int MaxAmmo { get; protected set; }
      public int ReserveAmmo { get; protected set; }

      // --- НОВЫЕ СВОЙСТВА ДЛЯ ПЕРЕЗАРЯДКИ ---
      public bool IsReloading { get; protected set; } // Флаг перезарядки
      protected int reloadTimer = 0;                  // Сам таймер
      protected int reloadTimeFrames = 60;            // Сколько кадров длится перезарядка (60 = ~1 сек)

      // --- НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ СТРЕЛЬБЫ ---
      public bool IsAutomatic { get; protected set; } // Автомат (true) или Пистолет (false)?
      protected int fireRateFrames = 0;               // Задержка между выстрелами (в кадрах)
      protected int fireCooldownTimer = 0;            // Текущий таймер задержки
      protected bool triggerReady = true;             // Отпустил ли игрок курок?

      public abstract void Attack(Canvas GameArea, double playerX, double playerY, List<WPFGame.Enemies.Enemy> enemies,
                            System.Windows.Controls.UIElementCollection mapElements,
                            List<System.Windows.Shapes.Line> tracers);

      // Метод, который будет вызываться каждый кадр
      public void Tick(bool isShootingHeld)
      {
         // Если мы в процессе перезарядки, уменьшаем таймер
         if (IsReloading)
         {
            reloadTimer--;

            // Когда таймер дошел до нуля - ВЫДАЕМ ПАТРОНЫ
            if (reloadTimer <= 0)
            {
               FinishReload();
            }
         }
         // 2. Таймер скорострельности (охлаждение ствола)
         if (fireCooldownTimer > 0)
         {
            fireCooldownTimer--;
         }

         // 3. Проверка спускового крючка
         if (!isShootingHeld)
         {
            triggerReady = true;
         }
      }

      // Метод перезарядки
      public void Reload()
      {
         // Если уже перезаряжаемся, магазин полон или нет запаса — ничего не делаем
         if (IsReloading || Ammo == MaxAmmo || ReserveAmmo <= 0) return;

         // Начинаем перезарядку!
         IsReloading = true;             // Включаем флаг для HUD и блокировки стрельбы
         reloadTimer = reloadTimeFrames; // Заводим таймер (на 90 кадров, как указано в Pistol)
      }

      // Вспомогательный метод (сама математика выдачи патронов)
      private void FinishReload()
      {
         int bulletsNeeded = MaxAmmo - Ammo;
         int bulletsToLoad = Math.Min(bulletsNeeded, ReserveAmmo);

         Ammo += bulletsToLoad;
         ReserveAmmo -= bulletsToLoad;

         IsReloading = false; // Перезарядка окончена!
      }
   }

}