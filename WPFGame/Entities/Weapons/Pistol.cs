using System.Windows.Controls;
using WPFGame.Projectiles;

namespace WPFGame.Weapons
{
   public class Pistol : Weapon
   {
      public Pistol()
      {
         Name = "Colt Python";
         Damage = 25;
         MaxAmmo = 6;
         Ammo = 6;
         ReserveAmmo = 24;
         reloadTimeFrames = 90; // Пусть пистолет перезаряжается 1.5 секунды (90 кадров)

         IsAutomatic = false;
         fireRateFrames = 15; // четверть секунды
      }

      public override void Attack(Canvas GameArea, double playerX, double playerY, bool facingRight, List<Bullet> activeBullets)
      {
         // 1. Блокируем стрельбу во время перезарядки
         if (IsReloading) return;
         // Проверяем, есть ли патроны. Если нет - просто выходим (осечка).
         if (Ammo <= 0) return;

         // 1. Проверка скорострельности (если ствол еще не остыл - выходим)
         if (fireCooldownTimer > 0) return;

         // 2. Проверка зажатой кнопки (если это не автомат, и курок еще нажат - выходим)
         if (!IsAutomatic && !triggerReady) return;

         Ammo -= 1;
         double spawnX = facingRight ? playerX + 50 : playerX - 10;
         double spawnY = playerY + 20;

         // Пулька
         Bullet newBullet = new Bullet(spawnX, spawnY, 15, facingRight, Damage);

         // Добавляем пульку физически
         activeBullets.Add(newBullet);

         // Добавляем пульку визуально
         GameArea.Children.Add(newBullet.VisualShape);

         // --- ПОСЛЕ ВЫСТРЕЛА ---
         fireCooldownTimer = fireRateFrames; // Запускаем задержку до следующего выстрела
         triggerReady = false;               // Блокируем курок (пока игрок не отпустит кнопку Z)

         // Автоматическая перезарядка
         if (Ammo == 0 && ReserveAmmo > 0)
         {
            Reload();
         }
      }
   }
}
