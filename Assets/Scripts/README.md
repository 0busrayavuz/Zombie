# Unity Zombie Code Pack

Bu klasor PDF'lerdeki Unity zombie shooter konularini code-only C# scriptlerine donusturur.

## Kullanim Mantigi

- Bos bir sahnede `ZombieSurvivalBootstrapper` otomatik devreye girer ve kodla temel demo sahnesi kurar.
- Mevcut bir Unity projesinde Player tag'li obje varsa bootstrapper yeni player olusturmaz.
- Kendi zombie prefab'in varsa `EnemySpawnManager` icindeki `enemyToSpawn` alanina verilebilir.
- Model ve animasyon yoksa sistem primitive capsule zombie ile de calisir.

## Script Haritasi

- Player: `Health`, `PlayerStats`, `PlayerMovement`, `PlayerLook`, `RifleWeapon`, `PlayerAnimation`, `PlayerGui`
- Enemy: `Health`, `EnemyMovement`, `EnemyAttack`, `EnemyAnimation`, opsiyonel `Ragdoll`
- Managers: `GameManager`, `EnemySpawnManager`, `CombatGui`
- Win flow: `Helicopter`, `HelicopterDoors`, `GameManager.WinPlayer()`

## PDF Konulari

Bu paket su ders kavramlarini kapsar: component yapisi, `Update`, `SerializeField`, `CharacterController`, keyboard/mouse input, TPS kamera, enemy AI, prefab/instantiate, raycast damage, health, death, trigger attack, spawn cooldown, `LayerMask`, random viewport spawn, ragdoll, animation, stats, game over ve win state.
