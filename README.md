# Zombie Survival

Unity ile geliştirilmiş, üçüncü şahıs kamera açısına sahip 3D zombi hayatta kalma oyunu.
Oyuncu, saldıran zombilere karşı hayatta kalır ve kurtarma helikopterine ulaşmaya çalışır.

## Özellikler

- WASD ile hareket, koşma ve zıplama
- Mouse ile yatay ve dikey kamera kontrolü
- Animasyonlu player ve zombi modelleri
- Raycast tabanlı tüfek sistemi
- Roketatar ve silah değiştirme
- Zombi takip, saldırı, hasar ve spawn sistemi
- Can, ölüm ve ragdoll sistemi
- Zombilerden ihtimalli can kiti düşmesi
- Ana menü, seçenekler ve zorluk seviyeleri
- Kan, mermi izi, ses ve isabet efektleri
- Helikopter inişi, kazanma ve Game Over ekranları

## Kontroller

| Kontrol | İşlev |
| --- | --- |
| `W A S D` | Hareket |
| `Mouse` | Kamerayı çevirme |
| `Space` | Zıplama |
| `Sol Mouse` | Ateş etme |
| `1` | Tüfek |
| `2` | Roketatar |
| `Esc` | Menü / mouse kilidini açma |

## Gereksinimler

- Unity `6000.3.9f1` veya uyumlu Unity 6 LTS sürümü
- Universal Render Pipeline
- Git LFS

## Kurulum

```bash
git lfs install
git clone https://github.com/0busrayavuz/Zombie.git
```

1. Unity Hub üzerinden projeyi açın.
2. `Assets/Scenes/SampleScene.unity` sahnesini açın.
3. Unity'nin assetleri içe aktarmasını bekleyin.
4. Play düğmesine basın ve `NEW GAME` seçeneğini kullanın.

## Proje Yapısı

```text
Assets/
  Editor/       Unity Editor araçları
  Generated/    Kodla üretilen materyaller
  Resources/    Model, doku, ses ve arayüz dosyaları
  Scenes/       Oyun sahnesi
  Scripts/      Oynanış kodları
Packages/       Unity paket tanımları
ProjectSettings/
```

## Teknik Notlar

- Sahne ve oynanış sistemleri C# koduyla kurulup yönetilir.
- Eski eğitim içeriğindeki giriş ve animasyon yöntemleri Unity 6 ile uyumlu hâle getirilmiştir.
- `Library`, `Temp`, `Logs` ve kullanıcıya özel Unity klasörleri depoya eklenmez.

## Geliştirici

**Büşra Yavuz**

Oyun Programlama dersi kapsamında geliştirilmiştir.

## Asset Notu

Bu proje eğitim amacıyla sağlanan üçüncü taraf model, doku ve ses dosyaları içerir.
Bu dosyaların kullanım ve yeniden dağıtım koşulları kendi kaynaklarına aittir.
